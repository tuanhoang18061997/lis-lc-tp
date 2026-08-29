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
    window.BarcodeScan.register('#sa_getsample_pidorseq', window.GetSample_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#sa_process_pidorseq', window.Process_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#sa_returnresult_pidorseq', window.ReturnResult_Search, {
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
    $('#sa_process_pidorseq, #sa_process_timeSearchFrom, #sa_process_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            Process_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của GetSample
    $('#sa_getsample_pidorseq, #sa_getsample_timeSearchFrom, #sa_getsample_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            GetSample_Search();
        }
    });

    // Gắn Enter key cho các input field tìm kiếm của ReturnResult
    $('#sa_returnresult_pidorseq, #sa_returnresult_timeSearchFrom, #sa_returnresult_timeSearchTo').on('keydown', function (e) {
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
            if ($('.sa-left-header').length === 0) {
                var header = $('<div class="sa-left-header">' +
                    '<div class="sa-left-header-title">Danh sách bệnh nhân</div>' +
                    '<button class="sa-left-header-close"><i class="bi bi-x-lg"></i></button>' +
                    '</div>');
                $('.sa-left').prepend(header);
            }
        } else {
            // Xóa các elements mobile trên desktop
            $('.mobile-toggle-patient-list').remove();
            $('.mobile-patient-list-overlay').remove();
            $('.sa-left-header').remove();
            $('.sa-left').removeClass('show');
        }
    }

    // Khởi tạo khi load trang
    initMobilePatientList();

    // Click vào nút toggle
    $(document).on('click', '.mobile-toggle-patient-list', function () {
        $('.sa-left').addClass('show');
        $('.mobile-patient-list-overlay').addClass('show');
        $(this).addClass('active');
        // Prevent scroll trên body
        $('body').css('overflow', 'hidden');
    });

    // Click vào nút đóng trong header
    $(document).on('click', '.sa-left-header-close', function () {
        closePatientList();
    });

    // Click vào overlay để đóng
    $(document).on('click', '.mobile-patient-list-overlay', function () {
        closePatientList();
    });

    // Hàm đóng danh sách
    function closePatientList() {
        $('.sa-left').removeClass('show');
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

    $(document).on('touchstart', '.sa-left', function (e) {
        if (window.innerWidth <= 768) {
            startX = e.touches[0].clientX;
            isDragging = true;
        }
    });

    $(document).on('touchmove', '.sa-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            currentX = e.touches[0].clientX;
            var diff = currentX - startX;

            // Chỉ cho phép swipe sang trái
            if (diff < 0) {
                $(this).css('transform', 'translateX(' + diff + 'px)');
            }
        }
    });

    $(document).on('touchend', '.sa-left', function (e) {
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
        $('#sa_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#sa_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#sa_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#sa_getsample_location').select2({
            placeholder: "-- Chọn --"
        });
    });   

    if (isLoadPage) {
        $('#sa_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#sa_getsample_doctor').select2({
            placeholder: "-- Chọn --"
        });
    });


    if (isLoadPage) {
        $('#sa_process_sampleresult').val("");
    }
    $(document).ready(function () {
        $('#sa_process_sampleresult').select2({
            placeholder: "-- Chọn --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#sa_process_userReturnResultSA').select2({
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
                if (e.id === "sa_getsample_location") {
                    $('#sa_getsample_location').select2('focus');
                }
                else if (e.id === "sa_getsample_doctor") {
                    $('#sa_getsample_doctor').select2('focus');
                }
                else if (e.id === "sa_getsample_userReturnResultSA") {
                    $('#sa_getsample_userReturnResultSA').select2('focus');
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
        url: "/SA_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#sa_getsample_timeSearchFrom").val(today);
            //$("#sa_getsample_timeSearchTo").val(today);
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
        url: "/SA_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#sa_getsample_timeSearchFrom").val(today);
            $("#sa_getsample_timeSearchTo").val(today);
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Search() {
    var sa_getsample_pidorseq = $("#sa_getsample_pidorseq").val();
    var timeSearchFrom = $("#sa_getsample_timeSearchFrom").val();
    var timeSearchTo = $("#sa_getsample_timeSearchTo").val();
    $.ajax({
        url: "/SA_GetSample/Search?" + "pidorseq=" + sa_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
$("#sa_getsample_savepatient").hide();
$("#sa_getsample_cancelpatient").hide();
function GetSample_HideButton(_new, _save, _delete, _cancel, _addservice, _getsample) {
    if (_new == 1) {
        $("#sa_getsample_newpatient").hide();
    }
    else {
        $("#sa_getsample_newpatient").show();
    }

    if (_save == 1) {
        $("#sa_getsample_savepatient").hide();
    }
    else {
        $("#sa_getsample_savepatient").show();
    }

    if (_delete == 1) {
        $("#sa_getsample_deletepatient").hide();
    }
    else {
        $("#sa_getsample_deletepatient").show();
    }

    if (_cancel == 1) {
        $("#sa_getsample_cancelpatient").hide();
    }
    else {
        $("#sa_getsample_cancelpatient").show();
    }

    if (_addservice == 1) {
        $("#sa_getsample_addService").hide();
    }
    else {
        $("#sa_getsample_addService").show();
    }

    if (_getsample == 1) {
        $("#sa_getsample_processresult").hide();
    }
    else {
        $("#sa_getsample_processresult").show();
    }
}


function GetSample_GetPatientInfo(id) {
    GetSample_HideButton(false, true, false, true, false, false);
    $.ajax({
        url: "/SA_GetSample/GetPatientInfo?id=" + id,
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
        url: "/sa_GetSample/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#sa-right-service-gridview").html(result);
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
    $("#sa_getsample_pidorseq").val('');
    $('#sa_getsample_id').val('');
    $('#sa_getsample_patientId').val('');
    $('#sa_getsample_seq').val('');
    $('#sa_getsample_sid').val('');
    $('#sa_getsample_patientName').val('');
    $('#sa_getsample_age').val('');
    $('#sa_getsample_sex').val('');
    $('#sa_getsample_obj').val('');
    $('#sa_getsample_type').val('');
    $('#sa_getsample_location').val('');
    $('#sa_getsample_doctor').val('');
    $('#sa_getsample_getSampleTime').val(dateTime);
    $('#sa_getsample_address').val('');
    $('#sa_getsample_diagnostic').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
    GetSample_SetSelect2_01(false);
}

function GetSample_NewPatient() {
    $('#sa_getsample_patientId').focus();
    GetSample_HideButton(true, false, true, false, true, true);
    GetSample_ResetInput();
}

function GetSample_DeletePatient() {
    var id = $('#sa_getsample_id').val();
    if (id == '') {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else {
        var choice = confirm("Bạn muốn xoá bệnh nhân và tất cả chỉ định xét nghiệm?");
        if (choice) {
            $.ajax({
                url: "/SA_GetSample/DeletePatientAndService?id=" + id,
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
    var id = $('#sa_getsample_id').val();
    if (id == '') {
        GetSample_ResetInput()
    }
    else {
        GetSample_GetPatientInfo(id);
    }
}

function GetSample_GetSID(seq) {
    $.ajax({
        url: "/SA_GetSample/GetSID?seq=" + seq,
        type: "GET",
        dataType: "text",
        cache: false,
        success: function (result) {
            $('#sa_getsample_sid').val(result);
        }
    });
}

function GetSample_SavePatient() {
    var validate = GetSample_ValidateInput('patientInfo');
    if (validate) {
        var id = $('#sa_getsample_id').val();
        var patientId = $('#sa_getsample_patientId').val();
        var seq = $('#sa_getsample_seq').val();
        var sid = $('#sa_getsample_sid').val();
        var patientName = $('#sa_getsample_patientName').val();
        var age = $('#sa_getsample_age').val();
        var sex = $('#sa_getsample_sex').val();
        var obj = $('#sa_getsample_obj').val();
        var type = $('#sa_getsample_type').val();
        var location = $('#sa_getsample_location').val();
        var doctor = $('#sa_getsample_doctor').val();
        var getSampleTime = $('#sa_getsample_getSampleTime').val();
        var address = $('#sa_getsample_address').val();
        var diagnostic = $('#sa_getsample_diagnostic').val();
        var category = $('#select-category').val();
        var service = $('#select-service').val();

        $.ajax({
            url: "/SA_GetSample/SavePatient?id= " + id + "&&patientId=" + patientId + "&&seq=" + seq + "&&sid=" + sid + "&&patientName=" + patientName + "&&age=" + age + "&&sex=" + sex + "&&obj=" + obj + "&&type=" + type + "&&location=" + location + "&&doctor=" + doctor + "&&getSampleTime=" + getSampleTime + "&&address=" + address + "&&diagnostic=" + diagnostic + "&&service=" + service,
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
    var id = $('#sa_getsample_id').val();
    var doctorId = $('#sa_getsample_doctor').val();
    console.log(doctorId);
    if (id === "") {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else if (doctorId == null) {
        $('#sa_getsample_doctor').select2('focus');
        //alert("Vui lòng chọn bác sĩ chỉ định!");
        //return;
    }
    else {
        var getSampleTime = $('#sa_getsample_getSampleTime').val();
        $.ajax({
            url: "/SA_GetSample/ProcessResult?id= " + id + "&getSampleTime=" + getSampleTime,
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
    var id = $('#sa_getsample_id').val();
    if (id === "") {
        $('#addServiceForm').modal('hide');
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else {
        GetSample_SetSelect2_02();
    }
}

function GetSample_DeleteServiceForPatient(idResultCDHA) {
    var patientId = $('#sa_getsample_id').val();
    // Bước 1: Kiểm tra trạng thái dịch vụ
    $.ajax({
        url: "/SA_GetSample/CheckAndDeleteService?id=" + idResultCDHA,
        type: "POST",
        dataType: "json",
        cache: false,
        success: function (result) {
            if (!result.success) {
                SwalHelper.Alert.error('Lỗi', result.message || 'Không thể kiểm tra dịch vụ. Vui lòng kiểm tra lại!');
                return;
            }

            var confirmMessage = "";
            if (result.hasResult) {
                confirmMessage = "Dịch vụ này đã có kết luận. Bạn có chắc chắn muốn hủy dịch vụ không?";
            } else {
                confirmMessage = "Bạn chắc chắn muốn hủy dịch vụ này?";
            }

            if (confirm(confirmMessage)) {
                // Bước 2: Thực hiện xóa và gọi API HIS
                $.ajax({
                    url: "/SA_GetSample/CancelServiceAndNotifyHIS",
                    type: "POST",
                    data: {
                        id: idResultCDHA,
                        ticketItemId: result.ticketItemId,
                        type: result.typeBenhAn
                    },
                    dataType: "json",
                    cache: false,
                    success: function (cancelResult) {
                        if (cancelResult.success) {
                            SwalHelper.Toast.success(cancelResult.message);
                            GetSample_GetListServiceForPatient(patientId);
                        } else {
                            SwalHelper.Alert.error('Lỗi', cancelResult.message || 'Không thể hủy dịch vụ. Vui lòng kiểm tra lại!');
                        }
                    },
                    error: function () {
                        SwalHelper.Alert.error('Lỗi', 'Có lỗi xảy ra khi hủy dịch vụ. Vui lòng kiểm tra lại!');
                    }
                });
            }
        },
        error: function () {
            SwalHelper.Alert.error('Lỗi', 'Không thể kiểm tra dịch vụ. Vui lòng kiểm tra lại!');
        }
    });
}

function GetSample_AddServiceForPatient() {
    var serviceId = $('#sa_getsample_service').val();
    var patientId = $('#sa_getsample_id').val();
    var doctorId = $('#sa_getsample_doctor').val();
    if (serviceId === "" || patientId === "") {
        SwalHelper.Alert.error('Lỗi', 'Chỉ định dịch vụ không thành công. Vui lòng kiểm tra lại !');
    }
    else {
        $.ajax({
            url: "/SA_GetSample/AddServiceForPatient?patientId=" + patientId + "&&serviceId=" + serviceId + "&&doctorId=" + doctorId,
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
        url: "/SA_GetSample/Get_Count/",
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
    var fromDate = document.getElementById('sa_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('sa_getsample_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/SA_GetSample/SaveSearchDates',
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
                if (e.id === "sa_process_userReturnResultSA") {
                    $('#sa_process_userReturnResultSA').select2('focus');
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
        url: "/SA_Process/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#sa_process_timeSearchFrom").val(today);
            //$("#sa_process_timeSearchTo").val(today);

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
    var sa_process_pidorseq = $("#sa_process_pidorseq").val();
    var timeSearchFrom = $("#sa_process_timeSearchFrom").val();
    var timeSearchTo = $("#sa_process_timeSearchTo").val();
    console.log(sa_process_pidorseq);
    $.ajax({
        url: "/SA_Process/Search?" + "pidorseq=" + sa_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#sa_process_pidorseq").val('');
    $('#sa_process_id').val('');
    $('#sa_process_patientId').val('');
    $('#sa_process_seq').val('');
    $('#sa_process_sid').val('');
    $('#sa_process_patientName').val('');
    $('#sa_process_age').val('');
    $('#sa_process_sex').val('');
    $('#sa_process_obj').val('');
    $('#sa_process_type').val('');
    $('#sa_process_location').val('');
    //$('#sa_process_doctor').val('');
    $('#sa_process_getSampleTime').val('');
    $('#sa_process_returnResultTime').val('');
    $('#sa_process_location').val('');
    //$('#sa_process_doctor').val('');
    $('#sa_process_userReturnResultSA').val('');
    $('#sa_process_address').val('');
    $('#sa_process_diagnostic').val('');
    $('#tbody-gridview-service').empty();
    $('#imageCDHA').empty();
    $('#sa_list_service').empty();
    $('#sa_process_result').empty();
    $('#sa_process_suggest').empty();
    CKEDITOR.instances["sa_process_description"].setData("");
    Process_SetSelect2_03(true);
}

function Process_GetPatientInfo(id) {
    $.ajax({
        url: "/SA_Process/Check_SelectDevice/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            if (result === "False") {
                $('#addDeviceForm').modal('show');
            } else {
                $.ajax({
                    url: "/SA_Process/GetPatientInfo?id=" + id,
                    type: "GET",
                    dataType: "html",
                    cache: false,
                    success: function (result) {
                        $(".list-group-item-action").removeClass("active");
                        $(`.list-group-item-action[data-id='${id}']`).addClass("active");
                        $("#process_patientInfo").html(result);
                        Process_SetSelect2_03();
                        GetSample_SetSelect2_01(true);
                        // Auto-chọn bác sĩ = user đang login (nếu chưa có giá trị)
                        var loginId = $("#sa_process_userLoginId").val();
                        var $sel = $("#sa_process_userReturnResultSA");
                        if (loginId && (!$sel.val() || $sel.val() === "")) {
                            $sel.val(loginId).trigger("change");
                        }

                        // Gắn handler đổi bác sĩ
                        $(document)
                            .off("change", "#sa_process_userReturnResultSA")
                            .on("change", "#sa_process_userReturnResultSA", Process_Load_SelectedDoctorInfo);
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
        }
    });
}

function Process_GetListServiceForPatient(id) {
    $.ajax({
        url: "/sa_Process/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_sa-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function Process_SaveResult() {
    var patientId = $('#sa_process_id').val();
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
                var returnResultTime = $('#sa_process_returnResultTime').val();
                var userReturnResult = $('#sa_process_userReturnResultSA').val();
                var description = CKEDITOR.instances['sa_process_description'].getData();
                var result = $('#sa_process_result').val();
                var suggest = $('#sa_process_suggest').val();

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
                    url: "/SA_Process/SaveResult/",
                    data: JSON.stringify(data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    type: "POST",
                    success: function (result) {
                        if (result == 'True') {
                            SwalHelper.Toast.success('Lưu thành công!');
                            Process_GetPatientInfo(patientId);
                        }
                        else {
                            SwalHelper.Alert.error('Lỗi', 'Lưu không thành công. Vui lòng kiểm tra lại!');
                        }
                    },
                    error: function () {
                        SwalHelper.Alert.error('Lỗi', 'Không thể kết nối đến server');
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

//function Process_ValidPrint() {
//    var patientId = $('#sa_process_id').val();
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
//                var returnResultTime = $('#sa_process_returnResultTime').val();
//                var userReturnResult = $('#sa_process_userReturnResultSA').val();
//                var description = CKEDITOR.instances['sa_process_description'].getData();
//                var result = $('#sa_process_result').val();
//                var suggest = $('#sa_process_suggest').val();
//                // Danh sách ảnh cần in
//                var selectedImageIds = $(".sa-print-check:checked")
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
//                    selectedImageIds: selectedImageIds
//                };
//                console.table(data);
//                $.ajax({
//                    url: "/SA_Process/ValidPrint/",
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
                url: "/SA_Process/ValidPrintMultiple/",
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
                url: "/SA_Process/ValidPrint/",
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

    var patientId = $('#sa_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Validate input chung (bác sĩ, thời điểm trả kết quả,...)
    var ok = Process_ValidateInput('process_patientInfo');
    if (!ok) return;

    var returnResultTime = $('#sa_process_returnResultTime').val();
    var userReturnResult = $('#sa_process_userReturnResultSA').val();

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

    // Nếu chỉ có 1 dịch vụ => hành vi cũ (dùng nội dung editor hiện tại nếu cần)
    // Nếu nhiều dịch vụ => in lần lượt, dùng dữ liệu đã LƯU ở server (useSaved=true)
    var isMulti = resultIds.length > 1;
    var description = null;
    var resultText = null;
    var suggest = null;
    var selectedImageIds = null;

    console.log("resultIdss: ", resultIds);
    if (!resultIds.length) {
        SwalHelper.Toast.warning('Không tìm thấy dịch vụ nào để in!');
        return;
    }
    else if (resultIds.length == 1) {
        console.log("resultIds.length == 1")
        //Process_CheckedBoxOnRow(resultIds[0]);
    }

    description = isMulti ? null : (CKEDITOR.instances['sa_process_description'].getData() || null);
    resultText = isMulti ? null : ($('#sa_process_result').val() || null);
    suggest = isMulti ? null : ($('#sa_process_suggest').val() || null);
    selectedImageIds = isMulti ? null : $(".sa-print-check:checked").map(function () { return $(this).val(); }).get(); // nếu có UI chọn ảnh riêng cho 1 dịch vụ

    var opts = {
        patientId: patientId,
        returnResultTime: returnResultTime,
        userReturnResult: userReturnResult,
        description: description,
        result: resultText,
        suggest: suggest,
        selectedImageIds: selectedImageIds
    };

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
    var id = $('#sa_process_id').val();
    if (id === "") {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    else {
        $.ajax({
            url: "/SA_Process/GetSample?id= " + id,
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
        url: "/SA_Process/Get_Count/",
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
//            $('#sa_process_sampleresult').val('');
//            $('#sa_process_sampleresult').select2({
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
    $('#sa_process_saveresult').show();
    $('#sa_process_result').attr('disabled', false);
    $('#sa_list_service .row-service').removeClass('multi-selected');
    $(".row-service .form-check-input").each(function () {
        var $chk = $(this);
        var resultCdhaId = String($chk.val());
        var isTarget = (resultCdhaId === String(id));
        $chk.prop("checked", isTarget);

        if (isTarget) {
            // đánh dấu dòng đang thao tác
            $('.row-service').removeClass('active');
            $chk.closest('.row-service').addClass('active');
            // 1) lấy ServiceId thực của dịch vụ từ data-serviceid
            var serviceId = $chk.data("serviceid");
            var bs_thuc_hien = $chk.data("bs-thuc-hien");
            if (bs_thuc_hien != null && bs_thuc_hien != '') {
                $('#sa_process_userReturnResultSA').val(bs_thuc_hien).trigger("change");
            } else {
                var loginId = $("#sa_process_userLoginId").val();
                $('#sa_process_userReturnResultSA').val(loginId).trigger("change");

            }
            // 2) gọi load và để hàm tự ưu tiên “đã lưu”, nếu không có sẽ fallback theo service+gender
            Process_Load_ResultAndImage_ForService(id, serviceId);
        }
    });
}

// Chuẩn hoá giới tính từ select id = sa_process_sex
function _normalizeSexFromUI() {
    var raw = ($("#sa_process_sex").val() || "").toString().trim().toUpperCase();
    // UI hiện hiển thị tiếng Việt hoặc code khác, coi "NAM" là M, còn lại là F
    if (raw === "NAM" || raw === "M" || raw === "1") return "M";
    return "F";
}

// Tự chọn Sample theo service + gender
function _autoPickSampleForService(serviceId) {
    var $sel = $("#sa_process_sampleresult");
    if (!$sel.length) return;

    var gender = _normalizeSexFromUI();           // "M" | "F"
    var accept = new Set([gender, "MF"]);         // luôn chấp nhận "MF" như yêu cầu

    // Lấy toàn bộ option có data phù hợp
    var $options = $sel.find("option").filter(function () {
        var optService = $(this).data("serviceid");
        var optGender = ($(this).data("gender") || "").toString().toUpperCase();
        return String(optService) === String(serviceId) && accept.has(optGender);
    });
    if (!$options.length) {
        $("#sa_process_sampleresult").val("");
        $('#sa_process_sampleresult').select2({
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
    var $sel = $("#sa_process_sampleresult");
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
//    $("#imageCDHA").empty();
//    CKEDITOR.instances["sa_process_description"].setData("");
//    $("#sa_process_result").val("");
//    $("#sa_process_suggest").val("");
//    $.ajax({
//        url: "/SA_Process/GetImageForService?id=" + id,
//        type: 'GET',
//        dataType: 'json',
//        success: function (response) {
//            var html = '';
//            $.each(response, function (key, value) {
//                if (value.id) {
//                    //$("#imageCDHA").append('<img class="image-item" id="' + value.id + '" src="' + value.name + '" onclick="Process_GetImageCDHAId(' + value.id + ')"/>');
//                    html += `
//                        <div class="img-cell">
//                            <img class="image-item"
//                                 id="img_${value.id}"
//                                 src="${value.name}"
//                                 data-id="${value.id}"
//                                 onclick="Process_GetImageCDHAId(${value.id}, this)" />
//                            <label class="sa-print-ctrl">
//                              <input type="checkbox"
//                                     class="sa-print-check"
//                                     value="${value.id}"
//                                     checked />
//                              Chọn in
//                            </label>
//                        </div>`;
//                    $("#imageCDHA").html(html);

//                    // Chỉ đánh dấu UI "đã chọn để IN" (không đụng tới ảnh đang chọn)
//                    $(".sa-print-check").off("change").on("change", function () {
//                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
//                    });

//                    // Khởi tạo trạng thái print-selected theo checkbox (mặc định đã checked)
//                    $(".sa-print-check").each(function () {
//                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
//                    });
//                }
//                CKEDITOR.instances["sa_process_description"].setData(value.description);
//                $("#sa_process_result").val(value.result);
//                $("#sa_process_suggest").val(value.suggest);
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
    $("#imageCDHA").empty();
    CKEDITOR.instances["sa_process_description"].setData("");
    $("#sa_process_result").val("");
    $("#sa_process_suggest").val("");
    $("#sa_process_sampleresult").val("");
    $('#sa_process_sampleresult').select2({
        placeholder: "-- Chọn --"
    });
    $.ajax({
        url: "/SA_Process/GetImageForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var html = '';
            var firstPayload = null;

            $.each(response, function (key, value) {
                if (!firstPayload && value) firstPayload = value;
                if (value.id) {
                    //$("#imageCDHA").append('<img class="image-item" id="' + value.id + '" src="' + value.name + '" onclick="Process_GetImageCDHAId(' + value.id + ')"/>');
                    html += `
                        <div class="img-cell">
                            <img class="image-item"
                                 id="img_${value.id}"
                                 src="${value.name}"
                                 data-id="${value.id}"
                                 onclick="Process_GetImageCDHAId(${value.id}, this)" />
                            <label class="sa-print-ctrl">
                              <input type="checkbox"
                                     class="sa-print-check"
                                     value="${value.id}"
                                     checked />
                              Chọn in
                            </label>
                        </div>`;
                }
                //CKEDITOR.instances["sa_process_description"].setData(value.description);
                //$("#sa_process_result").val(value.result);
                //$("#sa_process_suggest").val(value.suggest);
            });
            $("#imageCDHA").html(html);

            // Chỉ đánh dấu UI "đã chọn để IN" (không đụng tới ảnh đang chọn)
            $(".sa-print-check").off("change").on("change", function () {
                $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
            });

            // Khởi tạo trạng thái print-selected theo checkbox (mặc định đã checked)
            $(".sa-print-check").each(function () {
                $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
            });

            // set mô tả/kết quả/tư vấn (lấy từ phần tử đầu tiên nếu có)
            var desc = "", res = "", suggest = "";
            if (firstPayload) {
                desc = firstPayload.description ?? "";
                res = firstPayload.result ?? "";
                suggest = firstPayload.suggest ?? "";
            }
            if (CKEDITOR.instances["sa_process_description"]) {
                CKEDITOR.instances["sa_process_description"].setData(desc);
            }
            $("#sa_process_result").val(res);
            $("#sa_process_suggest").val(suggest);

            // ✅ CHỈ khi cả desc và result đều null/rỗng thì mới tự chọn sample
            var needAutoPick =
                (!desc || desc.trim() === "") &&
                (!res || res.trim() === "");

            if (needAutoPick && serviceId != null) {
                _autoPickSampleForService(serviceId);
            } else {
                $("#sa_process_sampleresult").val("");
                $('#sa_process_sampleresult').select2({
                    placeholder: "-- Chọn --"
                });
            }
            // nếu không needAutoPick: giữ nguyên select hiện tại, không can thiệp

            return { needAutoPick: !!needAutoPick };
        }
    });
}
function Process_GetImageCDHAId(imageCDHAId, el) {
    $("#sa_process_deletephoto").val(imageCDHAId); // giữ logic cũ
    $("#sa_process_xem").val(imageCDHAId);
    $(".image-item").removeClass("active");         // chỉ 1 ảnh được chọn
    $(el).addClass("active");
}

// Begin chụp hình
function StartCamera() {
    $.ajax({
        url: "/SA_Process/Check_SelectDevice/",
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
    document.getElementById("sa_process_startcamera").style.display = "none";
    document.getElementById("sa_process_snapshot").style.display = "unset";
    document.getElementById("sa_process_deletephoto").style.display = "unset";
    document.getElementById("sa_process_startvideo").style.display = "unset";
    document.getElementById("sa_process_xem").style.display = "unset";
}

function Process_SelectDevice() {
    var deviceId = $('#sa_getsample_select_device_select').val();
    if (deviceId === "") {
        SwalHelper.Toast.warning('Vui lòng chọn thiết bị !');
    }
    else {
        $.ajax({
            url: "/SA_Process/SelectDevice?deviceId=" + deviceId,
            type: 'GET',
            dataType: 'text',
            success: function (result) {
                if (result == 'False') {
                    SwalHelper.Alert.error('Lỗi', 'Không thể chọn thiết bị. Kiểm tra lại !');
                }
                //else {
                //    StartCamera_01();
                //}
            },
            error: function () {
                SwalHelper.Alert.error('Lỗi', 'Không thể chọn thiết bị. Kiểm tra lại !');
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
    const video = document.querySelector('#video');
    if (!video || !video.srcObject) {
        SwalHelper.Toast.warning('Vui lòng bật camera trước khi chụp ảnh!');
        return false;
    }
    if (countImage === 4) {
        SwalHelper.Toast.warning('Chỉ cho phép chụp tối đa 4 hình !');
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
                url: "/SA_Process/SaveImageCDHA/",
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
            SwalHelper.Toast.warning("Vui lòng chọn dịch vụ !");
        }
    }   
}

// ========================== FOOT SWITCH CAPTURE IMAGE ==========================
// Foot Switch USB nên cấu hình dạng HID Keyboard và map thành phím F2.
// Khi đạp Foot Switch => gọi SnapShot() để chụp ảnh từ máy siêu âm/camera.

(function () {
    const FOOT_SWITCH_KEY = 'F2';
    let lastCaptureAt = 0;
    const CAPTURE_DEBOUNCE_MS = 800;

    function isProcessPageReadyForCapture() {
        // Chỉ cho phép ở màn SA_Process có vùng ảnh/camera
        if ($('#sa_process_id').length === 0) return false;

        const patientId = $('#sa_process_id').val();
        if (!patientId) {
            SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân trước khi chụp ảnh!');
            return false;
        }

        let resultCDHAId = null;
        $('.row-service .form-check-input:checked').each(function () {
            resultCDHAId = $(this).val();
        });

        if (!resultCDHAId) {
            SwalHelper.Toast.warning('Vui lòng chọn dịch vụ trước khi chụp ảnh!');
            return false;
        }

        const video = document.querySelector('#video');
        if (!video || !video.srcObject) {
            SwalHelper.Toast.warning('Vui lòng bật camera trước khi chụp ảnh!');
            return false;
        }

        return true;``
    }

    function handleFootSwitchCapture(e) {
        if (e.key !== FOOT_SWITCH_KEY) return;

        e.preventDefault();
        e.stopPropagation();

        const now = Date.now();
        if (now - lastCaptureAt < CAPTURE_DEBOUNCE_MS) {
            return;
        }
        lastCaptureAt = now;

        if (!isProcessPageReadyForCapture()) {
            return;
        }
        console.log(e);
        try {
            SnapShot();
        } catch (err) {
            console.error('FootSwitch SnapShot error:', err);
            SwalHelper.Alert.error('Lỗi', 'Không thể chụp ảnh từ Foot Switch. Vui lòng kiểm tra lại!');
        }
    }

    // Bắt phím ở window/document
    window.addEventListener('keydown', handleFootSwitchCapture, true);
    document.addEventListener('keydown', handleFootSwitchCapture, true);

    // Bắt thêm trong CKEditor iframe, vì khi đang focus trong CKEditor,
    // keydown có thể không bubble ra document cha.
    if (window.CKEDITOR) {
        CKEDITOR.on('instanceReady', function (ev) {
            ev.editor.document.on('keydown', function (event) {
                const domEvent = event.data.$;
                handleFootSwitchCapture(domEvent);
            });
        });
    }
})();

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
    var imageCDHAId = $("#sa_process_deletephoto").val();
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
                    url: "/SA_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
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
    //        url: "/SA_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
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
// End chụp hình

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
            url: "/SA_Process/UploadHinh/",
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response !== 'True') {
                    SwalHelper.Alert.error('Lỗi', 'Không thể lưu ảnh. Vui lòng kiểm tra lại!');
                } else {
                    //Process_Load_ResultAndImage_ForService(resultCDHAId);
                    Process_CheckedBoxOnRow(resultCDHAId);
                }
            }
        });
    }
    else {
        SwalHelper.Toast.warning('Vui lòng chọn dịch vụ !');
    }
}
// Begin ghi video màn hình
let media_recorder = null;
let blobs_recorded = [];
let download_link = document.querySelector("#sa_process_downloadvideo_a");
function StartVideo() {
    document.getElementById("sa_process_startvideo").style.display = "none";
    document.getElementById("sa_process_stopvideo").style.display = "unset";

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
    document.getElementById("sa_process_stopvideo").style.display = "none";
    document.getElementById("sa_process_deletevideo").style.display = "unset";
    document.getElementById("sa_process_downloadvideo").style.display = "unset";

    media_recorder.stop();
}

function DeleteVideo() {
    document.getElementById("sa_process_startvideo").style.display = "unset";
    document.getElementById("sa_process_deletevideo").style.display = "none";
    document.getElementById("sa_process_downloadvideo").style.display = "none";

    media_recorder = null;
    blobs_recorded = [];
    download_link.href = null;
}

function DownloadVideo() {
    document.getElementById("sa_process_startvideo").style.display = "unset";
    document.getElementById("sa_process_deletevideo").style.display = "none";
    document.getElementById("sa_process_downloadvideo").style.display = "none";

    download_link.click();
    media_recorder = null;
    blobs_recorded = [];
    download_link.href = null;
}
// End ghi video màn hình

function Process_Load_Image_ForService(id) {
    $("#imageCDHA").empty();
    $.ajax({
        url: "/SA_Process/GetImageForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var html = '';
            $.each(response, function (key, value) {
                if (value.id) {
                    //$("#imageCDHA").append('<img class="image-item" id="' + value.id + '" src="' + value.name + '" onclick="Process_GetImageCDHAId(' + value.id + ')"/>');
                    html += `
                        <div class="img-cell">
                            <img class="image-item"
                                 id="img_${value.id}"
                                 src="${value.name}"
                                 data-id="${value.id}"
                                 onclick="Process_GetImageCDHAId(${value.id}, this)" />
                            <label class="sa-print-ctrl">
                              <input type="checkbox"
                                     class="sa-print-check"
                                     value="${value.id}"
                                     checked />
                              Chọn in
                            </label>
                        </div>`;
                    $("#imageCDHA").html(html);

                    // Chỉ đánh dấu UI "đã chọn để IN" (không đụng tới ảnh đang chọn)
                    $(".sa-print-check").off("change").on("change", function () {
                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
                    });

                    // Khởi tạo trạng thái print-selected theo checkbox (mặc định đã checked)
                    $(".sa-print-check").each(function () {
                        $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
                    });
                }
            });
        }
    });
}

function GetSampleForService(id) {
    $.ajax({
        url: "/SA_Process/GetSampleForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            CKEDITOR.instances["sa_process_description"].setData(response.description);
            $("#sa_process_result").val(response.result);
            $("#sa_process_suggest").val(response.suggest);
        }
    });
}

// Tải thông tin bác sĩ khi đổi select
function Process_Load_SelectedDoctorInfo() {
    var userId = $("#sa_process_userReturnResultSA").val();
    if (!userId) {
        $("#sa_process_signerCCCD").empty();
        $("#sa_process_doctorInfo").empty();
        return;
    }
    $.ajax({
        url: "/SA_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#sa_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#sa_process_signerCCCD").val(cccd);
        },
        error: function () {
            $("#sa_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
        }
    });
}

// ========================== KÝ SỐ PDF (Form: SIÊU ÂM) ==========================

async function testKySoTruyenFilePdf() {
    const fileUrl = `http://172.16.0.2:6868/download/Bien_ban_hoi_chan_thong_qua_phau_thuat.pdf`;
    const apiUrl = "http://172.16.0.7:8080/signserver/api/ext/signPdfWithMysign";
    const signerCCCD = "089097018854";// CCCD bác sĩ
    const taxCode = "0316876869";// Mã số thuế công ty
    const targetText = "Trần Minh Tuấn"; // Tên bác sĩ

    try {
        // 1. Tải file về dạng blob
        const pdfResponse = await axios.get(fileUrl, {
            responseType: 'blob'
        });

        // 2. Tạo đối tượng File từ blob
        const pdfFile = new File([pdfResponse.data], 'Bien_ban_hoi_chan.pdf', {
            type: 'application/pdf'
        });

        // 3. Tạo FormData
        const formData = new FormData();
        formData.append('file', pdfFile);
        formData.append('signUserId', signerCCCD);
        formData.append('taxcode', taxCode);
        formData.append('targetText', targetText);

        // 4. Gửi request ký số
        const result = await axios.post(apiUrl, formData);

        console.log("Đã ký PDF:", result.data);
        // Xử lý kết quả tại đây (ví dụ: lưu đường dẫn, reload...)
    } catch (err) {
        console.error("Lỗi khi ký PDF:", err.response?.data || err.message);
        showError("Ký thất bại.");
    }
}
function Process_SignPdf() {
    var patientId = $('#sa_process_id').val();
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
    var signerCCCD = $('#sa_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#sa_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#sa_process_patientId').val() || $('#sa_process_sid').val() || "";
    var patientName = $('#sa_process_patientName').val() || "";
    var doctorName = $('#sa_process_userReturnResultSA option:selected').text() || $('#sa_process_userReturnResultSA').val() || "";
    var performedAt = $('#sa_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "SieuAm"; 

    // 1) Gọi API xuất PDF base64 (tận dụng endpoint ValidPrint đang có)
    //    Nếu bạn có endpoint riêng chỉ "Export" (không đổi trạng thái), thay URL ở đây là tốt nhất.
    $('#showWaitting').modal('show');
    var dataExport = {
        patientId: patientId,
        resultCDHAId: resultCDHAId,
        returnResultTime: $('#sa_process_returnResultTime').val(),
        userReturnResult: $('#sa_process_userReturnResultSA').val(),
        description: CKEDITOR.instances['sa_process_description'].getData(),
        result: $('#sa_process_result').val(),
        suggest: $('#sa_process_suggest').val()
    };

    $.ajax({
        url: "/SA_Process/ValidPrint/",
        data: JSON.stringify(dataExport),
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        type: "POST",
        success: function (base64Pdf) {
            console.log("Sau khi validPrint: ", base64Pdf)
            //if (!base64Pdf) {
            //    $('#showWaitting').modal('hide');
            //    alert("Không thể xuất PDF để ký. Vui lòng kiểm tra lại!");
            //    return;
            //}

            // 2) Chuyển base64 -> File để gửi multipart/form-data
            var file = base64ToFile(base64Pdf, "temp.pdf", "application/pdf");
            console.log(file);
            // 3) Chuẩn bị FormData gửi đến API ký số chung (server sẽ tự đặt tên file)
            var formData = new FormData();
            formData.append("signerCCCD", signerCCCD);
            formData.append("file", file);

            // ---- context để server đặt tên theo template MySign:FileNameTemplates (FormType = SieuAm)
            formData.append("formType", "SieuAm");
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
                    // Gỉa sử object trả về từ response như sau
                    //var response = {
                    //    "id": "51627", "provider": "0", "documentId": "null", "documentType": "null", "documentName": "27433568_phieu_danh_gia_ban_dau.pdf", "documentDesc": "null", "documentHash": "7X38RRgfWpYULcvtZc863HnTfocDsSsji023UGoxBLM=", "timestamp": "2025-09-19T07:11:06.763046209Z", "transactionId": "308706a6-5dc7-4f0b-ba08-01448c15a306", "credentialId": "066194014120_7500452_20250821162101", "signatures": "SJpUV2IQVRR3FzMzK5WWLucKNX2jtFc4\/\/+rvi19Izj6yL1epn7xo1cxGhW\/67fI\r\niqLqz104zwke0xbWf9BZPjAEmSWD8Xn2RaouyGCBQIj4tnCnMZQGnPCBZ2laN4Ec\r\n3qjMU3lpoh22OnVBgLYZlQZSSvnNii1MF7v1vKRsdggl4YZpHEiZlgMef7iclkga\r\nMGUryw11teXAg77s15jGKYCjkM49jrJlGq+M5p0IVCj\/ZeUXByd3MsBeWAnWAHs1\r\n\/WoyEfBhwK3YABb37cjO6RNFACj+KJJv8910AyQwZeowyhmeQDdjLdGgfJRILORt\r\nDFZF1iABlsXIbp6pf0ek8w==", "status": "1", "filepath": "null", "url": "null", "docControlId": "null", "docControlType": "null", "error": "null", "login": "066194014120", "taxcode": "6001362081", "metadata": "null", "requesttime": "1758265850532", "contentContentType": "application\/pdf", "signUserId": "066194014120",
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

                        // 2) LƯU signStoreId THEO resultCDHAId ĐANG CHỌN
                        if (signStoreId) {
                            $.ajax({
                                url: "/SA_Process/SaveSignStoreIdForResultCDHA",
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
                                            referenceType: formType ?? "SieuAm",                // SA / SATIM / XQUANG...
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
                        } else {
                            SwalHelper.Alert.error("Ký số THẤT BẠI. Đã Valid kết quả. Chọn Invalid kết quả và tiến hành ký số lại.");
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
                    var msg = "Ký số THẤT BẠI. Đã Valid kết quả. Chọn Invalid kết quả và tiến hành ký số lại.";
                    //if (xhr && xhr.responseText) msg += "\n" + xhr.responseText;
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
    var patientId = $('#sa_process_id').val();
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
    var signerCCCD = $('#sa_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#sa_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#sa_process_patientId').val() || $('#sa_process_sid').val() || "";
    var patientMaBenhAn = $('#sa_process_maBenhAn').val() || $('#sa_process_sid').val() || "";
    var patientName = $('#sa_process_patientName').val() || "";
    var doctorName = $('#sa_process_userReturnResultSA option:selected').text() || $('#sa_process_userReturnResultSA').val() || "";
    var doctorId = $('#sa_process_userReturnResultSA').val();
    var performedAt = $('#sa_process_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "SieuAm"; // bạn có thể thay động tùy màn hình

    // 1) Gọi API xuất PDF base64 (tận dụng endpoint ValidPrint đang có)
    //    Nếu bạn có endpoint riêng chỉ "Export" (không đổi trạng thái), thay URL ở đây là tốt nhất.
    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tạo file PDF...'); } catch (e) { }

    // 2. PHASE 1: EXPORT tất cả PDF thô
    var pdfJobs = [];  // [{resultCDHAId, base64Pdf}]
    var chain = Promise.resolve();

    //var dataExport = {
    //    patientId: patientId,
    //    resultCDHAId: resultCDHAId,
    //    returnResultTime: $('#sa_process_returnResultTime').val(),
    //    userReturnResult: $('#sa_process_userReturnResultSA').val(),
    //    description: CKEDITOR.instances['sa_process_description'].getData(),
    //    result: $('#sa_process_result').val(),
    //    suggest: $('#sa_process_suggest').val()
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
                        url: "/SA_Process/ValidPrintMultiple/",
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
                    returnResultTime: $('#sa_process_returnResultTime').val(),
                    userReturnResult: $('#sa_process_userReturnResultSA').val(),
                    description: CKEDITOR.instances['sa_process_description'].getData(),
                    result: $('#sa_process_result').val(),
                    suggest: $('#sa_process_suggest').val()
                };
                console.log(resultIds[0], "====>", dataExport);
                $.ajax({
                    url: "/SA_Process/ValidPrint/",
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
                                    url: "/SA_Process/SaveSignStoreIdForResultCDHA",
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
        SwalHelper.Toast.warning('Chưa có signStoreId cho dịch vụ này!');
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
$(document).on('change', '#sa_chk_all', function () {
    var checked = this.checked === true;

    var $items = $('#sa_list_service').find('.sa-chk-service');
    $items.prop('checked', checked);

    // Đếm lại số đang được tick sau khi set
    var marked = $items.filter(':checked').length;
    console.log("marked: ", marked);
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
    } else if (marked == 1) {

        // Lấy checkbox duy nhất đang được tick
        var $only = $items.filter(':checked').first();

        // Lấy dòng chứa checkbox đó
        var $rowOnly = $only.closest('.row-service');

        // Xóa trạng thái multi-selected vì chỉ còn 1 dòng
        //$row.addClass('active');
        console.log($only);
        // Gọi lại hàm xử lý 1 dòng
        Process_CheckedBoxOnRow($only.val());

    } else {
        // xóa hết
        var $row = $(this).closest('.row-service');
        $row.removeClass('active');
        $('#sa_list_service .row-service').removeClass('multi-selected');
    }

    // Ẩn/hiện nút Lưu dựa trên trạng thái check all (giữ nguyên logic cũ)
    if (checked) {
        $('#sa_process_saveresult').hide();
    } else {
        $('#sa_process_saveresult').show();
    }
});

// Khai báo guard toàn cục (một lần)
if (window.__sa_row_selecting === undefined) {
    window.__sa_row_selecting = false;
}

// Tick/untick từng dịch vụ riêng lẻ
$(document).on('change', '.sa-chk-service', function () {
    var $all = $('#sa_list_service').find('.sa-chk-service');
    var total = $all.length;
    var marked = $all.filter(':checked').length;

    // Đồng bộ header "chọn tất cả"
    var allChecked = total > 0 && marked === total;
    $('#sa_chk_all').prop('checked', allChecked);

    // Quy tắc multi-selected:
    // - Nếu chỉ còn 0 hoặc 1 cái được tick → remove hết .multi-selected
    // - Nếu từ 2 cái trở lên → tất cả các dòng được tick đều có .multi-selected
    if (marked >= 2) {
        $('#sa_process_saveresult').hide(); // ẩn nút Lưu
        $('#imageCDHA').empty();
        CKEDITOR.instances.sa_process_description.setReadOnly(true); // disabled nhập mô tả kết quả
        CKEDITOR.instances.sa_process_description.setData('');
        $('#sa_process_result').attr('disabled', true); // disabled nhập kết luận
        $('#sa_process_result').val(''); // disabled nhập kết luận
        
        $all.each(function () {
            var $row = $(this).closest('.row-service');
            $row.removeClass('active');
            if (this.checked) $row.addClass('multi-selected');
            else $row.removeClass('multi-selected');
        });
    } else {
        //var $row = $(this).closest('.row-service');
        //$row.removeClass('active');
        $('#sa_process_saveresult').show();
        CKEDITOR.instances.sa_process_description.setReadOnly(false);
        $('#sa_process_result').attr('disabled', false);
        $('#sa_list_service .row-service').removeClass('multi-selected');
    }

    // === Điểm thêm mới theo yêu cầu ===
    // Nếu sau thao tác chỉ còn 1 checkbox được chọn:
    // -> Lấy resultCDHAId còn lại và gọi Process_CheckedBoxOnRow(resultCDHAId)
    if (marked === 1 && !window.__sa_row_selecting) {
        var $only = $all.filter(':checked').first();
        var resultId = $only.val();

        // Tránh loop sự kiện change khi Process_CheckedBoxOnRow thay đổi checkbox
        window.__sa_row_selecting = true;
        setTimeout(function () {
            try {
                Process_CheckedBoxOnRow(resultId);
            } finally {
                window.__sa_row_selecting = false;
            }
        }, 0);
    }
});

// --- Auto-search khi quét barcode vào ô #sa_getsample_pidorseq ---
(function () {
    var $box = $('#sa_getsample_pidorseq');
    if (!$box.length) return;

    // Tùy scanner: đa số sẽ gửi phím Enter ở cuối. Ta bắt cả Enter và fallback theo thời gian.
    var timer = null;
    var SCAN_IDLE_MS = 1000;   // sau 200ms không có phím mới => coi như quét xong (có thể tăng 250–300 nếu cần)
    var MIN_LEN = 8;          // độ dài tối thiểu để coi là barcode/hợp lệ

    function triggerSearch() {
        var v = $box.val() ? String($box.val()).trim() : '';
        if (!v || v.length < MIN_LEN) return;
        // Gọi hàm tìm kiếm
        try { GetSample_Search(); } catch (e) { console.warn('GetSample_Search() not found', e); }
        // Chọn lại toàn bộ text để quét mã tiếp theo nhanh hơn
        setTimeout(function () { $box.select(); }, 0);
    }

    // Enter => tìm ngay
    $(document).on('keydown', '#sa_getsample_pidorseq', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            clearTimeout(timer);
            triggerSearch();
            return;
        }
        // Fallback: đợi người dùng/scanner ngừng gõ 200ms => tìm
        clearTimeout(timer);
        timer = setTimeout(triggerSearch, SCAN_IDLE_MS);
    });

    // Dán (paste) => đợi 1 tick rồi tìm
    $(document).on('paste', '#sa_getsample_pidorseq', function () {
        clearTimeout(timer);
        setTimeout(triggerSearch, 50);
    });

    // Optional: khi field được focus, tự select để dễ quét liên tiếp
    $(document).on('focus', '#sa_getsample_pidorseq', function () {
        var el = this;
        setTimeout(function () { el.select(); }, 0);
    });
})();

function Process_SaveDateToSession() {
    var fromDate = document.getElementById('sa_process_timeSearchFrom').value;
    var toDate = document.getElementById('sa_process_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/SA_Process/SaveSearchDates',
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
        url: "/SA_ReturnResult/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#sa_returnresult_timeSearchFrom").val(today);
            //$("#sa_returnresult_timeSearchTo").val(today);
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
    var sa_returnresult_pidorseq = $("#sa_returnresult_pidorseq").val();
    var timeSearchFrom = $("#sa_returnresult_timeSearchFrom").val();
    var timeSearchTo = $("#sa_returnresult_timeSearchTo").val();
    $.ajax({
        url: "/SA_ReturnResult/Search?" + "pidorseq=" + sa_returnresult_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#sa_returnresult_pidorseq").val('');
    $('#sa_returnresult_id').val('');
    $('#sa_returnresult_patientId').val('');
    $('#sa_returnresult_seq').val('');
    $('#sa_returnresult_sid').val('');
    $('#sa_returnresult_patientName').val('');
    $('#sa_returnresult_age').val('');
    $('#sa_returnresult_sex').val('');
    $('#sa_returnresult_obj').val('');
    $('#sa_returnresult_type').val('');
    $('#sa_returnresult_location').val('');
    $('#sa_returnresult_doctor').val('');
    $('#sa_returnresult_getSampleTime').val('');
    $('#sa_returnresult_returnResultTime').val('');
    $('#sa_returnresult_location').val('');
    $('#sa_returnresult_doctor').val('');
    $('#sa_returnresult_userReturnResultSA').val('');
    $('#sa_returnresult_address').val('');
    $('#sa_returnresult_diagnostic').val('');
    $('#tbody-gridview-service').empty();
}

function ReturnResult_GetPatientInfo(id) {
    $.ajax({
        url: "/SA_ReturnResult/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            $("#returnresult_patientInfo").html(result);
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
    var userId = $("#sa_returnresult_userLoginId").val();
    console.log(userId);
    if (!userId) {
        $("#sa_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/SA_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            console.log(u);
            if (!u) {
                $("#sa_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#sa_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#sa_returnresult_signerCCCD").text("Không lấy đượcthông tin CCCD.");
        },
    });
}

function ReturnResult_Load_BSThucHienTheoDichVu() {
    var userId = $("#sa_returnresult_bsThucHien").val();
    console.log(userId);
    if (!userId) {
        $("#sa_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/SA_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            console.log(u);
            if (!u) {
                $("#sa_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            var name = u.name || "";
            $("#sa_returnresult_userReturnResultSA").val(name);
            $("#sa_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#sa_returnresult_signerCCCD").text("Không lấy đượcthông tin CCCD.");
        },
    });
}
function ReturnResult_GetListServiceForPatient(id) {
    $.ajax({
        url: "/SA_ReturnResult/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_sa-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function ReturnResult_Invalid() {
    var patientId = $('#sa_returnresult_id').val();

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // ResultCDHA.Id của các dịch vụ đang được chọn.
    // Backend sẽ tự lấy KeyResultForHis từ DB theo ResultCDHA.Id.
    var resultIds = [];

    $('.sa-returnresult-chk-service:checked').each(function () {
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
        url: "/SA_ReturnResult/Invalid",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify({
            patientId: parseInt(patientId, 10),
            resultIds: resultIds,
            categoryCode: "SA"
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
    var id = $('#sa_returnresult_id').val();
    if (id === "") {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
    }
    // Lấy ServiceId
    var resultCDHAId = "";
    var selectedResultIds = [];
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            if ($(this).is(':checked')) {
                resultCDHAId = $(this).val();
                var selectedId = parseInt($(this).val(), 10);
                if (selectedId > 0) selectedResultIds.push(selectedId);
            }
        });
    });

    if (resultCDHAId === "") {
        SwalHelper.Toast.warning('Vui lòng chọn dịch vụ!');
        return;
    }
    if (confirm('Bạn muốn InValid kết quả của bệnh nhân (hủy Ký Số kết quả này) ?')) {
        $.ajax({
            url: "/SA_ReturnResult/Invalid",
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'text',
            data: JSON.stringify({
                patientId: parseInt(id),
                resultIds: selectedResultIds
            }),
            success: function (result) {
                if (result === 'True') {
                    SwalHelper.Toast.success("Invalid thành công và đã xóa hủy ký số!");
                    UpdateSignStatus(resultCDHAId);
                    ReturnResult_Refresh();
                    //ReturnResult_Get_Count();
                }
                else {
                    SwalHelper.Alert.error("Không thể Invalid. Vui lòng kiểm tra lại!");
                }
            },
            error: function (xhr) {
                SwalHelper.Alert.error(xhr.responseText || "Không thể Invalid. Vui lòng kiểm tra lại!");
            }
        });
    }
}

function ReturnResult_Get_Count() {
    $.ajax({
        url: "/SA_ReturnResult/Get_Count/",
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
    var patientId = $('#sa_returnresult_id').val();
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
        url: "/SA_ReturnResult/Print?resultCDHAId=" + resultCDHAId,
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

                // ===== PHẦN MỚI: Xử lý cho cả Desktop và Mobile =====

                // Kiểm tra thiết bị
                var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);

                if (isMobile) {
                    // CÁCH 1: Tạo link download cho mobile
                    var link = document.createElement('a');
                    link.href = URL.createObjectURL(blob);
                    link.download = 'KetQua_SieuAm_' + resultCDHAId + '_' + new Date().getTime() + '.pdf';
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
                        link.download = 'KetQua_SieuAm_' + resultCDHAId + '.pdf';
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

//                // Check digital signature status and toggle buttons
//                var $row = $(this).closest('.row-service');
//                var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
//                ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
//            }
//            else {
//                $(this).prop("checked", false);
//            }
//        })

//    })
//}

function ReturnResult_CheckedBoxOnRow(id, event) {
    // Nếu click trực tiếp vào checkbox thì không xử lý (để người dùng tự chọn multiple)
    if (event && event.target.type === 'checkbox') {
        // Chỉ cập nhật trạng thái "chọn tất cả"
        var $rows = $('.sa-returnresult-chk-service');
        var total = $rows.length;
        var marked = $rows.filter(':checked').length;
        var allChecked = total > 0 && marked === total;
        $('#sa_returnresult_chk_all').prop('checked', allChecked);

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
            CKEDITOR.instances["sa_returnresult_description"].setData("");
            $("#sa_returnresult_result").val("");
            $("#sa_returnresult_suggest").val("");
            ReturnResult_ToggleDigitalSignButtons(false);
        } else {
            // Clear khi không có gì được chọn
            const $containHinh = $('.image-returnresult');
            $containHinh.empty();
            CKEDITOR.instances["sa_returnresult_description"].setData("");
            $("#sa_returnresult_result").val("");
            $("#sa_returnresult_suggest").val("");
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
            $('#sa_returnresult_bsThucHien').val(bs_thuc_hien).trigger("change");
            ReturnResult_Load_BSThucHienTheoDichVu();

            ReturnResult_LoadImageForService(id);

            // Check digital signature status and toggle buttons
            var $row = $chk.closest('.row-service');
            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
        }
    });

    // Update checkAll status
    var $rows = $('.sa-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;
    var allChecked = total > 0 && marked === total;
    $('#sa_returnresult_chk_all').prop('checked', allChecked);
}

// Event handler cho checkbox để chỉ xử lý việc chọn multiple
$(document).on('change', '.sa-returnresult-chk-service', function () {
    var $rows = $('.sa-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Update trạng thái "chọn tất cả"
    var allChecked = total > 0 && marked === total;
    $('#sa_returnresult_chk_all').prop('checked', allChecked);

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
        CKEDITOR.instances["sa_returnresult_description"].setData("");
        $("#sa_returnresult_result").val("");
        $("#sa_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    } else {
        // Không có gì được chọn → clear
        const $containHinh = $('.image-returnresult');
        $containHinh.empty();
        CKEDITOR.instances["sa_returnresult_description"].setData("");
        $("#sa_returnresult_result").val("");
        $("#sa_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    }
});

// Helper function to toggle digital signature buttons
function ReturnResult_ToggleDigitalSignButtons(isDigitallySigned) {
    console.log('Digital signature status:', isDigitallySigned);

    // Toggle "Tải PDF đã ký" button
    var $printSignedBtn = $('#sa_returnresult_print_signed');
    if ($printSignedBtn.length) {
        if (isDigitallySigned) {
            $printSignedBtn.show().prop('disabled', false);
        } else {
            $printSignedBtn.hide().prop('disabled', true);
        }
    }

    // Toggle "Invalid & Xóa Ký Số" button  
    var $invalidRemoveSignBtn = $('#sa_returnresult_invalid_removedigitalsign');
    if ($invalidRemoveSignBtn.length) {
        if (isDigitallySigned) {
            $invalidRemoveSignBtn.show().prop('disabled', false);
        } else {
            $invalidRemoveSignBtn.hide().prop('disabled', true);
        }
    }
}

function ReturnResult_LoadImageForService(id) {
    $("#image-returnresult").empty();
    CKEDITOR.instances["sa_returnresult_description"].setData("");
    $("#sa_returnresult_result").val("");
    $("#sa_returnresult_suggest").val("");
    $.ajax({
        url: "/SA_ReturnResult/GetImageForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                if (value.id) {
                    $("#image-returnresult").append('<div class="image-item"><img class="image" id="' + value.id + '" src="' + value.name + '" onclick="ReturnResult_GetImageCDHAId(' + value.id + ', this)" /></div > ');
                }
                CKEDITOR.instances["sa_returnresult_description"].setData(value.description);
                $("#sa_returnresult_result").val(value.result);
                $("#sa_returnresult_suggest").val(value.suggest);
            });
        }
    });
}

function ReturnResult_GetImageCDHAId(imageCDHAId, img) {
    $('.image-returnresult img').removeClass('active');
    $(img).toggleClass('active');
    $("#sa_returnresult_xem").val(imageCDHAId);
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

function ReturnResult_ViewSignedPdf() {
    var signStoreId = $('#sa_returnresult_signStoreId').val();

    function openSigned(id) {
        if (!id) { SwalHelper.Toast.warning('Chưa có signStoreId cho dịch vụ này!'); return; }
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
        SwalHelper.Toast.warning('Vui lòng chọn dịch vụ để xem PDF đã ký!');
        return;
    }

    // Gọi API của bạn để lấy signStoreId đã lưu trong DB theo resultId
    $.ajax({
        url: "/SA_ReturnResult/GetSignStoreId",
        type: "GET",
        data: { resultId: resultId },
        dataType: "json",
        success: function (res) {
            console.log(res);
            var id = (res && (res.signStoreId || res.id)) ? (res.signStoreId || res.id) : "";
            if (id) {
                openSigned(id);
            } else {
                SwalHelper.Toast.warning('Chưa lưu signStoreId cho dịch vụ này.');
            }
        },
        error: function () {
            SwalHelper.Alert.error('Lỗi', 'Không lấy được signStoreId. Vui lòng kiểm tra!');
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

function ReturnResult_SaveDateToSession() {
    var fromDate = document.getElementById('sa_returnresult_timeSearchFrom').value;
    var toDate = document.getElementById('sa_returnresult_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/SA_ReturnResult/SaveSearchDates',
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
    var patientId = $('#sa_returnresult_id').val();
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
        SwalHelper.Toast.warning('Vui lòng chọn ít nhất 1 dịch vụ để ký.');
        return;
    }

    // Lấy CCCD người ký (bác sĩ) - cần có input field tương ứng trong tab ReturnResult
    var signerCCCD = $('#sa_returnresult_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning('Vui lòng nhập CCCD người ký (Viettel MySign)!');
        $('#sa_returnresult_signerCCCD').focus();
        return;
    }

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#sa_returnresult_patientId').val() || $('#sa_returnresult_sid').val() || "";
    var patientMaBenhAn = $('#sa_returnresult_maBenhAn').val() || $('#sa_returnresult_sid').val() || "";
    var patientName = $('#sa_returnresult_patientName').val() || "";
    var doctorName = $('#sa_returnresult_userReturnResultSA option:selected').text() || $('#sa_returnresult_userReturnResultSA').val() || "";
    var doctorId = $('#sa_returnresult_userLoginId').val();
    var performedAt = $('#sa_returnresult_returnResultTime').val() || ""; // thời điểm trả kết quả
    var serviceCode = ""; // nếu cần, bạn lấy từ label dịch vụ đang chọn
    var formType = "SieuAm"; // bạn có thể thay động tùy màn hình

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
                    url: "/SA_ReturnResult/Print?resultCDHAId=" + rid,
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
$(document).on('change', '#sa_returnresult_chk_all', function () {
    var checked = this.checked === true;
    // chỉ tick các dịch vụ đang hiển thị
    $('.sa-returnresult-chk-service').prop('checked', checked);

    // Nếu check all thì load kết quả của dịch vụ đầu tiên
    const $containHinh = $('.image-returnresult');
    $containHinh.empty();
    CKEDITOR.instances["sa_returnresult_description"].setData("");
    $("#sa_returnresult_result").val("");
    $("#sa_returnresult_suggest").val("");
    ReturnResult_ToggleDigitalSignButtons(false);
});

// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả" cho ReturnResult
$(document).on('change', '.sa-returnresult-chk-service', function () {
    var $rows = $('.sa-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Nếu tất cả đều check => check header; ngược lại bỏ check header
    var allChecked = total > 0 && marked === total;
    $('#sa_returnresult_chk_all').prop('checked', allChecked);
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
