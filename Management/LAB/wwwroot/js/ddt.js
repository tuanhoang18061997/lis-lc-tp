// Tool tip cho control của form
//$(function () {
//    $('[data-toggle="tooltip"]').tooltip()
//})

//$(window).load(function () {
//    $('#addDeviceForm').modal('show');
//});

// *********************************************************************************** Select 2
$(function () {
    GetSample_SetSelect2_01(true);
})
$(function () {
    if (typeof window.BarcodeScan === 'undefined') return;

    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#ddt_getsample_pidorseq', window.GetSample_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#ddt_process_pidorseq', window.Process_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#ddt_returnresult_pidorseq', window.ReturnResult_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Tự động gắn cho các input có class .barcode-input (dùng chung nhiều màn hình)
    window.BarcodeScan.autowire('.barcode-input');
});
// ******************************* Gắn hotkey Enter cho tìm kiếm*********************************
$(document).ready(function () {
    // Gắn Enter key cho các input field tìm kiếm của XN_Process
    $('#ddt_process_pidorseq, #ddt_process_timeSearchFrom, #ddt_process_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            Process_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của GetSample
    $('#ddt_getsample_pidorseq, #ddt_getsample_timeSearchFrom, #ddt_getsample_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            GetSample_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của ReturnResult
    $('#ddt_returnresult_pidorseq, #ddt_returnresult_timeSearchFrom, #ddt_returnresult_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            ReturnResult_Search();
        }
    });
});
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
            if ($('.ddt-left-header').length === 0) {
                var header = $('<div class="ddt-left-header">' +
                    '<div class="ddt-left-header-title">Danh sách bệnh nhân</div>' +
                    '<button class="ddt-left-header-close"><i class="bi bi-x-lg"></i></button>' +
                    '</div>');
                $('.ddt-left').prepend(header);
            }
        } else {
            // Xóa các elements mobile trên desktop
            $('.mobile-toggle-patient-list').remove();
            $('.mobile-patient-list-overlay').remove();
            $('.ddt-left-header').remove();
            $('.ddt-left').removeClass('show');
        }
    }

    // Khởi tạo khi load trang
    initMobilePatientList();

    // Click vào nút toggle
    $(document).on('click', '.mobile-toggle-patient-list', function () {
        $('.ddt-left').addClass('show');
        $('.mobile-patient-list-overlay').addClass('show');
        $(this).addClass('active');
        // Prevent scroll trên body
        $('body').css('overflow', 'hidden');
    });

    // Click vào nút đóng trong header
    $(document).on('click', '.ddt-left-header-close', function () {
        closePatientList();
    });

    // Click vào overlay để đóng
    $(document).on('click', '.mobile-patient-list-overlay', function () {
        closePatientList();
    });

    // Hàm đóng danh sách
    function closePatientList() {
        $('.ddt-left').removeClass('show');
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

    $(document).on('touchstart', '.ddt-left', function (e) {
        if (window.innerWidth <= 768) {
            startX = e.touches[0].clientX;
            isDragging = true;
        }
    });

    $(document).on('touchmove', '.ddt-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            currentX = e.touches[0].clientX;
            var diff = currentX - startX;

            // Chỉ cho phép swipe sang trái
            if (diff < 0) {
                $(this).css('transform', 'translateX(' + diff + 'px)');
            }
        }
    });

    $(document).on('touchend', '.ddt-left', function (e) {
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
    console.log("🔍 Image zoom functionality loading...");

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

    // Debug: Test button existence when page loads
    setTimeout(function () {
        console.log("🔍 Checking zoom buttons after page load:");
        console.log("- #zoomIn:", $('#zoomIn').length);
        console.log("- #zoomOut:", $('#zoomOut').length);
        console.log("- #resetZoom:", $('#resetZoom').length);
        console.log("- #zoomLevel:", $('#zoomLevel').length);
        console.log("- #modalImage:", $('#modalImage').length);
    }, 2000);
});
// ************************************************** Hết Zoom ảnh *********************************
function GetSample_SetSelect2_02() {
    $(document).ready(function () {
        $('#ddt_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#ddt_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#ddt_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#ddt_getsample_location').select2({
            placeholder: "-- Chọn --"
        });
    });

    if (isLoadPage) {
        $('#ddt_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#ddt_getsample_doctor').select2({
            placeholder: "-- Chọn --"
        });
    });


    if (isLoadPage) {
        $('#ddt_process_sampleresult').val("");
    }
    $(document).ready(function () {
        $('#ddt_process_sampleresult').select2({
            placeholder: "-- Chọn --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#ddt_process_userReturnResultDDT').select2({
            placeholder: "-- Chọn --"
        });
    });
}




// *********************************************************************************** Get sample

// Kiểm tra nhập liệu trên form => Trường nào có class = valid thì sẽ kiểm tra rỗng và bắt nhập
function GetSample_ValidateInput(id) {
    var flag = true;
    $("#" + id).find(".valid").each(function (i, e) {
        var value = $(e).val();
        if (value === "" || !value) {
            if (flag) {
                if (e.id === "ddt_getsample_location") {
                    $('#ddt_getsample_location').select2('focus');
                }
                else if (e.id === "ddt_getsample_doctor") {
                    $('#ddt_getsample_doctor').select2('focus');
                }
                else if (e.id === "ddt_getsample_userReturnResultDDT") {
                    $('#ddt_getsample_userReturnResultDDT').select2('focus');
                }
                else {
                    $(e).focus();
                }
            }
            flag = false;
        }
    })
    return flag;
}

function GetSample_Refresh() {
    $.ajax({
        url: "/DDT_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#ddt_getsample_timeSearchFrom").val(today);
            //$("#ddt_getsample_timeSearchTo").val(today);
            $("#listPatient").html(result);

            GetSample_ResetInput();
            GetSample_Get_Count();
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Refresh_No_Clear_PatientInfo() {
    $.ajax({
        url: "/DDT_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#ddt_getsample_timeSearchFrom").val(today);
            $("#ddt_getsample_timeSearchTo").val(today);
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Search() {
    var ddt_getsample_pidorseq = $("#ddt_getsample_pidorseq").val();
    var timeSearchFrom = $("#ddt_getsample_timeSearchFrom").val();
    var timeSearchTo = $("#ddt_getsample_timeSearchTo").val();
    $.ajax({
        url: "/DDT_GetSample/Search?" + "pidorseq=" + ddt_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#listPatient").html(result);
            GetSample_Get_Count();
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

// Hiden SHow button
$("#ddt_getsample_savepatient").hide();
$("#ddt_getsample_cancelpatient").hide();
function GetSample_HideButton(_new, _save, _delete, _cancel, _addservice, _getsample) {
    if (_new == 1) {
        $("#ddt_getsample_newpatient").hide();
    }
    else {
        $("#ddt_getsample_newpatient").show();
    }

    if (_save == 1) {
        $("#ddt_getsample_savepatient").hide();
    }
    else {
        $("#ddt_getsample_savepatient").show();
    }

    if (_delete == 1) {
        $("#ddt_getsample_deletepatient").hide();
    }
    else {
        $("#ddt_getsample_deletepatient").show();
    }

    if (_cancel == 1) {
        $("#ddt_getsample_cancelpatient").hide();
    }
    else {
        $("#ddt_getsample_cancelpatient").show();
    }

    if (_addservice == 1) {
        $("#ddt_getsample_addService").hide();
    }
    else {
        $("#ddt_getsample_addService").show();
    }

    if (_getsample == 1) {
        $("#ddt_getsample_processresult").hide();
    }
    else {
        $("#ddt_getsample_processresult").show();
    }
}


function GetSample_GetPatientInfo(id) {
    GetSample_HideButton(false, true, false, true, false, false);
    $.ajax({
        url: "/DDT_GetSample/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#patientInfo").html(result);
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            GetSample_SetSelect2_01(false);
            GetSample_GetListServiceForPatient(id);
        },
        error: function () {
            $("#patientInfo").empty();
            $('#tbody-gridview-service').empty();
        }
    });
}


function GetSample_GetListServiceForPatient(id) {
    $.ajax({
        url: "/DDT_GetSample/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#ddt-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function GetSample_ResetInput() {
    var date = new Date();
    var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
    var time = ("0" + date.getHours()).slice(-2) + ":" + ("0" + date.getMinutes()).slice(-2);
    var dateTime = today + " " + time;
    $("#ddt_getsample_pidorseq").val('');
    $('#ddt_getsample_id').val('');
    $('#ddt_getsample_patientId').val('');
    $('#ddt_getsample_seq').val('');
    $('#ddt_getsample_sid').val('');
    $('#ddt_getsample_patientName').val('');
    $('#ddt_getsample_age').val('');
    $('#ddt_getsample_sex').val('');
    $('#ddt_getsample_obj').val('');
    $('#ddt_getsample_type').val('');
    $('#ddt_getsample_location').val('');
    $('#ddt_getsample_doctor').val('');
    $('#ddt_getsample_getSampleTime').val(dateTime);
    $('#ddt_getsample_address').val('');
    $('#ddt_getsample_diagnostic').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
    GetSample_SetSelect2_01(false);
}

function GetSample_NewPatient() {
    $('#ddt_getsample_patientId').focus();
    GetSample_HideButton(true, false, true, false, true, true);
    GetSample_ResetInput();
}


function GetSample_DeletePatient() {
    var id = $('#ddt_getsample_id').val();
    if (id == '') {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var choice = confirm("Bạn muốn xoá bệnh nhân và tất cả chỉ định xét nghiệm?");
        if (choice) {
            $.ajax({
                url: "/DDT_GetSample/DeletePatientAndService?id=" + id,
                type: "POST",
                dataType: "text",
                cache: false,
                success: function (result) {
                    if (result === '') {
                        SwalHelper.Toast.error("Xoá không thành công. Vui lòng kiểm tra lại!");
                    }
                    else {
                        GetSample_HideButton(false, true, false, true, false, false)
                        GetSample_Refresh(result);
                        GetSample_Get_Count();
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Xoá không thành công. Vui lòng kiểm tra lại!");
                }
            });
        }
    }
}

function GetSample_CancelPatient() {
    GetSample_HideButton(false, true, false, true, false, false);
    var id = $('#ddt_getsample_id').val();
    if (id == '') {
        GetSample_ResetInput()
    }
    else {
        GetSample_GetPatientInfo(id);
    }
}

function GetSample_GetSID(seq) {
    $.ajax({
        url: "/DDT_GetSample/GetSID?seq=" + seq,
        type: "GET",
        dataType: "text",
        cache: false,
        success: function (result) {
            $('#ddt_getsample_sid').val(result);
        }
    });
}

function GetSample_SavePatient() {
    var validate = GetSample_ValidateInput('patientInfo');
    if (validate) {
        var id = $('#ddt_getsample_id').val();
        var patientId = $('#ddt_getsample_patientId').val();
        var seq = $('#ddt_getsample_seq').val();
        var sid = $('#ddt_getsample_sid').val();
        var patientName = $('#ddt_getsample_patientName').val();
        var age = $('#ddt_getsample_age').val();
        var sex = $('#ddt_getsample_sex').val();
        var obj = $('#ddt_getsample_obj').val();
        var type = $('#ddt_getsample_type').val();
        var location = $('#ddt_getsample_location').val();
        var doctor = $('#ddt_getsample_doctor').val();
        var getSampleTime = $('#ddt_getsample_getSampleTime').val();
        var address = $('#ddt_getsample_address').val();
        var diagnostic = $('#ddt_getsample_diagnostic').val();
        var category = $('#select-category').val();
        var service = $('#select-service').val();

        $.ajax({
            url: "/DDT_GetSample/SavePatient?id= " + id + "&&patientId=" + patientId + "&&seq=" + seq + "&&sid=" + sid + "&&patientName=" + patientName + "&&age=" + age + "&&sex=" + sex + "&&obj=" + obj + "&&type=" + type + "&&location=" + location + "&&doctor=" + doctor + "&&getSampleTime=" + getSampleTime + "&&address=" + address + "&&diagnostic=" + diagnostic + "&&service=" + service,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result == "") {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại!");
                }
                else {
                    GetSample_GetPatientInfo(result);
                    GetSample_Refresh_No_Clear_PatientInfo();
                    GetSample_SetSelect2_01(false);
                    GetSample_Get_Count();
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại!");
            }
        });
    }
}

function GetSample_ProcessResult() {
    var id = $('#ddt_getsample_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var getSampleTime = $('#ddt_getsample_getSampleTime').val();
        $.ajax({
            url: "/DDT_GetSample/ProcessResult?id= " + id + "&getSampleTime=" + getSampleTime,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result === 'True') {
                    GetSample_Refresh();
                    GetSample_Get_Count();
                }
                else {
                    SwalHelper.Toast.error("Xử lý kết quả không thành công. Vui lòng kiểm tra lại!");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Xử lý kết quả không thành công. Vui lòng kiểm tra lại!");
            }
        });
    }
}

function GetSample_AddService() {
    var id = $('#ddt_getsample_id').val();
    if (id === "") {
        $('#addServiceForm').modal('hide');
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        GetSample_SetSelect2_02();
    }
}

function GetSample_DeleteServiceForPatient(idResultCDHA) {
    var patientId = $('#ddt_getsample_id').val();
    $.ajax({
        url: "/DDT_GetSample/DeleteServiceForPatient?id=" + idResultCDHA,
        type: "POST",
        dataType: "text",
        cache: false,
        success: function (result) {
            if (result == 'True') {
                GetSample_GetListServiceForPatient(patientId);
            }
            else {
                SwalHelper.Toast.error("Không thể xoá. Vui lòng kiểm tra lại !");
            }
        }
    });
}


function GetSample_AddServiceForPatient() {
    var serviceId = $('#ddt_getsample_service').val();
    var patientId = $('#ddt_getsample_id').val();
    var doctorId = $('#ddt_getsample_doctor').val();
    if (serviceId === "" || patientId === "") {
        SwalHelper.Toast.warning("Chỉ định dịch vụ không thành công. Vui lòng kiểm tra lại !")
    }
    else {
        $.ajax({
            url: "/DDT_GetSample/AddServiceForPatient?patientId=" + patientId + "&&serviceId=" + serviceId + "&&doctorId=" + doctorId,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result == 'True') {
                    GetSample_GetListServiceForPatient(patientId);
                }
                else {
                    SwalHelper.Toast.error("Thêm dịch vụ không thành công. Vui lòng kiểm tra lại!");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Thêm dịch vụ không thành công. Vui lòng kiểm tra lại!");
            }
        });

        GetSample_SetSelect2_02();
    }
}

function GetSample_Get_Count() {
    $.ajax({
        url: "/DDT_GetSample/Get_Count/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            var arrayResult = result.split(';');
            document.getElementById("countGetSample").innerHTML = arrayResult[0];
            document.getElementById("countProcess").innerHTML = arrayResult[1];
            document.getElementById("countReturnResult").innerHTML = arrayResult[2];
        }
    });
}

function GetSample_SaveDateToSession() {
    var fromDate = document.getElementById('ddt_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('ddt_getsample_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/DDT_GetSample/SaveSearchDates',
            type: 'POST',
            data: {
                timeSearchFrom: fromDate,
                timeSearchTo: toDate
            },
            success: function (result) {
                // Optionally handle success
            }
        });
    }
}

// ============================== IMPORT kết quả NGOÀI (PDF) ==============================

// Mở modal + đổ patientId hiện chọn
function GetSample_ShowImportModal() {
    var pid = $('#ddt_getsample_patientId').val();
    var pidTablePatient = $('#ddt_getsample_id').val();
    var pName = $('#ddt_getsample_patientName').val();
    var pMaBenhAn = $('#ddt_getsample_maBenhAn').val();

    $('#import_patientId').val(pid);
    $('#import_idTablePatient').val(pidTablePatient);
    $('#import_patientName').val(pName);
    $('#import_maBenhAn').val(pMaBenhAn);

    // clear UI
    $('#import_pdf_file').val('');
    $('#pdfPreview').html('<em class="text-muted">Chưa có tệp được chọn…</em>');

    if (pidTablePatient === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }
    // mở modal (nếu không dùng data-toggle)
    // tải danh sách PDF đã import
    GetSample_LoadImportedPdfList(pMaBenhAn, pid);
    try { $('#modal-import-external').modal('show'); } catch (e) { }
}

// Xem trước PDF ngay khi chọn file
function GetSample_HandlePdfPreview(input) {
    const container = document.getElementById('pdfPreview');
    if (!input || !input.files || input.files.length === 0) {
        container.innerHTML = '<em class="text-muted">Chưa có tệp được chọn…</em>';
        return;
    }
    const file = input.files[0];
    if (file.type !== 'application/pdf') {
        SwalHelper.Toast.warning('File không phải PDF!');
        input.value = '';
        container.innerHTML = '<em class="text-muted">Chưa có tệp được chọn…</em>';
        return;
    }
    const url = URL.createObjectURL(file);
    // Dùng <embed> để preview đơn giản, tương thích tốt
    container.innerHTML = '';
    const emb = document.createElement('embed');
    emb.src = url;
    emb.type = 'application/pdf';
    emb.style.width = '100%';
    emb.style.height = '70vh';
    container.appendChild(emb);
}

function GetSample_LoadImportedPdfList(pMaBenhAn, patientId) {
    var pId = $('#import_patientId').val();
    $('#imported_pdf_list').html('<div class="text-muted p-2">Đang tải…</div>');
    $.get('/DDT_GetSample/GetExternalResultFiles', { pMaBenhAn: pMaBenhAn, pId: pId, patientId: patientId }, function (res) {
        if (!res || res.success !== true) {
            $('#imported_pdf_list').html('<div class="text-danger p-2">Không tải được danh sách.</div>');
            return;
        }
        if (!res.merged || res.merged.length === 0) {
            $('#imported_pdf_list').html('<div class="text-muted p-2">Chưa có tệp nào.</div>');
            return;
        }
        var html = '<ul class="list-group list-group-flush">';
        res.merged.forEach(function (f) {
            var created = f.created ? new Date(f.created).toLocaleString() : '';
            html += `
            <li class="list-group-item py-2">
              <div class="d-flex justify-content-between align-items-center">
                <a href="${f.url}" target="_blank" class="font-weight-bold" title="Mở trong tab mới">${escapeHtml(f.name)}</a>
                <div style="display: flex; gap: .3rem">
                  <button type="button" class="btn btn-sm btn-primary me-1" onclick="GetSample_PreviewImportedPdf('${f.url.replace(/'/g, "\\'")}')">Preview</button>
                  <button type="button" class="btn btn-sm btn-outline-danger" style="text-wrap-mode: nowrap;" onclick="GetSample_DeleteImportedPdf('${escapeHtml(f.name)}', '${pMaBenhAn}', '${pId}', '${patientId}')" title="Xóa file">
                    <i class="fa fa-trash"></i>
                    Xóa file
                  </button>
                </div>
              </div>
              <div class="small text-muted">${created}${f.vendor ? ' • ' + escapeHtml(f.vendor) : ''}</div>
            </li>`;
        });
        html += '</ul>';
        $('#imported_pdf_list').html(html);
    }).fail(function () {
        $('#imported_pdf_list').html('<div class="text-danger p-2">Lỗi khi tải danh sách.</div>');
    });
}

function GetSample_DeleteImportedPdf(fileName, pMaBenhAn, pId, patientId) {
    if (!confirm(`Bạn có chắc chắn xóa file "${fileName}" không?`)) {
        return;
    }
    $.ajax({
        url: '/DDT_GetSample/DeleteExternalResultFile',
        type: 'POST',
        data: {
            fileName: fileName,
            pMaBenhAn: pMaBenhAn,
            pId: pId
        },
        success: function (res) {
            if (res && res.success) {
                SwalHelper.Toast.success('Xóa file thành công!');
                // Refresh danh sách file
                GetSample_LoadImportedPdfList(pMaBenhAn, patientId);
                // Clear preview n?u file dang du?c xem
                const container = document.getElementById('pdfPreview');
                container.innerHTML = '<em class="text-muted">Chua có tệp được chọn…</em>';
            } else {
                SwalHelper.Toast.error(res && res.message ? res.message : 'Xóa file thất bại!');
            }
        },
        error: function () {
            SwalHelper.Toast.error('Lỗi khi xóa file. Vui lòng thử lại!');
        }
    });
}
function GetSample_PreviewImportedPdf(url) {
    const container = document.getElementById('pdfPreview');
    container.innerHTML = '';
    const emb = document.createElement('embed');
    emb.src = url;
    emb.type = 'application/pdf';
    emb.style.width = '100%';
    emb.style.height = '70vh';
    container.appendChild(emb);
}

function escapeHtml(s) {
    if (!s) return '';
    return s.replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m]));
}

// Gửi kèm vendor vào FormData
function GetSample_SubmitExternalImport() {
    var pid = $('#import_patientId').val();
    var pidTablePatient = $('#import_idTablePatient').val();
    var pName = $('#import_patientName').val();
    var pMaBenhAn = $('#import_maBenhAn').val();
    console.log(pid);
    console.log(pidTablePatient);
    console.log(pName);
    console.log(pMaBenhAn);

    //var pName = $('#import_patientName').val();
    var fileInput = document.getElementById('import_pdf_file');
    if (!pid) { SwalHelper.Toast.warning('Thiếu patientId!'); return; }
    if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
        SwalHelper.Toast.warning('Vui lòng chọn file PDF!');
        return;
    }

    var overwrite = $('#import_overwrite').is(':checked');
    var markValid = $('#import_mark_valid').is(':checked');
    //var vendor = $('#import_vendor').val() || '';

    var fd = new FormData();
    fd.append('pMaBenhAn', pMaBenhAn);
    fd.append('pId', pid);
    fd.append('pidTablePatient', pidTablePatient);
    fd.append('pName', pName);
    fd.append('file', fileInput.files[0]);
    fd.append('overwrite', overwrite);
    fd.append('markValid', markValid);
    //fd.append('vendor', vendor);

    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: "/DDT_GetSample/ImportExternalResultPdf",
        type: "POST",
        data: fd,
        processData: false,
        contentType: false,
        success: function (res) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            if (!res || res.success !== true) {
                SwalHelper.Toast.error(res && res.message ? res.message : "Import thất bại.");
                return;
            }

            // Refresh danh sách file đã import
            GetSample_LoadImportedPdfList(pMaBenhAn, pid);

            // Thông báo thành công với thông tin file
            var message = "Lưu file PDF thành công!";
            if (res.fileName) {
                message += "\nTên file: " + res.fileName;
            }
            SwalHelper.Toast.success(message);

            try {
                $('#modal-import-external').modal('hide');
                $('.modal-backdrop').remove();
            } catch (e) { }
        },
        error: function () {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            SwalHelper.Toast.error("Không thể lưu file PDF. Vui lòng thử lại!");
        }
    });
}
//***************************************************************************************** Process

function Process_ValidateInput(id) {
    var flag = true;
    $("#" + id).find(".valid").each(function (i, e) {
        var value = $(e).val();
        if (value === "" || !value) {
            if (flag) {
                if (e.id === "ddt_process_userReturnResultDDT") {
                    $('#ddt_process_userReturnResultDDT').select2('focus');
                }
                else {
                    $(e).focus();
                }
            }
            flag = false;
        }
    })
    return flag;
}

function Process_Refresh() {
    $.ajax({
        url: "/DDT_Process/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#ddt_process_timeSearchFrom").val(today);
            //$("#ddt_process_timeSearchTo").val(today);

            $("#process_listPatient").html(result);
            Process_ResetInput();
            Process_Get_Count();
        },
        error: function () {
            $("#process_listPatient").empty();
        }
    });
}

function Process_Search() {
    var ddt_process_pidorseq = $("#ddt_process_pidorseq").val();
    var timeSearchFrom = $("#ddt_process_timeSearchFrom").val();
    var timeSearchTo = $("#ddt_process_timeSearchTo").val();
    $.ajax({
        url: "/DDT_Process/Search?" + "pidorseq=" + ddt_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_listPatient").html(result);
            Process_Get_Count();
        },
        error: function () {
            $("#process_listPatient").empty();
        }
    });
}
function Process_ResetInput() {
    $("#ddt_process_pidorseq").val('');
    $('#ddt_process_id').val('');
    $('#ddt_process_patientId').val('');
    $('#ddt_process_seq').val('');
    $('#ddt_process_sid').val('');
    $('#ddt_process_patientName').val('');
    $('#ddt_process_age').val('');
    $('#ddt_process_sex').val('');
    $('#ddt_process_obj').val('');
    $('#ddt_process_type').val('');
    $('#ddt_process_location').val('');
    //$('#ddt_process_doctor').val('');
    $('#ddt_process_getSampleTime').val('');
    $('#ddt_process_returnResultTime').val('');
    $('#ddt_process_location').val('');
    //$('#ddt_process_doctor').val('');
    $('#ddt_process_userReturnResultDDT').val('');
    $('#ddt_process_address').val('');
    $('#ddt_process_diagnostic').val('');
    $('#tbody-gridview-service').empty();
    $('#imageCDHA').empty();
    $('#ddt_process_result').empty();
    $('#ddt_process_suggest').empty();
    $('#ddt_text_key').empty();
    CKEDITOR.instances["ddt_process_description"].setData("");
    Process_SetSelect2_03(true);
}

function Process_GetPatientInfo(id) {
    $.ajax({
        url: "/DDT_Process/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_patientInfo").html(result);
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            Process_SetSelect2_03();
            // Auto-chọn bác sĩ = user đang login (nếu chưa có giá trị)
            var loginId = $("#ddt_process_userLoginId").val();
            console.log(loginId);
            var $sel = $("#ddt_process_userReturnResultDDT");
            if (loginId && (!$sel.val() || $sel.val() === "")) {
                $sel.val(loginId).trigger("change");
            }

            // Gắn handler đổi bác sĩ
            $(document)
                .off("change", "#ddt_process_userReturnResultDDT")
                .on("change", "#ddt_process_userReturnResultDDT", Process_Load_SelectedDoctorInfo);

            // Lần đầu nếu có sẵn value thì cũng load info
            Process_Load_SelectedDoctorInfo();
            Process_GetListServiceForPatient(id);
        },
        error: function () {
            $("#process_patientInfo").empty();
            $('#tbody-gridview-service').empty();
        }
    });
}

function Process_GetListServiceForPatient(id) {
    $.ajax({
        url: "/DDT_Process/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_ddt-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}


function Process_SaveResult() {
    var patientId = $('#ddt_process_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var validate = Process_ValidateInput('process_patientInfo');
        if (validate) {

            // Lấy ServiceId
            var resultCDHAId = "";
            $(".row-service").each(function () {
                $(this).find(".form-check-input").each(function () {
                    var isChecked = $(this).is(':checked');
                    if (isChecked) {
                        resultCDHAId = $(this).val();
                    }
                })
            })

            if (resultCDHAId === "") {
                SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
            }
            else {
                var returnResultTime = $('#ddt_process_returnResultTime').val();
                var userReturnResult = $('#ddt_process_userReturnResultDDT').val();
                var description = CKEDITOR.instances['ddt_process_description'].getData();
                var result = $('#ddt_process_result').val();
                var suggest = $('#ddt_process_suggest').val();

                var data = {
                    patientId: patientId,
                    resultCDHAId: resultCDHAId,
                    returnResultTime: returnResultTime,
                    userReturnResult: userReturnResult,
                    description: description,
                    result: result,
                    suggest: suggest
                };

                $.ajax({
                    url: "/DDT_Process/SaveResult/",
                    data: JSON.stringify(data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    type: "POST",
                    success: function (result) {
                        if (result == 'True') {
                            SwalHelper.Toast.success("Lưu thành công.");
                        }
                        else {
                            SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại!");
                        }
                    },
                    error: function () {
                        SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại!");
                    }
                });
                return true;
            }
        }
        else {
            return false
        }
    }
}

function Process_SelectDevice() {
    var deviceId = $('#ddt_getsample_select_device_select').val();
    if (deviceId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn thiết bị !")
    }
    else {
        $.ajax({
            url: "/DDT_Process/SelectDevice?deviceId=" + deviceId,
            type: 'GET',
            dataType: 'text',
            success: function (result) {
                if (result === 'False') {
                    SwalHelper.Toast.error("Không thể chọn thiết bị. Kiểm tra lại !");
                }
                else {
                    Process_ValidPrint();
                }
            },
            error: function () {
                SwalHelper.Toast.error("Không thể chọn thiết bị. Kiểm tra lại !");
            }
        });
    }
}


function Process_ValidPrint() {
    $.ajax({
        url: "/DDT_Process/Check_SelectDevice/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            if (result === "False") {
                $('#addDeviceForm').modal('show');
            }
            else {
                var patientId = $('#ddt_process_id').val();
                if (patientId === "") {
                    SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
                }
                else {
                    var validate = Process_ValidateInput('process_patientInfo');
                    if (validate) {

                        // Lấy ServiceId
                        var resultCDHAId = "";
                        $(".row-service").each(function () {
                            $(this).find(".form-check-input").each(function () {
                                var isChecked = $(this).is(':checked');
                                if (isChecked) {
                                    resultCDHAId = $(this).val();
                                }
                            })
                        })

                        if (resultCDHAId === "") {
                            SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
                        }
                        else {
                            $('#showWaitting').modal('show');
                            var returnResultTime = $('#ddt_process_returnResultTime').val();
                            var userReturnResult = $('#ddt_process_userReturnResultDDT').val();
                            var description = CKEDITOR.instances['ddt_process_description'].getData();
                            var result = $('#ddt_process_result').val();
                            var suggest = $('#ddt_process_suggest').val();

                            var data = {
                                patientId: patientId,
                                resultCDHAId: resultCDHAId,
                                returnResultTime: returnResultTime,
                                userReturnResult: userReturnResult,
                                description: description,
                                result: result,
                                suggest: suggest
                            };

                            $.ajax({
                                url: "/DDT_Process/ValidPrint/",
                                data: JSON.stringify(data),
                                contentType: "application/json; charset=utf-8",
                                dataType: "text",
                                type: "POST",
                                success: function (response) {
                                    $('#showWaitting').modal('hide');
                                    if (response === "") {
                                        SwalHelper.Toast.error("Valid & In không thành công. Vui lòng kiểm tra lại!");
                                    }
                                    else {
                                        Process_Refresh();
                                        Process_Get_Count();
                                        var byteCharacters = atob(response);
                                        var byteNumbers = new Array(byteCharacters.length);
                                        for (var i = 0; i < byteCharacters.length; i++) {
                                            byteNumbers[i] = byteCharacters.charCodeAt(i);
                                        }
                                        var byteArray = new Uint8Array(byteNumbers);
                                        var file = new Blob([byteArray], { type: 'application/pdf;base64' });
                                        var fileURL = URL.createObjectURL(file);
                                        window.open(fileURL);
                                    }
                                },
                                error: function () {
                                    $('#showWaitting').modal('hide');
                                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại!");
                                }
                            });
                            return true;
                        }
                    }
                    else {
                        return false
                    }
                }
            }
        }
    });
}


function Process_GetSample() {
    var id = $('#ddt_process_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        $.ajax({
            url: "/DDT_Process/GetSample?id= " + id,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result === 'True') {
                    Process_Refresh();
                }
                else {
                    SwalHelper.Toast.error("Lấy mẫu không thành công. Vui lòng kiểm tra lại!");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lấy mẫu không thành công. Vui lòng kiểm tra lại!");
            }
        });
    }
}



function Process_SetStatus_ClickButtonStatus(btn, value, index) {
    var idResult = "result" + index;
    var result = document.getElementById(idResult);
    if (value === '0') {
        btn.style.color = 'blue';
        result.style.color = 'blue';
        btn.value = 1;
    }
    if (value === '1') {
        btn.style.color = 'red';
        btn.value = 2;
        result.style.color = 'red';
    }
    if (value === '2') {
        btn.style.color = 'dimgrey';
        btn.value = 0;
        result.style.color = 'black';
    }
}


function Process_Get_Count() {
    $.ajax({
        url: "/DDT_Process/Get_Count/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            var arrayResult = result.split(';');
            document.getElementById("countGetSample").innerHTML = arrayResult[0];
            document.getElementById("countProcess").innerHTML = arrayResult[1];
            document.getElementById("countReturnResult").innerHTML = arrayResult[2];
        }
    });
}

function Process_CheckedBoxOnRow(id) {
    $(".row-service").each(function () { // Lấy value trên từng Row
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == id) {
                $(this).prop("checked", true);
                Process_LoadImageForService(id);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Process_LoadImageForService(id) {
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["ddt_process_description"].setData("");
    $("#ddt_process_result").val("");
    $("#ddt_process_suggest").val("");
    $.ajax({
        url: "/DDT_Process/Get_Description_Result_Suggest_ForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                if (value.id) {
                    const $hinh = $('<div class="hinh"></div>');
                    $hinh.append('<img id="' + value.id + '" src="' + value.name + '" onclick="Process_Hinh(' + value.id + ', this)"/>');
                    $containHinh.append($hinh);
                }
                CKEDITOR.instances["ddt_process_description"].setData(value.description);
                $("#ddt_process_result").val(value.result);
                $("#ddt_process_suggest").val(value.suggest);
            });
        }
    });
}

function GetSampleForService(id) {
    $.ajax({
        url: "/DDT_Process/GetSampleForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            CKEDITOR.instances["ddt_process_description"].setData(response.description);
            $("#ddt_process_result").val(response.result);
            $("#ddt_process_suggest").val(response.suggest);
        }
    });
}

function Process_Hinh(imageCDHAId, img) {
    $('.contain-hinh img').removeClass('selected');
    $(img).toggleClass('selected');
    $("#ddt_process_delete").val(imageCDHAId);
    $("#ddt_process_xem").val(imageCDHAId);
}

function UploadHinh(fileInput) {
    if (fileInput.files.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn file!");
        return;
    }

    var resultCDHAId = null;
    $(".row-service").each(function () { // Lấy value trên từng Row
        $(this).find(".form-check-input").each(function () {
            var isChecked = $(this).prop('checked');
            if (isChecked) {
                resultCDHAId = $(this).val();
            }
        })
    })

    if (resultCDHAId !== null) {

        const formData = new FormData();
        formData.append("file", fileInput.files[0]);
        formData.append("resultCDHAId", resultCDHAId);

        $.ajax({
            url: "/DDT_Process/UploadHinh/",
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response !== 'True') {
                    SwalHelper.Toast.error("Không thể lưu ảnh. Vui lòng kiểm tra lại!");
                } else {
                    Process_LoadImageForService(resultCDHAId);
                }
            }
        });
    }
    else {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ !");
    }
}

function XemHinh() {
    const selectedImg = $('.contain-hinh img.selected');
    if (selectedImg.length === 0) {
        SwalHelper.Toast.warning('Bạn chưa chọn hình!');
        return;
    }
    $('#modalImage').attr('src', selectedImg.attr('src'));
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    modal.show();
}

function DeleteHinh() {
    var imageCDHAId = $("#ddt_process_delete").val();
    if (imageCDHAId !== null) {
        Swal.fire({
            title: 'Xác nhận xóa',
            text: "Bạn có chắc chắn muốn xóa hình này không?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/DDT_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
                    type: 'POST',
                    dataType: 'text',
                    success: function (response) {
                        if (response === "") {
                            SwalHelper.Toast.error("Không thể xoá ảnh. Vui lòng kiểm tra lại!");
                        }
                        else {
                            SwalHelper.Toast.success("Xóa ảnh thành công!");
                            //Process_Load_ResultAndImage_ForService(response);
                            Process_CheckedBoxOnRow(response);
                        }
                    },
                    error: function () {
                        SwalHelper.Toast.error("Không thể xoá ảnh. Vui lòng kiểm tra lại!");
                    }
                });
            }
        });
    }
    else {
        SwalHelper.Toast.error("Không thể xoá ảnh. Vui lòng kiểm tra lại!");
    }
    //if (imageCDHAId !== null) {
    //    $.ajax({
    //        url: "/DDT_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
    //        type: 'POST',
    //        dataType: 'text',
    //        success: function (response) {
    //            if (response === "") {
    //                SwalHelper.Toast.error("Không thể xoá ảnh. Vui lòng kiểm tra lại!");
    //            }
    //            else {
    //                Process_LoadImageForService(response);
    //            }
    //        }
    //    });
    //}
    //else {
    //    SwalHelper.Toast.error("Không thể xoá ảnh. Vui lòng kiểm tra lại!");
    //}
}

// Tải thông tin bác sĩ khi đổi select
function Process_Load_SelectedDoctorInfo() {
    console.log("voday");
    var userId = $("#ddt_process_userReturnResultDDT").val();
    if (!userId) {
        $("#ddt_process_signerCCCD").empty();
        $("#ddt_process_doctorInfo").empty();
        return;
    }
    console.log(userId);
    $.ajax({
        url: "/DDT_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#ddt_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
                return;
            }
            console.log(u);
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#ddt_process_signerCCCD").val(cccd);
        },
        error: function () {
            $("#ddt_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
        }
    });
}
function Process_SignPdf_Multi(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    var patientId = $('#ddt_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Thu thập danh sách resultIds theo mode
    var resultIds = [];
    if (mode === 'current') {
        // lấy id của dòng đang active (tùy markup; ví dụ theo checkbox đang checked đầu tiên)
        var rid = $(".row-service .form-check-input:checked").first().val();
        if (rid) resultIds.push(rid);
    } else if (mode === 'selected') {
        $(".row-service .form-check-input:checked").each(function () {
            var v = $(this).val();
            if (v) resultIds.push(v);
        });
    } else if (mode === 'all') {
        $(".row-service").each(function () {
            var v = $(this).find(".form-check-input").first().val();
            if (v) resultIds.push(v);
        });
    }

    if (resultIds.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn ít nhất 1 dịch vụ để ký.");
        return;
    }

    // Lấy CCCD người ký (bác sĩ)
    var signerCCCD = $('#ddt_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#ddt_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#ddt_process_patientId').val() || $('#ddt_process_sid').val() || "";
    var patientMaBenhAn = $('#ddt_process_maBenhAn').val() || $('#ddt_process_sid').val() || "";
    var patientName = $('#ddt_process_patientName').val() || "";
    var doctorName = $('#ddt_process_userReturnResultDDT option:selected').text() || $('#ddt_process_userReturnResultDDT').val() || "";
    var doctorId = $('#ddt_process_userReturnResultDDT').val();
    var performedAt = $('#ddt_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "DoDienTim"; // bạn có thể thay động tùy màn hình

    // 1) Gọi API xuất PDF base64 (tận dụng endpoint ValidPrint đang có)
    //    Nếu bạn có endpoint riêng chỉ "Export" (không đổi trạng thái), thay URL ở đây là tốt nhất.
    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tạo file PDF...'); } catch (e) { }

    // 2. PHASE 1: EXPORT tất cả PDF thô
    var pdfJobs = [];  // [{resultCDHAId, base64Pdf}]
    var chain = Promise.resolve();

    console.log("Tổng ID dịch vụ: ", resultIds);
    if (resultIds.length > 1) {
        resultIds.forEach(function (rid) {
            chain = chain.then(function () {
                return new Promise(function (resolve) {
                    var dataExport = {
                        patientId: patientId,
                        resultCDHAId: rid,
                        // khi ký hàng loạt thì nên dùng dữ liệu đã lưu, không đẩy mô tả đang sửa
                        returnResultTime: performedAt,
                        userReturnResult: doctorId,
                        description: null,
                        result: null,
                        suggest: null
                    };
                    console.log(rid, "====>", dataExport);
                    $.ajax({
                        url: "/DDT_Process/ValidPrintMultiple/",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        data: JSON.stringify(dataExport),
                        success: function (base64Pdf) {
                            console.log(rid, "====>", base64Pdf);
                            if (base64Pdf) {
                                pdfJobs.push({
                                    resultCDHAId: rid,
                                    base64Pdf: base64Pdf
                                });
                            }
                            // dù có hay không vẫn resolve để chạy dịch vụ kế
                            resolve();
                        },
                        error: function () {
                            // lỗi 1 dịch vụ thì bỏ qua
                            resolve();
                        }
                    });
                });
            });
        });
    } else {
        chain = chain.then(function () {
            return new Promise(function (resolve) {
                var dataExport = {
                    patientId: patientId,
                    resultCDHAId: resultIds[0],
                    returnResultTime: $('#ddt_process_returnResultTime').val(),
                    userReturnResult: $('#ddt_process_userReturnResultDDT').val(),
                    description: CKEDITOR.instances['ddt_process_description'].getData(),
                    result: $('#ddt_process_result').val(),
                    suggest: $('#ddt_process_suggest').val()
                };
                console.log(resultIds[0], "====>", dataExport);
                $.ajax({
                    url: "/DDT_Process/ValidPrint/",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    data: JSON.stringify(dataExport),
                    success: function (base64Pdf) {
                        console.log(resultIds[0], "====>", base64Pdf);
                        if (base64Pdf) {
                            pdfJobs.push({
                                resultCDHAId: resultIds[0],
                                base64Pdf: base64Pdf
                            });
                        }
                        // dù có hay không vẫn resolve để chạy dịch vụ kế
                        resolve();
                    },
                    error: function () {
                        // lỗi 1 dịch vụ thì bỏ qua
                        resolve();
                    }
                });
            });
        });
    }

    console.log(pdfJobs);
    // 3. PHASE 2: Gửi 1 lần lên /api/ExternalSign/sign-pdf
    chain = chain.then(function () {
        if (pdfJobs.length === 0) {
            // không export được cái nào
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.warning("Không tạo được file PDF nào để ký.");
            return;
        }
        startCountdown(120);
        var formData = new FormData();
        formData.append("signerCCCD", signerCCCD);
        formData.append("formType", formType);
        formData.append("patientCode", patientCode);
        formData.append("patientMaBenhAn", patientMaBenhAn);
        formData.append("patientName", patientName);
        formData.append("doctorName", doctorName);
        if (performedAt) {
            formData.append("performedAt", "");
        }

        // thêm từng file & id tương ứng
        pdfJobs.forEach(function (job, idx) {
            var file = base64ToFile(job.base64Pdf, "temp.pdf", "application/pdf");
            formData.append("files", file, file.name);          // <-- gửi nhiều "file"
            formData.append("resultIds", job.resultCDHAId);     // <-- cùng thứ tự
            console.log(idx, "==================>", formData);
            for (let pair of formData.entries()) {
                console.log(pair[0] + ':', pair[1]);
            }
        });
        return new Promise(function (resolve) {
            $.ajax({
                url: "/api/ExternalSign/sign-pdf-multi",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (resp) {
                    console.log("SAU KHI KÝ: ", resp);
                    // resp có thể là dạng cũ (1 file) hoặc mới (nhiều file)
                    // mình normalize về mảng
                    var items = [];
                    if (resp) {
                        if (Array.isArray(resp.items)) {
                            // Lọc những item có chứa id
                            items = resp.items.filter(it => it && it.data.id);
                        } else if (resp.item) {
                            // Kiểm tra item đơn lẻ
                            if (resp.item.id) {
                                items = [resp.item];
                            }
                        } else if (resp.signStoreId || resp.data) {
                            // Trường hợp cũ: chỉ 1 file
                            if (resp.id) {
                                items = [resp];
                            }
                        }
                    }

                    console.log("Mãng tiếp tục để lưu digital: ", items);
                    if (!items || items.length === 0) {
                        resolve();
                        $('#showWaitting').modal('hide');
                        var errorMessage = resp.items[0].data.detail || "";
                        SwalHelper.Toast.error(`Xảy ra lỗi khi ký: ${errorMessage}`);
                        return;
                    }

                    // 4. LƯU từng dịch vụ
                    var saveChain = Promise.resolve();

                    items.forEach(function (it) {
                        saveChain = saveChain.then(function () {
                            return new Promise(function (resolveSave) {
                                var resultId = it.resultCDHAId || it.referenceId || null;
                                var objDataResponse = it.data;
                                var signStoreId =
                                    (it && it.signStoreId) ||
                                    (it && it.id) ||
                                    (it && it.data && (it.data.signStoreId || it.data.id)) ||
                                    null;

                                // fallback test
                                if (!signStoreId) {
                                    resolveSave();
                                }

                                if (!resultId) {
                                    // nếu backend không trả về id thì mình cố map bằng thứ tự
                                    // nhưng để đơn giản: bỏ qua
                                    resolveSave();
                                    return;
                                }
                                console.log("resultId ========> ", resultId);
                                console.log("signStoreId ========> ", signStoreId);
                                // Gọi để lưu signStoreId theo dịch vụ
                                $.ajax({
                                    url: "/DDT_Process/SaveSignStoreIdForResultCDHA",
                                    type: "POST",
                                    dataType: "text",
                                    data: {
                                        resultCDHAId: resultId,
                                        signStoreId: signStoreId
                                    },
                                    success: function (resText) {
                                        try {
                                            var res = JSON.parse(resText);
                                            console.log("response SaveSignStoreIdForResultCDHA: ", res);
                                            if (res && res.success) {
                                                var keyResult = res.keyResult;
                                                var statusNum = (objDataResponse && (objDataResponse.status === 1 || objDataResponse.status === "1")) ? 1 : 0;

                                                // Lưu vào bảng Digital_Sign
                                                var payload = {
                                                    referenceType: formType ?? "DienTim",
                                                    referenceKeyResult: keyResult,
                                                    signId: Number(signStoreId),
                                                    signUserId: signerCCCD,
                                                    taxCode: objDataResponse.taxcode || objDataResponse.taxCode || null,
                                                    targetText: doctorName,
                                                    requestUrl: window.location.origin + "/api/ExternalSign/sign-pdf-multi",
                                                    responseData: JSON.stringify(objDataResponse),
                                                    status: statusNum
                                                };
                                                console.log("payload lưu digitalSign: ", payload);
                                                $.ajax({
                                                    url: "/DigitalSign/Save",
                                                    type: "POST",
                                                    data: JSON.stringify(payload),
                                                    contentType: "application/json; charset=utf-8",
                                                    complete: function (r) {
                                                        resolveSave();
                                                        console.log("Lưu DigitalSign thành công: ", r);
                                                    }
                                                });
                                            } else {
                                                // lưu mapping thất bại
                                                resolveSave();
                                                $('#showWaitting').modal('hide');
                                            }
                                        } catch (e) {
                                            resolveSave();
                                            $('#showWaitting').modal('hide');
                                        }
                                    },
                                    error: function () {
                                        resolveSave();
                                        $('#showWaitting').modal('hide');
                                    }
                                });
                            });
                        });
                    });

                    saveChain.then(function () {
                        $('#showWaitting').modal('hide');
                        SwalHelper.Toast.success(`Đã ký số thành công ${items.length} kết quả`);
                        Process_Refresh();
                        Process_Get_Count();
                        resolve();
                    });
                },
                error: function (xhr) {
                    $('#showWaitting').modal('hide');
                    console.log(xhr);
                    var msg = "Ký số thất bại.";
                    if (xhr && xhr.responseJSON) msg += "\n" + xhr.responseJSON.message;
                    SwalHelper.Toast.error(msg);
                    resolve();
                }
            });
        });
    });
}

// Helpers
function base64ToFile(base64, filename, mime) {
    try {
        // Nếu backend trả 'data:application/pdf;base64,....' thì tách bỏ prefix
        var idx = base64.indexOf("base64,");
        var pure = (idx >= 0) ? base64.substring(idx + 7) : base64;

        var byteCharacters = atob(pure);
        var byteNumbers = new Array(byteCharacters.length);
        for (var i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        var byteArray = new Uint8Array(byteNumbers);
        var blob = new Blob([byteArray], { type: 'application/pdf' });
        console.error(blob);
        // Tên tạm – server sẽ tự đặt lại theo template nên không quan trọng
        var file = new File([blob], filename || "document.pdf", { type: 'application/pdf' });
        console.log(file)
        return file;
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
    var re = /^[A-Za-z0-9+/=\s]+$/; // thô sơ
    return re.test(s) && s.length > 1000; // PDF base64 thường dài
}
function DDT_TestPortalLink() {
    var patientId = $('#ddt_process_id').val();
    var maBenhAn = $('#ddt_process_maBenhAn').val(); // Hidden field we'll add

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân trước!");
        return;
    }

    if (!maBenhAn) {
        // Fallback: ask user to input
        maBenhAn = prompt("Vui lòng nhập mã bệnh án để test:", "BA000001");
        if (!maBenhAn) {
            return;
        }
    }

    var testData = {
        maBenhAn: maBenhAn,
        hisApiKey: "LisSecretKeyForHISLeanCare2025!@#$%"
    };

    // Show loading message
    var loadingAlert = "⏳ Đang tạo link portal...";
    console.log(loadingAlert);
    console.log(testData);
    $.ajax({
        url: "/Portal/CreatePortalLink",
        type: "POST",
        data: JSON.stringify(testData),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                //var message = "✅ Test Portal thành công!\n\n";
                //message += "🏥 Mã bệnh án: " + maBenhAn + "\n";
                //message += "🔗 Portal URL: " + response.portalUrl + "\n\n";
                //message += "🔑 Token: " + response.token.substring(0, 50) + "...\n\n";
                //message += "⏰ Hết hạn: " + new Date(response.expiryDate).toLocaleString('vi-VN') + "\n\n";
                //message += "Bạn có muốn mở portal trong tab mới không?";

                if (response.portalUrl != null) {
                    window.open(response.portalUrl, '_blank', 'width=1200,height=800,scrollbars=yes,resizable=yes');
                }
            } else {
                SwalHelper.Toast.error("❌ Lỗi tạo portal link: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.log(xhr);
            console.log(status);
            console.log(error);
            var errorMessage = "❌ Lỗi gọi API CreatePortalLink:\n\n";
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage += "Chi tiết: " + xhr.responseJSON.message;
            } else if (xhr.responseText) {
                errorMessage += "Response: " + xhr.responseText;
            } else {
                errorMessage += "Error: " + error + "\nStatus: " + status;
            }
            SwalHelper.Toast.error(errorMessage);
            console.error("Portal test error:", xhr, status, error);
        }
    });
}

function Process_SaveDateToSession() {
    var fromDate = document.getElementById('ddt_process_timeSearchFrom').value;
    var toDate = document.getElementById('ddt_process_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/DDT_Process/SaveSearchDates',
            type: 'POST',
            data: {
                timeSearchFrom: fromDate,
                timeSearchTo: toDate
            },
            success: function (result) {
                // Optionally handle success
            }
        });
    }
}

// Tick/untick tất cả
$(document).on('change', '#ddt_chk_all', function () {
    var checked = this.checked === true;
    // chỉ tick các dịch vụ đang hiển thị
    $('#ddt_list_service').find('.ddt-chk-service').prop('checked', checked);

    // Ẩn/hiện nút Lưu dựa trên trạng thái check all
    //if (checked) {
    //    $('#ddt_process_saveresult').hide();
    //} else {
    //    $('#ddt_process_saveresult').show();
    //}
});

// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả"
$(document).on('change', '.ddt-chk-service', function () {
    var $rows = $('#ddt_list_service').find('.ddt-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Nếu tất cả đều check => check header; ngược lại bỏ check header
    var allChecked = total > 0 && marked === total;
    $('#ddt_chk_all').prop('checked', allChecked);

    //// Ẩn/hiện nút Lưu dựa trên trạng thái check all
    //if (allChecked) {
    //    $('#ddt_process_saveresult').hide();
    //} else {
    //    $('#ddt_process_saveresult').show();
    //}
});

// ============================== IMPORT kết quả NGOÀI (PDF) ==============================

// Mở modal + đổ patientId hiện chọn
var currentImportedExternalFileId = 0;
var currentImportedExternalFileName = '';
function Process_ShowImportModal() {
    var pid = $('#ddt_process_patientId').val();
    var pidTablePatient = $('#ddt_process_id').val();
    var pName = $('#ddt_process_patientName').val();
    var pMaBenhAn = $('#ddt_process_maBenhAn').val();

    $('#import_patientId').val(pid);
    $('#import_idTablePatient').val(pidTablePatient);
    $('#import_patientName').val(pName);
    $('#import_maBenhAn').val(pMaBenhAn);

    // clear UI
    $('#import_pdf_file').val('');
    $('#pdfPreview').html('<em class="text-muted">Chưa có tệp được chọn…</em>');

    if (pidTablePatient === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }
    // mở modal (nếu không dùng data-toggle)
    // tải danh sách PDF đã import
    Process_LoadImportedPdfList(pMaBenhAn, pid, pidTablePatient);
    try { $('#modal-import-external').modal('show'); } catch (e) { }
}

// Xem trước PDF ngay khi chọn file
function Process_HandlePdfPreview(input) {
    const container = document.getElementById('pdfPreview');
    if (!input || !input.files || input.files.length === 0) {
        container.innerHTML = '<em class="text-muted">Chưa có tệp được chọn…</em>';
        return;
    }
    const file = input.files[0];
    if (file.type !== 'application/pdf') {
        SwalHelper.Toast.warning('File không phải PDF!');
        input.value = '';
        container.innerHTML = '<em class="text-muted">Chưa có tệp được chọn…</em>';
        return;
    }
    const url = URL.createObjectURL(file);
    // Dùng <embed> để preview đơn giản, tương thích tốt
    container.innerHTML = '';
    const emb = document.createElement('embed');
    emb.src = url;
    emb.type = 'application/pdf';
    emb.style.width = '100%';
    emb.style.height = '70vh';
    container.appendChild(emb);
}

function Process_LoadImportedPdfList(pMaBenhAn, pId, pidTablePatient) {
    $('#imported_pdf_list').html('<div class="text-muted p-2">Đang tải…</div>');
    $.get('/DDT_GetSample/GetExternalResultFiles', { pMaBenhAn: pMaBenhAn, pId: pId, pidTablePatient: pidTablePatient }, function (res) {
        if (!res || res.success !== true) {
            $('#imported_pdf_list').html('<div class="text-danger p-2">Không tải được danh sách.</div>');
            return;
        }
        if (!res.merged || res.merged.length === 0) {
            $('#imported_pdf_list').html('<div class="text-muted p-2">Chưa có tệp nào.</div>');
            return;
        }
        var html = '<ul class="list-group list-group-flush">';

        res.merged.forEach(function (f) {
            var created = f.created ? new Date(f.created).toLocaleString() : '';
            var badge = f.isSynced
                ? '<span class="badge badge-success"><i class="fa fa-check"></i> Synced</span>'
                : '<span class="badge badge-warning"><i class="fa fa-exclamation-triangle"></i> Chưa sync</span>';
            html += `
            <li class="list-group-item py-2">
              <div class="d-flex justify-content-between align-items-center">
                ${badge}
                <a href="${f.url}" target="_blank" class="font-weight-bold" title="Mở trong tab mới">${escapeHtml(f.name)}</a>
                <div style="display: flex; gap: .3rem">
                  <button
                    type="button"
                    class="btn btn-sm btn-primary me-1 btn-preview-imported-pdf"
                    data-id="${f.id || 0}"
                    data-url="${f.url}"
                    data-name="${escapeHtml(f.name)}"
                    data-visible="${f.isVisibleToUser === true ? 'true' : 'false'}">
                    Preview
                  </button>
                  <button type="button" class="btn btn-sm btn-outline-danger" style="text-wrap-mode: nowrap;" onclick="Process_DeleteImportedPdf('${escapeHtml(f.name)}', '${pMaBenhAn}', '${pId}', '${pidTablePatient}')" title="Xóa file">
                    <i class="fa fa-trash"></i>
                    Xóa file
                  </button>
                </div>
              </div>
              <div class="small text-muted">${created}${f.vendor ? ' • ' + escapeHtml(f.vendor) : ''}</div>
            </li>`;
        });
        html += '</ul>';
        $('#imported_pdf_list').html(html);
        $('#imported_pdf_list .btn-preview-imported-pdf').off('click').on('click', function () {
            var id = parseInt($(this).attr('data-id') || '0');
            var url = $(this).attr('data-url') || '';
            var name = $(this).attr('data-name') || '';
            var isVisible = ($(this).attr('data-visible') || '').toLowerCase() === 'true';

            Process_PreviewImportedPdf({
                id: id,
                url: url,
                name: name,
                isVisibleToUser: isVisible
            });
        });
    }).fail(function () {
        $('#imported_pdf_list').html('<div class="text-danger p-2">Lỗi khi tải danh sách.</div>');
    });
}

function Process_DeleteImportedPdf(fileName, pMaBenhAn, pId, patientId) {
    if (!confirm(`Bạn có chắc chắn xóa file "${fileName}" không?`)) {
        return;
    }
    $.ajax({
        url: '/DDT_GetSample/DeleteExternalResultFile',
        type: 'POST',
        data: {
            fileName: fileName,
            pMaBenhAn: pMaBenhAn,
            pId: pId
        },
        success: function (res) {
            if (res && res.success) {
                SwalHelper.Toast.success('Xóa file thành công!');
                // Refresh danh sách file
                Process_LoadImportedPdfList(pMaBenhAn, patientId);
                // Clear preview n?u file dang du?c xem
                const container = document.getElementById('pdfPreview');
                container.innerHTML = '<em class="text-muted">Chua có tệp được chọn…</em>';
            } else {
                SwalHelper.Toast.error(res && res.message ? res.message : 'Xóa file thất bại!');
            }
        },
        error: function () {
            SwalHelper.Toast.error('Lỗi khi xóa file. Vui lòng thử lại!');
        }
    });
}
function Process_PreviewImportedPdf(fileInfo) {
    if (!fileInfo || !fileInfo.url) return;

    currentImportedExternalFileId = fileInfo.id || 0;
    currentImportedExternalFileName = fileInfo.name || '';

    // 1) preview pdf
    const container = document.getElementById('pdfPreview');
    container.innerHTML = '';

    const emb = document.createElement('embed');
    emb.src = fileInfo.url;
    emb.type = 'application/pdf';
    emb.style.width = '100%';
    emb.style.height = '70vh';
    container.appendChild(emb);

    // 2) set toggle theo file đang preview
    $('#import_is_visible_to_user').prop('checked', fileInfo.isVisibleToUser === true);

    // 3) Ẩn nút Import, hiện nút Lưu
    $('#btn_import_external_pdf').hide();
    $('#btn_save_imported_pdf_setting').show();

    // 4) Có thể disable chọn file mới khi đang edit file cũ
    $('#import_pdf_file').prop('disabled', true);
}

function Process_SaveImportedPdfSetting() {
    if (!currentImportedExternalFileId || currentImportedExternalFileId <= 0) {
        alert('File này chưa được đồng bộ vào hệ thống nên chưa thể lưu thay đổi.');
        return;
    }

    var isVisibleToUser = $('#import_is_visible_to_user').is(':checked');

    $.post('/DDT_GetSample/SaveExternalResultFileVisibility', {
        id: currentImportedExternalFileId,
        isVisibleToUser: isVisibleToUser
    }, function (res) {
        if (!res || res.success !== true) {
            alert(res && res.message ? res.message : 'Lưu thay đổi thất bại.');
            return;
        }

        SwalHelper.Toast.success('Lưu thay đổi thành công');
        var pid = $('#import_patientId').val();
        var pidTablePatient = $('#import_idTablePatient').val();
        var pMaBenhAn = $('#import_maBenhAn').val();
        // preview xong lưu xong thì load lại danh sách
        Process_LoadImportedPdfList(pMaBenhAn, pid, pidTablePatient);
    }).fail(function () {
        alert('Lỗi khi lưu thay đổi.');
    });
}

function Process_ResetExternalImportMode() {
    currentImportedExternalFileId = 0;
    currentImportedExternalFileName = '';

    $('#btn_import_external_pdf').show();
    $('#btn_save_imported_pdf_setting').hide();

    $('#import_pdf_file').prop('disabled', false);

    $('#import_is_visible_to_user').prop('checked', true);

    const container = document.getElementById('pdfPreview');
    if (container) {
        container.innerHTML = '<div class="text-muted text-center mt-5">Chưa có tệp được chọn...</div>';
    }
}
$('#modal-import-external').on('hidden.bs.modal', function () {
    Process_ResetExternalImportMode();
});
function escapeHtml(s) {
    if (!s) return '';
    return s.replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m]));
}

// Gửi kèm vendor vào FormData
function Process_SubmitExternalImport() {
    var pid = $('#import_patientId').val();
    var pidTablePatient = $('#import_idTablePatient').val();
    var pName = $('#import_patientName').val();
    var pMaBenhAn = $('#import_maBenhAn').val();
    console.log(pid);
    console.log(pidTablePatient);
    console.log(pName);
    console.log(pMaBenhAn);

    //var pName = $('#import_patientName').val();
    var fileInput = document.getElementById('import_pdf_file');
    if (!pid) { SwalHelper.Toast.warning('Thiếu patientId!'); return; }
    if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
        SwalHelper.Toast.warning('Vui lòng chọn file PDF!');
        return;
    }

    var overwrite = $('#import_overwrite').is(':checked');
    var markValid = $('#import_mark_valid').is(':checked');
    var isVisibleToUser = $('#import_is_visible_to_user').is(':checked');

    //var vendor = $('#import_vendor').val() || '';

    var fd = new FormData();
    fd.append('pMaBenhAn', pMaBenhAn);
    fd.append('pId', pid);
    fd.append('pidTablePatient', pidTablePatient);
    fd.append('pName', pName);
    fd.append('file', fileInput.files[0]);
    fd.append('overwrite', overwrite);
    fd.append('markValid', markValid);
    fd.append('isVisibleToUser', isVisibleToUser);

    //fd.append('vendor', vendor);

    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: "/DDT_GetSample/ImportExternalResultPdf",
        type: "POST",
        data: fd,
        processData: false,
        contentType: false,
        success: function (res) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            if (!res || res.success !== true) {
                SwalHelper.Toast.error(res && res.message ? res.message : "Import thất bại.");
                return;
            }

            // Refresh danh sách file đã import
            Process_LoadImportedPdfList(pMaBenhAn, pid);

            // Thông báo thành công với thông tin file
            var message = "Lưu file PDF thành công!";
            if (res.fileName) {
                message += "\nTên file: " + res.fileName;
            }
            SwalHelper.Toast.success(message);

            try {
                $('#modal-import-external').modal('hide');
                $('.modal-backdrop').remove();
            } catch (e) { }
        },
        error: function () {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            SwalHelper.Toast.error("Không thể lưu file PDF. Vui lòng thử lại!");
        }
    });
}
//***************************************************************************************** Return Result

function ReturnResult_Refresh() {
    $.ajax({
        url: "/DDT_ReturnResult/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#ddt_returnresult_timeSearchFrom").val(today);
            //$("#ddt_returnresult_timeSearchTo").val(today);
            $("#returnresult_listPatient").html(result);

            ReturnResult_ResetInput();
            ReturnResult_Get_Count();
        },
        error: function () {
            $("#returnresult_listPatient").empty();
        }
    });
}

function ReturnResult_Search() {
    var ddt_process_pidorseq = $("#ddt_returnresult_pidorseq").val();
    var timeSearchFrom = $("#ddt_returnresult_timeSearchFrom").val();
    var timeSearchTo = $("#ddt_returnresult_timeSearchTo").val();
    $.ajax({
        url: "/DDT_ReturnResult/Search?" + "pidorseq=" + ddt_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_listPatient").html(result);
            ReturnResult_Get_Count();
        },
        error: function () {
            $("#returnresult_listPatient").empty();
        }
    });
}
function ReturnResult_ResetInput() {
    $("#ddt_returnresult_pidorseq").val('');
    $('#ddt_returnresult_id').val('');
    $('#ddt_returnresult_patientId').val('');
    $('#ddt_returnresult_seq').val('');
    $('#ddt_returnresult_sid').val('');
    $('#ddt_returnresult_patientName').val('');
    $('#ddt_returnresult_age').val('');
    $('#ddt_returnresult_sex').val('');
    $('#ddt_returnresult_obj').val('');
    $('#ddt_returnresult_type').val('');
    $('#ddt_returnresult_location').val('');
    $('#ddt_returnresult_doctor').val('');
    $('#ddt_returnresult_getSampleTime').val('');
    $('#ddt_returnresult_returnResultTime').val('');
    $('#ddt_returnresult_location').val('');
    $('#ddt_returnresult_doctor').val('');
    $('#ddt_returnresult_userReturnResultDDT').val('');
    $('#ddt_returnresult_address').val('');
    $('#ddt_returnresult_diagnostic').val('');
    $('#tbody-gridview-service').empty();
}

function ReturnResult_GetPatientInfo(id) {
    $.ajax({
        url: "/DDT_ReturnResult/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_patientInfo").html(result);
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            ReturnResult_GetListServiceForPatient(id);
            ReturnResult_Load_SelectedDoctorInfo();
        },
        error: function () {
            $("#returnresult_patientInfo").empty();
            $('#tbody-gridview-service').empty();
        }
    });
}
// Tải thông tin bác sĩ khi đổi select
function ReturnResult_Load_SelectedDoctorInfo() {
    var userId = $("#ddt_returnresult_userLoginId").val();
    console.log(userId);
    if (!userId) {
        $("#ddt_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/DDT_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            console.log(u);
            if (!u) {
                $("#ddt_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#ddt_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#ddt_returnresult_signerCCCD").text("Không lấy đượcthông tin CCCD.");
        },
    });
}
function ReturnResult_GetListServiceForPatient(id) {
    $.ajax({
        url: "/DDT_ReturnResult/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_ddt-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function ReturnResult_Invalid() {
    var patientId = $('#ddt_returnresult_id').val();

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // ResultCDHA.Id của các dịch vụ đang được chọn.
    // Backend sẽ tự lấy KeyResultForHis từ DB theo ResultCDHA.Id.
    var resultIds = [];

    $('.ddt-returnresult-chk-service:checked').each(function () {
        var resultId = parseInt($(this).val(), 10);

        if (!isNaN(resultId) && resultId > 0) {
            resultIds.push(resultId);
        }
    });

    if (resultIds.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn ít nhất một dịch vụ!");
        return;
    }

    console.log("ResultCDHA.Id được chọn:", resultIds);

    if (!confirm('Bạn muốn InValid kết quả của bệnh nhân ?')) {
        return;
    }

    $.ajax({
        url: "/DDT_ReturnResult/Invalid",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify({
            patientId: parseInt(patientId, 10),
            resultIds: resultIds,
            categoryCode: "DDT"
        }),
        success: function (result) {
            if (result === "True") {
                SwalHelper.Toast.success("Invalid thành công!");
                ReturnResult_Refresh();
                ReturnResult_Get_Count();
            }
            else {
                SwalHelper.Toast.error(result || "Không thể Invalid. Vui lòng kiểm tra lại!");
            }
        },
        error: function (xhr) {
            SwalHelper.Toast.error(xhr.responseText || "Không thể Invalid. Vui lòng kiểm tra lại!");
        }
    });
}

function ReturnResult_Invalid_RemoveDigitalSign() {
    var patientId = $('#ddt_returnresult_id').val();

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    var resultCDHAId = "";
    var resultIds = [];

    $(".row-service .form-check-input:checked").each(function () {
        var selectedId = parseInt($(this).val(), 10);

        if (!isNaN(selectedId) && selectedId > 0) {
            resultIds.push(selectedId);

            // Nút hủy ký số hiện chỉ thao tác trạng thái ký của một dịch vụ.
            // Giữ lại id đang chọn để UpdateSignStatus chạy như luồng cũ.
            resultCDHAId = selectedId;
        }
    });

    if (resultIds.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
        return;
    }

    if (!confirm('Bạn muốn InValid kết quả của bệnh nhân (hủy Ký Số kết quả này) ?')) {
        return;
    }

    $.ajax({
        url: "/DDT_ReturnResult/Invalid",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify({
            patientId: parseInt(patientId, 10),
            resultIds: resultIds,
            categoryCode: "DDT"
        }),
        success: function (result) {
            if (result === "True") {
                SwalHelper.Toast.success("Invalid thành công và đã hủy ký số!");
                UpdateSignStatus(resultCDHAId);
                ReturnResult_Refresh();
            }
            else {
                SwalHelper.Toast.error(result || "Không thể Invalid. Vui lòng kiểm tra lại!");
            }
        },
        error: function (xhr) {
            SwalHelper.Toast.error(xhr.responseText || "Không thể Invalid. Vui lòng kiểm tra lại!");
        }
    });
}

function UpdateSignStatus(resultCDHAId) {
    $.ajax({
        url: "/DigitalSign/UpdateSignStatus_Result?resultCDHAId=" + resultCDHAId,
        type: 'POST',
        dataType: 'text',
        success: function (res) {
            console.log("Update Status Sign: ", res);
            var response = JSON.parse(res);
            if (response.success == true) {
                SwalHelper.Toast.success("Thành công");
            }
            else {
                SwalHelper.Toast.error("Thao tác hủy ký chưa thành công");
            }
        },
        error: function () {
            SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại!");
        }
    });
}
function ReturnResult_Get_Count() {
    $.ajax({
        url: "/DDT_ReturnResult/Get_Count/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            var arrayResult = result.split(';');
            document.getElementById("countGetSample").innerHTML = arrayResult[0];
            document.getElementById("countProcess").innerHTML = arrayResult[1];
            document.getElementById("countReturnResult").innerHTML = arrayResult[2];
        }
    });
}

function ReturnResult_Print() {
    var patientId = $('#ddt_returnresult_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Lấy ServiceId
    var resultCDHAId = "";
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var isChecked = $(this).is(':checked');
            if (isChecked) {
                resultCDHAId = $(this).val();
            }
        })
    })

    if (resultCDHAId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
        return;
    }
    $('#showWaitting').modal('show');

    $.ajax({
        url: "/DDT_ReturnResult/Print?resultCDHAId=" + resultCDHAId,
        dataType: "text",
        type: "GET",
        success: function (response) {
            $('#showWaitting').modal('hide');

            if (response === "") {
                SwalHelper.Toast.error("In không thành công. Vui lòng kiểm tra lại!");
                return;
            }

            try {
                // Chuyển base64 thành binary
                var byteCharacters = atob(response);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var blob = new Blob([byteArray], { type: 'application/pdf' });

                // ===== PHẦN MỚI: Xử lý cho cả Desktop và Mobile =====

                // Kiểm tra thiết bị
                var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);

                if (isMobile) {
                    // CÁCH 1: Tạo link download cho mobile
                    var link = document.createElement('a');
                    link.href = URL.createObjectURL(blob);
                    link.download = 'KetQua_DDT_' + resultCDHAId + '_' + new Date().getTime() + '.pdf';
                    link.style.display = 'none';

                    document.body.appendChild(link);
                    link.click();

                    // Cleanup
                    setTimeout(function () {
                        document.body.removeChild(link);
                        URL.revokeObjectURL(link.href);
                    }, 100);

                    // Hiển thị thông báo cho user
                    SwalHelper.Toast.success('File PDF đã được tải xuống!');

                } else {
                    // Desktop: Mở trong tab mới
                    var fileURL = URL.createObjectURL(blob);
                    var newWindow = window.open(fileURL, '_blank');

                    if (!newWindow) {
                        // Nếu popup bị chặn, fallback sang download
                        var link = document.createElement('a');
                        link.href = fileURL;
                        link.download = 'KetQua_DDT_' + resultCDHAId + '.pdf';
                        link.click();
                        URL.revokeObjectURL(fileURL);
                        SwalHelper.Toast.info('File PDF đã được tải xuống do popup bị chặn!');
                    }
                }

            } catch (error) {
                console.error('Lỗi xử lý PDF:', error);
                SwalHelper.Alert.error('Lỗi', 'Không thể hiển thị file PDF. Vui lòng thử lại!');
            }
        },
        error: function (xhr, status, error) {
            $('#showWaitting').modal('hide');
            console.error('AJAX Error:', status, error);
            SwalHelper.Alert.error('Lỗi', 'In không thành công. Vui lòng kiểm tra lại!');
        }
    });
}

function ReturnResult_CheckedBoxOnRow(id) {
    $(".row-service").each(function () { // Lấy value trên từng Row
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == id) {
                $(this).prop("checked", true);
                ReturnResult_LoadImageForService(id);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function ReturnResult_CheckedBoxOnRow(id, event) {
    // Nếu click trực tiếp vào checkbox thì không xử lý (để người dùng tự chọn multiple)
    if (event && event.target.type === 'checkbox') {
        // Chỉ cập nhật trạng thái "chọn tất cả"
        var $rows = $('.ddt-returnresult-chk-service');
        var total = $rows.length;
        var marked = $rows.filter(':checked').length;
        var allChecked = total > 0 && marked === total;
        $('#ddt_returnresult_chk_all').prop('checked', allChecked);

        // Nếu chỉ có 1 checkbox được chọn sau khi click
        var selectedCheckboxes = $(".row-service .form-check-input:checked");
        if (selectedCheckboxes.length === 1) {
            var $row = selectedCheckboxes.first().closest('.row-service');
            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
        } else if (selectedCheckboxes.length > 1) {
            // Clear chi tiết khi chọn nhiều
            const $containHinh = $('.contain-hinh');
            $containHinh.empty();
            CKEDITOR.instances["ddt_returnresult_description"].setData("");
            $("#ddt_returnresult_result").val("");
            $("#ddt_returnresult_suggest").val("");
            ReturnResult_ToggleDigitalSignButtons(false);
        } else {
            // Clear khi không có gì được chọn
            const $containHinh = $('.contain-hinh');
            $containHinh.empty();
            CKEDITOR.instances["ddt_returnresult_description"].setData("");
            $("#ddt_returnresult_result").val("");
            $("#ddt_returnresult_suggest").val("");
            ReturnResult_ToggleDigitalSignButtons(false);
        }
        return;
    }

    // Click vào row (không phải checkbox) → chỉ chọn 1 dịch vụ và hiển thị chi tiết
    $(".row-service .form-check-input").each(function () {
        var $chk = $(this);
        var resultCdhaId = String($chk.val());
        var isTarget = (resultCdhaId === String(id));
        $chk.prop("checked", isTarget);

        if (isTarget) {
            ReturnResult_LoadImageForService(id);

            // Check digital signature status and toggle buttons
            var $row = $chk.closest('.row-service');
            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
        }
    });

    // Update checkAll status
    var $rows = $('.ddt-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;
    var allChecked = total > 0 && marked === total;
    $('#ddt_returnresult_chk_all').prop('checked', allChecked);
}

// Event handler cho checkbox để chỉ xử lý việc chọn multiple
$(document).on('change', '.ddt-returnresult-chk-service', function () {
    var $rows = $('.ddt-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Update trạng thái "chọn tất cả"
    var allChecked = total > 0 && marked === total;
    $('#ddt_returnresult_chk_all').prop('checked', allChecked);

    // Xử lý hiển thị chi tiết
    var selectedCheckboxes = $(".row-service .form-check-input:checked");

    if (selectedCheckboxes.length === 1) {
        // Chỉ 1 được chọn → hiển thị chi tiết
        var selectedId = selectedCheckboxes.first().val();
        ReturnResult_LoadImageForService(selectedId);

        var $row = selectedCheckboxes.first().closest('.row-service');
        var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
        ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
    } else if (selectedCheckboxes.length > 1) {
        // Nhiều hơn 1 → clear chi tiết
        const $containHinh = $('.contain-hinh');
        $containHinh.empty();
        CKEDITOR.instances["ddt_returnresult_description"].setData("");
        $("#ddt_returnresult_result").val("");
        $("#ddt_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    } else {
        // Không có gì được chọn → clear
        const $containHinh = $('.contain-hinh');
        $containHinh.empty();
        CKEDITOR.instances["ddt_returnresult_description"].setData("");
        $("#ddt_returnresult_result").val("");
        $("#ddt_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    }
});
function ReturnResult_LoadImageForService(id) {
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["ddt_returnresult_description"].setData("");
    $("#ddt_returnresult_result").val("");
    $("#ddt_returnresult_suggest").val("");
    $.ajax({
        url: "/DDT_ReturnResult/Get_Description_Result_Suggest_ForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                if (value.id) {
                    const $hinh = $('<div class="hinh"></div>');
                    $hinh.append('<img id="' + value.id + '" src="' + value.name + '" onclick="ReturnResult_Hinh(' + value.id + ', this)"/>');
                    $containHinh.append($hinh);
                }
                CKEDITOR.instances["ddt_returnresult_description"].setData(value.description);
                $("#ddt_returnresult_result").val(value.result);
                $("#ddt_returnresult_suggest").val(value.suggest);
            });
        }
    });
}

// Helper function to toggle digital signature buttons
function ReturnResult_ToggleDigitalSignButtons(isDigitallySigned) {
    console.log('Digital signature status:', isDigitallySigned);

    // Toggle "Tải PDF đã ký" button
    var $printSignedBtn = $('#ddt_returnresult_print_signed');
    if ($printSignedBtn.length) {
        if (isDigitallySigned) {
            $printSignedBtn.show().prop('disabled', false);
        } else {
            $printSignedBtn.hide().prop('disabled', true);
        }
    }

    // Toggle "Invalid & Xóa Ký Số" button  
    var $invalidRemoveSignBtn = $('#ddt_returnresult_invalid_removedigitalsign');
    if ($invalidRemoveSignBtn.length) {
        if (isDigitallySigned) {
            $invalidRemoveSignBtn.show().prop('disabled', false);
        } else {
            $invalidRemoveSignBtn.hide().prop('disabled', true);
        }
    }
}
function ReturnResult_Hinh(imageCDHAId, img) {
    $('.contain-hinh img').removeClass('selected');
    $(img).toggleClass('selected');
    $("#ddt_returnresult_xem").val(imageCDHAId);
}

function ReturnResult_ViewSignedPdf() {
    var signStoreId = $('#ddt_returnresult_signStoreId').val();

    function openSigned(id) {
        if (!id) { SwalHelper.Toast.warning("Chưa có signStoreId cho dịch vụ này!"); return; }
        var url = "/api/ExternalSign/view-signed/" + encodeURIComponent(id);
        window.open(url, "_blank");
    }

    if (signStoreId && $.trim(signStoreId) !== "") {
        openSigned(signStoreId);
        return;
    }

    // Nếu input rỗng → lấy dịch vụ đang chọn (giống ReturnResult_Print)
    var resultId = "";
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            if ($(this).is(':checked')) {
                resultId = $(this).val();
            }
        });
    });
    console.log(resultId);
    if (!resultId) {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ để xem PDF đã ký!");
        return;
    }

    // Gọi API của bạn để lấy signStoreId đã lưu trong DB theo resultId
    $.ajax({
        url: "/DDT_ReturnResult/GetSignStoreId",
        type: "GET",
        data: { resultId: resultId },
        dataType: "json",
        success: function (res) {
            console.log(res);
            var id = (res && (res.signStoreId || res.id)) ? (res.signStoreId || res.id) : "";
            if (id) {
                openSigned(id);
            } else {
                SwalHelper.Toast.warning("Chưa lưu signStoreId cho dịch vụ này.");
            }
        },
        error: function () {
            SwalHelper.Toast.error("Không lấy được signStoreId. Vui lòng kiểm tra!");
        }
    });
}
// ========================== KÝ SỐ PDF CHO TAB ĐÃ XONG (RETURN RESULT) ==========================
function ReturnResult_SignPdf_Multi(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    var patientId = $('#ddt_returnresult_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Thu thập danh sách resultIds theo mode
    var resultIds = [];
    if (mode === 'current') {
        // lấy id của dòng đang active (checkbox đang checked đầu tiên)
        var rid = $(".row-service .form-check-input:checked").first().val();
        if (rid) resultIds.push(rid);
    } else if (mode === 'selected') {
        $(".row-service .form-check-input:checked").each(function () {
            var v = $(this).val();
            if (v) resultIds.push(v);
        });
    } else if (mode === 'all') {
        $(".row-service").each(function () {
            var v = $(this).find(".form-check-input").first().val();
            if (v) resultIds.push(v);
        });
    }

    if (resultIds.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn ít nhất 1 dịch vụ để ký.");
        return;
    }

    // Lấy CCCD người ký (bác sĩ) - cần có input field tương ứng trong tab ReturnResult
    var signerCCCD = $('#ddt_returnresult_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#ddt_returnresult_signerCCCD').focus();
        return;
    }

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#ddt_returnresult_patientId').val() || $('#ddt_returnresult_sid').val() || "";
    var patientMaBenhAn = $('#ddt_returnresult_maBenhAn').val() || $('#ddt_returnresult_sid').val() || "";
    var patientName = $('#ddt_returnresult_patientName').val() || "";
    var doctorName = $('#ddt_returnresult_userReturnResultDDT option:selected').text() || $('#ddt_returnresult_userReturnResultDDT').val() || "";
    var doctorId = $('#ddt_returnresult_userLoginId').val();
    var performedAt = $('#ddt_returnresult_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "DoDienTim"; // bạn có thể thay động tùy màn hình

    console.log("Chuẩn bị ký số cho các resultCDHAId:", resultIds);
    console.log(patientCode);
    console.log(patientMaBenhAn);
    console.log(patientName);
    console.log(doctorName);
    console.log(doctorId);
    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tải file PDF...'); } catch (e) { }

    // PHASE 1: EXPORT tất cả PDF thô từ các resultCDHAId được chọn
    var pdfJobs = [];  // [{resultCDHAId, base64Pdf}]
    var chain = Promise.resolve();

    console.log("Tổng ID dịch vụ cần ký: ", resultIds);

    // Export PDF cho từng dịch vụ
    resultIds.forEach(function (rid) {
        chain = chain.then(function () {
            return new Promise(function (resolve) {
                // Gọi endpoint để lấy PDF đã lưu của dịch vụ này
                $.ajax({
                    url: "/DDT_ReturnResult/Print?resultCDHAId=" + rid,
                    type: "GET",
                    dataType: "text",
                    success: function (base64Pdf) {
                        console.log(rid, "====> PDF base64 length:", base64Pdf ? base64Pdf.length : 0);
                        if (base64Pdf) {
                            pdfJobs.push({
                                resultCDHAId: rid,
                                base64Pdf: base64Pdf
                            });
                        }
                        // dù có hay không vẫn resolve để chạy dịch vụ kế tiếp
                        resolve();
                    },
                    error: function () {
                        // lỗi 1 dịch vụ thì bỏ qua, tiếp tục các dịch vụ khác
                        console.error("Lỗi lấy PDF cho resultCDHAId:", rid);
                        resolve();
                    }
                });
            });
        });
    });

    // PHASE 2: Gửi tất cả PDF lên server ký số
    chain = chain.then(function () {
        if (pdfJobs.length === 0) {
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.warning("Không tạo được file PDF nào để ký.");
            return;
        }

        startCountdown(120);
        var formData = new FormData();
        formData.append("signerCCCD", signerCCCD);
        formData.append("formType", formType);
        formData.append("patientCode", patientCode);
        formData.append("patientMaBenhAn", patientMaBenhAn);
        formData.append("patientName", patientName);
        formData.append("doctorName", doctorName);
        if (performedAt) {
            formData.append("performedAt", performedAt);
        }

        // Thêm từng file & id tương ứng
        pdfJobs.forEach(function (job, idx) {
            var file = base64ToFile(job.base64Pdf, "temp.pdf", "application/pdf");
            formData.append("files", file, file.name);          // gửi nhiều file
            formData.append("resultIds", job.resultCDHAId);     // cùng thứ tự
        });

        return new Promise(function (resolve) {
            $.ajax({
                url: "/api/ExternalSign/sign-pdf-multi",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (resp) {
                    console.log("SAU KHI KÝ: ", resp);

                    // Normalize response về mảng
                    var items = [];
                    if (resp) {
                        if (Array.isArray(resp.items)) {
                            items = resp.items.filter(it => it && it.data && it.data.id);
                        } else if (resp.item && resp.item.data && resp.item.data.id) {
                            items = [resp.item];
                        } else if (resp.data && resp.data.id) {
                            items = [resp];
                        }
                    }

                    console.log("Các item đã ký thành công: ", items);
                    if (!items || items.length === 0) {
                        resolve();
                        $('#showWaitting').modal('hide');
                        var messageError = resp?.items[0]?.error || "";
                        SwalHelper.Toast.error(`Xảy ra lỗi khi ký số: ${messageError}`);
                        return;
                    }

                    // PHASE 3: Lưu thông tin ký số cho từng dịch vụ
                    var saveChain = Promise.resolve();

                    items.forEach(function (it) {
                        saveChain = saveChain.then(function () {
                            return new Promise(function (resolveSave) {
                                var resultId = it.resultCDHAId || it.referenceId || null;
                                var objDataResponse = it.data;
                                var signStoreId =
                                    (it && it.signStoreId) ||
                                    (it && it.id) ||
                                    (it && it.data && (it.data.signStoreId || it.data.id)) ||
                                    null;

                                if (!signStoreId || !resultId) {
                                    resolveSave();
                                    return;
                                }

                                console.log("Lưu signStoreId cho resultId:", resultId, "=>", signStoreId);

                                // Lưu signStoreId theo dịch vụ
                                $.ajax({
                                    url: "/DDT_ReturnResult/SaveSignStoreIdForResultCDHA",
                                    type: "POST",
                                    dataType: "text",
                                    data: {
                                        resultCDHAId: resultId,
                                        signStoreId: signStoreId
                                    },
                                    success: function (resText) {
                                        try {
                                            var res = JSON.parse(resText);
                                            console.log("Response SaveSignStoreId: ", res);
                                            if (res && res.success) {
                                                var keyResult = res.keyResult;
                                                var statusNum = (objDataResponse && (objDataResponse.status === 1 || objDataResponse.status === "1")) ? 1 : 0;

                                                // Lưu vào bảng Digital_Sign
                                                var payload = {
                                                    referenceType: formType ?? "DoDienTim",
                                                    referenceKeyResult: keyResult,
                                                    signId: Number(signStoreId),
                                                    signUserId: signerCCCD,
                                                    taxCode: objDataResponse.taxcode || objDataResponse.taxCode || null,
                                                    targetText: doctorName,
                                                    requestUrl: window.location.origin + "/api/ExternalSign/sign-pdf-multi",
                                                    responseData: JSON.stringify(objDataResponse),
                                                    status: statusNum
                                                };

                                                $.ajax({
                                                    url: "/DigitalSign/Save",
                                                    type: "POST",
                                                    data: JSON.stringify(payload),
                                                    contentType: "application/json; charset=utf-8",
                                                    complete: function (r) {
                                                        console.log("Lưu DigitalSign hoàn tất: ", r);
                                                        resolveSave();
                                                    }
                                                });
                                            } else {
                                                resolveSave();
                                            }
                                        } catch (e) {
                                            console.error("Parse error:", e);
                                            resolveSave();
                                        }
                                    },
                                    error: function () {
                                        console.error("Lỗi SaveSignStoreId");
                                        resolveSave();
                                    }
                                });
                            });
                        });
                    });

                    saveChain.then(function () {
                        $('#showWaitting').modal('hide');
                        SwalHelper.Toast.success(`Đã ký số thành công ${items.length} kết quả`);
                        ReturnResult_Refresh();
                        ReturnResult_Get_Count();
                        resolve();
                    });
                },
                error: function (xhr) {
                    $('#showWaitting').modal('hide');
                    console.log(xhr);
                    var msg = "Ký số thất bại.";
                    if (xhr && xhr.responseJSON) msg += "\n" + xhr.responseJSON.message;
                    SwalHelper.Toast.error(msg);
                    resolve();
                }
            });
        });
    });
}
function ReturnResult_SaveDateToSession() {
    var fromDate = document.getElementById('ddt_returnresult_timeSearchFrom').value;
    var toDate = document.getElementById('ddt_returnresult_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/DDT_ReturnResult/SaveSearchDates',
            type: 'POST',
            data: {
                timeSearchFrom: fromDate,
                timeSearchTo: toDate
            },
            success: function (result) {
                // Optionally handle success
            }
        });
    }
}

// Tick/untick tất cả cho ReturnResult
$(document).on('change', '#ddt_returnresult_chk_all', function () {
    var checked = this.checked === true;
    // chỉ tick các dịch vụ đang hiển thị
    $('.ddt-returnresult-chk-service').prop('checked', checked);

    // Nếu check all thì load kết quả của dịch vụ đầu tiên
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["ddt_returnresult_description"].setData("");
    $("#ddt_returnresult_result").val("");
    $("#ddt_returnresult_suggest").val("");
    ReturnResult_ToggleDigitalSignButtons(false);
});

// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả" cho ReturnResult
$(document).on('change', '.ddt-returnresult-chk-service', function () {
    var $rows = $('.ddt-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Nếu tất cả đều check => check header; ngược lại bỏ check header
    var allChecked = total > 0 && marked === total;
    $('#ddt_returnresult_chk_all').prop('checked', allChecked);
});

function startCountdown(durationInSeconds) {
    let remainingTime = durationInSeconds;

    const interval = setInterval(() => {
        // Tính toán phút và giây còn lại
        let minutes = Math.floor(remainingTime / 60);
        let seconds = remainingTime % 60;

        // Cập nhật nội dung
        updateLoadingText(`Đang chờ ký số (${minutes} phút ${seconds < 10 ? '0' : ''}${seconds} giây)...`);

        // Giảm thời gian còn lại
        remainingTime--;

        // Dừng đếm ngược khi hết thời gian
        if (remainingTime < 0) {
            clearInterval(interval);
            updateLoadingText("Ký số hoàn tất!");
        }
    }, 1000);
}