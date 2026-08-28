// Tool tip cho control của form
//$(function () {
//    $('[data-toggle="tooltip"]').tooltip()
//})

// *********************************************************************************** Select 2
$(function () {
    GetSample_SetSelect2_01(true);
})
$(function () {
    if (typeof window.BarcodeScan === 'undefined') return;

    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#xn_getsample_pidorseq', window.GetSample_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#xn_process_pidorseq', window.Process_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#xn_returnresult_pidorseq', window.ReturnResult_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Tự động gắn cho các input có class .barcode-input (dùng chung nhiều màn hình)
    window.BarcodeScan.autowire('.barcode-input');
});

// *********************************************************************************** Gắn hotkey Enter cho tìm kiếm
$(document).ready(function() {
    // Gắn Enter key cho các input field tìm kiếm của XN_Process
    $('#xn_process_pidorseq, #xn_process_timeSearchFrom, #xn_process_timeSearchTo, #xn_process_maDotKham').on('keydown', function(e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            Process_Search();
        }
    });
    
    // Gắn Enter key cho các input field tìm kiếm của GetSample
    $('#xn_getsample_pidorseq, #xn_getsample_timeSearchFrom, #xn_getsample_timeSearchTo').on('keydown', function(e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            GetSample_Search();
        }
    });
    
    // Gắn Enter key cho các input field tìm kiếm của ReturnResult
    $('#xn_returnresult_pidorseq, #xn_returnresult_timeSearchFrom, #xn_returnresult_timeSearchTo').on('keydown', function(e) {
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
            if ($('.xn-left-header').length === 0) {
                var header = $('<div class="xn-left-header">' +
                    '<div class="xn-left-header-title">Danh sách bệnh nhân</div>' +
                    '<button class="xn-left-header-close"><i class="bi bi-x-lg"></i></button>' +
                    '</div>');
                $('.xn-left').prepend(header);
            }
        } else {
            // Xóa các elements mobile trên desktop
            $('.mobile-toggle-patient-list').remove();
            $('.mobile-patient-list-overlay').remove();
            $('.xn-left-header').remove();
            $('.xn-left').removeClass('show');
        }
    }

    // Khởi tạo khi load trang
    initMobilePatientList();

    // Click vào nút toggle
    $(document).on('click', '.mobile-toggle-patient-list', function () {
        $('.xn-left').addClass('show');
        $('.mobile-patient-list-overlay').addClass('show');
        $(this).addClass('active');
        // Prevent scroll trên body
        $('body').css('overflow', 'hidden');
    });

    // Click vào nút đóng trong header
    $(document).on('click', '.xn-left-header-close', function () {
        closePatientList();
    });

    // Click vào overlay để đóng
    $(document).on('click', '.mobile-patient-list-overlay', function () {
        closePatientList();
    });

    // Hàm đóng danh sách
    function closePatientList() {
        $('.xn-left').removeClass('show');
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

    $(document).on('touchstart', '.xn-left', function (e) {
        if (window.innerWidth <= 768) {
            startX = e.touches[0].clientX;
            isDragging = true;
        }
    });

    $(document).on('touchmove', '.xn-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            currentX = e.touches[0].clientX;
            var diff = currentX - startX;

            // Chỉ cho phép swipe sang trái
            if (diff < 0) {
                $(this).css('transform', 'translateX(' + diff + 'px)');
            }
        }
    });

    $(document).on('touchend', '.xn-left', function (e) {
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
function GetSample_SetSelect2_02() {
    $(document).ready(function () {
        $('#xn_getsample_category').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn danh mục --"
        });
    });
    $(document).ready(function () {
        $('#xn_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#xn_getsample_category').val('');
    $('#xn_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#xn_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#xn_getsample_location').select2({
            placeholder: "-- Chọn --"
        });
    });

    if (isLoadPage) {
        $('#xn_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#xn_getsample_doctor').select2({
            placeholder: "-- Chọn --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#xn_process_userReturnResultXN').select2({
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
                if (e.id === "xn_getsample_location") {
                    $('#xn_getsample_location').select2('focus');
                }
                else if (e.id === "xn_getsample_doctor") {
                    $('#xn_getsample_doctor').select2('focus');
                }
                else if (e.id === "xn_getsample_userReturnResultXN") {
                    $('#xn_getsample_userReturnResultXN').select2('focus');
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
        url: "/XN_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#xn_getsample_timeSearchFrom").val(today);
            $("#xn_getsample_timeSearchTo").val(today);
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
        url: "/XN_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#xn_getsample_timeSearchFrom").val(today);
            $("#xn_getsample_timeSearchTo").val(today);
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Search() {
    var xn_getsample_pidorseq = $("#xn_getsample_pidorseq").val();
    var timeSearchFrom = $("#xn_getsample_timeSearchFrom").val();
    var timeSearchTo = $("#xn_getsample_timeSearchTo").val();
    $.ajax({
        url: "/XN_GetSample/Search?" + "pidorseq=" + xn_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
$("#xn_getsample_savepatient").hide();
$("#xn_getsample_cancelpatient").hide();
function GetSample_HideButton(_new, _save, _delete, _cancel, _addservice, _getsample) {
    if (_new == 1) {
        $("#xn_getsample_newpatient").hide();
    }
    else {
        $("#xn_getsample_newpatient").show();
    }

    if (_save == 1) {
        $("#xn_getsample_savepatient").hide();
    }
    else {
        $("#xn_getsample_savepatient").show();
    }

    if (_delete == 1) {
        $("#xn_getsample_deletepatient").hide();
    }
    else {
        $("#xn_getsample_deletepatient").show();
    }

    if (_cancel == 1) {
        $("#xn_getsample_cancelpatient").hide();
    }
    else {
        $("#xn_getsample_cancelpatient").show();
    }

    if (_addservice == 1) {
        $("#xn_getsample_addService").hide();
    }
    else {
        $("#xn_getsample_addService").show();
    }

    if (_getsample == 1) {
        $("#xn_getsample_processresult").hide();
    }
    else {
        $("#xn_getsample_processresult").show();
    }
}


function GetSample_GetPatientInfo(id) {
    GetSample_HideButton(false, true, false, true, false, false);
    $.ajax({
        url: "/XN_GetSample/GetPatientInfo?id=" + id,
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
        url: "/XN_GetSample/GetServiceForPatient?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#xn-right-service-gridview").html(result);
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
    $("#xn_getsample_pidorseq").val('');
    $('#xn_getsample_id').val('');
    $('#xn_getsample_patientId').val('');
    $('#xn_getsample_seq').val('');
    $('#xn_getsample_sid').val('');
    $('#xn_getsample_patientName').val('');
    $('#xn_getsample_age').val('');
    $('#xn_getsample_sex').val('');
    $('#xn_getsample_obj').val('');
    $('#xn_getsample_type').val('');
    $('#xn_getsample_location').val('');
    $('#xn_getsample_doctor').val('');
    $('#xn_getsample_getSampleTime').val(dateTime);
    $('#xn_getsample_address').val('');
    $('#xn_getsample_diagnostic').val('');
    $('#xn_getsample_hospital').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
    GetSample_SetSelect2_01(false);
}

function GetSample_NewPatient() {
    $('#xn_getsample_patientId').focus();
    GetSample_HideButton(true, false, true, false, true, true);
    GetSample_ResetInput();
}


function GetSample_DeletePatient() {
    var id = $('#xn_getsample_id').val();
    if (id == '') {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var choice = confirm("Bạn muốn xoá bệnh nhân và tất cả chỉ định xét nghiệm?");
        if (choice) {
            $.ajax({
                url: "/XN_GetSample/DeletePatientAndService?id=" + id,
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
    var id = $('#xn_getsample_id').val();
    if (id == '') {
        GetSample_ResetInput()
    }
    else {
        GetSample_GetPatientInfo(id);
    }
}

function GetSample_GetSID(seq) {
    $.ajax({
        url: "/XN_GetSample/GetSID?seq=" + seq,
        type: "GET",
        dataType: "text",
        cache: false,
        success: function (result) {
            $('#xn_getsample_sid').val(result);
        }
    });
}

function GetSample_SavePatient() {
    var validate = GetSample_ValidateInput('patientInfo');
    if (validate) {
        var id = $('#xn_getsample_id').val();
        var patientId = $('#xn_getsample_patientId').val();
        var seq = $('#xn_getsample_seq').val();
        var sid = $('#xn_getsample_sid').val();
        var patientName = $('#xn_getsample_patientName').val();
        var age = $('#xn_getsample_age').val();
        var sex = $('#xn_getsample_sex').val();
        var obj = $('#xn_getsample_obj').val();
        var type = $('#xn_getsample_type').val();
        var location = $('#xn_getsample_location').val();
        var doctor = $('#xn_getsample_doctor').val();
        var getSampleTime = $('#xn_getsample_getSampleTime').val();
        var address = $('#xn_getsample_address').val();
        var diagnostic = $('#xn_getsample_diagnostic').val();
        var hospital = $('#xn_getsample_hospital').val();
        var category = $('#select-category').val();
        var service = $('#select-service').val();

        $.ajax({
            url: "/XN_GetSample/SavePatient?id= " + id + "&&patientId=" + patientId + "&&seq=" + seq + "&&sid=" + sid + "&&patientName=" + patientName + "&&age=" + age + "&&sex=" + sex + "&&obj=" + obj + "&&type=" + type + "&&location=" + location + "&&doctor=" + doctor + "&&getSampleTime=" + getSampleTime + "&&address=" + address + "&&diagnostic=" + diagnostic + "&&service=" + service + "&&hospital=" + hospital,
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
    var id = $('#xn_getsample_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var getSampleTime = $('#xn_getsample_getSampleTime').val();
        $.ajax({
            url: "/XN_GetSample/ProcessResult?id= " + id + "&getSampleTime=" + getSampleTime,
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
    var id = $('#xn_getsample_id').val();
    if (id === "") {
        $('#addServiceForm').modal('hide');
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        GetSample_SetSelect2_02();
    }
}

function GetSample_DeleteServiceForPatient(idResultXN) {
    var patientId = $('#xn_getsample_id').val();
    $.ajax({
        url: "/XN_GetSample/DeleteServiceForPatient?id=" + idResultXN,
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


function GetSample_LoadService(categoryId) {
    $('#xn_getsample_service').empty();
    $.ajax({
        url: "/XN_GetSample/GetServiceByCategory?categoryId=" + categoryId,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                $("#xn_getsample_service").append('<option value=' + value.id + '>' + value.name + '</option>');
                $("#xn_getsample_service").val('');
            });
        }
    });
}

function GetSample_AddServiceForPatient() {
    var serviceId = $('#xn_getsample_service').val();
    var patientId = $('#xn_getsample_id').val();
    var doctorId = $('#xn_getsample_doctor').val();
    if (serviceId === "" || patientId === "") {
        SwalHelper.Toast.warning("Chỉ định dịch vụ không thành công. Vui lòng kiểm tra lại !")
    }
    else {
        $.ajax({
            url: "/XN_GetSample/AddServiceForPatient?patientId=" + patientId + "&&serviceId=" + serviceId + "&&doctorId=" + doctorId,
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
    }
}

function GetSample_Get_Count() {
    $.ajax({
        url: "/XN_GetSample/Get_Count/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            var arrayResult = result.split(';');
            document.getElementById("countGetSample").innerHTML = arrayResult[0];
            document.getElementById("countProcessNotFullResult").innerHTML = arrayResult[1];
            document.getElementById("countProcessFullResult").innerHTML = arrayResult[2];
            document.getElementById("countReturnResult").innerHTML = arrayResult[3];
        }
    });
}

function GetSample_PrintSEQ(soBangIn) {
    var date = new Date();
    var datePrint = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
    var timePrint = ("0" + date.getHours()).slice(-2) + ":" + ("0" + date.getMinutes()).slice(-2);
    var patientId = $('#xn_getsample_patientId').val();
    var seq = $('#xn_getsample_seq').val();
    var patientName = $('#xn_getsample_patientName').val(); 
    var dob = $('#xn_getsample_age').val(); 
    var gender = $('#xn_getsample_sex').val();
    var getInsertTimeXN = $('#xn_getsample_insertTimeXN').val();
    var maDotKham = $('#xn_getsample_maDotKham').val();
    // Tách phần ngày ra khỏi phần thời gian
    var datePart = getInsertTimeXN.split('T')[0]; // "2025-10-13"

    // Tách các phần tử năm, tháng, ngày
    var [yyyy, mm, dd] = datePart.split('-');

    // Ghép lại theo định dạng ddMMyy
    var formattedDate = dd + mm + yyyy.slice(-2);


    console.log(patientId, seq, patientName, dob, gender, datePrint, timePrint, formattedDate, soBangIn);
    if (!patientId || patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    try { $('#showWaitting').modal('show'); } catch (e) { }
    if (soBangIn == 1) {
        $.ajax({
            url: "/XN_GetSample/PrintSEQTemp",
            type: "POST",
            dataType: "text",
            data: {
                seq: seq,
                patientId: patientId,
                patientName: patientName,
                dob: dob,
                gender: gender,
                datePrint: datePrint,
                timePrint: timePrint,
                getSampleTime: formattedDate,
                maDotKham: maDotKham,
                soBangIn: soBangIn * 2
            },
            success: function (response) {
                try { $('#showWaitting').modal('hide'); } catch (e) { }
                if (!response) { SwalHelper.Toast.error("In SEQ không thành công!"); return; }
                GetSample_ProcessResult();
                // mở PDF
                var byteCharacters = atob(response);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var file = new Blob([byteArray], { type: 'application/pdf;base64' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
            },
            error: function () {
                try { $('#showWaitting').modal('hide'); } catch (e) { }
                SwalHelper.Toast.error("Lỗi khi gọi API in SEQ!");
            }
        });
    } else {
        $.ajax({
            url: "/XN_GetSample/PrintSEQTemp2x2New",
            type: "POST",
            dataType: "text",
            data: {
                seq: seq,
                patientId: patientId,
                patientName: patientName,
                dob: dob,
                gender: gender,
                datePrint: datePrint,
                timePrint: timePrint,
                getSampleTime: formattedDate,
                maDotKham: maDotKham,
                soBangIn: soBangIn
            },
            success: function (response) {
                console.log(response);
                try { $('#showWaitting').modal('hide'); } catch (e) { }
                if (!response) { SwalHelper.Toast.error("In SEQ không thành công!"); return; }
                GetSample_ProcessResult();
                // mở PDF
                var byteCharacters = atob(response);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var file = new Blob([byteArray], { type: 'application/pdf;base64' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
            },
            error: function () {
                try { $('#showWaitting').modal('hide'); } catch (e) { }
                SwalHelper.Toast.error("Lỗi khi gọi API in SEQ!");
            }
        });
    }
}

function GetSample_SaveDateToSession() {
    var fromDate = document.getElementById('xn_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('xn_getsample_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/XN_GetSample/SaveSearchDates',
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
function GetSample_ProcessResultAll() {
    // Lấy tất cả các patient từ danh sách hiển thị
    var patientIds = [];
    $('#listPatient .list-group-item').each(function () {
        var onclickAttr = $(this).attr('onclick');
        if (onclickAttr && onclickAttr.includes('GetSample_GetPatientInfo')) {
            // Extract patient ID from onclick="GetSample_GetPatientInfo(123)"
            var matches = onclickAttr.match(/GetSample_GetPatientInfo\((\d+)\)/);
            if (matches && matches[1]) {
                patientIds.push(parseInt(matches[1]));
            }
        }
    });

    if (patientIds.length === 0) {
        SwalHelper.Toast.warning("Không có bệnh nhân nào để xử lý!");
        return;
    }
    console.log(patientIds);
    // Xác nhận với người dùng
    var confirmMessage = `Bạn có chắc chắn muốn xử lý kết quả cho tất cả ${patientIds.length} bệnh nhân?`;
    if (!confirm(confirmMessage)) {
        return;
    }

    // Hiển thị loading
    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: "/XN_GetSample/ProcessResultAll",
        type: 'POST',
        data: JSON.stringify(patientIds),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }

            if (result.success) {
                SwalHelper.Toast.success(`Xử lý thành công ${result.successCount}/${patientIds.length} bệnh nhân!`);
                GetSample_Refresh();
                GetSample_Get_Count();
            } else {
                SwalHelper.Toast.error(`Xử lý thất bại! ${result.message || 'Vui lòng kiểm tra lại!'}`);
            }
        },
        error: function () {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            SwalHelper.Toast.error("Xử lý kết quả không thành công. Vui lòng kiểm tra lại!");
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
                if (e.id === "xn_process_userReturnResultXN") {
                    $('#xn_process_userReturnResultXN').select2('focus');
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
        url: "/XN_Process/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#xn_process_timeSearchFrom").val(today);
            //$("#xn_process_timeSearchTo").val(today);

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
    var xn_process_pidorseq = $("#xn_process_pidorseq").val();
    var timeSearchFrom = $("#xn_process_timeSearchFrom").val();
    var timeSearchTo = $("#xn_process_timeSearchTo").val();
    var maDotKham = $("#xn_process_maDotKham").val();

    $.ajax({
        url: "/XN_Process/Search?" + "pidorseq=" + xn_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo + "&maDotKham=" + encodeURIComponent(maDotKham),
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_listPatient").html(result);
            Process_Get_Count();

            // Load danh sách MaDotKham sau khi refresh
            Process_LoadMaDotKhamList();
        },
        error: function () {
            $("#process_listPatient").empty();
        }
    });
}


// Thêm function d? load danh sách MaDotKham
function Process_LoadMaDotKhamList() {
    var timeSearchFrom = $("#xn_process_timeSearchFrom").val();
    var timeSearchTo = $("#xn_process_timeSearchTo").val();

    if (!timeSearchFrom || !timeSearchTo) return;

    $.ajax({
        url: "/XN_Process/GetMaDotKhamList?timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
        type: "GET",
        dataType: "json",
        cache: false,
        success: function (result) {
            var $select = $("#xn_process_maDotKham");
            var currentValue = $select.val(); // Luu giá tr? dang ch?n

            // Xóa các option cu (tr? option d?u tiên "-- T?t c? --")
            $select.find('option:not(:first)').remove();

            // Thêm các option m?i t? k?t qu?
            if (result && result.length > 0) {
                $.each(result, function (index, maDotKham) {
                    $select.append('<option value="' + maDotKham + '">' + maDotKham + '</option>');
                });
            }

            // Khôi ph?c giá tr? dã ch?n n?u v?n t?n t?i trong danh sách m?i
            if (currentValue && $select.find('option[value="' + currentValue + '"]').length > 0) {
                $select.val(currentValue);
            }
        },
        error: function () {
            console.error("Không th? t?i danh sách MaDotKham");
        }
    });
}

function Process_ResetInput() {
    $("#xn_process_pidorseq").val('');
    $('#xn_process_id').val('');
    $('#xn_process_patientId').val('');
    $('#xn_process_seq').val('');
    $('#xn_process_sid').val('');
    $('#xn_process_patientName').val('');
    $('#xn_process_age').val('');
    $('#xn_process_sex').val('');
    $('#xn_process_obj').val('');
    $('#xn_process_type').val('');
    $('#xn_process_location').val('');
    //$('#xn_process_doctor').val('');
    $('#xn_process_getSampleTime').val('');
    $('#xn_process_returnResultTime').val('');
    $('#xn_process_location').val('');
    //$('#xn_process_doctor').val('');
    $('#xn_process_userReturnResultXN').val('');
    $('#xn_process_address').val('');
    $('#xn_process_diagnostic').val('');
    $('#tbody-gridview-service').empty();
    Process_SetSelect2_03();
}

function Process_GetPatientInfo(id, patientId) {
    // Tiếp tục với chức năng gốc để load thông tin bệnh nhân vào panel chính
    $.ajax({
        url: "/XN_Process/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_patientInfo").html(result);
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            Process_SetSelect2_03();
            // Auto-chọn bác sĩ = user đang login (nếu chưa có giá trị)
            var loginId = $("#xn_process_userLoginId").val();
            var $sel = $("#xn_process_userReturnResultXN");
            if (loginId && (!$sel.val() || $sel.val() === "")) {
                $sel.val(loginId).trigger("change");
            }

            // Gắn handler đổi bác sĩ
            $(document)
                .off("change", "#xn_process_userReturnResultXN")
                .on("change", "#xn_process_userReturnResultXN", Process_Load_SelectedDoctorInfo);
            // Lần đầu nếu có sẵn value thì cũng load info
            Process_Load_SelectedDoctorInfo();
            Process_GetListServiceWithPreviousResults(id, patientId);
        },
        error: function () {
            $("#process_patientInfo").empty();
            $('#tbody-gridview-service').empty();
        }
    });
}

function Process_GetListServiceWithPreviousResults(id, patientId) {
    $.ajax({
        url: "/XN_Process/GetServiceAndLastestResultsForPatient?id=" + id + "&patientId=" + patientId,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_xn-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function Process_GetListServiceForPatient(id) {
    $.ajax({
        url: "/XN_Process/GetServiceForPatient?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_xn-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}


function Process_SaveResult() {
    var patientId = $('#xn_process_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var validate = Process_ValidateInput('process_patientInfo');
        if (validate) {
            var returnResultTime = $('#xn_process_returnResultTime').val();
            var userReturnResult = $('#xn_process_userReturnResultXN').val();
            var note = $('#xn_process_note').val();
            var DATA = [];
            $(".process-row-result").each(function () { // Lấy value trên từng Row
                var id = "";
                var status = "";
                var result = "";
                var validPrint = "";

                $(this).find(".td-id").each(function () {
                    id = $(this).html();
                })

                $(this).find(".btn-status").each(function () {
                    status = $(this).val();
                })

                $(this).find(".input-result").each(function () {
                    result = $(this).val();
                })

                $(this).find(".validPrint").each(function () {
                    validPrint = $(this).prop('checked');
                })

                DATA.push({ patientId: patientId, returnResultTime: returnResultTime, userReturnResult: userReturnResult, id: id, status: status, result: result, validPrint: validPrint, note: note });
            })

            $.ajax({
                url: "/XN_Process/SaveResult/",
                data: JSON.stringify(DATA),
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                type: "POST",
                success: function (result) {
                    if (result == 'True') {
                        SwalHelper.Toast.success("Lưu thành công.");
                        //Process_GetListServiceForPatient(patientId);
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
        else {
            return false
        }
    }
}

function Process_PrintWindow() {
    var patientId = $('#xn_process_id').val();
    var sid = $('#xn_process_sid').val();
    var returnResultTime = $('#xn_process_returnResultTime').val() || "";
    var userReturnResult = $('#xn_process_userReturnResultXN').val() || "";
    var note = $('#xn_process_note').val() || "";

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    var isValid = Process_ValidateInput('process_patientInfo');
    if (!isValid) return;

    var previewUrl = "/XN_Process/PreviewValidPrint"
        + "?patientId=" + encodeURIComponent(patientId)
        + "&sid=" + encodeURIComponent(sid || "")
        + "&returnResultTime=" + encodeURIComponent(returnResultTime)
        + "&userReturnResult=" + encodeURIComponent(userReturnResult)
        + "&note=" + encodeURIComponent(note);

    //var w = window.open("", "PreviewValidPrint", "width=1100,height=800");
    //if (!w) {
    //    alert("Trình duyệt đang chặn popup, hãy cho phép popup.");
    //    return;
    //}
    var printWindow = window.open(previewUrl, '_blank', 'width=1024,height=768,scrollbars=yes,resizable=yes');

    if (printWindow) {
        printWindow.focus();
    } else {
        SwalHelper.Toast.warning("Không thể mở cửa sổ in. Vui lòng kiểm tra trình duyệt chặn popup!");
    }
    //var html =
    //    '<!DOCTYPE html><html><head><meta charset="utf-8" />' +
    //    '<title>Xem trước kết quả xét nghiệm</title>' +
    //    '<style>body{margin:0;padding:0;display:flex;flex-direction:column;height:100vh;font-family:Segoe UI,Arial,sans-serif;}' +
    //    '.toolbar{padding:8px 12px;background:#f5f5f5;border-bottom:1px solid #ddd;display:flex;justify-content:flex-end;gap:8px;}' +
    //    'button{padding:6px 14px;border-radius:4px;border:1px solid transparent;cursor:pointer;font-size:13px;}' +
    //    '.btn-confirm{background:#0d6efd;border-color:#0d6efd;color:#fff;}' +
    //    '.btn-close{background:#6c757d;border-color:#6c757d;color:#fff;}' +
    //    'iframe{flex:1;width:100%;border:0;}' +
    //    '</style></head><body>' +
    //    '<div class="toolbar">' +
    //    '<button class="btn-confirm" id="btnConfirm">Xác nhận Valid &amp; In</button>' +
    //    '<button class="btn-close" id="btnClose">Đóng</button>' +
    //    '</div>' +
    //    '<iframe src="' + previewUrl + '"></iframe>' +
    //    '<script>' +
    //    'document.getElementById("btnConfirm").onclick=function(){' +
    //    ' if(window.opener && typeof window.opener.Process_ValidPrint==="function"){window.opener.Process_ValidPrint();}' +
    //    ' else{alert("Không tìm thấy màn hình chính để Valid & In.");}' +
    //    ' window.close();};' +
    //    'document.getElementById("btnClose").onclick=function(){window.close();};' +
    //    '<\/script>' +
    //    '</body></html>';

    //w.document.open();
    //w.document.write(html);
    //w.document.close();
}

function Process_ValidPrint() {
    var patientId = $('#xn_process_id').val();
    var sid = $('#xn_process_sid').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        $('#showWaitting').modal('show');
        var validate = Process_ValidateInput('process_patientInfo');
        if (validate) {
            var returnResultTime = $('#xn_process_returnResultTime').val();
            var userReturnResult = $('#xn_process_userReturnResultXN').val();
            var note = $('#xn_process_note').val();
            var DATA = [];
            $(".process-row-result").each(function () { // Lấy value trên từng Row
                var id = "";
                var status = "";
                var result = "";
                var validPrint = "";

                $(this).find(".td-id").each(function () {
                    id = $(this).html();
                })

                $(this).find(".btn-status").each(function () {
                    status = $(this).val();
                })

                $(this).find(".input-result").each(function () {
                    result = $(this).val();
                })

                $(this).find(".validPrint").each(function () {
                    validPrint = $(this).prop('checked');
                })

                DATA.push({ patientId: patientId, sid: sid, returnResultTime: returnResultTime, userReturnResult: userReturnResult, id: id, status: status, result: result, validPrint: validPrint, note: note });
            })

            $.ajax({
                url: "/XN_Process/ValidPrint/",
                data: JSON.stringify(DATA),
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
                    SwalHelper.Toast.error("Valid & In không thành công. Vui lòng kiểm tra lại!");
                }
            });
            return true;
        }
        else {
            return false
        }
    }
}

function Process_ValidNotFullResult() {
    var patientId = $('#xn_process_id').val();
    var sid = $('#xn_process_sid').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        $('#showWaitting').modal('show');
        var validate = Process_ValidateInput('process_patientInfo');
        if (validate) {
            var returnResultTime = $('#xn_process_returnResultTime').val();
            var userReturnResult = $('#xn_process_userReturnResultXN').val();
            var note = $('#xn_process_note').val();
            var DATA = [];
            $(".process-row-result").each(function () { // Lấy value trên từng Row
                var id = "";
                var status = "";
                var result = "";
                var validPrint = "";

                $(this).find(".td-id").each(function () {
                    id = $(this).html();
                })

                $(this).find(".btn-status").each(function () {
                    status = $(this).val();
                })

                $(this).find(".input-result").each(function () {
                    result = $(this).val();
                })

                $(this).find(".validPrint").each(function () {
                    validPrint = $(this).prop('checked');
                })

                DATA.push({ patientId: patientId, sid: sid, returnResultTime: returnResultTime, userReturnResult: userReturnResult, id: id, status: status, result: result, validPrint: validPrint, note: note });
            })

            $.ajax({
                url: "/XN_Process/ValidNotFullResultXN/",
                data: JSON.stringify(DATA),
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
                        SwalHelper.Toast.success("Valid thành công!");
                        //var byteCharacters = atob(response);
                        //var byteNumbers = new Array(byteCharacters.length);
                        //for (var i = 0; i < byteCharacters.length; i++) {
                        //    byteNumbers[i] = byteCharacters.charCodeAt(i);
                        //}
                        //var byteArray = new Uint8Array(byteNumbers);
                        //var file = new Blob([byteArray], { type: 'application/pdf;base64' });
                        //var fileURL = URL.createObjectURL(file);
                        //window.open(fileURL);
                    }
                },
                error: function () {
                    $('#showWaitting').modal('hide');
                    SwalHelper.Toast.error("Valid & In không thành công. Vui lòng kiểm tra lại!");
                }
            });
            return true;
        }
        else {
            return false
        }
    }
}


function Process_GetSample() {
    var id = $('#xn_process_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        $.ajax({
            url: "/XN_Process/GetSample?id= " + id,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result === 'True') {
                    Process_Refresh();
                    Process_Get_Count();
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
        url: "/XN_Process/Get_Count/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            console.log(result);
            var arrayResult = result.split(';');
            document.getElementById("countGetSample").innerHTML = arrayResult[0];
            document.getElementById("countProcessNotFullResult").innerHTML = arrayResult[1];
            document.getElementById("countProcessFullResult").innerHTML = arrayResult[2];
            document.getElementById("countReturnResult").innerHTML = arrayResult[3];
        }
    });
}

// Tải thông tin bác sĩ khi đổi select
function Process_Load_SelectedDoctorInfo() {
    var userId = $("#xn_process_userReturnResultXN").val();
    if (!userId) {
        $("#xn_process_signerCCCD").empty();
        $("#xnx_process_doctorInfo").empty();
        return;
    }
    $.ajax({
        url: "/XN_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#xn_process_signerCCCD").text("Không lấy được thông tin CCCD bác sĩ.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#xn_process_signerCCCD").val(cccd);
        },
        error: function () {
            $("#xn_process_signerCCCD").text("Không lấy đượcthông tin CCCD bác sĩ.");
            $("#xn_process_doctorInfo").text("Không lấy được thông tin bác sĩ.");
        },
    });
}

// ========================== KÝ SỐ PDF (Form: SIÊU ÂM) ==========================
function Process_SignPdf() {
    var patientId = $('#xn_process_id').val();
    var sid = $('#xn_process_sid').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Lấy CCCD người ký (bác sĩ)
    var signerCCCD = $('#xn_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#xn_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#xn_process_patientId').val() || $('#sa_process_sid').val() || "";
    var patientMaBenhAn = $('#xn_process_maBenhAn').val() || $('#xn_process_sid').val() || "";
    var patientName = $('#xn_process_patientName').val() || "";
    var doctorName = $('#xn_process_userReturnResultXN option:selected').text() || $('#xn_process_userReturnResultXN').val() || "";
    var performedAt = $('#xn_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn

    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tạo file PDF...'); } catch (e) { }
    var validate = Process_ValidateInput('process_patientInfo');
    if (validate) {
        var returnResultTime = $('#xn_process_returnResultTime').val();
        var userReturnResult = $('#xn_process_userReturnResultXN').val();
        var note = $('#xn_process_note').val();

        var DATA = [];
        $(".process-row-result").each(function () { // Lấy value trên từng Row
            var id = "";
            var status = "";
            var result = "";
            var validPrint = "";

            $(this).find(".td-id").each(function () {
                id = $(this).html();
            })

            $(this).find(".btn-status").each(function () {
                status = $(this).val();
            })

            $(this).find(".input-result").each(function () {
                result = $(this).val();
            })

            $(this).find(".validPrint").each(function () {
                validPrint = $(this).prop('checked');
            })

            DATA.push({ patientId: patientId, sid: sid, returnResultTime: returnResultTime, userReturnResult: userReturnResult, id: id, status: status, result: result, validPrint: validPrint, note: note });
        })
        console.log(DATA);
        $.ajax({
            url: "/XN_Process/ValidPrint/",
            data: JSON.stringify(DATA),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (base64Pdf) {
                if (!base64Pdf) {
                    $('#showWaitting').modal('hide');
                    SwalHelper.Toast.error("Không thể xuất PDF để ký. Vui lòng kiểm tra lại!");
                    return;
                }
                startCountdown(120);
                // 2) Chuyển base64 -> File để gửi multipart/form-data
                var file = base64ToFile(base64Pdf, "temp.pdf", "application/pdf");
                console.log(file);
                // 3) Chuẩn bị FormData gửi đến API ký số chung (server sẽ tự đặt tên file)
                var formData = new FormData();
                formData.append("signerCCCD", signerCCCD);
                formData.append("file", file, file.name);

                // ---- context để server đặt tên theo template MySign:FileNameTemplates (FormType = SieuAm)
                formData.append("formType", "XetNghiem");
                formData.append("patientCode", patientCode);
                formData.append("patientMaBenhAn", patientMaBenhAn);
                formData.append("patientName", patientName);
                formData.append("serviceCode", serviceCode);
                formData.append("doctorName", doctorName);
                if (performedAt) formData.append("performedAt", performedAt); // ISO/yyyy-MM-dd HH:mm:ss
                // formData.append("extra", "SA-Extra"); // nếu cần
                console.log(formData);
                $.ajax({
                    url: "/api/ExternalSign/sign-pdf",
                    data: formData,
                    processData: false,
                    contentType: false,
                    type: "POST",
                    success: function (resp) {
                        console.log(resp)
                        $('#showWaitting').modal('hide');
                        //resp = {
                        //    "id": "51626", "provider": "0", "documentId": "null", "documentType": "null", "documentName": "27433568_phieu_danh_gia_ban_dau.pdf", "documentDesc": "null", "documentHash": "7X38RRgfWpYULcvtZc863HnTfocDsSsji023UGoxBLM=", "timestamp": "2025-09-19T07:11:06.763046209Z", "transactionId": "308706a6-5dc7-4f0b-ba08-01448c15a306", "credentialId": "066194014120_7500452_20250821162101", "signatures": "SJpUV2IQVRR3FzMzK5WWLucKNX2jtFc4\/\/+rvi19Izj6yL1epn7xo1cxGhW\/67fI\r\niqLqz104zwke0xbWf9BZPjAEmSWD8Xn2RaouyGCBQIj4tnCnMZQGnPCBZ2laN4Ec\r\n3qjMU3lpoh22OnVBgLYZlQZSSvnNii1MF7v1vKRsdggl4YZpHEiZlgMef7iclkga\r\nMGUryw11teXAg77s15jGKYCjkM49jrJlGq+M5p0IVCj\/ZeUXByd3MsBeWAnWAHs1\r\n\/WoyEfBhwK3YABb37cjO6RNFACj+KJJv8910AyQwZeowyhmeQDdjLdGgfJRILORt\r\nDFZF1iABlsXIbp6pf0ek8w==", "status": "1", "filepath": "null", "url": "null", "docControlId": "null", "docControlType": "null", "error": "null", "login": "066194014120", "taxcode": "6001362081", "metadata": "null", "requesttime": "1758265850532", "contentContentType": "application\/pdf", "signUserId": "066194014120",
                        //}
                        // Controller trả về "response.data" nếu có; tùy MySign:
                        // - Nếu có resp.fileUrl -> mở luôn
                        // - Nếu có resp.base64Pdf -> mở PDF
                        // - Nếu là string -> thử parse
                        try {
                            var obj = (typeof resp === "string") ? JSON.parse(resp) : resp;

                            // 1) THU THẬP signStoreId TỪ RESPONSE (ưu tiên data.id)
                            var signStoreId =
                                (obj && obj.data && (obj.data.id || obj.data.signStoreId)) ||
                                obj.id || obj.signStoreId || null;
                            // giả sử signStoreId đang null để test dev nên gán cứng
                            signStoreId = signStoreId;
                            var keyResult = null;

                            console.log("payloadXN: ", signStoreId)
                            // 2) LƯU signStoreId THEO resultCDHAId ĐANG CHỌN
                            if (signStoreId) {
                                var ids = DATA.map(function (x) { return Number(x.id); });

                                $.ajax({
                                    url: "/XN_Process/SaveSignStoreIdForResultXN?signStoreId=" + encodeURIComponent(signStoreId),
                                    type: "POST",
                                    data: JSON.stringify(ids),
                                    contentType: "application/json; charset=utf-8",               // <- BẮT BUỘC
                                    dataType: "json",
                                    success: function (res) {
                                        var objResult = res;
                                        if (objResult.success) {
                                            keyResult = objResult.keyResult;

                                            // ====== GỌI CONTROLLER LƯU DIGITAL_SIGN ======
                                            var targetText = doctorName;
                                            var statusNum = (obj && (obj.status === 1 || obj.status === "1")) ? 1 : 0;

                                            var digitalSignPayload = {
                                                referenceType: "XetNghiem",                // SA / SieuAmTim / XQUANG...
                                                referenceKeyResult: keyResult,          // Id dịch vụ
                                                signId: signStoreId ? Number(signStoreId) : null,     // Id từ resp trả về
                                                signUserId: signerCCCD,                     // CCCD người ký
                                                taxCode: (obj.taxcode || obj.taxCode) || null,
                                                targetText: targetText,
                                                requestUrl: window.location.origin + "/api/ExternalSign/sign-pdf",
                                                responseData: JSON.stringify(obj),          // lưu toàn bộ JSON
                                                status: statusNum                            // 1 thành công, 0 thất bại
                                                // CreatorId/CreatedAt/Deleted: server tự set
                                            };
                                            console.log("digitalSignPayload: ", digitalSignPayload);
                                            $.ajax({
                                                url: "/DigitalSign/Save",                    // <-- endpoint lưu bảng Digital_Sign
                                                type: "POST",
                                                data: JSON.stringify(digitalSignPayload),
                                                contentType: "application/json; charset=utf-8",
                                                dataType: "json",
                                                success: function (r) {
                                                    openSigned(signStoreId);
                                                    Process_Refresh();
                                                    SwalHelper.Toast.success("Ký số thành công!");
                                                    console.log("Lưu DigitalSign thành công: ", r);
                                                },
                                                error: function (f) {
                                                    console.log("Lưu DigitalSign thất bại: ", f);
                                                }
                                            })
                                        }
                                    },
                                    error: function (xhr) {
                                        console.error("SaveSignStoreIdForResultXN error:", xhr.responseText);
                                    }
                                });
                            } else {
                                SwalHelper.Toast.error("Ký số THẤT BẠI. Đã Valid kết quả. Chọn Invalid kết quả và tiến hành ký số lại.");
                            }
                        } catch (e) {
                            if (isProbablyBase64(resp)) {
                                openBase64Pdf(resp);
                            } else {
                                SwalHelper.Toast.info("Đã ký số, phản hồi:\n" + resp);
                            }
                        }
                    },
                    error: function (xhr) {
                        $('#showWaitting').modal('hide');
                        var msg = "Ký số THẤT BẠI. Đã Valid kết quả. Chọn Invalid kết quả và tiến hành ký số lại.";
                        if (xhr && xhr.responseText) msg += "\n" + xhr.responseText;
                        SwalHelper.Toast.error(msg);
                    }
                });
            },
            error: function () {
                $('#showWaitting').modal('hide');
                SwalHelper.Toast.error("Xuất PDF không thành công. Vui lòng kiểm tra lại!");
            }
        });
        return true;
    }
    else {
        return false
    }
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
        var blob = new Blob([byteArray], { type: mime || 'application/pdf' });

        // Tên tạm – server sẽ tự đặt lại theo template nên không quan trọng
        var file = new File([blob], filename || "document.pdf", { type: mime || 'application/pdf' });
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

// ============================== IMPORT XÉT NGHIỆM NGOÀI (PDF) ==============================

// Mở modal + đổ patientId hiện chọn
var currentImportedExternalFileId = 0;
var currentImportedExternalFileName = '';
function Process_ShowImportModal() {
    var pId = $('#xn_process_patientId').val();
    var pidTablePatient = $('#xn_process_id').val();
    var pName = $('#xn_process_patientName').val();
    var pMaBenhAn = $('#xn_process_maBenhAn').val();
    //if (!pMaBenhAn || pMaBenhAn === "") {
    //    alert("Vui lòng chọn bệnh nhân trước khi import!");
    //    return;
    //}
    $('#import_patientId').val(pId);
    $('#import_idTablePatient').val(pidTablePatient);
    $('#import_patientName').val(pName);
    $('#import_maBenhAn').val(pMaBenhAn);

    // clear UI
    $('#import_pdf_file').val('');
    $('#pdfPreview').html('<em class="text-muted">Chưa có tệp được chọn…</em>');
    //$('#import_overwrite').prop('checked', false);
    //$('#import_mark_valid').prop('checked', false);
    // mở modal (nếu không dùng data-toggle)
    // tải danh sách PDF đã import
    Process_LoadImportedPdfList(pMaBenhAn, pId, pidTablePatient);
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
    console.log("Mã Bệnh Án: ", pMaBenhAn);
    console.log("pId: ", pId);
    console.log("pidTablePatient: ", pidTablePatient);
    $.get('/XN_Process/GetExternalLabFiles', { pMaBenhAn: pMaBenhAn, pId: pId, pidTablePatient: pidTablePatient }, function (res) {
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
        url: '/XN_Process/DeleteExternalLabFile',
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

    $.post('/XN_Process/SaveExternalLabFileVisibility', {
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
    fd.append('pid', pid);
    fd.append('pidTablePatient', pidTablePatient);
    fd.append('pName', pName);
    fd.append('file', fileInput.files[0]);
    fd.append('overwrite', overwrite);
    fd.append('markValid', markValid);
    fd.append('isVisibleToUser', isVisibleToUser);
    //fd.append('vendor', vendor);

    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: "/XN_Process/ImportExternalLabPdf",
        type: "POST",
        data: fd,
        processData: false,
        contentType: false,
        success: function (res) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            if (!res || res.success !== true) {
                SwalHelper.Toast.error(res && res.message ? res.message : "Import thất bại.");
                Process_LoadImportedPdfList(pMaBenhAn, pid);
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
            Process_LoadImportedPdfList(pMaBenhAn, pid);
            SwalHelper.Toast.error("Không thể lưu file PDF. Vui lòng thử lại!");
        }
    });
}

function CloseImportModal() {
    $('#modal-import-external').removeClass('show');
}

function Process_SaveDateToSession() {
    var fromDate = document.getElementById('xn_process_timeSearchFrom').value;
    var toDate = document.getElementById('xn_process_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/XN_Process/SaveSearchDates',
            type: 'POST',
            data: {
                timeSearchFrom: fromDate,
                timeSearchTo: toDate
            },
            success: function (result) {
                // Load l?i danh sách MaDotKham khi thay d?i kho?ng th?i gian
                Process_LoadMaDotKhamList();
            }
        });
    }
}

// Function to toggle all validPrint checkboxes
function Process_ToggleAllValidPrint(masterCheckbox) {
    var checkboxes = document.querySelectorAll('.validPrint');
    var isChecked = masterCheckbox.checked;

    checkboxes.forEach(function (checkbox) {
        checkbox.checked = isChecked;
    });
}

// Function to update the master checkbox state based on individual checkboxes
function Process_UpdateCheckAllState() {
    var checkboxes = document.querySelectorAll('.validPrint');
    var masterCheckbox = document.getElementById('checkAllValidPrint');
    var checkedCount = 0;

    checkboxes.forEach(function (checkbox) {
        if (checkbox.checked) {
            checkedCount++;
        }
    });

    if (checkedCount === 0) {
        masterCheckbox.checked = false;
        masterCheckbox.indeterminate = false;
    } else if (checkedCount === checkboxes.length) {
        masterCheckbox.checked = true;
        masterCheckbox.indeterminate = false;
    } else {
        masterCheckbox.checked = false;
        masterCheckbox.indeterminate = true;
    }
    console.log("Tổng checkbox Valid&In: ", checkedCount);
}
//***************************************************************************************** Return Result

function ReturnResult_Refresh() {
    $.ajax({
        url: "/XN_ReturnResult/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#xn_returnresult_timeSearchFrom").val(today);
            //$("#xn_returnresult_timeSearchTo").val(today);

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
    var xn_getsample_pidorseq = $("#xn_returnresult_pidorseq").val();
    var timeSearchFrom = $("#xn_returnresult_timeSearchFrom").val();
    var timeSearchTo = $("#xn_returnresult_timeSearchTo").val();
    $.ajax({
        url: "/XN_ReturnResult/Search?" + "pidorseq=" + xn_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#xn_returnresult_pidorseq").val('');
    $('#xn_returnresult_id').val('');
    $('#xn_returnresult_patientId').val('');
    $('#xn_returnresult_seq').val('');
    $('#xn_returnresult_sid').val('');
    $('#xn_returnresult_patientName').val('');
    $('#xn_returnresult_age').val('');
    $('#xn_returnresult_sex').val('');
    $('#xn_returnresult_obj').val('');
    $('#xn_returnresult_type').val('');
    $('#xn_returnresult_location').val('');
    $('#xn_returnresult_doctor').val('');
    $('#xn_returnresult_getSampleTime').val('');
    $('#xn_returnresult_returnResultTime').val('');
    $('#xn_returnresult_location').val('');
    $('#xn_returnresult_doctor').val('');
    $('#xn_returnresult_userReturnResultXN').val('');
    $('#xn_returnresult_address').val('');
    $('#xn_returnresult_diagnostic').val('');
    $('#tbody-gridview-service').empty();
}

function ReturnResult_GetPatientInfo(id) {
    $.ajax({
        url: "/XN_ReturnResult/GetPatientInfo?id=" + id,
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
    var userId = $("#xn_returnresult_userLoginId").val();
    if (!userId) {
        $("#xn_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/XN_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#xn_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#xn_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#xn_returnresult_signerCCCD").text("Không lấy đượcthông tin CCCD.");
        },
    });
}
function ReturnResult_GetListServiceForPatient(id) {
    $.ajax({
        url: "/XN_ReturnResult/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_xn-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function ReturnResult_Invalid() {
    var id = $('#xn_returnresult_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        if (confirm('Bạn muốn InValid kết quả của bệnh nhân ?')) {
            $.ajax({
                url: "/XN_ReturnResult/Invalid?id=" + encodeURIComponent(id),
                type: 'POST',
                dataType: 'text',
                success: function (result) {
                    if (result === 'True') {
                        //UpdateSignStatus(id);
                        ReturnResult_Refresh();
                        ReturnResult_Get_Count();
                    }
                    else {
                        SwalHelper.Toast.error("Không thể Invalid. Vui lòng kiểm tra lại!");
                    }
                },
                error: function (xhr) {
                    SwalHelper.Toast.error(xhr.responseText || "Không thể Invalid. Vui lòng kiểm tra lại!");
                }
            });
        }
    }
}

function ReturnResult_Invalid_RemoveDigitalSign() {
    var id = $('#xn_returnresult_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        if (confirm('Bạn muốn InValid kết quả của bệnh nhân ?')) {
            $.ajax({
                url: "/XN_ReturnResult/Invalid?id= " + id,
                type: 'POST',
                dataType: 'text',
                success: function (result) {
                    if (result === 'True') {
                        UpdateSignStatus(id);
                        ReturnResult_Refresh();
                        ReturnResult_Get_Count();
                    }
                    else {
                        SwalHelper.Toast.error("Không thể Invalid. Vui lòng kiểm tra lại!");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Không thể Invalid. Vui lòng kiểm tra lại!");
                }
            });
        }
    }
}

// ========================== KÝ SỐ PDF CHO RETURNRESULT (Ký PDF đã tạo trước đó) ==========================
function ReturnResult_SignPdf() {
    var patientId = $('#xn_returnresult_id').val();
    var sid = $('#xn_returnresult_sid').val();

    if (!patientId || patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Hiển thị modal để nhập CCCD người ký
    ReturnResult_ExecuteSign();
}

function ReturnResult_ExecuteSign() {
    var patientId = $('#xn_returnresult_id').val();
    var sid = $('#xn_returnresult_sid').val();
    var signerCCCD = $('#xn_returnresult_signerCCCD').val();
    var note = $('#sign_note').val();

    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký!");
        $('#xn_returnresult_signerCCCD').focus();
        return;
    }
    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tải file PDF...'); } catch (e) { }

    // Lấy context để backend đặt tên file
    var patientCode = $('#xn_returnresult_patientId').val() || '';
    var patientMaBenhAn = $('#xn_returnresult_maBenhAn').val() || $('#xn_returnresult_sid').val() || "";
    var patientName = $('#xn_returnresult_patientName').val() || '';
    var doctorName = $('#xn_returnresult_userReturnResultXN').val() || '';
    var returnResultTime = $('#xn_returnresult_returnResultTime').val() || '';

    // Hiển thị loading
    try { $('#showWaitting').modal('show'); } catch (e) { }

    // 1. Lấy PDF đã tạo trước đó từ server
    $.ajax({
        url: "/XN_ReturnResult/Print?patientId=" + patientId + "&sid=" + sid,
        dataType: "text",
        type: "POST",
        success: function (base64Pdf) {
            if (!base64Pdf || base64Pdf === "") {
                try { $('#showWaitting').modal('hide'); } catch (e) { }
                SwalHelper.Toast.error("Không thể lấy PDF để ký. Vui lòng kiểm tra lại!");
                return;
            }
            startCountdown(120);
            // 2. Chuyển base64 -> File để gửi multipart/form-data
            var file = base64ToFile(base64Pdf, "temp.pdf", "application/pdf");

            // 3. Chuẩn bị FormData gửi đến API ký số
            var formData = new FormData();
            formData.append("signerCCCD", signerCCCD);
            formData.append("file", file, file.name);

            // Context để server đặt tên theo template
            formData.append("formType", "XetNghiem");
            formData.append("patientCode", patientCode);
            formData.append("patientMaBenhAn", patientMaBenhAn);
            formData.append("patientName", patientName);
            formData.append("doctorName", doctorName);
            if (returnResultTime) formData.append("performedAt", returnResultTime);
            if (note) formData.append("extra", note);
            for (let pair of formData.entries()) {
                console.log(pair[0] + ':', pair[1]);
            }
            // 4. Gửi request ký số
            $.ajax({
                url: "/api/ExternalSign/sign-pdf",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (resp) {
                    console.log("respppp sign-pdf", resp);
                    try { $('#showWaitting').modal('hide'); } catch (e) { }

                    try {
                        var obj = (typeof resp === "string") ? JSON.parse(resp) : resp;

                        // Lấy signStoreId từ response
                        var signStoreId =
                            (obj && obj.data && (obj.data.id || obj.data.signStoreId)) ||
                            obj.id || obj.signStoreId || null;

                        console.log("ReturnResult signStoreId: ", signStoreId);

                        if (signStoreId) {
                            // Lấy danh sách ResultXN IDs từ màn hình hiện tại
                            var resultIds = [];
                            $('.process-row-result').each(function () {
                                var id = $(this).find('.td-id').text();
                                if (id) {
                                    resultIds.push(Number(id));
                                }
                            });

                            // Lưu signStoreId vào database
                            $.ajax({
                                url: "/XN_ReturnResult/SaveSignStoreIdForResultXN?signStoreId=" + encodeURIComponent(signStoreId),
                                type: "POST",
                                data: JSON.stringify(resultIds),
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function (res) {
                                    console.log("Save signStoreId result:", res);

                                    if (res.success) {
                                        var keyResult = res.keyResult;

                                        // Lưu thông tin ký số vào bảng Digital_Sign
                                        var targetText = doctorName || patientName;
                                        var statusNum = (obj && (obj.status === 1 || obj.status === "1")) ? 1 : 0;

                                        var digitalSignPayload = {
                                            referenceType: "XetNghiem",
                                            referenceKeyResult: keyResult,
                                            signId: signStoreId ? Number(signStoreId) : null,
                                            signUserId: signerCCCD,
                                            taxCode: (obj.taxcode || obj.taxCode) || null,
                                            targetText: targetText,
                                            requestUrl: window.location.origin + "/api/ExternalSign/sign-pdf",
                                            responseData: JSON.stringify(obj),
                                            status: statusNum
                                        };

                                        $.ajax({
                                            url: "/DigitalSign/Save",
                                            type: "POST",
                                            data: JSON.stringify(digitalSignPayload),
                                            contentType: "application/json; charset=utf-8",
                                            dataType: "json",
                                            success: function (r) {
                                                console.log("Lưu DigitalSign thành công: ", r);
                                                SwalHelper.Toast.success("Ký số thành công!");

                                                // Mở PDF đã ký
                                                ReturnResult_OpenSignedPdf(signStoreId);

                                                // Refresh danh sách
                                                ReturnResult_Refresh();
                                            },
                                            error: function (f) {
                                                console.log("Lưu DigitalSign thất bại: ", f);
                                                SwalHelper.Toast.error("Ký số thành công nhưng không lưu được thông tin. Vui lòng kiểm tra!");
                                            }
                                        });
                                    } else {
                                        SwalHelper.Toast.error("Ký số thành công nhưng không lưu được signStoreId!");
                                    }
                                },
                                error: function (xhr) {
                                    console.error("SaveSignStoreIdForResultXN error:", xhr.responseText);
                                    SwalHelper.Toast.error("Ký số thành công nhưng không lưu được signStoreId!");
                                }
                            });
                        } else {
                            SwalHelper.Toast.error("Ký số THẤT BẠI hoặc không nhận được signStoreId từ server.");
                        }
                    } catch (e) {
                        // Fallback: nếu response là base64 PDF thì mở luôn
                        if (isProbablyBase64(resp)) {
                            openBase64Pdf(resp);
                            SwalHelper.Toast.info("Ký số có thể thành công. Vui lòng kiểm tra file PDF.");
                        } else {
                            SwalHelper.Toast.info("Đã ký số, phản hồi:\n" + resp);
                        }
                    }
                },
                error: function (xhr) {
                    $('#showWaitting').modal('hide');
                    console.log(xhr);
                    var msg = "Ký số thất bại.";
                    if (xhr && xhr.responseJSON) msg += "\n" + xhr.responseJSON.message;
                    SwalHelper.Toast.error(msg);
                }
            });
        },
        error: function () {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            SwalHelper.Toast.error("Không thể lấy PDF để ký. Vui lòng kiểm tra lại!");
        }
    });
}

function ReturnResult_OpenSignedPdf(signStoreId) {
    if (!signStoreId) {
        SwalHelper.Toast.warning("Chưa có signStoreId!");
        return;
    }
    var url = "/api/ExternalSign/view-signed/" + encodeURIComponent(signStoreId);
    window.open(url, "_blank");
}

function ReturnResult_Get_Count() {
    $.ajax({
        url: "/XN_ReturnResult/Get_Count/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            var arrayResult = result.split(';');
            document.getElementById("countGetSample").innerHTML = arrayResult[0];
            document.getElementById("countProcessNotFullResult").innerHTML = arrayResult[1];
            document.getElementById("countProcessFullResult").innerHTML = arrayResult[2];
            document.getElementById("countReturnResult").innerHTML = arrayResult[3];
        }
    });
}

function ReturnResult_Print() {
    var idTable = $('#xn_returnresult_id').val();
    var sid = $('#xn_returnresult_sid').val();


    var patientId = $('#xn_returnresult_patientId').val();
    var maBenhAn = $('#xn_returnresult_maBenhAn').val();
    var patientName = $('#xn_returnresult_patientName').val() || 'BenhNhan';

    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }
    $('#showWaitting').modal('show');
    $.ajax({
        url: "/XN_ReturnResult/Print?patientId=" + idTable + "&&sid=" + sid,
        dataType: "text",
        type: "POST",
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

                // ===== XỬ LÝ CHO CẢ DESKTOP VÀ MOBILE =====

                // Kiểm tra thiết bị
                var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);

                if (isMobile) {
                    // MOBILE: Tạo link download
                    var link = document.createElement('a');
                    link.href = URL.createObjectURL(blob);
                    link.download = 'KetQua_XetNghiem_' + idTable + '_' + new Date().getTime() + '.pdf';
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
                    // DESKTOP: Mở trong tab mới
                    var fileURL = URL.createObjectURL(blob);
                    var newWindow = window.open(fileURL, '_blank');

                    if (!newWindow) {
                        // Nếu popup bị chặn, fallback sang download
                        var link = document.createElement('a');
                        link.href = fileURL;
                        link.download = 'KetQua_XetNghiem_' + resultCDHAId + '.pdf';
                        link.click();
                        URL.revokeObjectURL(fileURL);
                        SwalHelper.Toast.info('File PDF đã được tải xuống do popup bị chặn!');
                    }
                }

            } catch (error) {
                console.error('Lỗi xử lý PDF:', error);
                SwalHelper.Toast.error('Không thể hiển thị file PDF. Vui lòng thử lại!');
            }
        },
        error: function (xhr, status, error) {
            $('#showWaitting').modal('hide');
            console.error('AJAX Error:', status, error);
            SwalHelper.Alert.error('Lỗi', 'In không thành công. Vui lòng kiểm tra lại!');
        }
    });
}

function ReturnResult_ViewSignedPdf() {
    var signStoreId = $('#xn_returnresult_signStoreId').val();

    function openSigned(id) {
        if (!id) { SwalHelper.Toast.warning("Chưa có signStoreId cho dịch vụ này!"); return; }
        var url = "/api/ExternalSign/view-signed/" + encodeURIComponent(id);
        window.open(url, "_blank");
    }

    if (signStoreId && $.trim(signStoreId) !== "") {
        openSigned(signStoreId);
        return;
    }

    // Nếu input rỗng → Chọn theo PatientId 
    var patientId = $("#xn_returnresult_id").val(); // patientId này là cột id của bảng Patient chứ không phải cột PatientId

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân để xem in lại kết quả!");
        return;
    }
    console.log(patientId);
    // Gọi API của bạn để lấy signStoreId đã lưu trong DB theo resultId
    $.ajax({
        url: "/XN_ReturnResult/GetSignStoreId",
        type: "GET",
        data: { patientId: patientId },
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

function UpdateSignStatus(patientId) {
    $.ajax({
        url: "/DigitalSign/UpdateSignStatus_ResultXN?patientId=" + patientId,
        type: 'POST',
        dataType: 'text',
        success: function (res) {
            console.log("Update Status Sign: ", res);
            var response = JSON.parse(res);
            if (response.success == true) {
                console.log("Lưu thành công.");
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

function ReturnResult_SaveDateToSession() {
    var fromDate = document.getElementById('xn_returnresult_timeSearchFrom').value;
    var toDate = document.getElementById('xn_returnresult_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/XN_ReturnResult/SaveSearchDates',
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

// ============================== IMPORT XÉT NGHIỆM NGOÀI (PDF) ==============================

// Mở modal + đổ patientId hiện chọn
function Returnresult_ShowImportModal() {
    var pid = $('#xn_returnresult_patientId').val();
    var pidTablePatient = $('#xn_returnresult_id').val();
    var pName = $('#xn_returnresult_patientName').val();
    var pMaBenhAn = $('#xn_returnresult_maBenhAn').val();
    //if (!pMaBenhAn || pMaBenhAn === "") {
    //    alert("Vui lòng chọn bệnh nhân trước khi import!");
    //    return;
    //}
    $('#import_patientId').val(pid);
    $('#import_idTablePatient').val(pidTablePatient);
    $('#import_patientName').val(pName);
    $('#import_maBenhAn').val(pMaBenhAn);

    // clear UI
    $('#import_pdf_file').val('');
    $('#pdfPreview').html('<em class="text-muted">Chưa có tệp được chọn…</em>');
    //$('#import_overwrite').prop('checked', false);
    //$('#import_mark_valid').prop('checked', false);
    // mở modal (nếu không dùng data-toggle)
    // tải danh sách PDF đã import
    Returnresult_LoadImportedPdfList(pMaBenhAn, pid);
    try { $('#modal-import-external').modal('show'); } catch (e) { }
}

// Xem trước PDF ngay khi chọn file
function Returnresult_HandlePdfPreview(input) {
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

function Returnresult_LoadImportedPdfList(pMaBenhAn, patientId) {
    var pId = $('#import_patientId').val() || patientId;
    $('#imported_pdf_list').html('<div class="text-muted p-2">Đang tải…</div>');
    $.get('/XN_Process/GetExternalLabFiles', { pMaBenhAn: pMaBenhAn, pId: pId, patientId: patientId }, function (res) {
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
                  <button type="button" class="btn btn-sm btn-primary me-1" onclick="ReturnResult_PreviewImportedPdf('${f.url.replace(/'/g, "\\'")}')">Preview</button>
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

function ReturnResult_PreviewImportedPdf(url) {
    const container = document.getElementById('pdfPreview');
    container.innerHTML = '';
    const emb = document.createElement('embed');
    emb.src = url;
    emb.type = 'application/pdf';
    emb.style.width = '100%';
    emb.style.height = '70vh';
    container.appendChild(emb);
}

function ReturnResult_SendZns() {
    var sid = $('#xn_returnresult_sid').val();

    if (!sid || $.trim(sid) === '') {
        SwalHelper.Toast.warning('Không tìm thấy SID để gửi ZNS!');
        return;
    }

    if (!confirm('Bạn có chắc chắn muốn gửi ZNS cho SID này không?')) {
        return;
    }

    var templateId = '569149'; // TODO: đổi lại ID template thực tế của bạn

    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: '/ZaloOA/SendResultXN',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({
            sid: sid,
            templateOAId: templateId
        }),
        success: function (res) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }

            if (!res) {
                SwalHelper.Toast.error('Không nhận được phản hồi từ server.');
                return;
            }

            if (res.success) {
                var msg = 'Gửi ZNS thành công';
                if (res.trackingId) {
                    msg += ' - TrackingId: ' + res.trackingId;
                }
                SwalHelper.Toast.success(msg);
            } else {
                var msgError = res.message || 'Gửi ZNS thất bại';
                if (res.errorCode) {
                    msgError = '[' + res.errorCode + '] ' + msgError;
                }
                SwalHelper.Toast.error(msgError);
            }
        },
        error: function (xhr) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }

            var msg = 'Có lỗi khi gọi API gửi ZNS.';
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                msg = xhr.responseJSON.message;
            }
            SwalHelper.Toast.error(msg);
        }
    });
}

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
