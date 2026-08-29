// Tool tip cho control của form
//$(function () {
//    $('[data-toggle="tooltip"]').tooltip()
//})


// *********************************************************************************** Select 2
$(function () {
    GetSample_SetSelect2_01(true);
})
// Auto chạy hàm *_Search sau khi quét barcode tìm ở input tìm kiếm bệnh nhân
$(function () {
    if (typeof window.BarcodeScan === 'undefined') return;

    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#sat_getsample_pidorseq', window.GetSample_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#sat_process_pidorseq', window.Process_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#sat_returnresult_pidorseq', window.ReturnResult_Search, {
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
    // Gắn Enter key cho các input field tìm kiếm của XN_Process
    $('#sat_process_pidorseq, #sat_process_timeSearchFrom, #sat_process_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            Process_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của GetSample
    $('#sat_getsample_pidorseq, #sat_getsample_timeSearchFrom, #sat_getsample_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            GetSample_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của ReturnResult
    $('#sat_returnresult_pidorseq, #sat_returnresult_timeSearchFrom, #sat_returnresult_timeSearchTo').on('keydown', function (e) {
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
            if ($('.sat-left-header').length === 0) {
                var header = $('<div class="sat-left-header">' +
                    '<div class="sat-left-header-title">Danh sách bệnh nhân</div>' +
                    '<button class="sat-left-header-close"><i class="bi bi-x-lg"></i></button>' +
                    '</div>');
                $('.sat-left').prepend(header);
            }
        } else {
            // Xóa các elements mobile trên desktop
            $('.mobile-toggle-patient-list').remove();
            $('.mobile-patient-list-overlay').remove();
            $('.sat-left-header').remove();
            $('.sat-left').removeClass('show');
        }
    }

    // Khởi tạo khi load trang
    initMobilePatientList();

    // Click vào nút toggle
    $(document).on('click', '.mobile-toggle-patient-list', function () {
        $('.sat-left').addClass('show');
        $('.mobile-patient-list-overlay').addClass('show');
        $(this).addClass('active');
        // Prevent scroll trên body
        $('body').css('overflow', 'hidden');
    });

    // Click vào nút đóng trong header
    $(document).on('click', '.sat-left-header-close', function () {
        closePatientList();
    });

    // Click vào overlay để đóng
    $(document).on('click', '.mobile-patient-list-overlay', function () {
        closePatientList();
    });

    // Hàm đóng danh sách
    function closePatientList() {
        $('.sat-left').removeClass('show');
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

    $(document).on('touchstart', '.sat-left', function (e) {
        if (window.innerWidth <= 768) {
            startX = e.touches[0].clientX;
            isDragging = true;
        }
    });

    $(document).on('touchmove', '.sat-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            currentX = e.touches[0].clientX;
            var diff = currentX - startX;

            // Chỉ cho phép swipe sang trái
            if (diff < 0) {
                $(this).css('transform', 'translateX(' + diff + 'px)');
            }
        }
    });

    $(document).on('touchend', '.sat-left', function (e) {
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
        $('#sat_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#sat_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#sat_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#sat_getsample_location').select2({
            placeholder: "-- Chọn --"
        });
    });

    if (isLoadPage) {
        $('#sat_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#sat_getsample_doctor').select2({
            placeholder: "-- Chọn --"
        });
    });


    if (isLoadPage) {
        $('#sat_process_sampleresult').val("");
    }
    $(document).ready(function () {
        $('#sat_process_sampleresult').select2({
            placeholder: "-- Chọn --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#sat_process_userReturnResultSAT').select2({
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
                if (e.id === "sat_getsample_location") {
                    $('#sat_getsample_location').select2('focus');
                }
                else if (e.id === "sat_getsample_doctor") {
                    $('#sat_getsample_doctor').select2('focus');
                }
                else if (e.id === "sat_getsample_userReturnResultSA") {
                    $('#sat_getsample_userReturnResultSA').select2('focus');
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
        url: "/SA_TIM_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#sat_getsample_timeSearchFrom").val(today);
            //$("#sat_getsample_timeSearchTo").val(today);
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
        url: "/SA_TIM_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#sat_getsample_timeSearchFrom").val(today);
            $("#sat_getsample_timeSearchTo").val(today);
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Search() {
    var sat_getsample_pidorseq = $("#sat_getsample_pidorseq").val();
    var timeSearchFrom = $("#sat_getsample_timeSearchFrom").val();
    var timeSearchTo = $("#sat_getsample_timeSearchTo").val();
    $.ajax({
        url: "/SA_TIM_GetSample/Search?" + "pidorseq=" + sat_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
$("#sat_getsample_savepatient").hide();
$("#sat_getsample_cancelpatient").hide();
function GetSample_HideButton(_new, _save, _delete, _cancel, _addservice, _getsample) {
    if (_new == 1) {
        $("#sat_getsample_newpatient").hide();
    }
    else {
        $("#sat_getsample_newpatient").show();
    }

    if (_save == 1) {
        $("#sat_getsample_savepatient").hide();
    }
    else {
        $("#sat_getsample_savepatient").show();
    }

    if (_delete == 1) {
        $("#sat_getsample_deletepatient").hide();
    }
    else {
        $("#sat_getsample_deletepatient").show();
    }

    if (_cancel == 1) {
        $("#sat_getsample_cancelpatient").hide();
    }
    else {
        $("#sat_getsample_cancelpatient").show();
    }

    if (_addservice == 1) {
        $("#sat_getsample_addService").hide();
    }
    else {
        $("#sat_getsample_addService").show();
    }

    if (_getsample == 1) {
        $("#sat_getsample_processresult").hide();
    }
    else {
        $("#sat_getsample_processresult").show();
    }
}


function GetSample_GetPatientInfo(id) {
    GetSample_HideButton(false, true, false, true, false, false);
    $.ajax({
        url: "/SA_TIM_GetSample/GetPatientInfo?id=" + id,
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
        url: "/SA_TIM_GetSample/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#sat-right-service-gridview").html(result);
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
    $("#sat_getsample_pidorseq").val('');
    $('#sat_getsample_id').val('');
    $('#sat_getsample_patientId').val('');
    $('#sat_getsample_seq').val('');
    $('#sat_getsample_sid').val('');
    $('#sat_getsample_patientName').val('');
    $('#sat_getsample_age').val('');
    $('#sat_getsample_sex').val('');
    $('#sat_getsample_obj').val('');
    $('#sat_getsample_type').val('');
    $('#sat_getsample_location').val('');
    $('#sat_getsample_doctor').val('');
    $('#sat_getsample_getSampleTime').val(dateTime);
    $('#sat_getsample_address').val('');
    $('#sat_getsample_diagnostic').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
    GetSample_SetSelect2_01(false);
}

function GetSample_NewPatient() {
    $('#sat_getsample_patientId').focus();
    GetSample_HideButton(true, false, true, false, true, true);
    GetSample_ResetInput();
}


function GetSample_DeletePatient() {
    var id = $('#sat_getsample_id').val();
    if (id == '') {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else {
        var choice = confirm("Bạn muốn xoá bệnh nhân và tất cả chỉ định xét nghiệm?");
        if (choice) {
            $.ajax({
                url: "/SA_TIM_GetSample/DeletePatientAndService?id=" + id,
                type: "POST",
                dataType: "text",
                cache: false,
                success: function (result) {
                    if (result === '') {
                        SwalHelper.Alert.error('Lỗi', 'Xoá không thành công. Vui lòng kiểm tra lại!');
                    }
                    else {
                        GetSample_HideButton(false, true, false, true, false, false)
                        GetSample_Refresh(result);
                        GetSample_Get_Count();
                    }
                },
                error: function () {
                    SwalHelper.Alert.error('Lỗi', 'Xoá không thành công. Vui lòng kiểm tra lại!');
                }
            });
        }
    }
}

function GetSample_CancelPatient() {
    GetSample_HideButton(false, true, false, true, false, false);
    var id = $('#sat_getsample_id').val();
    if (id == '') {
        GetSample_ResetInput()
    }
    else {
        GetSample_GetPatientInfo(id);
    }
}

function GetSample_GetSID(seq) {
    $.ajax({
        url: "/SA_TIM_GetSample/GetSID?seq=" + seq,
        type: "GET",
        dataType: "text",
        cache: false,
        success: function (result) {
            $('#sat_getsample_sid').val(result);
        }
    });
}

function GetSample_SavePatient() {
    var validate = GetSample_ValidateInput('patientInfo');
    if (validate) {
        var id = $('#sat_getsample_id').val();
        var patientId = $('#sat_getsample_patientId').val();
        var seq = $('#sat_getsample_seq').val();
        var sid = $('#sat_getsample_sid').val();
        var patientName = $('#sat_getsample_patientName').val();
        var age = $('#sat_getsample_age').val();
        var sex = $('#sat_getsample_sex').val();
        var obj = $('#sat_getsample_obj').val();
        var type = $('#sat_getsample_type').val();
        var location = $('#sat_getsample_location').val();
        var doctor = $('#sat_getsample_doctor').val();
        var getSampleTime = $('#sat_getsample_getSampleTime').val();
        var address = $('#sat_getsample_address').val();
        var diagnostic = $('#sat_getsample_diagnostic').val();
        var category = $('#select-category').val();
        var service = $('#select-service').val();

        $.ajax({
            url: "/SA_TIM_GetSample/SavePatient?id= " + id + "&&patientId=" + patientId + "&&seq=" + seq + "&&sid=" + sid + "&&patientName=" + patientName + "&&age=" + age + "&&sex=" + sex + "&&obj=" + obj + "&&type=" + type + "&&location=" + location + "&&doctor=" + doctor + "&&getSampleTime=" + getSampleTime + "&&address=" + address + "&&diagnostic=" + diagnostic + "&&service=" + service,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result == "") {
                    SwalHelper.Alert.error('Lỗi', 'Lưu không thành công. Vui lòng kiểm tra lại!');
                }
                else {
                    GetSample_GetPatientInfo(result);
                    GetSample_Refresh_No_Clear_PatientInfo();
                    GetSample_SetSelect2_01(false);
                    GetSample_Get_Count();
                }
            },
            error: function () {
                SwalHelper.Alert.error('Lỗi', 'Lưu không thành công. Vui lòng kiểm tra lại!');
            }
        });
    }
}

function GetSample_ProcessResult() {
    var id = $('#sat_getsample_id').val();
    if (id === "") {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else {
        var getSampleTime = $('#sat_getsample_getSampleTime').val();
        $.ajax({
            url: "/SA_TIM_GetSample/ProcessResult?id= " + id + "&getSampleTime=" + getSampleTime,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result === 'True') {
                    GetSample_Refresh();
                    GetSample_Get_Count();
                }
                else {
                    SwalHelper.Alert.error('Lỗi', 'Xử lý kết quả không thành công. Vui lòng kiểm tra lại!');
                }
            },
            error: function () {
                SwalHelper.Alert.error('Lỗi', 'Xử lý kết quả không thành công. Vui lòng kiểm tra lại!');
            }
        });
    }
}

function GetSample_AddService() {
    var id = $('#sat_getsample_id').val();
    if (id === "") {
        $('#addServiceForm').modal('hide');
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else {
        GetSample_SetSelect2_02();
    }
}

function GetSample_DeleteServiceForPatient(idResultCDHA) {
    var patientId = $('#sat_getsample_id').val();
    $.ajax({
        url: "/SA_TIM_GetSample/DeleteServiceForPatient?id=" + idResultCDHA,
        type: "POST",
        dataType: "text",
        cache: false,
        success: function (result) {
            if (result == 'True') {
                GetSample_GetListServiceForPatient(patientId);
            }
            else {
                SwalHelper.Alert.error("Không thể xoá. Vui lòng kiểm tra lại !");
            }
        }
    });
}


function GetSample_AddServiceForPatient() {
    var serviceId = $('#sat_getsample_service').val();
    var patientId = $('#sat_getsample_id').val();
    var doctorId = $('#sat_getsample_doctor').val();
    if (serviceId === "" || patientId === "") {
        SwalHelper.Alert.error('Lỗi', 'Chỉ định dịch vụ không thành công. Vui lòng kiểm tra lại !')
    }
    else {
        $.ajax({
            url: "/SA_TIM_GetSample/AddServiceForPatient?patientId=" + patientId + "&&serviceId=" + serviceId + "&&doctorId=" + doctorId,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result == 'True') {
                    GetSample_GetListServiceForPatient(patientId);
                }
                else {
                    SwalHelper.Alert.error('Lỗi', 'Thêm dịch vụ không thành công. Vui lòng kiểm tra lại!');
                }
            },
            error: function () {
                SwalHelper.Alert.error('Lỗi', 'Thêm dịch vụ không thành công. Vui lòng kiểm tra lại!');
            }
        });

        GetSample_SetSelect2_02();
    }
}


function GetSample_Get_Count() {
    $.ajax({
        url: "/SA_TIM_GetSample/Get_Count/",
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
    var fromDate = document.getElementById('sat_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('sat_getsample_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/SA_TIM_GetSample/SaveSearchDates',
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
                if (e.id === "sat_process_userReturnResultSAT") {
                    $('#sat_process_userReturnResultSAT').select2('focus');
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
        url: "/SA_TIM_Process/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#sat_process_timeSearchFrom").val(today);
            //$("#sat_process_timeSearchTo").val(today);
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
    var sat_process_pidorseq = $("#sat_process_pidorseq").val();
    var timeSearchFrom = $("#sat_process_timeSearchFrom").val();
    var timeSearchTo = $("#sat_process_timeSearchTo").val();
    $.ajax({
        url: "/SA_TIM_Process/Search?" + "pidorseq=" + sat_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#sat_process_pidorseq").val('');
    $('#sat_process_id').val('');
    $('#sat_process_patientId').val('');
    $('#sat_process_seq').val('');
    $('#sat_process_sid').val('');
    $('#sat_process_patientName').val('');
    $('#sat_process_age').val('');
    $('#sat_process_sex').val('');
    $('#sat_process_obj').val('');
    $('#sat_process_type').val('');
    $('#sat_process_location').val('');
    $('#sat_process_doctor').val('');
    $('#sat_process_getSampleTime').val('');
    $('#sat_process_returnResultTime').val('');
    $('#sat_process_location').val('');
    $('#sat_process_doctor').val('');
    $('#sat_process_userReturnResultSAT').val('');
    $('#sat_process_address').val('');
    $('#sat_process_diagnostic').val('');
    $('#tbody-gridview-service').empty();
    $('#imageCDHA').empty();
    $('#sat_list_service').empty();
    $('#sat_process_result').val('Chức năng tâm thu thất trái bảo tồn (EF# %).');
    $('#sat_process_suggest').empty();
    CKEDITOR.instances["sat_process_description"].setData("");
    Process_SetSelect2_03(true);
}

function Process_GetPatientInfo(id) {
    $.ajax({
        url: "/SA_TIM_Process/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_patientInfo").html(result);
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            Process_SetSelect2_03();
            // Auto-chọn bác sĩ = user đang login (nếu chưa có giá trị)
            var loginId = $("#sat_process_userLoginId").val();
            var $sel = $("#sat_process_userReturnResultSAT");
            if (loginId && (!$sel.val() || $sel.val() === "")) {
                $sel.val(loginId).trigger("change");
            }
            // Gắn handler đổi bác sĩ
            $(document)
                .off("change", "#sat_process_userReturnResultSAT")
                .on("change", "#sat_process_userReturnResultSAT", Process_Load_SelectedDoctorInfo);
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
        url: "/SA_TIM_Process/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_sat-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}


function Process_SaveResult() {
    var patientId = $('#sat_process_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
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
                SwalHelper.Toast.warning('Vui lòng chọn dịch vụ!');
            }
            else {
                var returnResultTime = $('#sat_process_returnResultTime').val();
                var userReturnResult = $('#sat_process_userReturnResultSAT').val();
                var description = CKEDITOR.instances['sat_process_description'].getData();
                var result = $('#sat_process_result').val();
                var suggest = $('#sat_process_suggest').val();

                var sieuamtimObj = {
                    sat_chieucao: $('#sat_chieucao').val(),
                    sat_cannang: $('#sat_cannang').val(),
                    sat_bsa: $('#sat_bsa').val(),
                    sat_bmi: $('#sat_bmi').val(),
                    sat_dmchu: $('#sat_dmchu').val(),
                    sat_nhitrai: $('#sat_nhitrai').val(),
                    sat_thatphai: $('#sat_thatphai').val(),
                    sat_ivsd: $('#sat_ivsd').val(),
                    sat_ivss: $('#sat_ivss').val(),
                    sat_lvidd: $('#sat_lvidd').val(),
                    sat_lvids: $('#sat_lvids').val(),
                    sat_lvpwd: $('#sat_lvpwd').val(),
                    sat_lvpws: $('#sat_lvpws').val(),
                    sat_rvdd: $('#sat_rvdd').val(),
                    sat_lvedv: $('#sat_lvedv').val(),
                    sat_fs: $('#sat_fs').val(),
                    sat_ef: $('#sat_ef').val(),
                    sat_vantoctoida_haila: $('#sat_vantoctoida_haila').val(),
                    sat_ea_haila: $('#sat_ea_haila').val(),
                    sat_chenhaptoida_haila: $('#sat_chenhaptoida_haila').val(),
                    sat_gdmean_haila: $('#sat_gdmean_haila').val(),
                    sat_dohovan_haila: $('#sat_dohovan_haila').val(),
                    sat_loaiho_haila: $('#sat_loaiho_haila').val(),
                    sat_hepvanhaila_haila: $('#sat_hepvanhaila_haila').val(),
                    sat_vantoctoida_bala: $('#sat_vantoctoida_bala').val(),
                    sat_chenhaptoida_bala: $('#sat_chenhaptoida_bala').val(),
                    sat_chenhaptb_bala: $('#sat_chenhaptb_bala').val(),
                    sat_hovan_bala: $('#sat_hovan_bala').val(),
                    sat_paps_bala: $('#sat_paps_bala').val(),
                    sat_vantoctoida_dongmachphoi: $('#sat_vantoctoida_dongmachphoi').val(),
                    sat_chenhaptoida_dongmachphoi: $('#sat_chenhaptoida_dongmachphoi').val(),
                    sat_gdmean_dongmachphoi: $('#sat_gdmean_dongmachphoi').val(),
                    sat_papm_dongmachphoi: $('#sat_papm_dongmachphoi').val(),
                    sat_papd_dongmachphoi: $('#sat_papd_dongmachphoi').val(),
                    sat_vantoctoida_dongmachchu: $('#sat_vantoctoida_dongmachchu').val(),
                    sat_chenhaptoida_dongmachchu: $('#sat_chenhaptoida_dongmachchu').val(),
                    sat_hovandmchu_dongmachchu: $('#sat_hovandmchu_dongmachchu').val(),
                    sat_hepvandmchu_dongmachchu: $('#sat_hepvandmchu_dongmachchu').val(),
                    sat_dongquavachliennhi: $('#sat_dongquavachliennhi').val(),
                    sat_dongquavachlienthat: $('#sat_dongquavachlienthat').val(),
                    sat_dongbatthuongkhac: $('#sat_dongbatthuongkhac').val(),
                    sat_tuthetim_2d: $('#sat_tuthetim_2d').val(),
                    sat_dmphoi_2d: $('#sat_dmphoi_2d').val(),
                    sat_mangngoaitim_2d: $('#sat_mangngoaitim_2d').val(),
                    sat_dmvanh_2d: $('#sat_dmvanh_2d').val(),
                    sat_nhitrai_2d: $('#sat_nhitrai_2d').val(),
                    sat_xoangvanh_2d: $('#sat_xoangvanh_2d').val(),
                    sat_nhiphai_2d: $('#sat_nhiphai_2d').val(),
                    sat_vanhaila_2d: $('#sat_vanhaila_2d').val(),
                    sat_tinhmach_2d: $('#sat_tinhmach_2d').val(),
                    sat_vanbala_2d: $('#sat_vanbala_2d').val(),
                    sat_dmchu_2d: $('#sat_dmchu_2d').val(),
                    sat_vandmc_2d: $('#sat_vandmc_2d').val(),
                    sat_vandmp_2d: $('#sat_vandmp_2d').val()
                }

                var data = {
                    patientId: patientId,
                    resultCDHAId: resultCDHAId,
                    returnResultTime: returnResultTime,
                    userReturnResult: userReturnResult,
                    description: description,
                    result: result,
                    suggest: suggest,
                    sieuamtim: JSON.stringify(sieuamtimObj)
                };

                $.ajax({
                    url: "/SA_TIM_Process/SaveResult/",
                    data: JSON.stringify(data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    type: "POST",
                    success: function (result) {
                        if (result == 'True') {
                            SwalHelper.Toast.success("Lưu thành công.");
                        }
                        else {
                            SwalHelper.Alert.error("Lưu không thành công. Vui lòng kiểm tra lại!");
                        }
                    },
                    error: function () {
                        SwalHelper.Alert.error("Lưu không thành công. Vui lòng kiểm tra lại!");
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
            selectedImageIds: Array.isArray(opts.selectedImageIds) ? opts.selectedImageIds : null,
            sieuamtim: opts.sieuamtim || null,
        };
        if (isMulti) {
            $.ajax({
                url: "/SA_TIM_Process/ValidPrintMultiple/",
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
                url: "/SA_TIM_Process/ValidPrint/",
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
//function Process_ValidPrint() {
//    var patientId = $('#sat_process_id').val();
//    if (patientId === "") {
//        alert("Vui lòng chọn bệnh nhân!");
//    }
//    else {
//        var validate = Process_ValidateInput('process_patientInfo');
//        if (validate) {

//            // Lấy ServiceId
//            var resultCDHAId = "";
//            $(".row-service").each(function () {
//                $(this).find(".form-check-input").each(function () {
//                    var isChecked = $(this).is(':checked');
//                    if (isChecked) {
//                        resultCDHAId = $(this).val();
//                    }
//                })
//            })

//            if (resultCDHAId === "") {
//                alert("Vui lòng chọn dịch vụ!");
//            }
//            else {
//                $('#showWaitting').modal('show');
//                var returnResultTime = $('#sat_process_returnResultTime').val();
//                var userReturnResult = $('#sat_process_userReturnResultSAT').val();
//                var description = CKEDITOR.instances['sat_process_description'].getData();
//                var result = $('#sat_process_result').val();
//                var suggest = $('#sat_process_suggest').val();

//                var sieuamtimObj = {
//                    sat_chieucao: $('#sat_chieucao').val(),
//                    sat_cannang: $('#sat_cannang').val(),
//                    sat_bsa: $('#sat_bsa').val(),
//                    sat_bmi: $('#sat_bmi').val(),
//                    sat_dmchu: $('#sat_dmchu').val(),
//                    sat_nhitrai: $('#sat_nhitrai').val(),
//                    sat_thatphai: $('#sat_thatphai').val(),
//                    sat_ivsd: $('#sat_ivsd').val(),
//                    sat_ivss: $('#sat_ivss').val(),
//                    sat_lvidd: $('#sat_lvidd').val(),
//                    sat_lvids: $('#sat_lvids').val(),
//                    sat_lvpwd: $('#sat_lvpwd').val(),
//                    sat_lvpws: $('#sat_lvpws').val(),
//                    sat_rvdd: $('#sat_rvdd').val(),
//                    sat_lvedv: $('#sat_lvedv').val(),
//                    sat_fs: $('#sat_fs').val(),
//                    sat_ef: $('#sat_ef').val(),
//                    sat_vantoctoida_haila: $('#sat_vantoctoida_haila').val(),
//                    sat_ea_haila: $('#sat_ea_haila').val(),
//                    sat_chenhaptoida_haila: $('#sat_chenhaptoida_haila').val(),
//                    sat_gdmean_haila: $('#sat_gdmean_haila').val(),
//                    sat_dohovan_haila: $('#sat_dohovan_haila').val(),
//                    sat_loaiho_haila: $('#sat_loaiho_haila').val(),
//                    sat_hepvanhaila_haila: $('#sat_hepvanhaila_haila').val(),
//                    sat_vantoctoida_bala: $('#sat_vantoctoida_bala').val(),
//                    sat_chenhaptoida_bala: $('#sat_chenhaptoida_bala').val(),
//                    sat_chenhaptb_bala: $('#sat_chenhaptb_bala').val(),
//                    sat_hovan_bala: $('#sat_hovan_bala').val(),
//                    sat_paps_bala: $('#sat_paps_bala').val(),
//                    sat_vantoctoida_dongmachphoi: $('#sat_vantoctoida_dongmachphoi').val(),
//                    sat_chenhaptoida_dongmachphoi: $('#sat_chenhaptoida_dongmachphoi').val(),
//                    sat_gdmean_dongmachphoi: $('#sat_gdmean_dongmachphoi').val(),
//                    sat_papm_dongmachphoi: $('#sat_papm_dongmachphoi').val(),
//                    sat_papd_dongmachphoi: $('#sat_papd_dongmachphoi').val(),
//                    sat_vantoctoida_dongmachchu: $('#sat_vantoctoida_dongmachchu').val(),
//                    sat_chenhaptoida_dongmachchu: $('#sat_chenhaptoida_dongmachchu').val(),
//                    sat_hovandmchu_dongmachchu: $('#sat_hovandmchu_dongmachchu').val(),
//                    sat_hepvandmchu_dongmachchu: $('#sat_hepvandmchu_dongmachchu').val(),
//                    sat_dongquavachliennhi: $('#sat_dongquavachliennhi').val(),
//                    sat_dongquavachlienthat: $('#sat_dongquavachlienthat').val(),
//                    sat_dongbatthuongkhac: $('#sat_dongbatthuongkhac').val(),
//                    sat_tuthetim_2d: $('#sat_tuthetim_2d').val(),
//                    sat_dmphoi_2d: $('#sat_dmphoi_2d').val(),
//                    sat_mangngoaitim_2d: $('#sat_mangngoaitim_2d').val(),
//                    sat_dmvanh_2d: $('#sat_dmvanh_2d').val(),
//                    sat_nhitrai_2d: $('#sat_nhitrai_2d').val(),
//                    sat_xoangvanh_2d: $('#sat_xoangvanh_2d').val(),
//                    sat_nhiphai_2d: $('#sat_nhiphai_2d').val(),
//                    sat_vanhaila_2d: $('#sat_vanhaila_2d').val(),
//                    sat_tinhmach_2d: $('#sat_tinhmach_2d').val(),
//                    sat_vanbala_2d: $('#sat_vanbala_2d').val(),
//                    sat_dmchu_2d: $('#sat_dmchu_2d').val(),
//                    sat_vandmc_2d: $('#sat_vandmc_2d').val(),
//                    sat_vandmp_2d: $('#sat_vandmp_2d').val()
//                }
//                // Danh sách ảnh cần in
//                var selectedImageIds = $(".sat-print-check:checked")
//                    .map(function () { return $(this).val(); })
//                    .get();

//                if (selectedImageIds.length == 0) {
//                    alert("Chọn ít nhất 1 ảnh để in kết quả!");
//                    $('#showWaitting').modal('hide');
//                    return;
//                }

//                var data = {
//                    patientId: patientId,
//                    resultCDHAId: resultCDHAId,
//                    returnResultTime: returnResultTime,
//                    userReturnResult: userReturnResult,
//                    description: description,
//                    result: result,
//                    suggest: suggest,
//                    sieuamtim: JSON.stringify(sieuamtimObj),
//                    selectedImageIds: selectedImageIds
//                };
//                console.log(data);
//                $.ajax({
//                    url: "/SA_TIM_Process/ValidPrint/",
//                    data: JSON.stringify(data),
//                    contentType: "application/json; charset=utf-8",
//                    dataType: "text",
//                    type: "POST",
//                    success: function (response) {
//                        $('#showWaitting').modal('hide');
//                        if (response === "") {
//                            alert("Valid & In không thành công. Vui lòng kiểm tra lại!");
//                        }
//                        else {
//                            Process_Refresh();
//                            Process_Get_Count();
//                            var byteCharacters = atob(response);
//                            var byteNumbers = new Array(byteCharacters.length);
//                            for (var i = 0; i < byteCharacters.length; i++) {
//                                byteNumbers[i] = byteCharacters.charCodeAt(i);
//                            }
//                            var byteArray = new Uint8Array(byteNumbers);
//                            var file = new Blob([byteArray], { type: 'application/pdf;base64' });
//                            var fileURL = URL.createObjectURL(file);
//                            window.open(fileURL);
//                        }
//                    },
//                    error: function () {
//                        $('#showWaitting').modal('hide');
//                        alert("Lưu không thành công. Vui lòng kiểm tra lại!");
//                    }
//                });
//                return true;
//            }
//        }
//        else {
//            return false
//        }
//    }
//}

function Process_ValidPrint(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';

    var patientId = $('#sat_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Validate input chung (bác sĩ, thời điểm trả kết quả,...)
    var ok = Process_ValidateInput('process_patientInfo');
    if (!ok) return;

    var returnResultTime = $('#sat_process_returnResultTime').val();
    var userReturnResult = $('#sat_process_userReturnResultSAT').val();

    // Thu thập danh sách resultIds theo mode
    var resultIds = [];
    if (mode === 'current') {
        console.log("current");

        // lấy id của dòng đang active (tùy markup; ví dụ theo checkbox đang checked đầu tiên)
        var rid = $(".row-service .form-check-input:checked").first().val();
        if (rid) resultIds.push(rid);
    } else if (mode === 'selected') {
        console.log("selecteddđ");
        $(".row-service .form-check-input:checked").each(function () {
            var v = $(this).val();
            if (v) resultIds.push(v);
        });
    } else if (mode === 'all') {
        console.log("allllll");
        $(".row-service").each(function () {
            var v = $(this).find(".form-check-input").first().val();
            console.log(v);
            if (v) resultIds.push(v);
        });
    }
    console.log("resultIdss: ", resultIds);
    if (!resultIds.length) {
        SwalHelper.Toast.warning('Không tìm thấy dịch vụ nào để in!');
        return;
    }

    // Nếu chỉ có 1 dịch vụ => hành vi cũ (dùng nội dung editor hiện tại nếu cần)
    // Nếu nhiều dịch vụ => in lần lượt, dùng dữ liệu đã LƯU ở server (useSaved=true)
    var isMulti = resultIds.length > 1;

    // Lấy nội dung editor hiện tại (nếu bạn đang cho phép in 1 dịch vụ với nội dung đang chỉnh)
    // Có thể tuỳ chỉnh theo app của bạn:
    var description = isMulti ? null : (CKEDITOR.instances['sat_process_description'].getData() || null);
    var resultText = isMulti ? null : ($('#sat_process_result').val() || null);
    var suggest = isMulti ? null : ($('#sat_process_suggest').val() || null);
    var selectedImageIds = isMulti ? null : $(".sat-print-check:checked").map(function () { return $(this).val(); }).get(); // nếu có UI chọn ảnh riêng cho 1 dịch vụ

    var opts = {
        patientId: patientId,
        returnResultTime: returnResultTime,
        userReturnResult: userReturnResult,
        description: description,
        result: resultText,
        suggest: suggest,
        selectedImageIds: selectedImageIds
    };

    if (!isMulti) {
        var sieuamtimObj = {
            sat_chieucao: $('#sat_chieucao').val(),
            sat_cannang: $('#sat_cannang').val(),
            sat_bsa: $('#sat_bsa').val(),
            sat_bmi: $('#sat_bmi').val(),
            sat_dmchu: $('#sat_dmchu').val(),
            sat_nhitrai: $('#sat_nhitrai').val(),
            sat_thatphai: $('#sat_thatphai').val(),
            sat_ivsd: $('#sat_ivsd').val(),
            sat_ivss: $('#sat_ivss').val(),
            sat_lvidd: $('#sat_lvidd').val(),
            sat_lvids: $('#sat_lvids').val(),
            sat_lvpwd: $('#sat_lvpwd').val(),
            sat_lvpws: $('#sat_lvpws').val(),
            sat_rvdd: $('#sat_rvdd').val(),
            sat_lvedv: $('#sat_lvedv').val(),
            sat_fs: $('#sat_fs').val(),
            sat_ef: $('#sat_ef').val(),
            sat_vantoctoida_haila: $('#sat_vantoctoida_haila').val(),
            sat_ea_haila: $('#sat_ea_haila').val(),
            sat_chenhaptoida_haila: $('#sat_chenhaptoida_haila').val(),
            sat_gdmean_haila: $('#sat_gdmean_haila').val(),
            sat_dohovan_haila: $('#sat_dohovan_haila').val(),
            sat_loaiho_haila: $('#sat_loaiho_haila').val(),
            sat_hepvanhaila_haila: $('#sat_hepvanhaila_haila').val(),
            sat_vantoctoida_bala: $('#sat_vantoctoida_bala').val(),
            sat_chenhaptoida_bala: $('#sat_chenhaptoida_bala').val(),
            sat_chenhaptb_bala: $('#sat_chenhaptb_bala').val(),
            sat_hovan_bala: $('#sat_hovan_bala').val(),
            sat_paps_bala: $('#sat_paps_bala').val(),
            sat_vantoctoida_dongmachphoi: $('#sat_vantoctoida_dongmachphoi').val(),
            sat_chenhaptoida_dongmachphoi: $('#sat_chenhaptoida_dongmachphoi').val(),
            sat_gdmean_dongmachphoi: $('#sat_gdmean_dongmachphoi').val(),
            sat_papm_dongmachphoi: $('#sat_papm_dongmachphoi').val(),
            sat_papd_dongmachphoi: $('#sat_papd_dongmachphoi').val(),
            sat_vantoctoida_dongmachchu: $('#sat_vantoctoida_dongmachchu').val(),
            sat_chenhaptoida_dongmachchu: $('#sat_chenhaptoida_dongmachchu').val(),
            sat_hovandmchu_dongmachchu: $('#sat_hovandmchu_dongmachchu').val(),
            sat_hepvandmchu_dongmachchu: $('#sat_hepvandmchu_dongmachchu').val(),
            sat_dongquavachliennhi: $('#sat_dongquavachliennhi').val(),
            sat_dongquavachlienthat: $('#sat_dongquavachlienthat').val(),
            sat_dongbatthuongkhac: $('#sat_dongbatthuongkhac').val(),
            sat_tuthetim_2d: $('#sat_tuthetim_2d').val(),
            sat_dmphoi_2d: $('#sat_dmphoi_2d').val(),
            sat_mangngoaitim_2d: $('#sat_mangngoaitim_2d').val(),
            sat_dmvanh_2d: $('#sat_dmvanh_2d').val(),
            sat_nhitrai_2d: $('#sat_nhitrai_2d').val(),
            sat_xoangvanh_2d: $('#sat_xoangvanh_2d').val(),
            sat_nhiphai_2d: $('#sat_nhiphai_2d').val(),
            sat_vanhaila_2d: $('#sat_vanhaila_2d').val(),
            sat_tinhmach_2d: $('#sat_tinhmach_2d').val(),
            sat_vanbala_2d: $('#sat_vanbala_2d').val(),
            sat_dmchu_2d: $('#sat_dmchu_2d').val(),
            sat_vandmc_2d: $('#sat_vandmc_2d').val(),
            sat_vandmp_2d: $('#sat_vandmp_2d').val()
        }
        opts.sieuamtim = JSON.stringify(sieuamtimObj);
    }
    console.log(opts);
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
    var id = $('#sat_process_id').val();
    if (id === "") {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else {
        $.ajax({
            url: "/SA_TIM_Process/GetSample?id= " + id,
            type: 'POST',
            dataType: 'text',
            success: function (result) {
                if (result === 'True') {
                    Process_Refresh();
                }
                else {
                    SwalHelper.Alert.error('Lỗi', 'Lấy mẫu không thành công. Vui lòng kiểm tra lại!');
                }
            },
            error: function () {
                SwalHelper.Alert.error('Lỗi', 'Lấy mẫu không thành công. Vui lòng kiểm tra lại!');
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
        url: "/SA_TIM_Process/Get_Count/",
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
    $(".row-service .form-check-input").each(function () {
        var $chk = $(this);
        var resultCdhaId = String($chk.val());
        var isTarget = (resultCdhaId === String(id));
        $chk.prop("checked", isTarget);

        if (isTarget) {
            // 1) lấy ServiceId thực của dịch vụ từ data-serviceid
            var serviceId = $chk.data("serviceid");
            console.log("id ResultCDHA: ", id);
            console.log("Mã dịch vụ dã chọn: ", serviceId);
            // 2) gọi load và để hàm tự ưu tiên “đã lưu”, nếu không có sẽ fallback theo service+gender
            Process_Load_ResultAndImage_ForService(id);
        }
    });
}
function Process_Load_ResultAndImage_ForService(id) {
    $("#imageCDHA").empty();
    CKEDITOR.instances["sat_process_description"].setData("");
    $("#sat_process_result").val("Chức năng tâm thu thất trái bảo tồn (EF# %).");
    $("#sat_process_suggest").val("");
    $.ajax({
        url: "/SA_TIM_Process/GetImageForService_SieuAmTim?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var html = '';
            $.each(response, function (key, value) {
                if (value.id) {
                    //$("#imageCDHA").append('<img class="image-item" id="' + value.id + '" src="' + value.name + '" onclick="Process_GetImageCDHAId(' + value.id + ', this)"/>');
                    html += `
                        <div class="img-cell">
                            <img class="image-item"
                                 id="img_${value.id}"
                                 src="${value.name}"
                                 data-id="${value.id}"
                                 onclick="Process_GetImageCDHAId(${value.id}, this)" />
                            <label class="sat-print-ctrl">
                              <input type="checkbox"
                                     class="sat-print-check"
                                     value="${value.id}"
                                     checked />
                              Chọn in
                            </label>
                        </div>`;
                    $("#imageCDHA").html(html);

                    // Chỉ đánh dấu UI "đã chọn để IN" (không đụng tới ảnh đang chọn)
                    $(".sat-print-check").off("change").on("change", function () {
                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
                    });

                    // Khởi tạo trạng thái print-selected theo checkbox (mặc định đã checked)
                    $(".sat-print-check").each(function () {
                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
                    });
                }
                CKEDITOR.instances["sat_process_description"].setData(value.description);
                $("#sat_process_result").val(value.result ?? "Chức năng tâm thu thất trái bảo tồn (EF# %).");
                $("#sat_process_suggest").val(value.suggest);

                $('#sat_chieucao').val(value.sat_chieucao);
                $('#sat_cannang').val(value.sat_cannang);
                $('#sat_bsa').val(value.sat_bsa);
                $('#sat_bmi').val(value.sat_bmi);
                $('#sat_dmchu').val(value.sat_dmchu);
                $('#sat_nhitrai').val(value.sat_nhitrai);
                $('#sat_thatphai').val(value.sat_thatphai);
                $('#sat_ivsd').val(value.sat_ivsd);
                $('#sat_ivss').val(value.sat_ivss);
                $('#sat_lvidd').val(value.sat_lvidd);
                $('#sat_lvids').val(value.sat_lvids);
                $('#sat_lvpwd').val(value.sat_lvpwd);
                $('#sat_lvpws').val(value.sat_lvpws);
                $('#sat_rvdd').val(value.sat_rvdd);
                $('#sat_lvedv').val(value.sat_lvedv);
                $('#sat_fs').val(value.sat_fs);
                $('#sat_ef').val(value.sat_ef);
                $('#sat_vantoctoida_haila').val(value.sat_vantoctoida_haila);
                $('#sat_ea_haila').val(value.sat_ea_haila);
                $('#sat_chenhaptoida_haila').val(value.sat_chenhaptoida_haila);
                $('#sat_gdmean_haila').val(value.sat_gdmean_haila);
                $('#sat_dohovan_haila').val(value.sat_dohovan_haila);
                $('#sat_loaiho_haila').val(value.sat_loaiho_haila);
                $('#sat_hepvanhaila_haila').val(value.sat_hepvanhaila_haila);
                $('#sat_vantoctoida_bala').val(value.sat_vantoctoida_bala);
                $('#sat_chenhaptoida_bala').val(value.sat_chenhaptoida_bala);
                $('#sat_chenhaptb_bala').val(value.sat_chenhaptb_bala);
                $('#sat_hovan_bala').val(value.sat_hovan_bala);
                $('#sat_paps_bala').val(value.sat_paps_bala);
                $('#sat_vantoctoida_dongmachphoi').val(value.sat_vantoctoida_dongmachphoi);
                $('#sat_chenhaptoida_dongmachphoi').val(value.sat_chenhaptoida_dongmachphoi);
                $('#sat_gdmean_dongmachphoi').val(value.sat_gdmean_dongmachphoi);
                $('#sat_papm_dongmachphoi').val(value.sat_papm_dongmachphoi);
                $('#sat_papd_dongmachphoi').val(value.sat_papd_dongmachphoi);
                $('#sat_vantoctoida_dongmachchu').val(value.sat_vantoctoida_dongmachchu);
                $('#sat_chenhaptoida_dongmachchu').val(value.sat_chenhaptoida_dongmachchu);
                $('#sat_hovandmchu_dongmachchu').val(value.sat_hovandmchu_dongmachchu);
                $('#sat_hepvandmchu_dongmachchu').val(value.sat_hepvandmchu_dongmachchu);
                $('#sat_dongquavachliennhi').val(value.sat_dongquavachliennhi);
                $('#sat_dongquavachlienthat').val(value.sat_dongquavachlienthat);
                $('#sat_dongbatthuongkhac').val(value.sat_dongbatthuongkhac);

                if (value.sat_tuthetim_2d === "" || !value.sat_tuthetim_2d)
                    $('#sat_tuthetim_2d').val("SOLITUS LEVOCARDIA");
                else
                    $('#sat_tuthetim_2d').val(value.sat_tuthetim_2d);

                if (value.sat_dmphoi_2d === "" || !value.sat_dmphoi_2d)
                    $('#sat_dmphoi_2d').val("Bình thường");
                else
                    $('#sat_dmphoi_2d').val(value.sat_dmphoi_2d);

                if (value.sat_mangngoaitim_2d === "" || !value.sat_mangngoaitim_2d)
                    $('#sat_mangngoaitim_2d').val("Không dịch");
                else
                    $('#sat_mangngoaitim_2d').val(value.sat_mangngoaitim_2d);

                if (value.sat_dmvanh_2d === "" || !value.sat_dmvanh_2d)
                    $('#sat_dmvanh_2d').val("Không dãn");
                else
                    $('#sat_dmvanh_2d').val(value.sat_dmvanh_2d);

                if (value.sat_nhitrai_2d === "" || !value.sat_nhitrai_2d)
                    $('#sat_nhitrai_2d').val("Không giãn");
                else
                    $('#sat_nhitrai_2d').val(value.sat_nhitrai_2d);

                if (value.sat_xoangvanh_2d === "" || !value.sat_xoangvanh_2d)
                    $('#sat_xoangvanh_2d').val("Không giãn");
                else
                    $('#sat_xoangvanh_2d').val(value.sat_xoangvanh_2d);

                if (value.sat_nhiphai_2d === "" || !value.sat_nhiphai_2d)
                    $('#sat_nhiphai_2d').val("Không giãn");
                else
                    $('#sat_nhiphai_2d').val(value.sat_nhiphai_2d);

                if (value.sat_nhiphai_2d === "" || !value.sat_nhiphai_2d)
                    $('#sat_vanhaila_2d').val("Mềm mại");
                else
                    $('#sat_vanhaila_2d').val(value.sat_vanhaila_2d);

                if (value.sat_tinhmach_2d === "" || !value.sat_tinhmach_2d)
                    $('#sat_tinhmach_2d').val("Bình thường");
                else
                    $('#sat_tinhmach_2d').val(value.sat_tinhmach_2d);

                if (value.sat_tinhmach_2d === "" || !value.sat_tinhmach_2d)
                    $('#sat_vanbala_2d').val("Mềm mại");
                else
                    $('#sat_vanbala_2d').val(value.sat_vanbala_2d);

                if (value.sat_tinhmach_2d === "" || !value.sat_tinhmach_2d)
                    $('#sat_dmchu_2d').val("Bình thường");
                else
                    $('#sat_dmchu_2d').val(value.sat_dmchu_2d);

                if (value.sat_tinhmach_2d === "" || !value.sat_tinhmach_2d)
                    $('#sat_vandmc_2d').val("Mềm mại");
                else
                    $('#sat_vandmc_2d').val(value.sat_vandmc_2d);

                if (value.sat_tinhmach_2d === "" || !value.sat_tinhmach_2d)
                    $('#sat_vandmp_2d').val("Mềm mại");
                else
                    $('#sat_vandmp_2d').val(value.sat_vandmp_2d);
            });
        }
    });
}

function Process_GetImageCDHAId(imageCDHAId, el) {
    $("#sat_process_deletephoto").val(imageCDHAId); // giữ logic cũ
    $(".image-item").removeClass("active");         // chỉ 1 ảnh được chọn
    $(el).addClass("active");
}

function StartCamera() {
    $.ajax({
        url: "/SA_TIM_Process/Check_SelectDevice/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            if (result === 'True') {
                StartCamera_01();
            }
            else {
                $('#addDeviceForm').modal('show');
            }
        },
        error: function () {
            SwalHelper.Alert.error('Lỗi', 'Không thể chọn thiết bị. Vui lòng kiểm tra lại!');
        }
    });
}

let camera_stream = null;
function StartCamera_01() {
    var video = document.querySelector("#video");
    navigator.mediaDevices.getUserMedia({ video: true, audio: false }).then(function success(stream) {
        video.srcObject = stream;
        camera_stream = stream;
    });
    document.getElementById("sat_process_startcamera").style.display = "none";
    document.getElementById("sat_process_snapshot").style.display = "unset";
    document.getElementById("sat_process_deletephoto").style.display = "unset";
    document.getElementById("sat_process_startvideo").style.display = "unset";
}

function Process_SelectDevice() {
    var deviceId = $('#sat_getsample_select_device_select').val();
    if (deviceId === "") {
        SwalHelper.Toast.warning('Vui lòng chọn thiết bị !');
    }
    else {
        $.ajax({
            url: "/SA_TIM_Process/SelectDevice?deviceId=" + deviceId,
            type: 'GET',
            dataType: 'text',
            success: function (result) {
                if (result == 'False') {
                    SwalHelper.Alert.error("Không thể chọn thiết bị. Kiểm tra lại !");
                }
                else {
                    StartCamera_01();
                }
            },
            error: function () {
                SwalHelper.Alert.error("Không thể chọn thiết bị. Kiểm tra lại !");
            }
        });
    }
}

function SnapShot() {
    var countImage = 0;
    $(".imageCDHA").each(function () { // Đếm Image đã chụp
        $(this).find(".image-item").each(function () {
            countImage++;
        })
    })

    if (countImage === 6) {
        SwalHelper.Toast.warning("Chỉ cho phép chụp tối đa 6 hình !");
    }
    else {
        var canvas = document.getElementById("canvas");
        //canvas.width = 1500;
        //canvas.height = 1133;
        canvas.width = 750;
        canvas.height = 567;
        //canvas.width = 300;
        //canvas.height = 228;
        canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
        var image_data_url = document.getElementById("canvas").toDataURL("image/png");
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
            var data = {
                imageString: image_data_url,
                resultCDHAId: resultCDHAId
            };
            $.ajax({
                url: "/SA_TIM_Process/SaveImageCDHA/",
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(data),
                dataType: 'text',
                success: function (response) {
                    if (response !== 'True') {
                        SwalHelper.Alert.error("Không thể lưu ảnh. Vui lòng kiểm tra lại!");
                    }
                    else {
                        Process_Load_Image_ForService(resultCDHAId);
                    }
                }
            });
        }
        else {
            SwalHelper.Alert.error("Vui lòng chọn dịch vụ !");
        }
    }
}

function XemHinh() {
    const selectedImg = $('.imageCDHA img.active');
    if (selectedImg.length === 0) {
        SwalHelper.Toast.warning('Bạn chưa chọn hình!');
        return;
    }
    $('#modalImage').attr('src', selectedImg.attr('src'));
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    modal.show();
}

function DeletePhoto() {
    var imageCDHAId = $("#sat_process_deletephoto").val();
    const selectedImg = $('.imageCDHA img.active');
    if (selectedImg.length === 0) {
        SwalHelper.Toast.warning('Bạn chưa chọn hình!');
        return;
    }
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
                    url: "/SA_TIM_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
                    type: 'POST',
                    dataType: 'text',
                    success: function (response) {
                        if (response === "") {
                            SwalHelper.Alert.error('Lỗi', 'Không thể xoá ảnh. Vui lòng kiểm tra lại!');
                        }
                        else {
                            SwalHelper.Toast.success('Xóa ảnh thành công!');
                            Process_Load_Image_ForService(response);
                        }
                    },
                    error: function () {
                        SwalHelper.Alert.error('Lỗi', 'Không thể xoá ảnh. Vui lòng kiểm tra lại!');
                    }
                });
            }
        });
    }
    else {
        SwalHelper.Alert.error('Lỗi', 'Không thể xoá ảnh. Vui lòng kiểm tra lại!');
    }
    //if (imageCDHAId !== null) {
    //    $.ajax({
    //        url: "/SA_TIM_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
    //        type: 'POST',
    //        dataType: 'text',
    //        success: function (response) {
    //            if (response === "") {
    //                SwalHelper.Alert.error('Lỗi', 'Không thể xoá ảnh. Vui lòng kiểm tra lại!');
    //            }
    //            else {
    //                Process_Load_Image_ForService(response);
    //            }
    //        }
    //    });
    //}
    //else {
    //    SwalHelper.Alert.error('Lỗi', 'Không thể xoá ảnh. Vui lòng kiểm tra lại!');
    //}
}

// Upload hình
function UploadHinh(fileInput) {
    if (fileInput.files.length === 0) {
        SwalHelper.Toast.warning('Vui lòng chọn file!');
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
            url: "/SA_TIM_Process/UploadHinh/",
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response !== 'True') {
                    SwalHelper.Alert.error("Không thể lưu ảnh. Vui lòng kiểm tra lại!");
                } else {
                    //Process_Load_ResultAndImage_ForService(resultCDHAId);
                    Process_CheckedBoxOnRow(resultCDHAId);
                }
            }
        });
    }
    else {
        SwalHelper.Alert.error("Vui lòng chọn dịch vụ !");
    }
}

// Begin ghi video màn hình
let media_recorder = null;
let blobs_recorded = [];
let download_link = document.querySelector("#sat_process_downloadvideo_a");
function StartVideo() {
    document.getElementById("sat_process_startvideo").style.display = "none";
    document.getElementById("sat_process_stopvideo").style.display = "unset";

    media_recorder = new MediaRecorder(camera_stream, { mimeType: 'video/webm' });
    media_recorder.addEventListener('dataavailable', function (e) {
        blobs_recorded.push(e.data);
    });

    media_recorder.addEventListener('stop', function () {
        let video_local = URL.createObjectURL(new Blob(blobs_recorded, { type: 'video/webm' }));
        download_link.href = video_local;
    });
    media_recorder.start(1000);
}

function StopVideo() {
    document.getElementById("sat_process_stopvideo").style.display = "none";
    document.getElementById("sat_process_deletevideo").style.display = "unset";
    document.getElementById("sat_process_downloadvideo").style.display = "unset";

    media_recorder.stop();
}

function DeleteVideo() {
    document.getElementById("sat_process_startvideo").style.display = "unset";
    document.getElementById("sat_process_deletevideo").style.display = "none";
    document.getElementById("sat_process_downloadvideo").style.display = "none";

    media_recorder = null;
    blobs_recorded = [];
    download_link.href = null;
}

function DownloadVideo() {
    document.getElementById("sat_process_startvideo").style.display = "unset";
    document.getElementById("sat_process_deletevideo").style.display = "none";
    document.getElementById("sat_process_downloadvideo").style.display = "none";

    download_link.click();
    media_recorder = null;
    blobs_recorded = [];
    download_link.href = null;
}
// End ghi video màn hình

function Process_Load_Image_ForService(id) {
    $("#imageCDHA").empty();
    $.ajax({
        url: "/SA_TIM_Process/GetImageForService_SieuAmTim?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var html = '';
            $.each(response, function (key, value) {
                if (value.id) {
                    /*$("#imageCDHA").append('<img class="image-item" id="' + value.id + '" src="' + value.name + '" onclick="Process_GetImageCDHAId(' + value.id + ', this)"/>');*/
                    // bọc mỗi ảnh + checkbox trong 1 div
                    html += `
                        <div class="img-cell">
                            <img class="image-item"
                                 id="img_${value.id}"
                                 src="${value.name}"
                                 data-id="${value.id}"
                                 onclick="Process_GetImageCDHAId(${value.id}, this)" />
                            <label class="sat-print-ctrl">
                              <input type="checkbox"
                                     class="sat-print-check"
                                     value="${value.id}"
                                     checked />
                              Chọn in
                            </label>
                        </div>`;
                    $("#imageCDHA").html(html);

                    // Chỉ đánh dấu UI "đã chọn để IN" (không đụng tới ảnh đang chọn)
                    $(".sat-print-check").off("change").on("change", function () {
                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
                    });

                    // Khởi tạo trạng thái print-selected theo checkbox (mặc định đã checked)
                    $(".sat-print-check").each(function () {
                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
                    });
                }
            });
        }
    });
}

function GetSampleForService(id) {
    $.ajax({
        url: "/SA_TIM_Process/GetSampleForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            CKEDITOR.instances["sat_process_description"].setData(response.description);
            $("#sat_process_result").val(response.result);
            $("#sat_process_suggest").val(response.suggest);
        }
    });
}
// Tải thông tin bác sĩ khi đổi select
function Process_Load_SelectedDoctorInfo() {
    var userId = $("#sat_process_userReturnResultSAT").val();
    if (!userId) {
        $("#sat_process_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/SA_TIM_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#sat_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
                return;
            }
            console.log(u)

            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#sat_process_signerCCCD").val(cccd);
        },
        error: function () {
            $("#sat_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
        }
    });
}

// ========================== KÝ SỐ PDF (Form: SIÊU ÂM TIM) ==========================
function Process_SignPdf() {
    var patientId = $('#sat_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
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
    var signerCCCD = $('#sat_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#sat_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#sat_process_patientId').val() || $('#sat_process_sid').val() || "";
    var patientName = $('#sat_process_patientName').val() || "";
    var doctorName = $('#sat_process_userReturnResultSAT option:selected').text() || $('#sat_process_userReturnResultSAT').val() || "";
    var performedAt = $('#sat_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn

    // 1) Gọi API xuất PDF base64 (tận dụng endpoint ValidPrint đang có)
    //    Nếu bạn có endpoint riêng chỉ "Export" (không đổi trạng thái), thay URL ở đây là tốt nhất.
    $('#showWaitting').modal('show');
    var returnResultTime = $('#sat_process_returnResultTime').val();
    var userReturnResult = $('#sat_process_userReturnResultSAT').val();
    var description = CKEDITOR.instances['sat_process_description'].getData();
    var result = $('#sat_process_result').val();
    var suggest = $('#sat_process_suggest').val();

    var sieuamtimObj = {
        sat_chieucao: $('#sat_chieucao').val(),
        sat_cannang: $('#sat_cannang').val(),
        sat_bsa: $('#sat_bsa').val(),
        sat_bmi: $('#sat_bmi').val(),
        sat_dmchu: $('#sat_dmchu').val(),
        sat_nhitrai: $('#sat_nhitrai').val(),
        sat_thatphai: $('#sat_thatphai').val(),
        sat_ivsd: $('#sat_ivsd').val(),
        sat_ivss: $('#sat_ivss').val(),
        sat_lvidd: $('#sat_lvidd').val(),
        sat_lvids: $('#sat_lvids').val(),
        sat_lvpwd: $('#sat_lvpwd').val(),
        sat_lvpws: $('#sat_lvpws').val(),
        sat_rvdd: $('#sat_rvdd').val(),
        sat_lvedv: $('#sat_lvedv').val(),
        sat_fs: $('#sat_fs').val(),
        sat_ef: $('#sat_ef').val(),
        sat_vantoctoida_haila: $('#sat_vantoctoida_haila').val(),
        sat_ea_haila: $('#sat_ea_haila').val(),
        sat_chenhaptoida_haila: $('#sat_chenhaptoida_haila').val(),
        sat_gdmean_haila: $('#sat_gdmean_haila').val(),
        sat_dohovan_haila: $('#sat_dohovan_haila').val(),
        sat_loaiho_haila: $('#sat_loaiho_haila').val(),
        sat_hepvanhaila_haila: $('#sat_hepvanhaila_haila').val(),
        sat_vantoctoida_bala: $('#sat_vantoctoida_bala').val(),
        sat_chenhaptoida_bala: $('#sat_chenhaptoida_bala').val(),
        sat_chenhaptb_bala: $('#sat_chenhaptb_bala').val(),
        sat_hovan_bala: $('#sat_hovan_bala').val(),
        sat_paps_bala: $('#sat_paps_bala').val(),
        sat_vantoctoida_dongmachphoi: $('#sat_vantoctoida_dongmachphoi').val(),
        sat_chenhaptoida_dongmachphoi: $('#sat_chenhaptoida_dongmachphoi').val(),
        sat_gdmean_dongmachphoi: $('#sat_gdmean_dongmachphoi').val(),
        sat_papm_dongmachphoi: $('#sat_papm_dongmachphoi').val(),
        sat_papd_dongmachphoi: $('#sat_papd_dongmachphoi').val(),
        sat_vantoctoida_dongmachchu: $('#sat_vantoctoida_dongmachchu').val(),
        sat_chenhaptoida_dongmachchu: $('#sat_chenhaptoida_dongmachchu').val(),
        sat_hovandmchu_dongmachchu: $('#sat_hovandmchu_dongmachchu').val(),
        sat_hepvandmchu_dongmachchu: $('#sat_hepvandmchu_dongmachchu').val(),
        sat_dongquavachliennhi: $('#sat_dongquavachliennhi').val(),
        sat_dongquavachlienthat: $('#sat_dongquavachlienthat').val(),
        sat_dongbatthuongkhac: $('#sat_dongbatthuongkhac').val(),
        sat_tuthetim_2d: $('#sat_tuthetim_2d').val(),
        sat_dmphoi_2d: $('#sat_dmphoi_2d').val(),
        sat_mangngoaitim_2d: $('#sat_mangngoaitim_2d').val(),
        sat_dmvanh_2d: $('#sat_dmvanh_2d').val(),
        sat_nhitrai_2d: $('#sat_nhitrai_2d').val(),
        sat_xoangvanh_2d: $('#sat_xoangvanh_2d').val(),
        sat_nhiphai_2d: $('#sat_nhiphai_2d').val(),
        sat_vanhaila_2d: $('#sat_vanhaila_2d').val(),
        sat_tinhmach_2d: $('#sat_tinhmach_2d').val(),
        sat_vanbala_2d: $('#sat_vanbala_2d').val(),
        sat_dmchu_2d: $('#sat_dmchu_2d').val(),
        sat_vandmc_2d: $('#sat_vandmc_2d').val(),
        sat_vandmp_2d: $('#sat_vandmp_2d').val()
    }

    var data = {
        patientId: patientId,
        resultCDHAId: resultCDHAId,
        returnResultTime: returnResultTime,
        userReturnResult: userReturnResult,
        description: description,
        result: result,
        suggest: suggest,
        sieuamtim: JSON.stringify(sieuamtimObj)
    };
    console.log(data)
    $.ajax({
        url: "/SA_TIM_Process/ValidPrint/",
        data: JSON.stringify(data),
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        type: "POST",
        success: function (base64Pdf) {
            if (!base64Pdf) {
                $('#showWaitting').modal('hide');
                SwalHelper.Alert.error("Không thể xuất PDF để ký. Vui lòng kiểm tra lại!");
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
            formData.append("formType", "SieuAmTim");
            formData.append("patientCode", patientCode);
            formData.append("patientName", patientName);
            formData.append("serviceCode", serviceCode);
            formData.append("doctorName", doctorName);
            if (performedAt) formData.append("performedAt", ""); // ISO/yyyy-MM-dd HH:mm:ss
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
                    // Gỉa sử object trả về từ response như sau
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
                        signStoreId = signStoreId || "51402";
                        var keyResult = null;

                        // 2) LƯU signStoreId THEO resultCDHAId ĐANG CHỌN
                        if (signStoreId) {
                            $.ajax({
                                url: "/SA_TIM_Process/SaveSignStoreIdForResultCDHA",
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
                                            referenceType: "SieuAmTim",                // SA / SATIM / XQUANG...
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

                    } catch (e) {
                        if (isProbablyBase64(resp)) {
                            openBase64Pdf(resp);
                        } else {
                            SwalHelper.Alert.error("Đã ký số, phản hồi:\n" + resp);
                        }
                    }
                },
                error: function (xhr) {
                    $('#showWaitting').modal('hide');
                    var msg = "Ký số thất bại.";
                    if (xhr && xhr.responseText) msg += "\n" + xhr.responseText;
                    SwalHelper.Alert.error(msg);
                }
            });
        },
        error: function () {
            $('#showWaitting').modal('hide');
            SwalHelper.Alert.error("Xuất PDF không thành công. Vui lòng kiểm tra lại!");
        }
    });
}

function Process_SignPdf_Multi(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    var patientId = $('#sat_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
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
    var signerCCCD = $('#sat_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#sat_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#sat_process_patientId').val() || $('#sat_process_sid').val() || "";
    var patientMaBenhAn = $('#sat_process_maBenhAn').val() || $('#sat_process_sid').val() || "";
    var patientName = $('#sat_process_patientName').val() || "";
    var doctorName = $('#sat_process_userReturnResultSAT option:selected').text() || $('#sat_process_userReturnResultSAT').val() || "";
    var doctorId = $('#sat_process_userReturnResultSAT').val();
    var performedAt = $('#sat_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "SieuAmTim"; // bạn có thể thay động tùy màn hình

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
                        url: "/SA_TIM_Process/ValidPrintMultiple/",
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
                var returnResultTime = $('#sat_process_returnResultTime').val();
                var userReturnResult = $('#sat_process_userReturnResultSAT').val();
                var description = CKEDITOR.instances['sat_process_description'].getData();
                var result = $('#sat_process_result').val();
                var suggest = $('#sat_process_suggest').val();

                var sieuamtimObj = {
                    sat_chieucao: $('#sat_chieucao').val(),
                    sat_cannang: $('#sat_cannang').val(),
                    sat_bsa: $('#sat_bsa').val(),
                    sat_bmi: $('#sat_bmi').val(),
                    sat_dmchu: $('#sat_dmchu').val(),
                    sat_nhitrai: $('#sat_nhitrai').val(),
                    sat_thatphai: $('#sat_thatphai').val(),
                    sat_ivsd: $('#sat_ivsd').val(),
                    sat_ivss: $('#sat_ivss').val(),
                    sat_lvidd: $('#sat_lvidd').val(),
                    sat_lvids: $('#sat_lvids').val(),
                    sat_lvpwd: $('#sat_lvpwd').val(),
                    sat_lvpws: $('#sat_lvpws').val(),
                    sat_rvdd: $('#sat_rvdd').val(),
                    sat_lvedv: $('#sat_lvedv').val(),
                    sat_fs: $('#sat_fs').val(),
                    sat_ef: $('#sat_ef').val(),
                    sat_vantoctoida_haila: $('#sat_vantoctoida_haila').val(),
                    sat_ea_haila: $('#sat_ea_haila').val(),
                    sat_chenhaptoida_haila: $('#sat_chenhaptoida_haila').val(),
                    sat_gdmean_haila: $('#sat_gdmean_haila').val(),
                    sat_dohovan_haila: $('#sat_dohovan_haila').val(),
                    sat_loaiho_haila: $('#sat_loaiho_haila').val(),
                    sat_hepvanhaila_haila: $('#sat_hepvanhaila_haila').val(),
                    sat_vantoctoida_bala: $('#sat_vantoctoida_bala').val(),
                    sat_chenhaptoida_bala: $('#sat_chenhaptoida_bala').val(),
                    sat_chenhaptb_bala: $('#sat_chenhaptb_bala').val(),
                    sat_hovan_bala: $('#sat_hovan_bala').val(),
                    sat_paps_bala: $('#sat_paps_bala').val(),
                    sat_vantoctoida_dongmachphoi: $('#sat_vantoctoida_dongmachphoi').val(),
                    sat_chenhaptoida_dongmachphoi: $('#sat_chenhaptoida_dongmachphoi').val(),
                    sat_gdmean_dongmachphoi: $('#sat_gdmean_dongmachphoi').val(),
                    sat_papm_dongmachphoi: $('#sat_papm_dongmachphoi').val(),
                    sat_papd_dongmachphoi: $('#sat_papd_dongmachphoi').val(),
                    sat_vantoctoida_dongmachchu: $('#sat_vantoctoida_dongmachchu').val(),
                    sat_chenhaptoida_dongmachchu: $('#sat_chenhaptoida_dongmachchu').val(),
                    sat_hovandmchu_dongmachchu: $('#sat_hovandmchu_dongmachchu').val(),
                    sat_hepvandmchu_dongmachchu: $('#sat_hepvandmchu_dongmachchu').val(),
                    sat_dongquavachliennhi: $('#sat_dongquavachliennhi').val(),
                    sat_dongquavachlienthat: $('#sat_dongquavachlienthat').val(),
                    sat_dongbatthuongkhac: $('#sat_dongbatthuongkhac').val(),
                    sat_tuthetim_2d: $('#sat_tuthetim_2d').val(),
                    sat_dmphoi_2d: $('#sat_dmphoi_2d').val(),
                    sat_mangngoaitim_2d: $('#sat_mangngoaitim_2d').val(),
                    sat_dmvanh_2d: $('#sat_dmvanh_2d').val(),
                    sat_nhitrai_2d: $('#sat_nhitrai_2d').val(),
                    sat_xoangvanh_2d: $('#sat_xoangvanh_2d').val(),
                    sat_nhiphai_2d: $('#sat_nhiphai_2d').val(),
                    sat_vanhaila_2d: $('#sat_vanhaila_2d').val(),
                    sat_tinhmach_2d: $('#sat_tinhmach_2d').val(),
                    sat_vanbala_2d: $('#sat_vanbala_2d').val(),
                    sat_dmchu_2d: $('#sat_dmchu_2d').val(),
                    sat_vandmc_2d: $('#sat_vandmc_2d').val(),
                    sat_vandmp_2d: $('#sat_vandmp_2d').val()
                }

                var dataExport = {
                    patientId: patientId,
                    resultCDHAId: resultIds[0],
                    returnResultTime: returnResultTime,
                    userReturnResult: userReturnResult,
                    description: description,
                    result: result,
                    suggest: suggest,
                    sieuamtim: JSON.stringify(sieuamtimObj)
                };

                console.log(resultIds[0], "====>", dataExport);
                $.ajax({
                    url: "/SA_TIM_Process/ValidPrint/",
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
            SwalHelper.Alert.error("Không tạo được file PDF nào để ký.");
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
                        SwalHelper.Alert.error('Lỗi', `Xảy ra lỗi khi ký: ${errorMessage}`);
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
                                    url: "/SA_TIM_Process/SaveSignStoreIdForResultCDHA",
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
                                                    referenceType: formType ?? "SieuAm",
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
                    var msg = "Ký số thất bại.";
                    if (xhr && xhr.responseJSON) msg += "\n" + xhr.responseJSON.message;
                    SwalHelper.Alert.error(msg);
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
        SwalHelper.Alert.error('Lỗi', 'Lỗi chuyển đổi base64 -> file: ' + e);
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

function Process_ClearAllInputs() {
    // Clear ảnh
    $("#imageCDHA").empty();

    // Clear CKEditor
    CKEDITOR.instances["sat_process_description"].setData("");

    // Clear kết luận và đề nghị
    $("#sat_process_result").val("Chức năng tâm thu thất trái bảo tồn (EF# %).");
    $("#sat_process_suggest").val("");

    // Clear các chỉ số tim
    $('#sat_chieucao').val('');
    $('#sat_cannang').val('');
    $('#sat_bsa').val('');
    $('#sat_bmi').val('');
    $('#sat_dmchu').val('');
    $('#sat_nhitrai').val('');
    $('#sat_thatphai').val('');
    $('#sat_ivsd').val('');
    $('#sat_ivss').val('');
    $('#sat_lvidd').val('');
    $('#sat_lvids').val('');
    $('#sat_lvpwd').val('');
    $('#sat_lvpws').val('');
    $('#sat_rvdd').val('');
    $('#sat_lvedv').val('');
    $('#sat_fs').val('');
    $('#sat_ef').val('');

    // Van hai lá
    $('#sat_vantoctoida_haila').val('');
    $('#sat_ea_haila').val('');
    $('#sat_chenhaptoida_haila').val('');
    $('#sat_gdmean_haila').val('');
    $('#sat_dohovan_haila').val('');
    $('#sat_loaiho_haila').val('');
    $('#sat_hepvanhaila_haila').val('');

    // Van ba lá
    $('#sat_vantoctoida_bala').val('');
    $('#sat_chenhaptoida_bala').val('');
    $('#sat_chenhaptb_bala').val('');
    $('#sat_hovan_bala').val('');
    $('#sat_paps_bala').val('');

    // Động mạch phổi
    $('#sat_vantoctoida_dongmachphoi').val('');
    $('#sat_chenhaptoida_dongmachphoi').val('');
    $('#sat_gdmean_dongmachphoi').val('');
    $('#sat_papm_dongmachphoi').val('');
    $('#sat_papd_dongmachphoi').val('');

    // Động mạch chủ
    $('#sat_vantoctoida_dongmachchu').val('');
    $('#sat_chenhaptoida_dongmachchu').val('');
    $('#sat_hovandmchu_dongmachchu').val('');
    $('#sat_hepvandmchu_dongmachchu').val('');

    // Dòng bất thường
    $('#sat_dongquavachliennhi').val('');
    $('#sat_dongquavachlienthat').val('');
    $('#sat_dongbatthuongkhac').val('');

    // 2D
    $('#sat_tuthetim_2d').val('');
    $('#sat_dmphoi_2d').val('');
    $('#sat_mangngoaitim_2d').val('');
    $('#sat_dmvanh_2d').val('');
    $('#sat_nhitrai_2d').val('');
    $('#sat_xoangvanh_2d').val('');
    $('#sat_nhiphai_2d').val('');
    $('#sat_vanhaila_2d').val('');
    $('#sat_tinhmach_2d').val('');
    $('#sat_vanbala_2d').val('');
    $('#sat_dmchu_2d').val('');
    $('#sat_vandmc_2d').val('');
    $('#sat_vandmp_2d').val('');
}

// =======================Checkbox all dịch vụ=======================

// Tick/untick tất cả
//$(document).on('change', '#sat_chk_all', function () {
//    var checked = this.checked === true;
//    // chỉ tick các dịch vụ đang hiển thị
//    $('#sat_list_service').find('.sat-chk-service').prop('checked', checked);

//    // **THÊM**: Nếu bỏ chọn tất cả thì clear inputs
//    if (!checked) {
//        Process_ClearAllInputs();
//    }
//    // Ẩn/hiện nút Lưu dựa trên trạng thái check all
//    if (checked) {
//        $('#sat_process_saveresult').hide();
//    } else {
//        $('#sat_process_saveresult').show();
//    }
//});

//// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả"
//$(document).on('change', '.sat-chk-service', function () {
//    var $rows = $('#sat_list_service').find('.sat-chk-service');
//    var total = $rows.length;
//    var marked = $rows.filter(':checked').length;

//    // **THÊM**: Nếu không còn dịch vụ nào được chọn thì clear
//    if (marked === 0) {
//        Process_ClearAllInputs();
//    }
//    // Nếu tất cả đều check => check header; ngược lại bỏ check header
//    //var allChecked = total > 0 && marked === total;
//    //$('#sat_chk_all').prop('checked', allChecked);

//    //// Ẩn/hiện nút Lưu dựa trên trạng thái check all
//    //if (allChecked) {
//    //    $('#sat_process_saveresult').hide();
//    //} else {
//    //    $('#sat_process_saveresult').show();
//    //}
//});

// Tick/untick tất cả dịch vụ
$(document).on('change', '#sat_chk_all', function () {
    var checked = this.checked === true;

    var $items = $('#sat_list_service').find('.sat-chk-service');
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
        $('#sat_list_service .row-service').removeClass('multi-selected');
    }

    // Ẩn/hiện nút Lưu dựa trên trạng thái check all (giữ nguyên logic cũ)
    if (checked) {
        $('#sat_process_saveresult').hide();
    } else {
        $('#sat_process_saveresult').show();
    }
});
// Khai báo guard toàn cục (một lần)
if (window.__sat_row_selecting === undefined) {
    window.__sat_row_selecting = false;
}
// Tick/untick từng dịch vụ riêng lẻ
$(document).on('change', '.sat-chk-service', function () {
    var $all = $('#sat_list_service').find('.sat-chk-service');
    var total = $all.length;
    var marked = $all.filter(':checked').length;

    // Đồng bộ header "chọn tất cả"
    var allChecked = total > 0 && marked === total;
    $('#sat_chk_all').prop('checked', allChecked);

    // Quy tắc multi-selected:
    // - Nếu chỉ còn 0 hoặc 1 cái được tick → remove hết .multi-selected
    // - Nếu từ 2 cái trở lên → tất cả các dòng được tick đều có .multi-selected
    if (marked >= 2) {
        $('#sat_process_saveresult').hide(); // ẩn nút Lưu
        $('#imageCDHA').empty();
        CKEDITOR.instances.sat_process_description.setReadOnly(true); // disabled nhập mô tả kết quả
        Process_ClearAllInputs();
        $('#sat_process_result').attr('disabled', true); // disabled nhập kết luận
        $('#sat_process_result').val(''); // disabled nhập kết luận

        $all.each(function () {
            var $row = $(this).closest('.row-service');
            $row.removeClass('active');
            if (this.checked) $row.addClass('multi-selected');
            else $row.removeClass('multi-selected');
        });
    } else {
        //var $row = $(this).closest('.row-service');
        //$row.removeClass('active');
        $('#sat_process_saveresult').show();
        CKEDITOR.instances.sat_process_description.setReadOnly(false);
        $('#sat_process_result').attr('disabled', false);
        $('#sat_list_service .row-service').removeClass('multi-selected');
    }

    // === Điểm thêm mới theo yêu cầu ===
    // Nếu sau thao tác chỉ còn 1 checkbox được chọn:
    // -> Lấy resultCDHAId còn lại và gọi Process_CheckedBoxOnRow(resultCDHAId)
    if (marked === 1 && !window.__sat_row_selecting) {
        console.log("vodayyy");
        var $only = $all.filter(':checked').first();
        var resultId = $only.val();
        console.log(resultId);

        // Tránh loop sự kiện change khi Process_CheckedBoxOnRow thay đổi checkbox
        window.__sat_row_selecting = true;
        setTimeout(function () {
            try {
                Process_CheckedBoxOnRow(resultId);
            } finally {
                window.__sat_row_selecting = false;
            }
        }, 0);
    }
});
function Process_SaveDateToSession() {
    var fromDate = document.getElementById('sat_process_timeSearchFrom').value;
    var toDate = document.getElementById('sat_process_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/SA_TIM_Process/SaveSearchDates',
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
//***************************************************************************************** Return Result

function ReturnResult_Refresh() {
    $.ajax({
        url: "/SA_TIM_ReturnResult/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#sat_returnresult_timeSearchFrom").val(today);
            //$("#sat_returnresult_timeSearchTo").val(today);

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
    var sat_returnresult_pidorseq = $("#sat_returnresult_pidorseq").val();
    var timeSearchFrom = $("#sat_returnresult_timeSearchFrom").val();
    var timeSearchTo = $("#sat_returnresult_timeSearchTo").val();
    $.ajax({
        url: "/SA_TIM_ReturnResult/Search?" + "pidorseq=" + sat_returnresult_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#sat_returnresult_pidorseq").val('');
    $('#sat_returnresult_id').val('');
    $('#sat_returnresult_patientId').val('');
    $('#sat_returnresult_seq').val('');
    $('#sat_returnresult_sid').val('');
    $('#sat_returnresult_patientName').val('');
    $('#sat_returnresult_age').val('');
    $('#sat_returnresult_sex').val('');
    $('#sat_returnresult_obj').val('');
    $('#sat_returnresult_type').val('');
    $('#sat_returnresult_location').val('');
    $('#sat_returnresult_doctor').val('');
    $('#sat_returnresult_getSampleTime').val('');
    $('#sat_returnresult_returnResultTime').val('');
    $('#sat_returnresult_location').val('');
    $('#sat_returnresult_doctor').val('');
    $('#sat_returnresult_userReturnResultSAT').val('');
    $('#sat_returnresult_address').val('');
    $('#sat_returnresult_diagnostic').val('');
    $('#tbody-gridview-service').empty();
}

function ReturnResult_GetPatientInfo(id) {
    $.ajax({
        url: "/SA_TIM_ReturnResult/GetPatientInfo?id=" + id,
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

function ReturnResult_GetListServiceForPatient(id) {
    $.ajax({
        url: "/SA_TIM_ReturnResult/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_sat-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function ReturnResult_Invalid() {
    var patientId = $('#sat_returnresult_id').val();

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // ResultCDHA.Id của các dịch vụ đang được chọn.
    // Backend sẽ tự lấy KeyResultForHis từ DB theo ResultCDHA.Id.
    var resultIds = [];

    $('.sat-returnresult-chk-service:checked').each(function () {
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
        url: "/SA_TIM_ReturnResult/Invalid",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify({
            patientId: parseInt(patientId, 10),
            resultIds: resultIds,
            categoryCode: "SAT"
        }),
        success: function (result) {
            if (result === "True") {
                SwalHelper.Toast.success("Invalid thành công!");
                ReturnResult_Refresh();
                ReturnResult_Get_Count();
            }
            else {
                SwalHelper.Alert.error('Lỗi', result || 'Không thể Invalid. Vui lòng kiểm tra lại!');
            }
        },
        error: function (xhr) {
            SwalHelper.Alert.error('Lỗi', xhr.responseText || 'Không thể Invalid. Vui lòng kiểm tra lại!');
        }
    });
}

function ReturnResult_Invalid_RemoveDigitalSign() {
    var patientId = $('#sat_returnresult_id').val();

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
        url: "/SA_TIM_ReturnResult/Invalid",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify({
            patientId: parseInt(patientId, 10),
            resultIds: resultIds,
            categoryCode: "SAT"
        }),
        success: function (result) {
            if (result === "True") {
                SwalHelper.Toast.success("Invalid thành công và đã hủy ký số!");
                UpdateSignStatus(resultCDHAId);
                ReturnResult_Refresh();
            }
            else {
                SwalHelper.Alert.error('Lỗi', result || 'Không thể Invalid. Vui lòng kiểm tra lại!');
            }
        },
        error: function (xhr) {
            SwalHelper.Alert.error('Lỗi', xhr.responseText || 'Không thể Invalid. Vui lòng kiểm tra lại!');
        }
    });
}
function ReturnResult_Get_Count() {
    $.ajax({
        url: "/SA_TIM_ReturnResult/Get_Count/",
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
    var patientId = $('#sat_returnresult_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
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
        SwalHelper.Toast.warning('Vui lòng chọn dịch vụ!');
        return;
    }

    $('#showWaitting').modal('show');

    $.ajax({
        url: "/SA_TIM_ReturnResult/Print?resultCDHAId=" + resultCDHAId,
        dataType: "text",
        type: "GET",
        success: function (response) {
            $('#showWaitting').modal('hide');

            if (response === "") {
                SwalHelper.Alert.error('Lỗi', 'In không thành công. Vui lòng kiểm tra lại!');
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
                    link.download = 'KetQua_SieuAmTim_' + resultCDHAId + '_' + new Date().getTime() + '.pdf';
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
                        link.download = 'KetQua_SieuAmTim_' + resultCDHAId + '.pdf';
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

//function ReturnResult_CheckedBoxOnRow(id) {
//    $(".row-service").each(function () { // Lấy value trên từng Row
//        $(this).find(".form-check-input").each(function () {
//            var value = $(this).val();
//            if (value == id) {
//                $(this).prop("checked", true);
//                ReturnResult_LoadImageForService(id);
//                ReturnResult_Load_SelectedDoctorInfo();

//                // Check digital signature status and toggle buttons
//                var $row = $(this).closest('.row-service');
//                var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
//                ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
//            }
//            else {
//                $(this).prop("checked", false);
//                ReturnResult_Load_SelectedDoctorInfo();

//            }
//        })

//    })
//    // >>> THÊM DÒNG NÀY:
//    ReturnResult_CheckSignStatus(id);
//}

function ReturnResult_CheckedBoxOnRow(id, event) {
    // Nếu click trực tiếp vào checkbox thì không xử lý (để người dùng tự chọn multiple)
    if (event && event.target.type === 'checkbox') {
        // Chỉ cập nhật trạng thái "chọn tất cả"
        var $rows = $('.sat-returnresult-chk-service');
        var total = $rows.length;
        var marked = $rows.filter(':checked').length;
        var allChecked = total > 0 && marked === total;
        $('#sat_returnresult_chk_all').prop('checked', allChecked);

        // Nếu chỉ có 1 checkbox được chọn sau khi click
        var selectedCheckboxes = $(".row-service .form-check-input:checked");
        if (selectedCheckboxes.length === 1) {
            var $row = selectedCheckboxes.first().closest('.row-service');
            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
        } else if (selectedCheckboxes.length > 1) {
            // Clear chi tiết khi chọn nhiều
            const $containHinh = $('.image-returnresult');
            $containHinh.empty();
            ReturnResult_ClearAllInputs();
            ReturnResult_ToggleDigitalSignButtons(false);
        } else {
            // Clear khi không có gì được chọn
            const $containHinh = $('.image-returnresult');
            $containHinh.empty();
            ReturnResult_ClearAllInputs();
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
    var $rows = $('.sat-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;
    var allChecked = total > 0 && marked === total;
    $('#sat_returnresult_chk_all').prop('checked', allChecked);
}

// Event handler cho checkbox để chỉ xử lý việc chọn multiple
$(document).on('change', '.sat-returnresult-chk-service', function () {
    var $rows = $('.sat-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Update trạng thái "chọn tất cả"
    var allChecked = total > 0 && marked === total;
    $('#sat_returnresult_chk_all').prop('checked', allChecked);

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
        const $containHinh = $('.image-returnresult');
        $containHinh.empty();
        ReturnResult_ClearAllInputs();
        ReturnResult_ToggleDigitalSignButtons(false);
    } else {
        // Không có gì được chọn → clear
        const $containHinh = $('.image-returnresult');
        $containHinh.empty();
        ReturnResult_ClearAllInputs();
        ReturnResult_ToggleDigitalSignButtons(false);
    }
});

function ReturnResult_LoadImageForService(id) {
    $("#image-returnresult").empty();
    CKEDITOR.instances["sat_returnresult_description"].setData("");
    $("#sat_returnresult_result").val("");
    $("#sat_returnresult_suggest").val("");
    $.ajax({
        url: "/SA_TIM_ReturnResult/GetImageForService_SieuAmTim?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                if (value.id) {
                    $("#image-returnresult").append('<div class="image-item"><img class="image" id="' + value.id + '" src="' + value.name + '" onclick="ReturnResult_GetImageCDHAId(' + value.id + ', this)" /></div > ');
                }
                CKEDITOR.instances["sat_returnresult_description"].setData(value.description);
                $("#sat_returnresult_result").val(value.result);
                $("#sat_returnresult_suggest").val(value.suggest);

                $('#sat_chieucao').val(value.sat_chieucao);
                $('#sat_cannang').val(value.sat_cannang);
                $('#sat_bsa').val(value.sat_bsa);
                $('#sat_bmi').val(value.sat_bmi);
                $('#sat_dmchu').val(value.sat_dmchu);
                $('#sat_nhitrai').val(value.sat_nhitrai);
                $('#sat_thatphai').val(value.sat_thatphai);
                $('#sat_ivsd').val(value.sat_ivsd);
                $('#sat_ivss').val(value.sat_ivss);
                $('#sat_lvidd').val(value.sat_lvidd);
                $('#sat_lvids').val(value.sat_lvids);
                $('#sat_lvpwd').val(value.sat_lvpwd);
                $('#sat_lvpws').val(value.sat_lvpws);
                $('#sat_rvdd').val(value.sat_rvdd);
                $('#sat_lvedv').val(value.sat_lvedv);
                $('#sat_fs').val(value.sat_fs);
                $('#sat_ef').val(value.sat_ef);
                $('#sat_vantoctoida_haila').val(value.sat_vantoctoida_haila);
                $('#sat_ea_haila').val(value.sat_ea_haila);
                $('#sat_chenhaptoida_haila').val(value.sat_chenhaptoida_haila);
                $('#sat_gdmean_haila').val(value.sat_gdmean_haila);
                $('#sat_dohovan_haila').val(value.sat_dohovan_haila);
                $('#sat_loaiho_haila').val(value.sat_loaiho_haila);
                $('#sat_hepvanhaila_haila').val(value.sat_hepvanhaila_haila);
                $('#sat_vantoctoida_bala').val(value.sat_vantoctoida_bala);
                $('#sat_chenhaptoida_bala').val(value.sat_chenhaptoida_bala);
                $('#sat_chenhaptb_bala').val(value.sat_chenhaptb_bala);
                $('#sat_hovan_bala').val(value.sat_hovan_bala);
                $('#sat_paps_bala').val(value.sat_paps_bala);
                $('#sat_vantoctoida_dongmachphoi').val(value.sat_vantoctoida_dongmachphoi);
                $('#sat_chenhaptoida_dongmachphoi').val(value.sat_chenhaptoida_dongmachphoi);
                $('#sat_gdmean_dongmachphoi').val(value.sat_gdmean_dongmachphoi);
                $('#sat_papm_dongmachphoi').val(value.sat_papm_dongmachphoi);
                $('#sat_papd_dongmachphoi').val(value.sat_papd_dongmachphoi);
                $('#sat_vantoctoida_dongmachchu').val(value.sat_vantoctoida_dongmachchu);
                $('#sat_chenhaptoida_dongmachchu').val(value.sat_chenhaptoida_dongmachchu);
                $('#sat_hovandmchu_dongmachchu').val(value.sat_hovandmchu_dongmachchu);
                $('#sat_hepvandmchu_dongmachchu').val(value.sat_hepvandmchu_dongmachchu);
                $('#sat_dongquavachliennhi').val(value.sat_dongquavachliennhi);
                $('#sat_dongquavachlienthat').val(value.sat_dongquavachlienthat);
                $('#sat_dongbatthuongkhac').val(value.sat_dongbatthuongkhac);
                $('#sat_tuthetim_2d').val(value.sat_tuthetim_2d);
                $('#sat_dmphoi_2d').val(value.sat_dmphoi_2d);
                $('#sat_mangngoaitim_2d').val(value.sat_mangngoaitim_2d);
                $('#sat_dmvanh_2d').val(value.sat_dmvanh_2d);
                $('#sat_nhitrai_2d').val(value.sat_nhitrai_2d);
                $('#sat_xoangvanh_2d').val(value.sat_xoangvanh_2d);
                $('#sat_nhiphai_2d').val(value.sat_nhiphai_2d);
                $('#sat_vanhaila_2d').val(value.sat_vanhaila_2d);
                $('#sat_tinhmach_2d').val(value.sat_tinhmach_2d);
                $('#sat_vanbala_2d').val(value.sat_vanbala_2d);
                $('#sat_dmchu_2d').val(value.sat_dmchu_2d);
                $('#sat_vandmc_2d').val(value.sat_vandmc_2d);
                $('#sat_vandmp_2d').val(value.sat_vandmp_2d);
            });
        }
    });
}

function ReturnResult_ClearAllInputs() {
    // Clear ảnh
    $("#image-returnresult").empty();

    // Clear CKEditor
    CKEDITOR.instances["sat_returnresult_description"].setData("");

    // Clear kết luận và đề nghị
    $("#sat_returnresult_result").val("Chức năng tâm thu thất trái bảo tồn (EF# %).");
    $("#sat_returnresult_suggest").val("");

    // Clear các chỉ số tim
    $('#sat_chieucao').val('');
    $('#sat_cannang').val('');
    $('#sat_bsa').val('');
    $('#sat_bmi').val('');
    $('#sat_dmchu').val('');
    $('#sat_nhitrai').val('');
    $('#sat_thatphai').val('');
    $('#sat_ivsd').val('');
    $('#sat_ivss').val('');
    $('#sat_lvidd').val('');
    $('#sat_lvids').val('');
    $('#sat_lvpwd').val('');
    $('#sat_lvpws').val('');
    $('#sat_rvdd').val('');
    $('#sat_lvedv').val('');
    $('#sat_fs').val('');
    $('#sat_ef').val('');

    // Van hai lá
    $('#sat_vantoctoida_haila').val('');
    $('#sat_ea_haila').val('');
    $('#sat_chenhaptoida_haila').val('');
    $('#sat_gdmean_haila').val('');
    $('#sat_dohovan_haila').val('');
    $('#sat_loaiho_haila').val('');
    $('#sat_hepvanhaila_haila').val('');

    // Van ba lá
    $('#sat_vantoctoida_bala').val('');
    $('#sat_chenhaptoida_bala').val('');
    $('#sat_chenhaptb_bala').val('');
    $('#sat_hovan_bala').val('');
    $('#sat_paps_bala').val('');

    // Động mạch phổi
    $('#sat_vantoctoida_dongmachphoi').val('');
    $('#sat_chenhaptoida_dongmachphoi').val('');
    $('#sat_gdmean_dongmachphoi').val('');
    $('#sat_papm_dongmachphoi').val('');
    $('#sat_papd_dongmachphoi').val('');

    // Động mạch chủ
    $('#sat_vantoctoida_dongmachchu').val('');
    $('#sat_chenhaptoida_dongmachchu').val('');
    $('#sat_hovandmchu_dongmachchu').val('');
    $('#sat_hepvandmchu_dongmachchu').val('');

    // Dòng bất thường
    $('#sat_dongquavachliennhi').val('');
    $('#sat_dongquavachlienthat').val('');
    $('#sat_dongbatthuongkhac').val('');

    // 2D
    $('#sat_tuthetim_2d').val('');
    $('#sat_dmphoi_2d').val('');
    $('#sat_mangngoaitim_2d').val('');
    $('#sat_dmvanh_2d').val('');
    $('#sat_nhitrai_2d').val('');
    $('#sat_xoangvanh_2d').val('');
    $('#sat_nhiphai_2d').val('');
    $('#sat_vanhaila_2d').val('');
    $('#sat_tinhmach_2d').val('');
    $('#sat_vanbala_2d').val('');
    $('#sat_dmchu_2d').val('');
    $('#sat_vandmc_2d').val('');
    $('#sat_vandmp_2d').val('');
}
function ReturnResult_GetImageCDHAId(imageCDHAId, img) {
    $('.image-returnresult img').removeClass('active');
    $(img).toggleClass('active');
    $("#sat_returnresult_xem").val(imageCDHAId);
}

function ReturnResult_XemHinh() {
    const selectedImg = $('.image-returnresult img.active');
    if (selectedImg.length === 0) {
        SwalHelper.Toast.warning('Bạn chưa chọn hình!');
        return;
    }
    $('#modalImage').attr('src', selectedImg.attr('src'));
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    modal.show();
}

// Kiểm tra trạng thái ký & hiện nút phù hợp
function ReturnResult_CheckSignStatus(resultCDHAId) {
    // Tuỳ bạn đặt endpoint, ví dụ:
    // GET /SignStore/ByResult?resultCDHAId=123
    // Trả: { signStoreId: 456, fileUrl: "/results/SA/202509/BN0001/xxx.pdf" } hoặc { } nếu chưa ký

    // Hiện “đang kiểm tra…”
    toggleSignButtons({ loading: true });
    console.log(resultCDHAId);
    $.ajax({
        url: "/SA_TIM_ReturnResult/GetSignStoreId",
        type: "GET",
        dataType: "json",
        data: { resultCDHAId: resultCDHAId },
        success: function (res) {
            console.log(res);
            if (res && res.signStoreId) {
                // ĐÃ CÓ FILE KÝ
                $('#btn-download-signed')
                    .data('signStoreId', res.signStoreId || null)
                    .data('fileUrl', res.fileUrl || null);

                toggleSignButtons({ hasSigned: true });
            } else {
                // CHƯA KÝ
                toggleSignButtons({ hasSigned: false });
            }
        },
        error: function () {
            // Lỗi API -> coi như chưa ký để user có thể ký lại
            toggleSignButtons({ hasSigned: false });
        }
    });
}

// Bật/tắt nút theo trạng thái
function toggleSignButtons(opts) {
    if (opts.loading) {
        $('#btn-sign-print').hide();
        $('#btn-download-signed').hide();
        return;
    }
    if (opts.hasSigned) {
        $('#btn-sign-print').hide();
        $('#btn-download-signed').show();
    } else {
        $('#btn-download-signed').hide();
        $('#btn-sign-print').show();
    }
}

//============================ Ký PDF ở giao diện trả kết quả ============================
function ReturnResult_SignPdf() {
    var patientId = $('#sat_returnresult_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
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
        SwalHelper.Toast.warning('Vui lòng chọn dịch vụ!');
        return;
    }

    // Lấy CCCD người ký (bác sĩ)
    var signerCCCD = $('#sat_returnresult_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning('Vui lòng nhập CCCD người ký (Viettel MySign)!');
        $('#sat_returnresult_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#sat_returnresult_patientId').val() || $('#sat_returnresult_sid').val() || "";
    var patientName = $('#sat_returnresult_patientName').val() || "";
    var doctorName = $('#sat_returnresult_userReturnResultSAT option:selected').text() || $('#sat_returnresult_userReturnResultSAT').val() || "";
    var performedAt = $('#sat_returnresult_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn

    // 1) Gọi API xuất PDF base64 (tận dụng endpoint ValidPrint đang có)
    //    Nếu bạn có endpoint riêng chỉ "Export" (không đổi trạng thái), thay URL ở đây là tốt nhất.
    $('#showWaitting').modal('show');
    var returnResultTime = $('#sat_returnresult_returnResultTime').val();
    var userReturnResult = $('#sat_returnresult_userLoginId').val();
    var description = CKEDITOR.instances['sat_returnresult_description'].getData();
    var result = $('#sat_returnresult_result').val();
    var suggest = $('#sat_returnresult_suggest').val();
    console.log(userReturnResult);
    var sieuamtimObj = {
        sat_chieucao: $('#sat_chieucao').val(),
        sat_cannang: $('#sat_cannang').val(),
        sat_bsa: $('#sat_bsa').val(),
        sat_bmi: $('#sat_bmi').val(),
        sat_dmchu: $('#sat_dmchu').val(),
        sat_nhitrai: $('#sat_nhitrai').val(),
        sat_thatphai: $('#sat_thatphai').val(),
        sat_ivsd: $('#sat_ivsd').val(),
        sat_ivss: $('#sat_ivss').val(),
        sat_lvidd: $('#sat_lvidd').val(),
        sat_lvids: $('#sat_lvids').val(),
        sat_lvpwd: $('#sat_lvpwd').val(),
        sat_lvpws: $('#sat_lvpws').val(),
        sat_rvdd: $('#sat_rvdd').val(),
        sat_lvedv: $('#sat_lvedv').val(),
        sat_fs: $('#sat_fs').val(),
        sat_ef: $('#sat_ef').val(),
        sat_vantoctoida_haila: $('#sat_vantoctoida_haila').val(),
        sat_ea_haila: $('#sat_ea_haila').val(),
        sat_chenhaptoida_haila: $('#sat_chenhaptoida_haila').val(),
        sat_gdmean_haila: $('#sat_gdmean_haila').val(),
        sat_dohovan_haila: $('#sat_dohovan_haila').val(),
        sat_loaiho_haila: $('#sat_loaiho_haila').val(),
        sat_hepvanhaila_haila: $('#sat_hepvanhaila_haila').val(),
        sat_vantoctoida_bala: $('#sat_vantoctoida_bala').val(),
        sat_chenhaptoida_bala: $('#sat_chenhaptoida_bala').val(),
        sat_chenhaptb_bala: $('#sat_chenhaptb_bala').val(),
        sat_hovan_bala: $('#sat_hovan_bala').val(),
        sat_paps_bala: $('#sat_paps_bala').val(),
        sat_vantoctoida_dongmachphoi: $('#sat_vantoctoida_dongmachphoi').val(),
        sat_chenhaptoida_dongmachphoi: $('#sat_chenhaptoida_dongmachphoi').val(),
        sat_gdmean_dongmachphoi: $('#sat_gdmean_dongmachphoi').val(),
        sat_papm_dongmachphoi: $('#sat_papm_dongmachphoi').val(),
        sat_papd_dongmachphoi: $('#sat_papd_dongmachphoi').val(),
        sat_vantoctoida_dongmachchu: $('#sat_vantoctoida_dongmachchu').val(),
        sat_chenhaptoida_dongmachchu: $('#sat_chenhaptoida_dongmachchu').val(),
        sat_hovandmchu_dongmachchu: $('#sat_hovandmchu_dongmachchu').val(),
        sat_hepvandmchu_dongmachchu: $('#sat_hepvandmchu_dongmachchu').val(),
        sat_dongquavachliennhi: $('#sat_dongquavachliennhi').val(),
        sat_dongquavachlienthat: $('#sat_dongquavachlienthat').val(),
        sat_dongbatthuongkhac: $('#sat_dongbatthuongkhac').val(),
        sat_tuthetim_2d: $('#sat_tuthetim_2d').val(),
        sat_dmphoi_2d: $('#sat_dmphoi_2d').val(),
        sat_mangngoaitim_2d: $('#sat_mangngoaitim_2d').val(),
        sat_dmvanh_2d: $('#sat_dmvanh_2d').val(),
        sat_nhitrai_2d: $('#sat_nhitrai_2d').val(),
        sat_xoangvanh_2d: $('#sat_xoangvanh_2d').val(),
        sat_nhiphai_2d: $('#sat_nhiphai_2d').val(),
        sat_vanhaila_2d: $('#sat_vanhaila_2d').val(),
        sat_tinhmach_2d: $('#sat_tinhmach_2d').val(),
        sat_vanbala_2d: $('#sat_vanbala_2d').val(),
        sat_dmchu_2d: $('#sat_dmchu_2d').val(),
        sat_vandmc_2d: $('#sat_vandmc_2d').val(),
        sat_vandmp_2d: $('#sat_vandmp_2d').val()
    }

    dataExport = {
        patientId: patientId,
        resultCDHAId: resultCDHAId,
        returnResultTime: returnResultTime,
        userReturnResult: userReturnResult,
        description: description,
        result: result,
        suggest: suggest,
        sieuamtim: JSON.stringify(sieuamtimObj)
    };
    console.log(dataExport)
    $.ajax({
        url: "/SA_TIM_ReturnResult/RePrint/",
        data: JSON.stringify(dataExport),
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        type: "POST",
        success: function (base64Pdf) {
            if (!base64Pdf) {
                $('#showWaitting').modal('hide');
                SwalHelper.Alert.error("Không thể xuất PDF để ký. Vui lòng kiểm tra lại!");
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
            formData.append("formType", "SieuAmTim");
            formData.append("patientCode", patientCode);
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
                        signStoreId = signStoreId || "51402";
                        // 2) LƯU signStoreId THEO resultCDHAId ĐANG CHỌN
                        if (signStoreId) {
                            $.ajax({
                                url: "/SA_TIM_Process/SaveSignStoreIdForResultCDHA",
                                type: "POST",
                                dataType: "text",
                                data: { resultCDHAId: resultCDHAId, signStoreId: signStoreId }
                            }).done(function (r) {
                                // tuỳ ý: thông báo nhẹ
                                // if (r !== "True") alert("Lưu signStoreId không thành công!");
                            });
                            // (tuỳ chọn) điền sẵn vào ô ở màn hình trả kết quả nếu bạn có
                            $("#sat_returnresult_signStoreId").val(signStoreId);
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
                        SwalHelper.Toast.success("Đã ký số xong, response:\n" + JSON.stringify(obj));

                    } catch (e) {
                        if (isProbablyBase64(resp)) {
                            openBase64Pdf(resp);
                        } else {
                            SwalHelper.Alert.error("Đã ký số, phản hồi:\n" + resp);
                        }
                    }
                },
                error: function (xhr) {
                    $('#showWaitting').modal('hide');
                    var msg = "Ký số thất bại.";
                    if (xhr && xhr.responseText) msg += "\n" + xhr.responseText;
                    SwalHelper.Alert.error(msg);
                }
            });
        },
        error: function () {
            $('#showWaitting').modal('hide');
            SwalHelper.Alert.error("Xuất PDF không thành công. Vui lòng kiểm tra lại!");
        }
    });
}

//============================ In file đã ký ============================
function ReturnResult_ViewSignedPdf() {
    var signStoreId = $('#sat_returnresult_signStoreId').val();

    function openSigned(id) {
        if (!id) { alert("Chưa có signStoreId cho dịch vụ này!"); return; }
        var url = "/api/ExternalSign/view-signed/" + encodeURIComponent(id);
        window.open(url, "_blank");
    }

    if (signStoreId && $.trim(signStoreId) !== "") {
        openSigned(signStoreId);
        return;
    }

    // Nếu input rỗng → lấy dịch vụ đang chọn (giống ReturnResult_Print)
    var resultCDHAId = "";
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            if ($(this).is(':checked')) {
                resultCDHAId = $(this).val();
            }
        });
    });
    if (!resultCDHAId) {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ để xem PDF đã ký!");
        return;
    }

    // Gọi API của bạn để lấy signStoreId đã lưu trong DB theo resultCDHAId
    $.ajax({
        url: "/SA_TIM_ReturnResult/GetSignStoreId",
        type: "GET",
        data: { resultCDHAId: resultCDHAId },
        dataType: "json",
        success: function (res) {
            console.log(res);
            var id = (res && (res.signStoreId || res.id)) ? (res.signStoreId || res.id) : "";
            if (id) {
                $('#sat_returnresult_signStoreId').val(id);
                openSigned(id);
            } else {
                SwalHelper.Toast.warning("Chưa lưu signStoreId cho dịch vụ này.");
            }
        },
        error: function () {
            SwalHelper.Toast.warning("Không lấy được signStoreId. Vui lòng kiểm tra!");
        }
    });
}

function ReturnResult_Load_SelectedDoctorInfo() {
    var userId = $("#sat_returnresult_userLoginId").val();
    console.log(userId)
    if (!userId) {
        $("#sat_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/SA_TIM_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#sat_returnresult_signerCCCD").text("Không lấy được thông tin bác sĩ.");
                return;
            }
            console.log(u)

            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#sat_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#sat_returnresult_signerCCCD").text("Không lấy được thông tin bác sĩ.");
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
                SwalHelper.Alert.error('Lỗi', 'Thao tác hủy ký chưa thành công');
            }
        },
        error: function () {
            SwalHelper.Alert.error('Lỗi', 'Lưu không thành công. Vui lòng kiểm tra lại!');
        }
    });
}
// ========================== KÝ SỐ PDF CHO TAB ĐÃ XONG (RETURN RESULT) ==========================
function ReturnResult_SignPdf_Multi(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    var patientId = $('#sat_returnresult_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
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
    var signerCCCD = $('#sat_returnresult_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#sat_returnresult_signerCCCD').focus();
        return;
    }

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#sat_returnresult_patientId').val() || $('#sat_returnresult_sid').val() || "";
    var patientMaBenhAn = $('#sat_returnresult_maBenhAn').val() || $('#sat_returnresult_sid').val() || "";
    var patientName = $('#sat_returnresult_patientName').val() || "";
    var doctorName = $('#sat_returnresult_userReturnResultSAT option:selected').text() || $('#sat_returnresult_userReturnResultSAT').val() || "";
    var doctorId = $('#sat_returnresult_userLoginId').val();
    var performedAt = $('#sat_returnresult_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "SieuAmTim"; // bạn có thể thay động tùy màn hình

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
                    url: "/SA_TIM_ReturnResult/Print?resultCDHAId=" + rid,
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
            SwalHelper.Alert.error('Lỗi', 'Không tạo được file PDF nào để ký.');
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
                        SwalHelper.Alert.error('Lỗi', `Xảy ra lỗi khi ký số: ${messageError}`);
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
                                    url: "/SA_ReturnResult/SaveSignStoreIdForResultCDHA",
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
                                                    referenceType: formType ?? "SieuAm",
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
                    SwalHelper.Alert.error(msg);
                    resolve();
                }
            });
        });
    });
}
// Tick/untick tất cả cho ReturnResult
$(document).on('change', '#sat_returnresult_chk_all', function () {
    var checked = this.checked === true;
    // chỉ tick các dịch vụ đang hiển thị
    $('.sat-returnresult-chk-service').prop('checked', checked);

    // Nếu check all thì load kết quả của dịch vụ đầu tiên
    const $containHinh = $('.image-returnresult');
    $containHinh.empty();
    ReturnResult_ClearAllInputs();
    ReturnResult_ToggleDigitalSignButtons(false);
});

// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả" cho ReturnResult
$(document).on('change', '.sat-returnresult-chk-service', function () {
    var $rows = $('.sat-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Nếu tất cả đều check => check header; ngược lại bỏ check header
    //var allChecked = total > 0 && marked === total;
    //$('#sat_returnresult_chk_all').prop('checked', allChecked);
});

// Helper function to toggle digital signature buttons
function ReturnResult_ToggleDigitalSignButtons(isDigitallySigned) {
    console.log('Digital signature status:', isDigitallySigned);

    // Toggle "Tải PDF đã ký" button
    var $printSignedBtn = $('#sat_returnresult_print_signed');
    if ($printSignedBtn.length) {
        if (isDigitallySigned) {
            $printSignedBtn.show().prop('disabled', false);
        } else {
            $printSignedBtn.hide().prop('disabled', true);
        }
    }

    // Toggle "Invalid & Xóa Ký Số" button  
    var $invalidRemoveSignBtn = $('#sat_returnresult_invalid_removedigitalsign');
    if ($invalidRemoveSignBtn.length) {
        if (isDigitallySigned) {
            $invalidRemoveSignBtn.show().prop('disabled', false);
        } else {
            $invalidRemoveSignBtn.hide().prop('disabled', true);
        }
    }
}
function ReturnResult_SaveDateToSession() {
    var fromDate = document.getElementById('sat_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('sat_returnresult_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/SA_TIM_ReturnResult/SaveSearchDates',
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