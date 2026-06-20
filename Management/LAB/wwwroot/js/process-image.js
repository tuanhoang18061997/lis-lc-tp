// process-image.js
(function (w) {
    // Nếu đã init rồi thì thoát (tránh chạy lại khi partial reload)
    if (w.ProcessImage && w.ProcessImage.__initialized) return;

    const MAX_FILES = 2;              // giờ nằm trong scope riêng, không đụng global
    let selectedFiles = [];           // trạng thái nội bộ

    function isImage(file) {
        return file && file.type && file.type.startsWith("image/");
    }

    function syncInputFiles() {
        const input = document.getElementById("process_images");
        if (!input) return;
        const dt = new DataTransfer();
        selectedFiles.forEach(f => dt.items.add(f));
        input.files = dt.files;
    }

    function renderPreview() {
        const preview = document.getElementById("preview_images");
        if (!preview) return;
        preview.innerHTML = "";

        selectedFiles.forEach((file, idx) => {
            const reader = new FileReader();
            reader.onload = (e) => {
                const wrapper = document.createElement("div");
                wrapper.classList.add("position-relative", "d-inline-block", "m-1");

                const img = document.createElement("img");
                img.src = e.target.result;
                img.style.maxWidth = "120px";
                img.style.maxHeight = "120px";
                img.classList.add("img-thumbnail");

                const btn = document.createElement("button");
                btn.type = "button";
                btn.innerHTML = "×";
                btn.setAttribute("aria-label", "Xoá ảnh");
                btn.classList.add("btn", "btn-sm", "btn-danger", "position-absolute");
                btn.style.top = "0";
                btn.style.right = "0";

                btn.addEventListener("click", () => {
                    selectedFiles.splice(idx, 1);
                    syncInputFiles();
                    renderPreview();
                });

                wrapper.appendChild(img);
                wrapper.appendChild(btn);
                preview.appendChild(wrapper);
            };
            reader.readAsDataURL(file);
        });
    }

    function addFiles(newFileList) {
        for (const file of newFileList) {
            if (!isImage(file)) continue;
            if (selectedFiles.length >= MAX_FILES) {
                alert(`Bạn chỉ được chọn tối đa ${MAX_FILES} hình.`);
                break;
            }
            const exists = selectedFiles.some(f =>
                f.name === file.name && f.size === file.size && f.lastModified === file.lastModified
            );
            if (!exists) selectedFiles.push(file);
        }
        syncInputFiles();
        renderPreview();
    }

    function showChooseImageComponent() {
        const comp = document.getElementById("choose_image_component");
        if (comp && comp.style.display === "none") comp.style.display = "block";
    }

    function bindOnce() {
        // Gắn change handler nếu chưa có
        const input = document.getElementById("process_images");
        if (input && !input.__bindedProcessImage) {
            input.addEventListener("change", function (event) {
                addFiles(event.target.files);
                event.target.value = ""; // cho phép chọn lại cùng file tên cũ
            });
            input.__bindedProcessImage = true;
        }

        // Khởi tạo CKEditor nếu có textarea & CKEDITOR
        if (typeof CKEDITOR !== "undefined") {
            const desc = document.getElementById("xq_process_description");
            if (desc && !desc.__ckeditorInited) {
                CKEDITOR.replace("xq_process_description");
                desc.__ckeditorInited = true;
            }
        }

        // Patch Process_CheckedBoxOnRow chỉ 1 lần
        if (!w.__Process_CheckedBoxOnRow_patched) {
            const originalFn = w.Process_CheckedBoxOnRow;
            w.Process_CheckedBoxOnRow = function () {
                if (typeof originalFn === "function") originalFn.apply(this, arguments);
                showChooseImageComponent();
            };
            w.__Process_CheckedBoxOnRow_patched = true;
        }
    }

    // --- Helpers Promise ---
    function readAsDataURL(file) {
        return new Promise((resolve, reject) => {
            const fr = new FileReader();
            fr.onload = e => resolve(e.target.result);
            fr.onerror = reject;
            fr.readAsDataURL(file);
        });
    }

    function postJson(url, payload) {
        return $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'text',
            data: JSON.stringify(payload)
        });
    }

    // --- Public API: upload ảnh (tối đa 2 đã đảm bảo sẵn) ---
    async function uploadImages(resultCDHAId) {
        if (!resultCDHAId) throw new Error("resultCDHAId is required");
        if (!selectedFiles || selectedFiles.length === 0) return;

        for (const file of selectedFiles) {
            const image_data_url = await readAsDataURL(file);
            // chỉ lấy phần base64 sau dấu phẩy
            const base64 = image_data_url.split(',')[1] || image_data_url;

            const data = { imageString: base64, resultCDHAId: String(resultCDHAId) };

            const resp = await $.ajax({
                url: '/XQ_Process/SaveImageCDHA',   // đúng route controller của bạn
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                dataType: 'text',
                data: JSON.stringify(data)
            });
            if (resp !== 'True') throw new Error('Upload image failed');
        }
    }

    // --- xóa ảnh đang chọn & UI preview ---
    function clearSelection() {
        selectedFiles = [];
        const input = document.getElementById("process_images");
        if (input) {
            input.value = "";                     // clear hiển thị chọn file
            const dt = new DataTransfer();        // clear input.files
            input.files = dt.files;
        }
        const preview = document.getElementById("preview_images");
        if (preview) preview.innerHTML = "";
    }

    let currentServiceId = null;
    function setService(id) {
        const sid = String(id);
        if (currentServiceId !== sid) {
            clearSelection();                     // đổi dịch vụ -> xóa ảnh cũ
            currentServiceId = sid;
        }
        showChooseImageComponent();             // đảm bảo component hiện
    }

    // Public API (bổ sung)
    w.ProcessImage = {
        __initialized: true,
        reinit: bindOnce,
        show: showChooseImageComponent,
        uploadImages // <-- expose để nơi khác gọi
    };

    // Public API (nếu cần gọi lại sau khi AJAX render lại HTML)
    w.ProcessImage = {
        __initialized: true,
        reinit: bindOnce,     // gọi lại sau mỗi lần partial reload
        show: showChooseImageComponent,
        uploadImages, // <-- expose để nơi khác gọi
        clear: clearSelection,
        setService: setService
    };

    // Init lần đầu
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", bindOnce);
    } else {
        bindOnce();
    }
})(window);
