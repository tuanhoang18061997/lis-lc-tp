/* xq_fs.js
 * FS Access API + IndexedDB + Upload + Sync (X-Quang)
 * Requires: jQuery
 */
// ---- đặt gần đầu file, trước khi dùng ----
const IMAGE_EXTS = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".webp"];

function isImageFile(name) {
    const lower = String(name || "").toLowerCase();
    return IMAGE_EXTS.some(ext => lower.endsWith(ext));
}
/* ========================= IndexedDB helpers ========================= */
const XQ_IDB = {
    db: null,
    open() {
        return new Promise((resolve, reject) => {
            const req = indexedDB.open("xqFS", 1);
            req.onupgradeneeded = (e) => {
                const db = e.target.result;
                if (!db.objectStoreNames.contains("handles")) {
                    db.createObjectStore("handles", { keyPath: "key" });
                }
            };
            req.onsuccess = (e) => { this.db = e.target.result; resolve(this.db); };
            req.onerror = (e) => reject(e);
        });
    },
    async put(key, value) {
        if (!this.db) await this.open();
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction("handles", "readwrite");
            tx.objectStore("handles").put({ key, value });
            tx.oncomplete = () => resolve(true);
            tx.onerror = (e) => reject(e);
        });
    },
    async get(key) {
        if (!this.db) await this.open();
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction("handles", "readonly");
            const req = tx.objectStore("handles").get(key);
            req.onsuccess = () => resolve(req.result ? req.result.value : null);
            req.onerror = (e) => reject(e);
        });
    },
    async del(key) {
        if (!this.db) await this.open();
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction("handles", "readwrite");
            tx.objectStore("handles").delete(key);
            tx.oncomplete = () => resolve(true);
            tx.onerror = (e) => reject(e);
        });
    },
};

/* ========================= Core config ========================= */
const XQFS = (() => {
    const XQ_DIR_KEY = "cvs3_xq_directory";

    // Thay đổi nếu markup khác
    let selectors = {
        patientCode: "#xq_process_seq",  // input chứa "mã khách"
        reloadBtn: "#xq_process_reload", // nút reload
        textKey: "#xq_text_key",     // input nơi user nhập text_key
    };

    function SetSelectors(opts) {
        selectors = { ...selectors, ...(opts || {}) };
    }

    /* ========== Quyền truy cập thư mục ========== */
    async function requestDirPermission(dirHandle) {
        if (!dirHandle) return false;
        let perm = await dirHandle.queryPermission({ mode: "read" });
        if (perm === "granted") return true;
        if (perm === "prompt") {
            perm = await dirHandle.requestPermission({ mode: "read" });
            return perm === "granted";
        }
        return false;
    }

    /* ========== Chọn/khôi phục thư mục đã lưu (IndexedDB) ========== */
    async function chooseOrRestoreXQDirectory() {
        let dirHandle = await XQ_IDB.get(XQ_DIR_KEY);
        if (dirHandle && (await requestDirPermission(dirHandle))) {
            renderFolderNameToLabel(dirHandle);
            return dirHandle;
        }

        if (!window.showDirectoryPicker) {
            renderFolderNameToLabel(null);
            return null; // fallback
        }

        dirHandle = await window.showDirectoryPicker();
        const ok = await requestDirPermission(dirHandle);
        if (!ok) throw new Error("User denied directory permission");

        await XQ_IDB.put(XQ_DIR_KEY, dirHandle);
        renderFolderNameToLabel(dirHandle);
        return dirHandle;
    }

    async function ChangeFolder() {
        if (!window.showDirectoryPicker) {
            SwalHelper.Toast.warning("Trình duyệt không hỗ trợ chọn thư mục trực tiếp. Vui lòng dùng nút Reload và chọn file thủ công.");
            return;
        }
        try {
            const dirHandle = await window.showDirectoryPicker();
            const ok = await requestDirPermission(dirHandle);
            if (!ok) { SwalHelper.Toast.error("Không có quyền đọc thư mục."); return; }
            await XQ_IDB.put(XQ_DIR_KEY, dirHandle);
            renderFolderNameToLabel(dirHandle);
        } catch { /* user Cancel */ }
    }

    /* ========== XQ: tìm SUBFOLDER theo text_key ========== */
    function escapeRegex(s) {
        return String(s || "").replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    }

    // Mẫu: ^\d{8}_[^_]+_<text_key>_.+$
    function buildSubdirPattern(textKey) {
        const safe = escapeRegex(textKey.trim());
        return new RegExp(`^\\d{8}_[^_]+_${safe}_.+$`, "i");
    }

    // Chọn subfolder khớp text_key; nếu có nhiều → lấy yyyyMMdd lớn nhất
    async function findTargetSubdirByTextKey(rootDirHandle, textKey) {
        if (!textKey) return null;
        const dirPattern = buildSubdirPattern(textKey);
        let best = null;

        for await (const entry of rootDirHandle.values()) {
            if (entry.kind === "directory" && dirPattern.test(entry.name)) {
                const ymd = entry.name.slice(0, 8);
                if (/^\d{8}$/.test(ymd)) {
                    if (!best || ymd > best.ymd) best = { handle: entry, ymd };
                } else if (!best) {
                    best = { handle: entry, ymd: "00000000" };
                }
            }
        }
        return best ? best.handle : null;
    }

    /* ========== Tìm file mới nhất khớp yyMMdd-<code>-00-RES- (trong ngày) ========== */
    // Nay: chỉ cần là file ảnh → chọn file có lastModified mới nhất
    async function findXQFilesInDirectory(dirHandle) {
        const files = [];
        for await (const entry of dirHandle.values()) {
            if (entry.kind === "file" && isImageFile(entry.name)) {
                const file = await entry.getFile();
                files.push(file);
            }
        }
        // Upload theo thứ tự cũ → mới
        files.sort((a, b) => a.lastModified - b.lastModified);
        return files; // có thể rỗng []
    }

    // Kết hợp: tìm subfolder theo text_key rồi tìm file bên trong
    async function findXQFilesInNestedDirectory(rootDirHandle, textKey) {
        const sub = await findTargetSubdirByTextKey(rootDirHandle, textKey);
        if (!sub) return [];
        return await findXQFilesInDirectory(sub);
    }

    /* ========== Upload lên server (thư mục xq_export) ========== */
    async function uploadXQFileToServer(file, resultCDHAId, patientCode) {
        const form = new FormData();
        form.append("resultCDHAId", resultCDHAId);
        form.append("patientCode", patientCode);
        form.append("file", file, file.name);

        const resp = await fetch("/XQ_Process/UploadXQAndSync", { method: "POST", body: form });
        const text = await resp.text();
        if (text !== "True") throw new Error("UploadXQAndSync failed");
    }

    /* ========== Fallback: chọn file thủ công khi không có FS API ========== */
    function getTodayPrefix() {
        const today = new Date();
        return (
            String(today.getFullYear()).slice(-2) +
            String(today.getMonth() + 1).padStart(2, "0") +
            String(today.getDate()).padStart(2, "0")
        );
    }

    async function fallbackSelectAndUploadFile(resultCDHAId, patientCode) {
        return new Promise((resolve, reject) => {
            const input = document.createElement("input");
            input.type = "file";
            input.accept = IMAGE_EXTS.join(","); // ".png,.jpg,..."
            input.onchange = async (e) => {
                try {
                    const files = Array.from(e.target.files || []);
                    if (!files.length) return resolve();

                    // Lọc ảnh, upload từng cái
                    for (const file of files) {
                        if (!isImageFile(file.name)) continue;
                        await uploadXQFileToServer(file, resultCDHAId, patientCode);
                    }

                    await uploadXQFileToServer(file, resultCDHAId, patientCode);

                    // Gọi đồng bộ server sau khi upload
                    await $.ajax({
                        url: "/XQ_Process/ScanFolderAndSyncImages",
                        type: "POST",
                        data: { resultCDHAId, patientCode },
                    });

                    if (typeof Process_LoadImageForService === "function") {
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

    /* ========== Lấy resultCDHAId đang chọn từ UI ========== */
    function getSelectedResultCDHAId() {
        let resultCDHAId = null;
        $(".row-service").each(function () {
            $(this).find(".form-check-input").each(function () {
                if ($(this).prop("checked")) resultCDHAId = $(this).val();
            });
        });
        return resultCDHAId;
    }

    /* ========== Public main: Reload ========== */
    async function Reload() {
        // --- GIỚI HẠN SỐ HÌNH ---
        var countImage = 0;
        $(".contain-hinh").each(function () { // Đếm Image đã chụp
            $(this).find(".hinh").each(function () {
                countImage++;
            });
        });
        console.log("Số hình hiện tại:", countImage);
        
        const MAX_IMAGES = 2;
        if (countImage >= MAX_IMAGES) { // Giới hạn 2 hình
            SwalHelper.Toast.warning("Chỉ cho phép chụp tối đa 2 hình!");
            return; // Dừng luôn, không chạy reload
        }
        
        const remainingSlots = MAX_IMAGES - countImage; // Số hình còn có thể upload
        console.log("Số hình còn có thể upload:", remainingSlots);
        // --- HẾT GIỚI HẠN ---
        
        const resultCDHAId = getSelectedResultCDHAId();
        if (!resultCDHAId) { SwalHelper.Toast.warning("Vui lòng chọn dịch vụ trước khi reload!"); return; }

        const patientCode = $(selectors.patientCode).val();
        if (!patientCode) { SwalHelper.Toast.warning("Không tìm thấy mã hồ sơ trên form!"); return; }

        const textKey = (($(selectors.textKey).val() || "").trim());

        // Spinner
        const $btn = $(selectors.reloadBtn);
        const oldHtml = $btn.html();
        if ($btn.length) $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Đang reload');

        try {
            let usedFSAPI = false;
            let filesToUpload = [];

            if (window.showDirectoryPicker) {
                const dirHandle = await chooseOrRestoreXQDirectory();
                if (dirHandle) {
                    if (textKey) {
                        filesToUpload = await findXQFilesInNestedDirectory(dirHandle, textKey);
                    }
                    if (!filesToUpload.length) {
                        filesToUpload = await findXQFilesInDirectory(dirHandle);
                    }
                    usedFSAPI = true;
                }
            }

            if (!filesToUpload.length && !usedFSAPI) {
                // Không có FS API → chọn file thủ công
                await fallbackSelectAndUploadFile(resultCDHAId, patientCode);
            } else if (!filesToUpload.length && usedFSAPI) {
                const tp = getTodayPrefix();
                const suffix = textKey ? `\nĐã thử tìm trong subfolder theo "${textKey}".` : "";
                SwalHelper.Toast.info(`Không tìm thấy file hình ảnh phù hợp trong thư mục đã lưu${textKey ? ` (đã thử subfolder theo "${textKey}")` : ""}.`);
            } else {
                // Kiểm tra số lượng hình sẽ upload
                if (filesToUpload.length > remainingSlots) {
                    SwalHelper.Toast.warning(`Tìm thấy ${filesToUpload.length} hình, nhưng chỉ có thể upload thêm ${remainingSlots} hình. Sẽ upload ${remainingSlots} hình đầu tiên.`);
                    filesToUpload = filesToUpload.slice(0, remainingSlots); // Chỉ lấy số hình còn thiếu
                }
                
                console.log(`Sẽ upload ${filesToUpload.length} hình:`, filesToUpload.map(f => f.name));
                
                // Upload từng ảnh (tuần tự)
                for (const file of filesToUpload) {
                    try {
                        await uploadXQFileToServer(file, resultCDHAId, patientCode);
                    } catch (err) {
                        console.warn('Upload lỗi với file:', file?.name, err);
                        // tiếp tục các file khác
                    }
                }

                // Gọi sync trên server
                await $.ajax({
                    url: "/XQ_Process/ScanFolderAndSyncImages",
                    type: "POST",
                    data: { resultCDHAId, patientCode },
                });

                // Refresh hình ảnh dịch vụ
                if (typeof Process_Load_ResultAndImage_ForService === "function") {
                    Process_CheckedBoxOnRow(resultCDHAId);
                }
            }
        } catch (e) {
            console.error(e);
            SwalHelper.Toast.error("Có lỗi khi reload từ thư mục PC. Vui lòng thử lại.");
        } finally {
            if ($btn.length) $btn.prop("disabled", false).html(oldHtml);
        }
    }

    /* ========== Hiển thị tên thư mục đang lưu ========== */
    let folderLabelSelector = null;

    async function getSavedDirHandleOrNull() {
        try {
            const h = await XQ_IDB.get(XQ_DIR_KEY);
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
            $el.text("Chưa chọn thư mục").addClass("xq-folder-badge--empty");
            return;
        }
        // FS API không cho lấy full path → chỉ tên thư mục
        $el.text(`Thư mục: ${handle.name}`).removeClass("xq-folder-badge--empty");
    }

    async function MountFolderLabel(selector) {
        folderLabelSelector = selector;
        const handle = await getSavedDirHandleOrNull();
        renderFolderNameToLabel(handle);
    }

    // === THÊM VÀO BÊN TRONG IIFE XQFS ===

    // Tự động reload khi quét barcode vào ô patientCode
    function EnableBarcodeAutoReload(opts = {}) {
        const inputSel = selectors.textKey;   // vd: '#xq_text_key'
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
    return { Reload, ChangeFolder, SetSelectors, MountFolderLabel, EnableBarcodeAutoReload };
})();

/* Attach global */
window.XQFS = XQFS;
// =======================
// Hotkey: F5 cho nút #xq_process_reload
// =======================
$(document).on('keydown', function (e) {
    // Nếu bấm F5
    if (e.key === 'F5') {
        e.preventDefault(); // chặn reload trang mặc định
        var $btn = $('#xq_process_reload');
        if ($btn.length) {
            $btn.trigger('click');
            // hoặc gọi trực tiếp
            // XQFS.Reload();
        }
    }
});