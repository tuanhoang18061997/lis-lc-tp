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
    window.BarcodeScan.register('#xq_getsample_pidorseq', window.GetSample_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#xq_process_pidorseq', window.Process_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#xq_returnresult_pidorseq', window.ReturnResult_Search, {
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
$(document).ready(function () {
    // Gắn Enter key cho các input field tìm kiếm của XQ_Process
    $('#xq_process_pidorseq, #xq_process_timeSearchFrom, #xq_process_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            Process_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của GetSample
    $('#xq_getsample_pidorseq, #xq_getsample_timeSearchFrom, #xq_getsample_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            GetSample_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của ReturnResult
    $('#xq_returnresult_pidorseq, #xq_returnresult_timeSearchFrom, #xq_returnresult_timeSearchTo').on('keydown', function (e) {
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
            if ($('.xq-left-header').length === 0) {
                var header = $('<div class="xq-left-header">' +
                    '<div class="xq-left-header-title">Danh sách bệnh nhân</div>' +
                    '<button class="xq-left-header-close"><i class="bi bi-x-lg"></i></button>' +
                    '</div>');
                $('.xq-left').prepend(header);
            }
        } else {
            // Xóa các elements mobile trên desktop
            $('.mobile-toggle-patient-list').remove();
            $('.mobile-patient-list-overlay').remove();
            $('.xq-left-header').remove();
            $('.xq-left').removeClass('show');
        }
    }

    // Khởi tạo khi load trang
    initMobilePatientList();

    // Click vào nút toggle
    $(document).on('click', '.mobile-toggle-patient-list', function () {
        $('.xq-left').addClass('show');
        $('.mobile-patient-list-overlay').addClass('show');
        $(this).addClass('active');
        // Prevent scroll trên body
        $('body').css('overflow', 'hidden');
    });

    // Click vào nút đóng trong header
    $(document).on('click', '.xq-left-header-close', function () {
        closePatientList();
    });

    // Click vào overlay để đóng
    $(document).on('click', '.mobile-patient-list-overlay', function () {
        closePatientList();
    });

    // Hàm đóng danh sách
    function closePatientList() {
        $('.xq-left').removeClass('show');
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

    $(document).on('touchstart', '.xq-left', function (e) {
        if (window.innerWidth <= 768) {
            startX = e.touches[0].clientX;
            isDragging = true;
        }
    });

    $(document).on('touchmove', '.xq-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            currentX = e.touches[0].clientX;
            var diff = currentX - startX;

            // Chỉ cho phép swipe sang trái
            if (diff < 0) {
                $(this).css('transform', 'translateX(' + diff + 'px)');
            }
        }
    });

    $(document).on('touchend', '.xq-left', function (e) {
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
        $('#xq_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#xq_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#xq_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#xq_getsample_location').select2({
            placeholder: "-- Chọn --"
        });
    });   

    if (isLoadPage) {
        $('#xq_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#xq_getsample_doctor').select2({
            placeholder: "-- Chọn --"
        });
    });


    if (isLoadPage) {
        $('#xq_process_sampleresult').val("");
    }
    $(document).ready(function () {
        $('#xq_process_sampleresult').select2({
            placeholder: "-- Chọn --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#xq_process_userReturnResultXQ').select2({
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
                if (e.id === "xq_getsample_location") {
                    $('#xq_getsample_location').select2('focus');
                }
                else if (e.id === "xq_getsample_doctor") {
                    $('#xq_getsample_doctor').select2('focus');
                }
                else if (e.id === "xq_getsample_userReturnResultXQ") {
                    $('#xq_getsample_userReturnResultXQ').select2('focus');
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
        url: "/XQ_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#xq_getsample_timeSearchFrom").val(today);
            //$("#xq_getsample_timeSearchTo").val(today);

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
        url: "/XQ_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#xq_getsample_timeSearchFrom").val(today);
            $("#xq_getsample_timeSearchTo").val(today);
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Search() {
    var xq_getsample_pidorseq = $("#xq_getsample_pidorseq").val();
    var timeSearchFrom = $("#xq_getsample_timeSearchFrom").val();
    var timeSearchTo = $("#xq_getsample_timeSearchTo").val();
    $.ajax({
        url: "/XQ_GetSample/Search?" + "pidorseq=" + xq_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
$("#xq_getsample_savepatient").hide();
$("#xq_getsample_cancelpatient").hide();
function GetSample_HideButton(_new, _save, _delete, _cancel, _addservice, _getsample) {
    if (_new == 1) {
        $("#xq_getsample_newpatient").hide();
    }
    else {
        $("#xq_getsample_newpatient").show();
    }

    if (_save == 1) {
        $("#xq_getsample_savepatient").hide();
    }
    else {
        $("#xq_getsample_savepatient").show();
    }

    if (_delete == 1) {
        $("#xq_getsample_deletepatient").hide();
    }
    else {
        $("#xq_getsample_deletepatient").show();
    }

    if (_cancel == 1) {
        $("#xq_getsample_cancelpatient").hide();
    }
    else {
        $("#xq_getsample_cancelpatient").show();
    }

    if (_addservice == 1) {
        $("#xq_getsample_addService").hide();
    }
    else {
        $("#xq_getsample_addService").show();
    }

    if (_getsample == 1) {
        $("#xq_getsample_processresult").hide();
    }
    else {
        $("#xq_getsample_processresult").show();
    }
}


function GetSample_GetPatientInfo(id) {
    GetSample_HideButton(false, true, false, true, false, false);
    $.ajax({
        url: "/XQ_GetSample/GetPatientInfo?id=" + id,
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
        url: "/XQ_GetSample/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#xq-right-service-gridview").html(result);
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
    $("#xq_getsample_pidorseq").val('');
    $('#xq_getsample_id').val('');
    $('#xq_getsample_patientId').val('');
    $('#xq_getsample_maBenhAn').val('');
    $('#xq_getsample_seq').val('');
    $('#xq_getsample_sid').val('');
    $('#xq_getsample_patientName').val('');
    $('#xq_getsample_age').val('');
    $('#xq_getsample_sex').val('');
    $('#xq_getsample_obj').val('');
    $('#xq_getsample_type').val('');
    $('#xq_getsample_location').val('');
    $('#xq_getsample_doctor').val('');
    $('#xq_getsample_getSampleTime').val(dateTime);
    $('#xq_getsample_address').val('');
    $('#xq_getsample_diagnostic').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
    GetSample_SetSelect2_01(false);
}

function GetSample_NewPatient() {
    $('#xq_getsample_patientId').focus();
    GetSample_HideButton(true, false, true, false, true, true);
    GetSample_ResetInput();
}


function GetSample_DeletePatient() {
    var id = $('#xq_getsample_id').val();
    if (id == '') {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var choice = confirm("Bạn muốn xoá bệnh nhân và tất cả chỉ định xét nghiệm?");
        if (choice) {
            $.ajax({
                url: "/XQ_GetSample/DeletePatientAndService?id=" + id,
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
    var id = $('#xq_getsample_id').val();
    if (id == '') {
        GetSample_ResetInput()
    }
    else {
        GetSample_GetPatientInfo(id);
    }
}

function GetSample_GetSID(seq) {
    $.ajax({
        url: "/XQ_GetSample/GetSID?seq=" + seq,
        type: "GET",
        dataType: "text",
        cache: false,
        success: function (result) {
            $('#xq_getsample_sid').val(result);
        }
    });
}

function GetSample_SavePatient() {
    var validate = GetSample_ValidateInput('patientInfo');
    if (validate) {
        var id = $('#xq_getsample_id').val();
        var patientId = $('#xq_getsample_patientId').val();
        var seq = $('#xq_getsample_seq').val();
        var sid = $('#xq_getsample_sid').val();
        var patientName = $('#xq_getsample_patientName').val();
        var age = $('#xq_getsample_age').val();
        var sex = $('#xq_getsample_sex').val();
        var obj = $('#xq_getsample_obj').val();
        var type = $('#xq_getsample_type').val();
        var location = $('#xq_getsample_location').val();
        var doctor = $('#xq_getsample_doctor').val();
        var getSampleTime = $('#xq_getsample_getSampleTime').val();
        var address = $('#xq_getsample_address').val();
        var diagnostic = $('#xq_getsample_diagnostic').val();
        var category = $('#select-category').val();
        var service = $('#select-service').val();

        $.ajax({
            url: "/XQ_GetSample/SavePatient?id= " + id + "&&patientId=" + patientId + "&&seq=" + seq + "&&sid=" + sid + "&&patientName=" + patientName + "&&age=" + age + "&&sex=" + sex + "&&obj=" + obj + "&&type=" + type + "&&location=" + location + "&&doctor=" + doctor + "&&getSampleTime=" + getSampleTime + "&&address=" + address + "&&diagnostic=" + diagnostic + "&&service=" + service,
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
    var id = $('#xq_getsample_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var getSampleTime = $('#xq_getsample_getSampleTime').val();
        $.ajax({
            url: "/XQ_GetSample/ProcessResult?id= " + id + "&getSampleTime=" + getSampleTime,
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
    var id = $('#xq_getsample_id').val();
    if (id === "") {
        $('#addServiceForm').modal('hide');
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        GetSample_SetSelect2_02();
    }
}

function GetSample_DeleteServiceForPatient(idResultCDHA) {
    var patientId = $('#xq_getsample_id').val();
    $.ajax({
        url: "/XQ_GetSample/DeleteServiceForPatient?id=" + idResultCDHA,
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
    var serviceId = $('#xq_getsample_service').val();
    var patientId = $('#xq_getsample_id').val();
    var doctorId = $('#xq_getsample_doctor').val();
    if (serviceId === "" || patientId === "") {
        SwalHelper.Toast.warning("Chỉ định dịch vụ không thành công. Vui lòng kiểm tra lại !")
    }
    else {
        $.ajax({
            url: "/XQ_GetSample/AddServiceForPatient?patientId=" + patientId + "&&serviceId=" + serviceId + "&&doctorId=" + doctorId,
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
        url: "/XQ_GetSample/Get_Count/",
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
    var fromDate = document.getElementById('xq_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('xq_getsample_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/XQ_GetSample/SaveSearchDates',
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

//***************************************************************************************** Process

function Process_ValidateInput(id) {
    var flag = true;
    $("#" + id).find(".valid").each(function (i, e) {
        var value = $(e).val();
        if (value === "" || !value) {
            if (flag) {
                if (e.id === "xq_process_userReturnResultXQ") {
                    $('#xq_process_userReturnResultXQ').select2('focus');
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
        url: "/XQ_Process/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#xq_process_timeSearchFrom").val(today);
            //$("#xq_process_timeSearchTo").val(today);
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
    var xq_process_pidorseq = $("#xq_process_pidorseq").val();
    var timeSearchFrom = $("#xq_process_timeSearchFrom").val();
    var timeSearchTo = $("#xq_process_timeSearchTo").val();
    $.ajax({
        url: "/XQ_Process/Search?" + "pidorseq=" + xq_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#xq_process_pidorseq").val('');
    $('#xq_process_id').val('');
    $('#xq_process_patientId').val('');
    $('#xq_process_maBenhAn').val('');
    $('#xq_process_seq').val('');
    $('#xq_process_sid').val('');
    $('#xq_process_patientName').val('');
    $('#xq_process_age').val('');
    $('#xq_process_sex').val('');
    $('#xq_process_obj').val('');
    $('#xq_process_type').val('');
    $('#xq_process_location').val('');
    //$('#xq_process_doctor').val('');
    $('#xq_process_getSampleTime').val('');
    $('#xq_process_returnResultTime').val('');
    $('#xq_process_location').val('');
    //$('#xq_process_doctor').val('');
    $('#xq_process_userReturnResultXQ').val('');
    $('#xq_process_address').val('');
    $('#xq_process_diagnostic').val('');
    $('#tbody-gridview-service').empty();
    $('#imageCDHA').empty();
    $('#xq_list_service').empty();
    $('#xq_process_result').empty();
    $('#xq_process_suggest').empty();
    $('#xq_text_key').empty();
    CKEDITOR.instances["xq_process_description"].setData(""); 
    Process_SetSelect2_03(true);
}

function Process_GetPatientInfo(id) {
    $.ajax({
        url: "/XQ_Process/Check_SelectDevice/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            if (result === "False") {
                $('#addDeviceForm').modal('show');
            } else {
                $.ajax({
                    url: "/XQ_Process/GetPatientInfo?id=" + id,
                    type: "GET",
                    dataType: "html",
                    cache: false,
                    success: function (result) {
                        $("#process_patientInfo").html(result);
                        $(".list-group-item-action").removeClass("active");
                        $(`.list-group-item-action[data-id='${id}']`).addClass("active");
                        Process_SetSelect2_03();
                        GetSample_SetSelect2_01(true);
                        // Auto-chọn bác sĩ = user đang login (nếu chưa có giá trị)
                        //var loginId = $("#xq_process_userLoginId").val();
                        //var $sel = $("#xq_process_userReturnResultXQ");
                        //if (loginId && (!$sel.val() || $sel.val() === "")) {
                        //    $sel.val(loginId).trigger("change");
                        //}

                        // Auto-chọn bác sĩ theo user login hoặc từ localStorage
                        var loginId = $("#xq_process_userLoginId").val();
                        var $sel = $("#xq_process_userReturnResultXQ");

                        // Lấy lựa chọn trước đó từ localStorage
                        var savedDoctor = localStorage.getItem("xq.process.selectedDoctor");
                        if (savedDoctor && $sel.find('option[value="' + savedDoctor + '"]').length > 0) {
                            $sel.val(savedDoctor).trigger("change");
                        } else if (loginId && (!$sel.val() || $sel.val() === "")) {
                            // Nếu chưa có gì thì dùng user login
                            $sel.val(loginId).trigger("change");
                        } else {
                            // Nếu có sẵn thì vẫn trigger để load info
                            $sel.trigger("change");
                        }

                        //// Gắn handler đổi bác sĩ
                        //$(document)
                        //    .off("change", "#xq_process_userReturnResultXQ")
                        //    .on("change", "#xq_process_userReturnResultXQ", Process_Load_SelectedDoctorInfo);
                        //// Lần đầu nếu có sẵn value thì cũng load info
                        //Process_Load_SelectedDoctorInfo();
                        //Process_GetListServiceForPatient(id);

                        $(document)
                            .off("change", "#xq_process_userReturnResultXQ")
                            .on("change", "#xq_process_userReturnResultXQ", function () {
                                var selected = $(this).val();
                                if (selected) {
                                    // Lưu ngay lựa chọn BS thực hiện mới
                                    localStorage.setItem("xq.process.selectedDoctor", selected);
                                    // Gán lại giá trị vừa chọn (phòng trường hợp DOM refresh lại)
                                    $("#xq_process_userReturnResultXQ").val(selected);
                                }
                                //Process_Load_SelectedDoctorInfo();
                                //Process_GetListServiceForPatient(id);
                            });
                        Process_Load_SelectedDoctorInfo();
                        Process_GetListServiceForPatient(id);
                    },
                    error: function () {
                        $("#process_patientInfo").empty();
                        $('#tbody-gridview-service').empty();
                    }
                });
            }
        }
    })
    
}

function Process_GetListServiceForPatient(id) {
    $.ajax({
        url: "/XQ_Process/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_xq-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}


function Process_SaveResult() {
    var patientId = $('#xq_process_id').val();
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
                var returnResultTime = $('#xq_process_returnResultTime').val();
                var userReturnResult = $('#xq_process_userReturnResultXQ').val();
                var description = CKEDITOR.instances['xq_process_description'].getData();
                var result = $('#xq_process_result').val();
                var suggest = $('#xq_process_suggest').val();

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
                    url: "/XQ_Process/SaveResult/",
                    data: JSON.stringify(data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    type: "POST",
                    success: function (result) {
                        if (result == 'True') {
                            SwalHelper.Toast.success("Lưu thành công.");
                            Process_GetPatientInfo(patientId);
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
    var deviceId = $('#xq_getsample_select_device_select').val();
    if (deviceId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn thiết bị !")
    }
    else {
        $.ajax({
            url: "/XQ_Process/SelectDevice?deviceId=" + deviceId,
            type: 'GET',
            dataType: 'text',
            success: function (result) {
                if (result === 'False') {
                    SwalHelper.Toast.error("Không thể chọn thiết bị. Kiểm tra lại !");
                }
                //else {
                //    Process_ValidPrint();
                //}
            },
            error: function () {
                SwalHelper.Toast.error("Không thể chọn thiết bị. Kiểm tra lại !");
            }
        });
    }
}

// --- GỌI IN 1 DỊCH VỤ (hành vi cũ), tách riêng thành hàm dùng lại ---
function _validPrintOne(resultCDHAId, opts, isMulti = false) {
    // opts: { patientId, returnResultTime, userReturnResult, useSaved, description, result, suggest, selectedImageIds }
    return new Promise(function (resolve) {
        var payload = {
            patientId: opts.patientId,
            resultCDHAId: resultCDHAId,
            returnResultTime: opts.returnResultTime,
            userReturnResult: opts.userReturnResult,
            // các field dưới đây chỉ hữu dụng khi in 1 dịch vụ đang mở editor:
            description: opts.description || null,
            result: opts.result || null,
            suggest: opts.suggest || null,
            selectedImageIds: Array.isArray(opts.selectedImageIds) ? opts.selectedImageIds : null
        };
        if (isMulti) {
            $.ajax({
                url: "/XQ_Process/ValidPrintMultiple/",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                data: JSON.stringify(payload),
                success: function (resp) {
                    if (resp) {
                        // mở PDF
                        var bytes = atob(resp);
                        var arr = new Uint8Array(bytes.length);
                        for (var i = 0; i < bytes.length; i++) arr[i] = bytes.charCodeAt(i);
                        var blob = new Blob([arr], { type: 'application/pdf;base64' });
                        window.open(URL.createObjectURL(blob));
                    }
                    resolve();
                },
                error: function () {
                    // không chặn chuỗi, tiếp tục cái tiếp theo
                    resolve();
                }
            });
        } else {
            $.ajax({
                url: "/XQ_Process/ValidPrint/",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                data: JSON.stringify(payload),
                success: function (resp) {
                    if (resp) {
                        // mở PDF
                        var bytes = atob(resp);
                        var arr = new Uint8Array(bytes.length);
                        for (var i = 0; i < bytes.length; i++) arr[i] = bytes.charCodeAt(i);
                        var blob = new Blob([arr], { type: 'application/pdf;base64' });
                        window.open(URL.createObjectURL(blob));
                    }
                    resolve();
                },
                error: function () {
                    // không chặn chuỗi, tiếp tục cái tiếp theo
                    resolve();
                }
            });
        }

    });
}

function Process_ValidPrint(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    console.log("modedđ: ", mode);
    var patientId = $('#xq_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }
    var ok = Process_ValidateInput('process_patientInfo');
    if (!ok) return;
    var returnResultTime = $('#xq_process_returnResultTime').val();
    var userReturnResult = $('#xq_process_userReturnResultXQ').val();

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

    if (!resultIds.length) {
        SwalHelper.Toast.warning("Không tìm thấy dịch vụ nào để in!");
        return;
    }

    // Nếu chỉ có 1 dịch vụ => hành vi cũ (dùng nội dung editor hiện tại nếu cần)
    // Nếu nhiều dịch vụ => in lần lượt, dùng dữ liệu đã LƯU ở server (useSaved=true)
    var isMulti = resultIds.length > 1;

    // Lấy nội dung editor hiện tại (nếu bạn đang cho phép in 1 dịch vụ với nội dung đang chỉnh)
    // Có thể tuỳ chỉnh theo app của bạn:
    var description = isMulti ? null : (CKEDITOR.instances['xq_process_description'].getData() || null);
    var resultText = isMulti ? null : ($('#xq_process_result').val() || null);
    var suggest = isMulti ? null : ($('#xq_process_suggest').val() || null);
    var selectedImageIds = isMulti ? null : $(".xq-print-check:checked").map(function () { return $(this).val(); }).get(); // nếu có UI chọn ảnh riêng cho 1 dịch vụ

    var opts = {
        patientId: patientId,
        returnResultTime: returnResultTime,
        userReturnResult: userReturnResult,
        description: description,
        result: resultText,
        suggest: suggest,
        selectedImageIds: selectedImageIds
    };
    console.log(resultIds);
    $('#showWaitting').modal('show');

    // CHUỖI SEQUENTIAL: nếu nhiều dịch vụ => in lần lượt
    var chain = Promise.resolve();
    resultIds.forEach(function (rid) {
        chain = chain.then(function () {
            return _validPrintOne(rid, opts, isMulti);
        });
    });

    chain.then(function () {
        try { $('#showWaitting').modal('hide'); } catch (e) { }
        Process_Refresh();
        Process_Get_Count();
    });
}


function Process_GetSample() {
    var id = $('#xq_process_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        $.ajax({
            url: "/XQ_Process/GetSample?id= " + id,
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
        url: "/XQ_Process/Get_Count/",
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
//function Process_CheckedBoxOnRow(id) {
//    $(".row-service").each(function () { // Lấy value trên từng Row
//        $(this).find(".form-check-input").each(function () {
//            var value = $(this).val();
//            $(this).prop("checked", true);
//            $('#xq_process_sampleresult').val('');
//            $('#xq_process_sampleresult').select2({
//                placeholder: "-- Chọn --"
//            });
//            if (value == id) {
//                Process_Load_ResultAndImage_ForService(id);
//            }
//            else {
//                $(this).prop("checked", false);
//            }
//        })

//    })
//}

function Process_CheckedBoxOnRow(id) {
    $(".row-service .form-check-input").each(function () {
        var $chk = $(this);
        var resultCdhaId = String($chk.val());
        var isTarget = (resultCdhaId === String(id));
        $chk.prop("checked", isTarget);

        if (isTarget) {
            // 1) lấy ServiceId thực của dịch vụ từ data-serviceid
            var serviceId = $chk.data("serviceid");
            var bs_thuc_hien = $chk.data("bs-thuc-hien");
            console.log("bs THUC HIEN: ", bs_thuc_hien);
            if (bs_thuc_hien != null && bs_thuc_hien != '') {
                $('#xq_process_userReturnResultXQ').val(bs_thuc_hien).trigger("change");
            } else {
                var loginId = $("#xq_process_userLoginId").val();
                console.log(loginId)
                $('#xq_process_userReturnResultXQ').val(loginId).trigger("change");
            }
            console.log("id ResultCDHA: ", id);
            console.log("Mã dịch vụ dã chọn: ", serviceId);
            // 2) gọi load và để hàm tự ưu tiên “đã lưu”, nếu không có sẽ fallback theo service+gender
            Process_Load_ResultAndImage_ForService(id, serviceId);
        }
    });
}

// Chuẩn hoá giới tính từ select id = xq_process_sex
function _normalizeSexFromUI() {
    var raw = ($("#xq_process_sex").val() || "").toString().trim().toUpperCase();
    // UI hiện hiển thị tiếng Việt hoặc code khác, coi "NAM" là M, còn lại là F
    if (raw === "NAM" || raw === "M" || raw === "1") return "M";
    return "F";
}

// Tự chọn Sample theo service + gender
function _autoPickSampleForService(serviceId) {
    var $sel = $("#xq_process_sampleresult");
    if (!$sel.length) return;

    var gender = _normalizeSexFromUI();           // "M" | "F"
    var accept = new Set([gender, "MF"]);         // luôn chấp nhận "MF" như yêu cầu

    // Lấy toàn bộ option có data phù hợp
    var $options = $sel.find("option").filter(function () {
        var optService = $(this).data("serviceid");
        var optGender = ($(this).data("gender") || "").toString().toUpperCase();
        return String(optService) === String(serviceId) && accept.has(optGender);
    });
    console.log($options);
    if (!$options.length) {
        $("#xq_process_sampleresult").val("");
        $('#xq_process_sampleresult').select2({
            placeholder: "-- Chọn --"
        });
        return;
    }

    // Ưu tiên option có gender == đúng M/F, sau đó mới đến MF
    var $best = $options.filter(function () {
        return ($(this).data("gender") || "").toString().toUpperCase() === gender;
    });
    if (!$best.length) $best = $options.filter(function () {
        return ($(this).data("gender") || "").toString().toUpperCase() === "MF";
    });

    var val = ($best.length ? $best.first().val() : $options.first().val()) || "";
    $sel.val(val).trigger("change"); // để Select2 cập nhật UI
}

function _setSampleSelectValue(sampleId, sampleName) { //Hàm set giá trị cho Mô tả (đảm bảo option tồn tại)
    var $sel = $("#xq_process_sampleresult");
    if (!$sel.length) return;

    // Nếu option chưa tồn tại (VD: sample cũ bị ẩn do filter), ta thêm tạm để hiển thị đúng dữ liệu đã lưu
    var has = $sel.find('option[value="' + sampleId + '"]').length > 0;
    if (!has) {
        $sel.append($('<option>', {
            value: sampleId,
            text: sampleName ? sampleName : ('Sample #' + sampleId)
        }));
    }
    $sel.val(String(sampleId)).trigger("change");
}

//function Process_Load_ResultAndImage_ForService(id) {
//    const $containHinh = $('.contain-hinh');
//    $containHinh.empty();
//    CKEDITOR.instances["xq_process_description"].setData("");
//    $("#xq_process_result").val("");
//    $("#xq_process_suggest").val("");
//    $.ajax({
//        url: "/XQ_Process/GetImageForService?id=" + id,
//        type: 'GET',
//        dataType: 'json',
//        success: function (response) {
//            $.each(response, function (key, value) {
//                if (value.id) {
//                    const $hinh = $('<div class="hinh"></div>');
//                    $hinh.append('<img id="' + value.id + '" src="' + value.name + '" onclick="Process_Hinh(' + value.id + ', this)"/>');
//                    $containHinh.append($hinh);
//                }
//                CKEDITOR.instances["xq_process_description"].setData(value.description);
//                $("#xq_process_result").val(value.result);
//                $("#xq_process_suggest").val(value.suggest);
//            });
//        }
//    });
//}

/**
 * Load ảnh + mô tả/kết quả cho 1 dịch vụ (resultCdhaId)
 * Trả về Promise resolve({ savedSampleId, savedSampleName, serviceIdFromApi })
 * 
 * @param {number|string} id  id dòng dịch vụ (ResultCDHAId)
 * @param {number|string} serviceId  ServiceId (đọc từ checkbox)
 */
function Process_Load_ResultAndImage_ForService(id, serviceId) {
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["xq_process_description"].setData("");
    $("#xq_process_result").val("");
    $("#xq_process_suggest").val("");
    $.ajax({
        url: "/XQ_Process/GetImageForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var firstPayload = null;

            $.each(response, function (key, value) {
                if (!firstPayload && value) firstPayload = value;
                if (value.id) {
                    const $hinh = $('<div class="hinh"></div>');
                    $hinh.append('<img id="' + value.id + '" src="' + value.name + '" onclick="Process_Hinh(' + value.id + ', this)"/>');
                    $containHinh.append($hinh);
                }
                CKEDITOR.instances["xq_process_description"].setData(value.description);
                $("#xq_process_result").val(value.result);
                $("#xq_process_suggest").val(value.suggest);
            });

            // set mô tả/kết quả/tư vấn (lấy từ phần tử đầu tiên nếu có)
            var desc = "", res = "", suggest = "";
            if (firstPayload) {
                desc = firstPayload.description ?? "";
                res = firstPayload.result ?? "";
                suggest = firstPayload.suggest ?? "";
            }
            console.log(firstPayload);
            if (CKEDITOR.instances["xq_process_description"]) {
                CKEDITOR.instances["xq_process_description"].setData(desc);
            }
            $("#xq_process_result").val(res);
            $("#xq_process_suggest").val(suggest);

            // ✅ CHỈ khi cả desc và result đều null/rỗng thì mới tự chọn sample
            var needAutoPick =
                (!desc || desc.trim() === "") &&
                (!res || res.trim() === "");

            if (needAutoPick && serviceId != null) {
                _autoPickSampleForService(serviceId);
            } else {
                $("#xq_process_sampleresult").val("");
                $('#xq_process_sampleresult').select2({
                    placeholder: "-- Chọn --"
                });
            }
            // nếu không needAutoPick: giữ nguyên select hiện tại, không can thiệp

            return { needAutoPick: !!needAutoPick };
        }
    });
}
function Process_Hinh(imageCDHAId, img) {
    $('.contain-hinh img').removeClass('selected');
    $(img).toggleClass('selected');
    $("#xq_process_delete").val(imageCDHAId);
    $("#xq_process_xem").val(imageCDHAId);
}

function GetSampleForService(id) {
    $.ajax({
        url: "/XQ_Process/GetSampleForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            CKEDITOR.instances["xq_process_description"].setData(response.description);
            $("#xq_process_result").val(response.result);
            $("#xq_process_suggest").val(response.suggest);
        }
    });
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
            url: "/XQ_Process/UploadHinh/",
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response !== 'True') {
                    SwalHelper.Toast.error("Không thể lưu ảnh. Vui lòng kiểm tra lại!");
                } else {
                    //Process_Load_ResultAndImage_ForService(resultCDHAId);
                    Process_CheckedBoxOnRow(resultCDHAId);
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
    var imageCDHAId = $("#xq_process_delete").val();
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
                    url: "/XQ_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
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
    //        url: "/XQ_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
    //        type: 'POST',
    //        dataType: 'text',
    //        success: function (response) {
    //            if (response === "") {
    //                SwalHelper.Toast.error("Không thể xoá ảnh. Vui lòng kiểm tra lại!");
    //            }
    //            else {
    //                //Process_Load_ResultAndImage_ForService(response);
    //                Process_CheckedBoxOnRow(response);
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
    var userId = $("#xq_process_userReturnResultXQ").val();
    if (!userId) {
        $("#xq_process_signerCCCD").empty();
        $("#xq_process_doctorInfo").empty();
        return;
    }
    $.ajax({
        url: "/XQ_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#xq_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#xq_process_signerCCCD").val(cccd);
        },
        error: function () {
            $("#xq_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
        }
    });
}

// ========================== KÝ SỐ PDF (Form: XQUANG) ==========================
function Process_SignPdf() {
    var patientId = $('#xq_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Lấy ResultCDHAId (dịch vụ đang chọn) – giống logic ValidPrint
    var resultCDHAId = "";
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            if ($(this).is(':checked')) {
                resultCDHAId = $(this).val();
            }
        })
    });
    if (!resultCDHAId) {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
        return;
    }

    // Lấy CCCD người ký (bác sĩ)
    var signerCCCD = $('#xq_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#xq_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#xq_process_patientId').val() || $('#xq_process_sid').val() || "";
    var patientMaBenhAn = $('#xq_process_maBenhAn').val() || $('#xq_process_sid').val() || "";
    var patientName = $('#xq_process_patientName').val() || "";
    var doctorName = $('#xq_process_userReturnResultXQ option:selected').text() || $('#xq_process_userReturnResultXQ').val() || "";
    var performedAt = $('#xq_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "XQuang"; // nếu cần, bạn lấy từ label dịch vụ đang chọn

    // 1) Gọi API xuất PDF base64 (tận dụng endpoint ValidPrint đang có)
    //    Nếu bạn có endpoint riêng chỉ "Export" (không đổi trạng thái), thay URL ở đây là tốt nhất.
    $('#showWaitting').modal('show');
    var dataExport = {
        patientId: patientId,
        resultCDHAId: resultCDHAId,
        returnResultTime: $('#xq_process_returnResultTime').val(),
        userReturnResult: $('#xq_process_userReturnResultXQ').val(),
        description: CKEDITOR.instances['xq_process_description'].getData(),
        result: $('#xq_process_result').val(),
        suggest: $('#xq_process_suggest').val()
    };

    $.ajax({
        url: "/XQ_Process/ValidPrint/",
        data: JSON.stringify(dataExport),
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        type: "POST",
        success: function (base64Pdf) {
            if (!base64Pdf) {
                $('#showWaitting').modal('hide');
                SwalHelper.Toast.error("Không thể xuất PDF để ký. Vui lòng kiểm tra lại!");
                return;
            }

            // 2) Chuyển base64 -> File để gửi multipart/form-data
            var file = base64ToFile(base64Pdf, "temp.pdf", "application/pdf");
            console.log(file);
            // 3) Chuẩn bị FormData gửi đến API ký số chung (server sẽ tự đặt tên file)
            var formData = new FormData();
            formData.append("signerCCCD", signerCCCD);
            formData.append("file", file, file.name);

            // ---- context để server đặt tên theo template MySign:FileNameTemplates (FormType = SieuAm)
            formData.append("formType", formType);
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
                    // Gỉa sử object trả về từ response như sau
                    //resp = {
                    //    "id": "51626", "provider": "0", "documentId": "null", "documentType": "null", "documentName": "27433568_phieu_danh_gia_ban_dau.pdf", "documentDesc": "null", "documentHash": "7X38RRgfWpYULcvtZc863HnTfocDsSsji023UGoxBLM=", "timestamp": "2025-09-19T07:11:06.763046209Z", "transactionId": "308706a6-5dc7-4f0b-ba08-01448c15a306", "credentialId": "066194014120_7500452_20250821162101", "signatures": "SJpUV2IQVRR3FzMzK5WWLucKNX2jtFc4\/\/+rvi19Izj6yL1epn7xo1cxGhW\/67fI\r\niqLqz104zwke0xbWf9BZPjAEmSWD8Xn2RaouyGCBQIj4tnCnMZQGnPCBZ2laN4Ec\r\n3qjMU3lpoh22OnVBgLYZlQZSSvnNii1MF7v1vKRsdggl4YZpHEiZlgMef7iclkga\r\nMGUryw11teXAg77s15jGKYCjkM49jrJlGq+M5p0IVCj\/ZeUXByd3MsBeWAnWAHs1\r\n\/WoyEfBhwK3YABb37cjO6RNFACj+KJJv8910AyQwZeowyhmeQDdjLdGgfJRILORt\r\nDFZF1iABlsXIbp6pf0ek8w==", "status": "1", "filepath": "null", "url": "null", "docControlId": "null", "docControlType": "null", "error": "null", "login": "066194014120", "taxcode": "6001362081", "metadata": "null", "requesttime": "1758265850532", "contentContentType": "application\/pdf", "signUserId": "066194014120",
                    //}
                    $('#showWaitting').modal('hide');

                    // Controller trả về "response.data" nếu có; tùy MySign:
                    // - Nếu có resp.fileUrl -> mở luôn
                    // - Nếu có resp.base64Pdf -> mở PDF
                    // - Nếu là string -> thử parse
                    try {
                        var obj = (typeof resp === "string") ? JSON.parse(resp) : resp; //obj này sẽ trả về response.data luôn

                        // 1) THU THẬP signStoreId TỪ RESPONSE (ưu tiên data.id)
                        var signStoreId =
                            (obj && obj.data && (obj.data.id || obj.data.signStoreId)) ||
                            obj.id || obj.signStoreId || null;
                        // giả sử signStoreId đang null để test dev nên gán cứng
                        signStoreId = signStoreId || "51402"; 
                        var keyResult = null;

                        // 2) LƯU signStoreId THEO resultCDHAId ĐANG CHỌN
                        if (signStoreId) {
                            $.ajax({
                                url: "/XQ_Process/SaveSignStoreIdForResultCDHA",
                                type: "POST",
                                dataType: "text",
                                data: { resultCDHAId: resultCDHAId, signStoreId: signStoreId },
                                success: function (res) {
                                    var objResult = JSON.parse(res);
                                    openSigned(signStoreId);
                                    if (objResult.success == true) {
                                        keyResult = objResult.keyResult;

                                        // ====== GỌI CONTROLLER LƯU DIGITAL_SIGN ======
                                        var targetText = doctorName;
                                        var statusNum = (obj && (obj.status === 1 || obj.status === "1")) ? 1 : 0;

                                        var digitalSignPayload = {
                                            referenceType: formType ?? "XQuang",                // SA / SATIM / XQUANG...
                                            referenceKeyResult: keyResult,          // keyResultForHis của resultCDHAId 
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
                                                console.log("Lưu DigitalSign thành công: ", r);
                                            },
                                            error: function (f) {
                                                console.log("Lưu DigitalSign thất bại: ", f);
                                            }
                                        })

                                        // 3) XỬ LÝ HIỂN THỊ FILE ĐÃ KÝ NHƯ CŨ
                                        if (obj && obj.fileUrl) {
                                            window.open(obj.fileUrl, "_blank");
                                            return;
                                        }
                                        if (obj && (obj.base64Pdf || obj.base64)) {
                                            openBase64Pdf(obj.base64Pdf || obj.base64);
                                            return;
                                        }
                                    }
                                },
                                error: function (xhr) {
                                    console.error("SaveSignStoreIdForResultXN error:", xhr.responseText);
                                }
                            });
                        }

                        // 3) XỬ LÝ HIỂN THỊ FILE ĐÃ KÝ NHƯ CŨ
                        if (obj && obj.fileUrl) {
                            window.open(obj.fileUrl, "_blank");
                            return;
                        }
                        if (obj && (obj.base64Pdf || obj.base64)) {
                            openBase64Pdf(obj.base64Pdf || obj.base64);
                            return;
                        }
                        SwalHelper.Alert.info("Đã ký số xong", JSON.stringify(obj));

                    } catch (e) {
                        if (isProbablyBase64(resp)) {
                            openBase64Pdf(resp);
                        } else {
                            SwalHelper.Alert.info("Đã ký số", resp);
                        }
                    }
                },
                error: function (xhr) {
                    $('#showWaitting').modal('hide');
                    var msg = "Ký số thất bại.";
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
}

function Process_SignPdf_Multi(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    var patientId = $('#xq_process_id').val();
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
    var signerCCCD = $('#xq_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#xq_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#xq_process_patientId').val() || $('#xq_process_sid').val() || "";
    var patientMaBenhAn = $('#xq_process_maBenhAn').val() || $('#xq_process_sid').val() || "";
    var patientName = $('#xq_process_patientName').val() || "";
    var doctorName = $('#xq_process_userReturnResultXQ option:selected').text() || $('#xq_process_userReturnResultXQ').val() || "";
    var doctorId = $('#xq_process_userReturnResultXQ').val();
    var performedAt = $('#xq_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "XQuang"; // bạn có thể thay động tùy màn hình

    // 1) Gọi API xuất PDF base64 (tận dụng endpoint ValidPrint đang có)
    //    Nếu bạn có endpoint riêng chỉ "Export" (không đổi trạng thái), thay URL ở đây là tốt nhất.
    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tạo file PDF...'); } catch (e) { }
    // 2. PHASE 1: EXPORT tất cả PDF thô
    var pdfJobs = [];  // [{resultCDHAId, base64Pdf}]
    var chain = Promise.resolve();

    //var dataExport = {
    //    patientId: patientId,
    //    resultCDHAId: resultCDHAId,
    //    returnResultTime: $('#xq_process_returnResultTime').val(),
    //    userReturnResult: $('#xq_process_userReturnResultXQ').val(),
    //    description: CKEDITOR.instances['xq_process_description'].getData(),
    //    result: $('#xq_process_result').val(),
    //    suggest: $('#xq_process_suggest').val()
    //};
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
                        url: "/XQ_Process/ValidPrintMultiple/",
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
                    returnResultTime: $('#xq_process_returnResultTime').val(),
                    userReturnResult: $('#xq_process_userReturnResultXQ').val(),
                    description: CKEDITOR.instances['xq_process_description'].getData(),
                    result: $('#xq_process_result').val(),
                    suggest: $('#xq_process_suggest').val()
                };
                console.log(resultIds[0], "====>", dataExport);
                $.ajax({
                    url: "/XQ_Process/ValidPrint/",
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
            formData.append("performedAt", performedAt);
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
                                    url: "/XQ_Process/SaveSignStoreIdForResultCDHA",
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
                                                    referenceType: formType ?? "XQuang",
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
// ===============================================================================
// Tick/untick tất cả dịch vụ
$(document).on('change', '#xq_chk_all', function () {
    var checked = this.checked === true;

    var $items = $('#xq_list_service').find('.xq-chk-service');
    $items.prop('checked', checked);

    // Đếm lại số đang được tick sau khi set
    var marked = $items.filter(':checked').length;

    // Quy tắc multi-selected:
    // - >=2 checked: tất cả các dòng đang checked thêm class
    // - <2 checked : xóa hết class
    if (marked >= 2) {
        // thêm cho tất cả dòng đang checked
        $items.each(function () {
            var $row = $(this).closest('.row-service');
            $row.removeClass('active');
            if (this.checked) $row.addClass('multi-selected');
            else $row.removeClass('multi-selected');
        });
    } else {
        // xóa hết
        var $row = $(this).closest('.row-service');
        $row.removeClass('active');
        $('#xq_list_service .row-service').removeClass('multi-selected');
    }

    // Ẩn/hiện nút Lưu dựa trên trạng thái check all (giữ nguyên logic cũ)
    if (checked) {
        $('#xq_process_saveresult').hide();
    } else {
        $('#xq_process_saveresult').show();
    }
});
// Khai báo guard toàn cục (một lần)
if (window.__xq_row_selecting === undefined) {
    window.__xq_row_selecting = false;
}
// Tick/untick từng dịch vụ riêng lẻ
$(document).on('change', '.xq-chk-service', function () {
    var $all = $('#xq_list_service').find('.xq-chk-service');
    var total = $all.length;
    var marked = $all.filter(':checked').length;

    // Đồng bộ header "chọn tất cả"
    var allChecked = total > 0 && marked === total;
    $('#xq_chk_all').prop('checked', allChecked);

    // Quy tắc multi-selected:
    // - Nếu chỉ còn 0 hoặc 1 cái được tick → remove hết .multi-selected
    // - Nếu từ 2 cái trở lên → tất cả các dòng được tick đều có .multi-selected
    if (marked >= 2) {
        $('#xq_process_saveresult').hide(); // ẩn nút Lưu
        $('.contain-hinh').empty();
        CKEDITOR.instances.xq_process_description.setReadOnly(true); // disabled nhập mô tả kết quả
        CKEDITOR.instances.xq_process_description.setData("");
        $('#xq_process_result').attr('disabled', true); // disabled nhập kết luận
        $('#xq_process_result').val(''); // disabled nhập kết luận

        $all.each(function () {
            var $row = $(this).closest('.row-service');
            $row.removeClass('active');
            if (this.checked) $row.addClass('multi-selected');
            else $row.removeClass('multi-selected');
        });
    } else {
        //var $row = $(this).closest('.row-service');
        //$row.removeClass('active');
        $('#xq_process_saveresult').show();
        CKEDITOR.instances.xq_process_description.setReadOnly(false);
        CKEDITOR.instances.xq_process_description.setData("");
        $('#xq_process_result').attr('disabled', false);
        $('#xq_list_service .row-service').removeClass('multi-selected');
    }

    // === Điểm thêm mới theo yêu cầu ===
    // Nếu sau thao tác chỉ còn 1 checkbox được chọn:
    // -> Lấy resultCDHAId còn lại và gọi Process_CheckedBoxOnRow(resultCDHAId)
    if (marked === 1 && !window.__xq_row_selecting) {
        console.log("vodayyy");
        var $only = $all.filter(':checked').first();
        var resultId = $only.val();
        console.log(resultId);

        // Tránh loop sự kiện change khi Process_CheckedBoxOnRow thay đổi checkbox
        window.__xq_row_selecting = true;
        setTimeout(function () {
            try {
                Process_CheckedBoxOnRow(resultId);
            } finally {
                window.__xq_row_selecting = false;
            }
        }, 0);
    }
});
//***************************************************************************************** Return Result

function ReturnResult_Refresh() {
    $.ajax({
        url: "/XQ_ReturnResult/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#xq_returnresult_timeSearchFrom").val(today);
            //$("#xq_returnresult_timeSearchTo").val(today);

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
    var xq_getsample_pidorseq = $("#xq_returnresult_pidorseq").val();
    var timeSearchFrom = $("#xq_returnresult_timeSearchFrom").val();
    var timeSearchTo = $("#xq_returnresult_timeSearchTo").val();
    $.ajax({
        url: "/XQ_ReturnResult/Search?" + "pidorseq=" + xq_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#xq_returnresult_pidorseq").val('');
    $('#xq_returnresult_id').val('');
    $('#xq_returnresult_patientId').val('');
    $('#xq_returnresult_seq').val('');
    $('#xq_returnresult_sid').val('');
    $('#xq_returnresult_patientName').val('');
    $('#xq_returnresult_age').val('');
    $('#xq_returnresult_sex').val('');
    $('#xq_returnresult_obj').val('');
    $('#xq_returnresult_type').val('');
    $('#xq_returnresult_location').val('');
    $('#xq_returnresult_doctor').val('');
    $('#xq_returnresult_getSampleTime').val('');
    $('#xq_returnresult_returnResultTime').val('');
    $('#xq_returnresult_location').val('');
    $('#xq_returnresult_doctor').val('');
    $('#xq_returnresult_userReturnResultXQ').val('');
    $('#xq_returnresult_address').val('');
    $('#xq_returnresult_diagnostic').val('');
    $('#tbody-gridview-service').empty();
}

function ReturnResult_GetPatientInfo(id) {
    $.ajax({
        url: "/XQ_ReturnResult/GetPatientInfo?id=" + id,
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
    var userId = $("#xq_returnresult_userLoginId").val();
    console.log(userId);
    if (!userId) {
        $("#xq_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/XQ_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            console.log(u);
            if (!u) {
                $("#xq_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#xq_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#xq_returnresult_signerCCCD").text("Không lấy đượcthông tin CCCD.");
        },
    });
}

function ReturnResult_Load_BSThucHienTheoDichVu() {
    var userId = $("#xq_returnresult_bsThucHien").val();
    console.log(userId);
    if (!userId) {
        $("#xq_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/XQ_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            console.log(u);
            if (!u) {
                $("#xq_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            var name = u.name || "";
            $("#xq_returnresult_userReturnResultXQ").val(name);
            $("#xq_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#xq_returnresult_signerCCCD").text("Không lấy đượcthông tin CCCD.");
        },
    });
}
function ReturnResult_GetListServiceForPatient(id) {
    $.ajax({
        url: "/XQ_ReturnResult/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_xq-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function ReturnResult_Invalid() {
    var id = $('#xq_returnresult_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    // Lấy tất cả checkbox được chọn và KeyResultForHis tương ứng
    const selectedKeys = [];

    $('.xq-returnresult-chk-service:checked').each(function () {
        var keyResult = $(this).data('key-result');
        if (keyResult) {
            selectedKeys.push(keyResult);
        }
    });

    if (selectedKeys.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn ít nhất một dịch vụ!");
        return;
    }

    console.log("KeyResultForHis được chọn:", selectedKeys);
    if (confirm('Bạn muốn InValid kết quả của bệnh nhân ?')) {
        $.ajax({
            url: "/XQ_ReturnResult/Invalid",
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'text',
            data: JSON.stringify({
                patientId: parseInt(id),
                keyResultList: selectedKeys,
                categoryCode: 'XQ'
            }),
            success: function (result) {
                if (result === 'True') {
                    SwalHelper.Toast.success("Invalid thành công và đã xóa file PDF!");
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

function ReturnResult_Invalid_RemoveDigitalSign() {
    var id = $('#xq_returnresult_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    // Lấy ServiceId
    var resultCDHAId = "";
    var selectedKeys = [];
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            if ($(this).is(':checked')) {
                resultCDHAId = $(this).val();
                var keyResult = $(this).data('key-result');
                if (keyResult) selectedKeys.push(keyResult);
            }
        });
    });

    if (resultCDHAId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
        return;
    }
    if (confirm('Bạn muốn InValid kết quả của bệnh nhân (hủy Ký Số kết quả này) ?')) {
        $.ajax({
            url: "/XQ_ReturnResult/Invalid",
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'text',
            data: JSON.stringify({
                patientId: parseInt(id),
                keyResultList: selectedKeys,
                categoryCode: 'XQ'
            }),
            success: function (result) {
                if (result === 'True') {
                    SwalHelper.Toast.success("Invalid thành công và đã hủy ký số!");
                    UpdateSignStatus(resultCDHAId);
                    ReturnResult_Refresh();
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

function ReturnResult_Get_Count() {
    $.ajax({
        url: "/XQ_ReturnResult/Get_Count/",
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
    var patientId = $('#xq_returnresult_id').val();
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
        url: "/XQ_ReturnResult/Print?resultCDHAId=" + resultCDHAId,
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

                // ===== XỬ LÝ CHO CẢ DESKTOP VÀ MOBILE =====

                // Kiểm tra thiết bị
                var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);

                if (isMobile) {
                    // MOBILE: Tạo link download
                    var link = document.createElement('a');
                    link.href = URL.createObjectURL(blob);
                    link.download = 'KetQua_XQuang_' + resultCDHAId + '_' + new Date().getTime() + '.pdf';
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
                        link.download = 'KetQua_XQuang_' + resultCDHAId + '.pdf';
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
            SwalHelper.Toast.error("In không thành công. Vui lòng kiểm tra lại!");
        }
    });
}

//function ReturnResult_CheckedBoxOnRow(id) {
//    $(".row-service .form-check-input").each(function () {
//        var $chk = $(this);
//        var resultCdhaId = String($chk.val());
//        var isTarget = (resultCdhaId === String(id));
//        $chk.prop("checked", isTarget);

//        if (isTarget) {
//            ReturnResult_Load_ResultAndImage_ForService(id);

//            // Check digital signature status and toggle buttons
//            var $row = $chk.closest('.row-service');
//            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
//            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
//        }
//    });

//    // Update checkAll status
//    var $rows = $('.xq-returnresult-chk-service');
//    var total = $rows.length;
//    var marked = $rows.filter(':checked').length;
//    var allChecked = total > 0 && marked === total;
//    $('#xq_returnresult_chk_all').prop('checked', allChecked);
//}

function ReturnResult_CheckedBoxOnRow(id, event) {
    // Nếu click trực tiếp vào checkbox thì không xử lý (để người dùng tự chọn multiple)
    if (event && event.target.type === 'checkbox') {
        // Chỉ cập nhật trạng thái "chọn tất cả"
        var $rows = $('.xq-returnresult-chk-service');
        var total = $rows.length;
        var marked = $rows.filter(':checked').length;
        var allChecked = total > 0 && marked === total;
        $('#xq_returnresult_chk_all').prop('checked', allChecked);
        
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
            CKEDITOR.instances["xq_returnresult_description"].setData("");
            $("#xq_returnresult_result").val("");
            $("#xq_returnresult_suggest").val("");
            ReturnResult_ToggleDigitalSignButtons(false);
        } else {
            // Clear khi không có gì được chọn
            const $containHinh = $('.contain-hinh');
            $containHinh.empty();
            CKEDITOR.instances["xq_returnresult_description"].setData("");
            $("#xq_returnresult_result").val("");
            $("#xq_returnresult_suggest").val("");
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
            var bs_thuc_hien = $chk.data("bs-thuc-hien");
            console.log("bs THUC HIEN: ", bs_thuc_hien);
            $('#xq_returnresult_bsThucHien').val(bs_thuc_hien).trigger("change");
            ReturnResult_Load_BSThucHienTheoDichVu();

            ReturnResult_Load_ResultAndImage_ForService(id);

            // Check digital signature status and toggle buttons
            var $row = $chk.closest('.row-service');
            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
        }
    });

    // Update checkAll status
    var $rows = $('.xq-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;
    var allChecked = total > 0 && marked === total;
    $('#xq_returnresult_chk_all').prop('checked', allChecked);
}

// Event handler cho checkbox để chỉ xử lý việc chọn multiple
$(document).on('change', '.xq-returnresult-chk-service', function () {
    var $rows = $('.xq-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Update trạng thái "chọn tất cả"
    var allChecked = total > 0 && marked === total;
    $('#xq_returnresult_chk_all').prop('checked', allChecked);

    // Xử lý hiển thị chi tiết
    var selectedCheckboxes = $(".row-service .form-check-input:checked");
    
    if (selectedCheckboxes.length === 1) {
        console.log("vo === 1");
        // Chỉ 1 được chọn → hiển thị chi tiết
        var selectedId = selectedCheckboxes.first().val();
        ReturnResult_Load_ResultAndImage_ForService(selectedId);
        
        var $row = selectedCheckboxes.first().closest('.row-service');
        var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
        ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
    } else if (selectedCheckboxes.length > 1) {
        console.log("vo > 1");
        // Nhiều hơn 1 → clear chi tiết
        const $containHinh = $('.contain-hinh');
        $containHinh.empty();
        CKEDITOR.instances["xq_returnresult_description"].setData("");
        $("#xq_returnresult_result").val("");
        $("#xq_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    } else {
        console.log("vo else");
        // Không có gì được chọn → clear
        const $containHinh = $('.contain-hinh');
        $containHinh.empty();
        CKEDITOR.instances["xq_returnresult_description"].setData("");
        $("#xq_returnresult_result").val("");
        $("#xq_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    }
});

// Helper function to toggle digital signature buttons
function ReturnResult_ToggleDigitalSignButtons(isDigitallySigned) {
    console.log('Digital signature status:', isDigitallySigned);

    // Toggle "Tải PDF đã ký" button
    var $printSignedBtn = $('#xq_returnresult_print_signed');
    if ($printSignedBtn.length) {
        if (isDigitallySigned) {
            $printSignedBtn.show().prop('disabled', false);
        } else {
            $printSignedBtn.hide().prop('disabled', true);
        }
    }

    // Toggle "Invalid & Xóa Ký Số" button  
    var $invalidRemoveSignBtn = $('#xq_returnresult_invalid_removedigitalsign');
    if ($invalidRemoveSignBtn.length) {
        if (isDigitallySigned) {
            $invalidRemoveSignBtn.show().prop('disabled', false);
        } else {
            $invalidRemoveSignBtn.hide().prop('disabled', true);
        }
    }
}

function ReturnResult_Load_ResultAndImage_ForService(id) {
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["xq_returnresult_description"].setData("");
    $("#xq_returnresult_result").val("");
    $("#xq_returnresult_suggest").val("");
    $.ajax({
        url: "/XQ_ReturnResult/GetImageForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                if (value.id) {
                    const $hinh = $('<div class="hinh"></div>');
                    $hinh.append('<img id="' + value.id + '" src="' + value.name + '" onclick="ReturnResult_Hinh(' + value.id + ', this)"/>');
                    $containHinh.append($hinh);
                }
                CKEDITOR.instances["xq_returnresult_description"].setData(value.description);
                $("#xq_returnresult_result").val(value.result);
                $("#xq_returnresult_suggest").val(value.suggest);
            });
        }
    });
}
function ReturnResult_Hinh(imageCDHAId, img) {
    $('.contain-hinh img').removeClass('selected');
    $(img).toggleClass('selected');
    $("#xq_returnresult_xem").val(imageCDHAId);
}


function ReturnResult_ViewSignedPdf() {

    function openSigned(id) {
        if (!id) { SwalHelper.Toast.warning("Chưa có signStoreId cho dịch vụ này!"); return; }
        var url = "/api/ExternalSign/view-signed/" + encodeURIComponent(id);
        window.open(url, "_blank");
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
        url: "/XQ_ReturnResult/GetSignStoreId",
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
function UpdateSignStatus(resultCDHAId) {
    $.ajax({
        url: "/DigitalSign/UpdateSignStatus_Result?resultCDHAId=" + resultCDHAId,
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
    var fromDate = document.getElementById('xq_returnresult_timeSearchFrom').value;
    var toDate = document.getElementById('xq_returnresult_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/XQ_ReturnResult/SaveSearchDates',
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

// ========================== KÝ SỐ PDF CHO TAB ĐÃ XONG (RETURN RESULT) ==========================
function ReturnResult_SignPdf_Multi(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    var patientId = $('#xq_returnresult_id').val();
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
    var signerCCCD = $('#xq_returnresult_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#xq_returnresult_signerCCCD').focus();
        return;
    }

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#xq_returnresult_patientId').val() || $('#xq_returnresult_sid').val() || "";
    var patientMaBenhAn = $('#xq_returnresult_maBenhAn').val() || $('#xq_returnresult_sid').val() || "";
    var patientName = $('#xq_returnresult_patientName').val() || "";
    var doctorName = $('#xq_returnresult_userReturnResultXQ option:selected').text() || $('#xq_returnresult_userReturnResultXQ').val() || "";
    var doctorId = $('#xq_returnresult_userLoginId').val();
    var performedAt = $('#xq_returnresult_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "XQuang"; // bạn có thể thay động tùy màn hình

    console.log("Chuẩn bị ký số cho các resultCDHAId:", resultIds);
    console.log(patientCode);
    console.log(patientMaBenhAn);
    console.log(patientName);
    console.log(doctorName);
    console.log(doctorId);
    console.log(doctorName);
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
                    url: "/XQ_ReturnResult/Print?resultCDHAId=" + rid,
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
                                    url: "/XQ_ReturnResult/SaveSignStoreIdForResultCDHA",
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
                                                    referenceType: formType ?? "XQuang",
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
// Tick/untick tất cả cho ReturnResult
$(document).on('change', '#xq_returnresult_chk_all', function () {
    var checked = this.checked === true;
    // chỉ tick các dịch vụ đang hiển thị
    $('.xq-returnresult-chk-service').prop('checked', checked);

    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["xq_returnresult_description"].setData("");
    $("#xq_returnresult_result").val("");
    $("#xq_returnresult_suggest").val("");
    ReturnResult_ToggleDigitalSignButtons(false);
});

// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả" cho ReturnResult
$(document).on('change', '.xq-returnresult-chk-service', function () {
    var $rows = $('.xq-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Nếu tất cả đều check => check header; ngược lại bỏ check header
    var allChecked = total > 0 && marked === total;
    $('#xq_returnresult_chk_all').prop('checked', allChecked);
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