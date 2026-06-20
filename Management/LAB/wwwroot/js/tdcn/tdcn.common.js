// ============================================================================
// TDCN Common JS
// Tách từ tdcn.js hiện tại.
// Chứa: UI common, mobile patient list, zoom ảnh, Select2 helpers, helper dùng chung.
// Load trước các file: tdcn.getsample.js, tdcn.process.js, tdcn.returnresult.js.
// ============================================================================

// Tool tip cho control của form
//$(function () {
//    $('[data-toggle="tooltip"]').tooltip()
//})

//$(window).load(function () {
//    $('#addDeviceForm').modal('show');
//});


// Xử lý toggle danh sách bệnh nhân trên mobile
$(document).ready(function () {
    // Tạo các elements cho mobile nếu màn hình nhỏ
    function initMobilePatientList() {
        if (window.innerWidth <= 768) {
            // Tạo toggle button nếu chưa có
            if ($('.mobile-toggle-patient-list').length === 0) {
                var toggleBtn = $('<button class="mobile-toggle-patient-list"><i class="bi bi-list"></i></button>');
                $('body').append(toggleBtn);
            }

            // Tạo overlay nếu chưa có
            if ($('.mobile-patient-list-overlay').length === 0) {
                var overlay = $('<div class="mobile-patient-list-overlay"></div>');
                $('body').append(overlay);
            }

            // Tạo header cho danh sách nếu chưa có
            if ($('.tdcn-left-header').length === 0) {
                var header = $('<div class="tdcn-left-header">' +
                    '<div class="tdcn-left-header-title">Danh sách bệnh nhân</div>' +
                    '<button class="tdcn-left-header-close"><i class="bi bi-x-lg"></i></button>' +
                    '</div>');
                $('.tdcn-left').prepend(header);
            }
        } else {
            // Xóa các elements mobile trên desktop
            $('.mobile-toggle-patient-list').remove();
            $('.mobile-patient-list-overlay').remove();
            $('.tdcn-left-header').remove();
            $('.tdcn-left').removeClass('show');
        }
    }

    // Khởi tạo khi load trang
    initMobilePatientList();

    // Click vào nút toggle
    $(document).on('click', '.mobile-toggle-patient-list', function () {
        $('.tdcn-left').addClass('show');
        $('.mobile-patient-list-overlay').addClass('show');
        $(this).addClass('active');
        // Prevent scroll trên body
        $('body').css('overflow', 'hidden');
    });

    // Click vào nút đóng trong header
    $(document).on('click', '.tdcn-left-header-close', function () {
        closePatientList();
    });

    // Click vào overlay để đóng
    $(document).on('click', '.mobile-patient-list-overlay', function () {
        closePatientList();
    });

    // Hàm đóng danh sách
    function closePatientList() {
        $('.tdcn-left').removeClass('show');
        $('.mobile-patient-list-overlay').removeClass('show');
        $('.mobile-toggle-patient-list').removeClass('active');
        // Cho phép scroll lại
        $('body').css('overflow', '');
    }

    // Khi chọn bệnh nhân, tự động đóng danh sách trên mobile
    $(document).on('click', '.list-group-item-action', function () {
        if (window.innerWidth <= 768) {
            setTimeout(function () {
                closePatientList();
            }, 300); // Delay nhẹ để user thấy được selection
        }
    });

    // Xử lý resize window
    var resizeTimer;
    $(window).on('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            initMobilePatientList();
        }, 250);
    });

    // Xử lý swipe để đóng danh sách (optional - nâng cao)
    var startX = 0;
    var currentX = 0;
    var isDragging = false;

    $(document).on('touchstart', '.tdcn-left', function (e) {
        if (window.innerWidth <= 768) {
            startX = e.touches[0].clientX;
            isDragging = true;
        }
    });

    $(document).on('touchmove', '.tdcn-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            currentX = e.touches[0].clientX;
            var diff = currentX - startX;

            // Chỉ cho phép swipe sang trái
            if (diff < 0) {
                $(this).css('transform', 'translateX(' + diff + 'px)');
            }
        }
    });

    $(document).on('touchend', '.tdcn-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            var diff = currentX - startX;

            // Nếu swipe quá 100px thì đóng
            if (diff < -100) {
                closePatientList();
            }

            // Reset transform
            $(this).css('transform', '');
            isDragging = false;
        }
    });
});

// ************************************************** Zoom ảnh *********************************
$(document).ready(function () {
    let currentZoom = 1;
    let isDragging = false;
    let startX, startY, startScrollLeft, startScrollTop;

    // Sử dụng event delegation để bind events cho elements có thể được tạo động
    $(document).on('click', '#zoomIn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        console.log("➕ Zoom in button clicked!");
        zoomImage(1.2);
    });

    $(document).on('click', '#zoomOut', function (e) {
        e.preventDefault();
        e.stopPropagation();
        console.log("➖ Zoom out button clicked!");
        zoomImage(0.8);
    });

    $(document).on('click', '#resetZoom', function (e) {
        e.preventDefault();
        e.stopPropagation();
        console.log("🔄 Reset zoom button clicked!");
        resetZoom();
    });

    // Event khi modal mở
    $(document).on('show.bs.modal', '#imageModal', function () {
        console.log("📸 Image modal is opening...");
        setTimeout(function () {
            resetZoom();
            console.log("📸 Modal opened and zoom reset");
        }, 100);
    });

    // Mouse wheel zoom
    $(document).on('wheel', '#imageModal .modal-body', function (e) {
        e.preventDefault();
        const delta = e.originalEvent.deltaY;
        if (delta < 0) {
            console.log("🖱️ Mouse wheel zoom in");
            zoomImage(1.1);
        } else {
            console.log("🖱️ Mouse wheel zoom out");
            zoomImage(0.9);
        }
    });

    // Double click reset
    $(document).on('dblclick', '#modalImage', function () {
        console.log("👆👆 Double click reset zoom");
        resetZoom();
    });

    // Keyboard shortcuts
    $(document).on('keydown', function (e) {
        if ($('#imageModal').hasClass('show')) {
            switch (e.key) {
                case '+':
                case '=':
                    e.preventDefault();
                    console.log("⌨️ Keyboard zoom in (+)");
                    zoomImage(1.2);
                    break;
                case '-':
                    e.preventDefault();
                    console.log("⌨️ Keyboard zoom out (-)");
                    zoomImage(0.8);
                    break;
                case '0':
                    e.preventDefault();
                    console.log("⌨️ Keyboard reset zoom (0)");
                    resetZoom();
                    break;
            }
        }
    });

    // Drag functionality - FIXED VERSION
    $(document).on('mousedown', '#modalImage', function (e) {
        if (currentZoom > 1) {
            isDragging = true;
            $(this).addClass('dragging');

            const $modalBody = $('#imageModal .modal-body');

            // Lưu vị trí chuột ban đầu (tương đối với viewport)
            startX = e.clientX;
            startY = e.clientY;

            // Lưu scroll position ban đầu của modal body
            startScrollLeft = $modalBody.scrollLeft();
            startScrollTop = $modalBody.scrollTop();

            e.preventDefault();
            console.log("👆 Start dragging image - startX:", startX, "startY:", startY);
            console.log("Initial scroll - Left:", startScrollLeft, "Top:", startScrollTop);
        }
    });

    $(document).on('mousemove', function (e) {
        if (!isDragging) return;
        e.preventDefault();

        const $modalBody = $('#imageModal .modal-body');

        // Tính toán khoảng cách di chuyển từ vị trí ban đầu
        const deltaX = e.clientX - startX;
        const deltaY = e.clientY - startY;

        // Áp dụng scroll ngược lại với hướng di chuyển chuột (sensitivity = 1)
        const newScrollLeft = startScrollLeft - deltaX;
        const newScrollTop = startScrollTop - deltaY;

        $modalBody.scrollLeft(newScrollLeft);
        $modalBody.scrollTop(newScrollTop);

        // Debug log (có thể bỏ sau khi test xong)
        // console.log("Moving - deltaX:", deltaX, "deltaY:", deltaY, "newScrollLeft:", newScrollLeft, "newScrollTop:", newScrollTop);
    });

    $(document).on('mouseup', function () {
        if (isDragging) {
            console.log("👆 Stop dragging image");
            isDragging = false;
            $('#modalImage').removeClass('dragging');
        }
    });

    // Cũng dừng drag khi chuột rời khỏi window
    $(document).on('mouseleave', function () {
        if (isDragging) {
            console.log("👆 Stop dragging image (mouse leave)");
            isDragging = false;
            $('#modalImage').removeClass('dragging');
        }
    });

    function zoomImage(factor) {
        console.log(`🔍 Zooming with factor: ${factor}, current: ${currentZoom}`);

        const $modalBody = $('#imageModal .modal-body');
        const $modalImage = $('#modalImage');

        if ($modalImage.length === 0) {
            console.log("❌ Modal image not found!");
            return;
        }

        // Lưu scroll position trước khi zoom
        const scrollLeft = $modalBody.scrollLeft();
        const scrollTop = $modalBody.scrollTop();
        const bodyWidth = $modalBody.width();
        const bodyHeight = $modalBody.height();

        // Tính toán tâm hiện tại
        const centerX = scrollLeft + bodyWidth / 2;
        const centerY = scrollTop + bodyHeight / 2;

        // Cập nhật zoom level
        const oldZoom = currentZoom;
        currentZoom *= factor;
        currentZoom = Math.max(0.1, Math.min(currentZoom, 5));

        // Áp dụng transform
        $modalImage.css('transform', `scale(${currentZoom})`);

        // Tính toán scroll position mới để giữ tâm cố định
        if (currentZoom > 1) {
            const zoomRatio = currentZoom / oldZoom;
            const newScrollLeft = centerX * zoomRatio - bodyWidth / 2;
            const newScrollTop = centerY * zoomRatio - bodyHeight / 2;

            setTimeout(() => {
                $modalBody.scrollLeft(newScrollLeft);
                $modalBody.scrollTop(newScrollTop);
            }, 10);
        }

        updateZoomLevel();

        // Enable/disable buttons
        $('#zoomOut').prop('disabled', currentZoom <= 0.1);
        $('#zoomIn').prop('disabled', currentZoom >= 5);

        console.log(`🔍 Zoom applied: ${Math.round(currentZoom * 100)}%`);
    }

    function resetZoom() {
        console.log("🔄 Resetting zoom to 100%");

        currentZoom = 1;
        const $modalImage = $('#modalImage');
        const $modalBody = $('#imageModal .modal-body');

        if ($modalImage.length > 0) {
            $modalImage.css('transform', 'scale(1)');
        }

        if ($modalBody.length > 0) {
            $modalBody.scrollLeft(0).scrollTop(0);
        }

        updateZoomLevel();
        $('#zoomOut, #zoomIn').prop('disabled', false);
    }

    function updateZoomLevel() {
        const percentage = Math.round(currentZoom * 100) + '%';
        const $zoomLevel = $('#zoomLevel');
        if ($zoomLevel.length > 0) {
            $zoomLevel.text(percentage);
            console.log(`📊 Zoom level updated: ${percentage}`);
        }
    }
});
// ************************************************** Hết Zoom ảnh *********************************
function GetSample_SetSelect2_02() {
    $(document).ready(function () {
        $('#tdcn_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#tdcn_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#tdcn_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#tdcn_getsample_location').select2({
            placeholder: "-- Chọn --"
        });
    });   

    if (isLoadPage) {
        $('#tdcn_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#tdcn_getsample_doctor').select2({
            placeholder: "-- Chọn --"
        });
    });


    if (isLoadPage) {
        $('#tdcn_process_sampleresult').val("");
    }
    $(document).ready(function () {
        $('#tdcn_process_sampleresult').select2({
            placeholder: "-- Chọn --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#tdcn_process_userReturnResultTDCN').select2({
            placeholder: "-- Chọn --"
        });
    });
}






// ============================== COMMON HELPERS ==============================
// Các helper này được đưa vào common để Process/ReturnResult đều dùng được nếu sau này load riêng từng màn.
// Một số helper vẫn còn tồn tại trong file tách theo section để giữ nguyên hành vi tdcn.js cũ.

function escapeHtml(s) {
    if (!s) return '';
    return s.replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m]));
}

function base64ToFile(base64, filename, mime) {
    try {
        var idx = base64.indexOf("base64,");
        var pure = (idx >= 0) ? base64.substring(idx + 7) : base64;

        var byteCharacters = atob(pure);
        var byteNumbers = new Array(byteCharacters.length);
        for (var i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        var byteArray = new Uint8Array(byteNumbers);
        var blob = new Blob([byteArray], { type: mime || 'application/pdf' });
        return new File([blob], filename || "document.pdf", { type: mime || 'application/pdf' });
    } catch (e) {
        SwalHelper.Toast.error("Lỗi chuyển đổi base64 -> file: " + e);
        throw e;
    }
}

function openBase64Pdf(base64) {
    var idx = base64.indexOf("base64,");
    var pure = (idx >= 0) ? base64.substring(idx + 7) : base64;

    var byteCharacters = atob(pure);
    var byteNumbers = new Array(byteCharacters.length);
    for (var i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    var byteArray = new Uint8Array(byteNumbers);
    var file = new Blob([byteArray], { type: 'application/pdf' });
    var fileURL = URL.createObjectURL(file);
    window.open(fileURL);
}

function openSigned(id) {
    if (!id) {
        SwalHelper.Toast.warning("Chưa có signStoreId cho dịch vụ này!");
        return;
    }
    var url = "/api/ExternalSign/view-signed/" + encodeURIComponent(id);
    window.open(url, "_blank");
}

function isProbablyBase64(s) {
    if (!s || typeof s !== "string") return false;
    var re = /^[A-Za-z0-9+/=\s]+$/;
    return re.test(s) && s.length > 1000;
}

function startCountdown(durationInSeconds) {
    let remainingTime = durationInSeconds;

    const interval = setInterval(() => {
        let minutes = Math.floor(remainingTime / 60);
        let seconds = remainingTime % 60;
        updateLoadingText(`Đang chờ ký số (${minutes} phút ${seconds < 10 ? '0' : ''}${seconds} giây)...`);
        remainingTime--;

        if (remainingTime < 0) {
            clearInterval(interval);
            updateLoadingText("Ký số hoàn tất!");
        }
    }, 1000);
}
