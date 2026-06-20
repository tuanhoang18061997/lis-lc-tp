/* ecg_fs.js
 * One-file utilities for: FS Access API + IndexedDB + Upload + Sync
 * Requires: jQuery
 * Works best on HTTPS or http://localhost. Falls back to <input type="file"> on plain HTTP/IP.
 */

/* ========================= IndexedDB helpers ========================= */
const ECG_IDB = {
    db: null,
    open() {
        return new Promise((resolve, reject) => {
            const req = indexedDB.open('ecgFS', 1);
            req.onupgradeneeded = (e) => {
                const db = e.target.result;
                if (!db.objectStoreNames.contains('handles')) {
                    db.createObjectStore('handles', { keyPath: 'key' });
                }
            };
            req.onsuccess = (e) => { this.db = e.target.result; resolve(this.db); };
            req.onerror = (e) => reject(e);
        });
    },
    async put(key, value) {
        if (!this.db) await this.open();
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction('handles', 'readwrite');
            tx.objectStore('handles').put({ key, value });
            tx.oncomplete = () => resolve(true);
            tx.onerror = (e) => reject(e);
        });
    },
    async get(key) {
        if (!this.db) await this.open();
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction('handles', 'readonly');
            const req = tx.objectStore('handles').get(key);
            req.onsuccess = () => resolve(req.result ? req.result.value : null);
            req.onerror = (e) => reject(e);
        });
    },
    async del(key) {
        if (!this.db) await this.open();
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction('handles', 'readwrite');
            tx.objectStore('handles').delete(key);
            tx.oncomplete = () => resolve(true);
            tx.onerror = (e) => reject(e);
        });
    }
};

/* ========================= Core config ========================= */
const ECGFS = (() => {
    const ECG_DIR_KEY = 'cvs3_ecg_directory';

    // Change these selectors if your markup differs:
    let selectors = {
        patientCode: '#ddt_process_seq', // input chứa "mã khách"
        reloadBtn: '#ddt_process_reload',       // nút reload (để show spinner)
        textKey: "#ddt_text_key",     // input nơi user nhập text_key
    };

    function setSelectors(opts) {
        selectors = { ...selectors, ...(opts || {}) };
    }

    /* ========== Permissions for FS handle ========== */
    async function requestDirPermission(dirHandle) {
        if (!dirHandle) return false;
        let perm = await dirHandle.queryPermission({ mode: 'read' });
        if (perm === 'granted') return true;
        if (perm === 'prompt') {
            perm = await dirHandle.requestPermission({ mode: 'read' });
            return perm === 'granted';
        }
        return false;
    }

    /* ========== Restore or choose directory (persisted via IndexedDB) ========== */
    async function chooseOrRestoreECGDirectory() {
        let dirHandle = await ECG_IDB.get(ECG_DIR_KEY);
        if (dirHandle && await requestDirPermission(dirHandle)) {
            renderFolderNameToLabel(dirHandle);
            return dirHandle;
        }

        if (!window.showDirectoryPicker) {
            renderFolderNameToLabel(null);
            return null; // fallback
        }

        dirHandle = await window.showDirectoryPicker();
        const ok = await requestDirPermission(dirHandle);
        if (!ok) throw new Error('User denied directory permission');

        await ECG_IDB.put(ECG_DIR_KEY, dirHandle);
        renderFolderNameToLabel(dirHandle);
        return dirHandle;
    }


    async function changeECGDirectory() {
        if (!window.showDirectoryPicker) {
            SwalHelper.Toast.warning('Trình duyệt không hỗ trợ chọn thư mục trực tiếp. Vui lòng dùng nút Reload và chọn file thủ công.');
            return;
        }
        try {
            const dirHandle = await window.showDirectoryPicker();
            const ok = await requestDirPermission(dirHandle);
            if (!ok) {
                SwalHelper.Toast.warning('Không có quyền đọc thư mục.');
                return;
            }
            await ECG_IDB.put(ECG_DIR_KEY, dirHandle);
            renderFolderNameToLabel(dirHandle);
        } catch (e) {
            // user có thể bấm Cancel
        }
    }


    /* ========== Find newest file matching yyMMdd-<code>-00-RES- (today) ========== */
    async function findECGFileInDirectory(dirHandle, maBenhAn) {
        const today = new Date();
        const yyyy = String(today.getFullYear());
        const MM = String(today.getMonth() + 1).padStart(2, '0');
        const dd = String(today.getDate()).padStart(2, '0');
        const todayPrefix = `${yyyy}${MM}${dd}`;

        const pattern = new RegExp(`${maBenhAn}`, 'i');

        let newest = null;
        for await (const entry of dirHandle.values()) {
            if (entry.kind === 'file') {
                const name = entry.name;
                if (pattern.test(name)) {
                    const file = await entry.getFile();
                    if (!newest || file.lastModified > newest.lastModified) {
                        newest = file;
                    }
                }
            }
        }
        console.log(newest);
        return newest; // or null
    }

    /* ========== Upload to server (into ecg_export) ========== */
    async function uploadECGFileToServer(file, resultCDHAId, patientCode) {
        const form = new FormData();
        form.append('resultCDHAId', resultCDHAId);
        form.append('patientCode', patientCode);
        form.append('file', file, file.name);

        const resp = await fetch('/DDT_Process/UploadECGAndSync', { method: 'POST', body: form });
        console.log(resp);
        const text = await resp.text();
        if (text !== 'True') throw new Error('UploadECGAndSync failed');
    }

    /* ========== Fallback: user picks file manually (no FS API / HTTP over IP) ========== */
    function getTodayPrefix() {
        const today = new Date();
        return String(today.getFullYear())
            + String(today.getMonth() + 1).padStart(2, '0')
            + String(today.getDate()).padStart(2, '0');
    }

    async function fallbackSelectAndUploadFile(resultCDHAId, patientCode) {
        return new Promise((resolve, reject) => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = '.png,.jpg,.jpeg,.pdf';
            input.onchange = async (e) => {
                try {
                    const file = e.target.files[0];
                    if (!file) return resolve();

                    const todayPrefix = getTodayPrefix();
                    const pattern = new RegExp(`${todayPrefix}-${patientCode}-00-RES-`, 'i');

                    if (!pattern.test(file.name)) {
                        SwalHelper.Toast.warning(`File không khớp mẫu ${todayPrefix}-${patientCode}-00-RES- của ngày hôm nay.`);
                        return resolve();
                    }

                    await uploadECGFileToServer(file, resultCDHAId, patientCode);

                    // Call server sync after placing file into ecg_export
                    await $.ajax({
                        url: "/DDT_Process/ScanFolderAndSyncImages",
                        type: "POST",
                        data: { resultCDHAId: resultCDHAId, patientCode: patientCode }
                    });

                    if (typeof Process_LoadImageForService === 'function') {
                        Process_LoadImageForService(resultCDHAId);
                    }
                    resolve();
                } catch (err) {
                    reject(err);
                }
            };
            input.click();
        });
    }

    /* ========== Helper: get selected resultCDHAId from UI ========== */
    function getSelectedResultCDHAId() {
        let resultCDHAId = null;
        $(".row-service").each(function () {
            $(this).find(".form-check-input").each(function () {
                if ($(this).prop('checked')) resultCDHAId = $(this).val();
            });
        });
        return resultCDHAId;
    }

    /* ========== Public main: Reload (persisted-folder) ========== */
    async function reloadECGImages() {
        // --- GIỚI HẠN SỐ HÌNH ---
        var countImage = 0;
        $(".contain-hinh").each(function () { // Đếm Image đã chụp
            $(this).find(".hinh").each(function () {
                countImage++;
            });
        });

        if (countImage >= 1) { // Giới hạn 1 hình
            SwalHelper.Toast.warning("Chỉ cho phép chụp tối đa 1 hình!");
            return; // Dừng luôn, không chạy reload
        }
        // --- HẾT GIỚI HẠN ---
        const resultCDHAId = getSelectedResultCDHAId();
        if (!resultCDHAId) { SwalHelper.Toast.warning("Vui lòng chọn dịch vụ trước khi reload!"); return; }

        const patientCode = $(selectors.patientCode).val();
        if (!patientCode) { SwalHelper.Toast.warning("Không tìm thấy mã khách trên form!"); return; }

        const maBenhAn = $(selectors.textKey).val();
        if (!maBenhAn) { SwalHelper.Toast.warning("Không tìm thấy mã BA trên form!"); return; }
        // Spinner on button (optional)
        const $btn = $(selectors.reloadBtn);
        const oldHtml = $btn.html();
        if ($btn.length) $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Đang reload');

        try {
            let usedFSAPI = false;
            let fileToUpload = null;

            if (window.showDirectoryPicker) {
                const dirHandle = await chooseOrRestoreECGDirectory();
                if (dirHandle) {
                    fileToUpload = await findECGFileInDirectory(dirHandle, maBenhAn);
                    usedFSAPI = true;
                }
            }
            if (!fileToUpload && !usedFSAPI) {
                // Browser doesn't support FS API → fallback to manual file picker
                await fallbackSelectAndUploadFile(resultCDHAId, patientCode);
            } else if (!fileToUpload && usedFSAPI) {
                SwalHelper.Toast.warning(`Không tìm thấy file ECG phù hợp trong thư mục đã lưu (mẫu ${maBenhAn}-${getTodayPrefix()}).`);
            } else {
                // Upload found file
                await uploadECGFileToServer(fileToUpload, resultCDHAId, patientCode);

                // Now trigger server-side sync & move -> _imported
                await $.ajax({
                    url: "/DDT_Process/ScanFolderAndSyncImages",
                    type: "POST",
                    data: { resultCDHAId: resultCDHAId, maBenhAn: maBenhAn }
                });

                // Refresh images for current service
                if (typeof Process_LoadImageForService === 'function') {
                    Process_LoadImageForService(resultCDHAId);
                }
            }
        } catch (e) {
            console.error(e);
            SwalHelper.Toast.error("Có lỗi khi reload từ thư mục PC. Vui lòng thử lại.");
        } finally {
            if ($btn.length) $btn.prop("disabled", false).html(oldHtml);
        }
    }
    // ===== THÊM vào bên trong IIFE ECGFS (cùng scope với các hàm khác) =====
    let folderLabelSelector = null;

    async function getSavedDirHandleOrNull() {
        try {
            const h = await ECG_IDB.get('cvs3_ecg_directory');
            if (!h) return null;
            const ok = await requestDirPermission(h);
            return ok ? h : null;
        } catch { return null; }
    }

    function renderFolderNameToLabel(handle) {
        if (!folderLabelSelector) return;
        const $el = $(folderLabelSelector);
        if (!$el.length) return;

        if (!handle) {
            $el.text('Chưa chọn thư mục').addClass('ecg-folder-badge--empty');
            return;
        }
        // Bảo mật FS API không cho lấy full path → chỉ hiển thị tên thư mục
        $el.text(`Thư mục: ${handle.name}`).removeClass('ecg-folder-badge--empty');
    }
    async function mountFolderLabel(selector) {
        folderLabelSelector = selector;
        const handle = await getSavedDirHandleOrNull();
        renderFolderNameToLabel(handle);
    }

    // Tự động reload khi quét barcode vào ô patientCode
    function enableBarcodeAutoReload(opts = {}) {
        const inputSel = selectors.textKey;   // vd: '#ddt_text_key'
        const minLen = opts.minLen ?? 6;     // độ dài tối thiểu để coi là barcode
        const endGapMs = opts.endGapMs ?? 120;   // thời gian nghỉ kết thúc quét
        const fastMs = opts.fastMs ?? 35;    // ngưỡng trung bình ms/nhấn để coi là "rất nhanh"
        const debounceMs = opts.debounceMs ?? 800;   // chống bấm đúp

        let lastTs = 0, sumDelta = 0, cntDelta = 0, timer = null, lastFire = 0;

        function resetMeter() { lastTs = 0; sumDelta = 0; cntDelta = 0; }

        function triggerReload() {
            const now = Date.now();
            if (now - lastFire < debounceMs) return; // chặn gọi liên tiếp
            lastFire = now;
            XQFS.Reload();
            resetMeter();
        }

        // Gắn sự kiện cho input
        const $inp = $(inputSel);
        if (!$inp.length) return;

        // 1) Scanner gửi Enter → Reload ngay
        $inp.on('keydown.xqscan', function (e) {
            if (e.key === 'Enter' || e.key === 'NumpadEnter') {
                e.preventDefault();
                triggerReload();
                return;
            }

            // 2) Ghi nhận tốc độ nhập để phân biệt quét vs gõ tay
            if (e.key && e.key.length === 1) {
                const t = Date.now();
                if (lastTs) { sumDelta += (t - lastTs); cntDelta++; }
                lastTs = t;
            }

            clearTimeout(timer);
            timer = setTimeout(() => {
                const val = $inp.val().toString().trim();
                const avg = cntDelta ? (sumDelta / cntDelta) : 999;
                if (val.length >= minLen && avg <= fastMs) {
                    // nhập rất nhanh và đủ dài → coi là quét barcode
                    triggerReload();
                }
                // nếu không đủ nhanh: coi như người gõ tay → KHÔNG auto reload
                resetMeter();
            }, endGapMs);
        });

        // Optional: nếu muốn blur/change cũng Reload (bật khi quy trình cần)
        if (opts.onBlurReload) {
            $inp.on('change.xqscan', function () {
                const v = $(this).val().toString().trim();
                if (v.length >= minLen) triggerReload();
            });
        }
    }
    // expose
    return {
        Reload: reloadECGImages,
        ChangeFolder: changeECGDirectory,
        SetSelectors: setSelectors,
        MountFolderLabel: mountFolderLabel,
        EnableBarcodeAutoReload: enableBarcodeAutoReload
    };
})();

/* Attach to window for usage in inline onclick handlers if needed */
window.ECGFS = ECGFS;
