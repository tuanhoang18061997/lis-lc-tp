// *********************************************************************************** Select 2
$(function () {
    GetSample_SetSelect2_01(true);
})
$(function () {
    if (typeof window.BarcodeScan === 'undefined') return;

    window.BarcodeScan.register('#nsctc_getsample_pidorseq', window.GetSample_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    window.BarcodeScan.register('#nsctc_process_pidorseq', window.Process_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    window.BarcodeScan.register('#nsctc_returnresult_pidorseq', window.ReturnResult_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    window.BarcodeScan.autowire('.barcode-input');
});

// *********************************************************************************** Gắn hotkey Enter cho tìm kiếm
$(document).ready(function () {
    $('#nsctc_process_pidorseq, #nsctc_process_timeSearchFrom, #nsctc_process_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            Process_Search();
        }
    });

    $('#nsctc_getsample_pidorseq, #nsctc_getsample_timeSearchFrom, #nsctc_getsample_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            GetSample_Search();
        }
    });

    $('#nsctc_returnresult_pidorseq, #nsctc_returnresult_timeSearchFrom, #nsctc_returnresult_timeSearchTo').on('keydown', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            ReturnResult_Search();
        }
    });
});

// Xử lý toggle danh sách bệnh nhân trên mobile
$(document).ready(function () {
    function initMobilePatientList() {
        if (window.innerWidth <= 768) {
            if ($('.mobile-toggle-patient-list').length === 0) {
                var toggleBtn = $('<button class="mobile-toggle-patient-list"><i class="bi bi-list"></i></button>');
                $('body').append(toggleBtn);
            }
            if ($('.mobile-patient-list-overlay').length === 0) {
                var overlay = $('<div class="mobile-patient-list-overlay"></div>');
                $('body').append(overlay);
            }
            if ($('.nsctc-left-header').length === 0) {
                var header = $('<div class="nsctc-left-header">' +
                    '<div class="nsctc-left-header-title">Danh sách bệnh nhân</div>' +
                    '<button class="nsctc-left-header-close"><i class="bi bi-x-lg"></i></button>' +
                    '</div>');
                $('.nsctc-left').prepend(header);
            }
        } else {
            $('.mobile-toggle-patient-list').remove();
            $('.mobile-patient-list-overlay').remove();
            $('.nsctc-left-header').remove();
            $('.nsctc-left').removeClass('show');
        }
    }

    initMobilePatientList();

    $(document).on('click', '.mobile-toggle-patient-list', function () {
        $('.nsctc-left').addClass('show');
        $('.mobile-patient-list-overlay').addClass('show');
        $(this).addClass('active');
        $('body').css('overflow', 'hidden');
    });

    $(document).on('click', '.nsctc-left-header-close', function () {
        closePatientList();
    });

    $(document).on('click', '.mobile-patient-list-overlay', function () {
        closePatientList();
    });

    function closePatientList() {
        $('.nsctc-left').removeClass('show');
        $('.mobile-patient-list-overlay').removeClass('show');
        $('.mobile-toggle-patient-list').removeClass('active');
        $('body').css('overflow', '');
    }

    $(document).on('click', '.list-group-item-action', function () {
        if (window.innerWidth <= 768) {
            setTimeout(function () {
                closePatientList();
            }, 300);
        }
    });

    var resizeTimer;
    $(window).on('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            initMobilePatientList();
        }, 250);
    });

    var startX = 0;
    var currentX = 0;
    var isDragging = false;

    $(document).on('touchstart', '.nsctc-left', function (e) {
        if (window.innerWidth <= 768) {
            startX = e.touches[0].clientX;
            isDragging = true;
        }
    });

    $(document).on('touchmove', '.nsctc-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            currentX = e.touches[0].clientX;
            var diff = currentX - startX;
            if (diff < 0) {
                $(this).css('transform', 'translateX(' + diff + 'px)');
            }
        }
    });

    $(document).on('touchend', '.nsctc-left', function (e) {
        if (isDragging && window.innerWidth <= 768) {
            var diff = currentX - startX;
            if (diff < -100) {
                closePatientList();
            }
            $(this).css('transform', '');
            isDragging = false;
        }
    });
});

function GetSample_SetSelect2_02() {
    $(document).ready(function () {
        $('#nsctc_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#nsctc_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#nsctc_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#nsctc_getsample_location').select2({
            placeholder: "-- Chọn --"
        });
    });

    if (isLoadPage) {
        $('#nsctc_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#nsctc_getsample_doctor').select2({
            placeholder: "-- Chọn --"
        });
    });

    if (isLoadPage) {
        $('#nsctc_process_sampleresult').val("");
    }
    $(document).ready(function () {
        $('#nsctc_process_sampleresult').select2({
            placeholder: "-- Chọn --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#nsctc_process_userReturnResultNSCTC').select2({
            placeholder: "-- Chọn --"
        });
    });
}


// *********************************************************************************** Get sample

function GetSample_ValidateInput(id) {
    var flag = true;
    $("#" + id).find(".valid").each(function (i, e) {
        var value = $(e).val();
        if (value === "" || !value) {
            if (flag) {
                if (e.id === "nsctc_getsample_location") {
                    $('#nsctc_getsample_location').select2('focus');
                }
                else if (e.id === "nsctc_getsample_doctor") {
                    $('#nsctc_getsample_doctor').select2('focus');
                }
                else if (e.id === "nsctc_getsample_userReturnResultNSCTC") {
                    $('#nsctc_getsample_userReturnResultNSCTC').select2('focus');
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
        url: "/NSCTC_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
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
        url: "/NSCTC_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#nsctc_getsample_timeSearchFrom").val(today);
            $("#nsctc_getsample_timeSearchTo").val(today);
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Search() {
    var nsctc_getsample_pidorseq = $("#nsctc_getsample_pidorseq").val();
    var timeSearchFrom = $("#nsctc_getsample_timeSearchFrom").val();
    var timeSearchTo = $("#nsctc_getsample_timeSearchTo").val();
    $.ajax({
        url: "/NSCTC_GetSample/Search?" + "pidorseq=" + nsctc_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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

// Hidden Show button
$("#nsctc_getsample_savepatient").hide();
$("#nsctc_getsample_cancelpatient").hide();
function GetSample_HideButton(_new, _save, _delete, _cancel, _addservice, _getsample) {
    if (_new == 1) {
        $("#nsctc_getsample_newpatient").hide();
    } else {
        $("#nsctc_getsample_newpatient").show();
    }

    if (_save == 1) {
        $("#nsctc_getsample_savepatient").hide();
    } else {
        $("#nsctc_getsample_savepatient").show();
    }

    if (_delete == 1) {
        $("#nsctc_getsample_deletepatient").hide();
    } else {
        $("#nsctc_getsample_deletepatient").show();
    }

    if (_cancel == 1) {
        $("#nsctc_getsample_cancelpatient").hide();
    } else {
        $("#nsctc_getsample_cancelpatient").show();
    }

    if (_addservice == 1) {
        $("#nsctc_getsample_addService").hide();
    } else {
        $("#nsctc_getsample_addService").show();
    }

    if (_getsample == 1) {
        $("#nsctc_getsample_processresult").hide();
    } else {
        $("#nsctc_getsample_processresult").show();
    }
}

function GetSample_GetPatientInfo(id) {
    GetSample_HideButton(false, true, false, true, false, false);
    $.ajax({
        url: "/NSCTC_GetSample/GetPatientInfo?id=" + id,
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
        url: "/NSCTC_GetSample/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#nsctc-right-service-gridview").html(result);
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
    $("#nsctc_getsample_pidorseq").val('');
    $('#nsctc_getsample_id').val('');
    $('#nsctc_getsample_patientId').val('');
    $('#nsctc_getsample_seq').val('');
    $('#nsctc_getsample_sid').val('');
    $('#nsctc_getsample_patientName').val('');
    $('#nsctc_getsample_age').val('');
    $('#nsctc_getsample_sex').val('');
    $('#nsctc_getsample_obj').val('');
    $('#nsctc_getsample_type').val('');
    $('#nsctc_getsample_location').val('');
    $('#nsctc_getsample_doctor').val('');
    $('#nsctc_getsample_getSampleTime').val(dateTime);
    $('#nsctc_getsample_address').val('');
    $('#nsctc_getsample_diagnostic').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
    GetSample_SetSelect2_01(false);
}

function GetSample_NewPatient() {
    $('#nsctc_getsample_patientId').focus();
    GetSample_HideButton(true, false, true, false, true, true);
    GetSample_ResetInput();
}

function GetSample_DeletePatient() {
    var id = $('#nsctc_getsample_id').val();
    if (id == '') {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var choice = confirm("Bạn muốn xoá bệnh nhân và tất cả chỉ định xét nghiệm?");
        if (choice) {
            $.ajax({
                url: "/NSCTC_GetSample/DeletePatientAndService?id=" + id,
                type: "POST",
                dataType: "text",
                cache: false,
                success: function (result) {
                    if (result === '') {
                        SwalHelper.Toast.error("Xoá không thành công. Vui lòng kiểm tra lại!");
                    }
                    else {
                        GetSample_HideButton(false, true, false, true, false, false);
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
    var id = $('#nsctc_getsample_id').val();
    if (id == '') {
        GetSample_ResetInput();
    }
    else {
        GetSample_GetPatientInfo(id);
    }
}

function GetSample_GetSID(seq) {
    $.ajax({
        url: "/NSCTC_GetSample/GetSID?seq=" + seq,
        type: "GET",
        dataType: "text",
        cache: false,
        success: function (result) {
            $('#nsctc_getsample_sid').val(result);
        }
    });
}

function GetSample_SavePatient() {
    var validate = GetSample_ValidateInput('patientInfo');
    if (validate) {
        var id = $('#nsctc_getsample_id').val();
        var patientId = $('#nsctc_getsample_patientId').val();
        var seq = $('#nsctc_getsample_seq').val();
        var sid = $('#nsctc_getsample_sid').val();
        var patientName = $('#nsctc_getsample_patientName').val();
        var age = $('#nsctc_getsample_age').val();
        var sex = $('#nsctc_getsample_sex').val();
        var obj = $('#nsctc_getsample_obj').val();
        var type = $('#nsctc_getsample_type').val();
        var location = $('#nsctc_getsample_location').val();
        var doctor = $('#nsctc_getsample_doctor').val();
        var getSampleTime = $('#nsctc_getsample_getSampleTime').val();
        var address = $('#nsctc_getsample_address').val();
        var diagnostic = $('#nsctc_getsample_diagnostic').val();
        var category = $('#select-category').val();
        var service = $('#select-service').val();

        $.ajax({
            url: "/NSCTC_GetSample/SavePatient?id= " + id + "&&patientId=" + patientId + "&&seq=" + seq + "&&sid=" + sid + "&&patientName=" + patientName + "&&age=" + age + "&&sex=" + sex + "&&obj=" + obj + "&&type=" + type + "&&location=" + location + "&&doctor=" + doctor + "&&getSampleTime=" + getSampleTime + "&&address=" + address + "&&diagnostic=" + diagnostic + "&&service=" + service,
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
    var id = $('#nsctc_getsample_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var getSampleTime = $('#nsctc_getsample_getSampleTime').val();
        $.ajax({
            url: "/NSCTC_GetSample/ProcessResult?id= " + id + "&getSampleTime=" + getSampleTime,
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
    var id = $('#nsctc_getsample_id').val();
    if (id === "") {
        $('#addServiceForm').modal('hide');
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        GetSample_SetSelect2_02();
    }
}

function GetSample_DeleteServiceForPatient(idResultCDHA) {
    var patientId = $('#nsctc_getsample_id').val();
    $.ajax({
        url: "/NSCTC_GetSample/DeleteServiceForPatient?id=" + idResultCDHA,
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
    var serviceId = $('#nsctc_getsample_service').val();
    var patientId = $('#nsctc_getsample_id').val();
    var doctorId = $('#nsctc_getsample_doctor').val();
    if (serviceId === "" || patientId === "") {
        SwalHelper.Toast.warning("Chỉ định dịch vụ không thành công. Vui lòng kiểm tra lại !")
    }
    else {
        $.ajax({
            url: "/NSCTC_GetSample/AddServiceForPatient?patientId=" + patientId + "&&serviceId=" + serviceId + "&&doctorId=" + doctorId,
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
        url: "/NSCTC_GetSample/Get_Count/",
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
    var fromDate = document.getElementById('nsctc_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('nsctc_getsample_timeSearchTo').value;

    if (fromDate && toDate) {
        $.ajax({
            url: '/NSCTC_GetSample/SaveSearchDates',
            type: 'POST',
            data: {
                timeSearchFrom: fromDate,
                timeSearchTo: toDate
            },
            success: function (result) { }
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
                if (e.id === "nsctc_process_userReturnResultNSCTC") {
                    $('#nsctc_process_userReturnResultNSCTC').select2('focus');
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
        url: "/NSCTC_Process/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
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
    var nsctc_process_pidorseq = $("#nsctc_process_pidorseq").val();
    var timeSearchFrom = $("#nsctc_process_timeSearchFrom").val();
    var timeSearchTo = $("#nsctc_process_timeSearchTo").val();
    $.ajax({
        url: "/NSCTC_Process/Search?" + "pidorseq=" + nsctc_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#nsctc_process_pidorseq").val('');
    $('#nsctc_process_id').val('');
    $('#nsctc_process_patientId').val('');
    $('#nsctc_process_seq').val('');
    $('#nsctc_process_sid').val('');
    $('#nsctc_process_patientName').val('');
    $('#nsctc_process_age').val('');
    $('#nsctc_process_sex').val('');
    $('#nsctc_process_obj').val('');
    $('#nsctc_process_type').val('');
    $('#nsctc_process_location').val('');
    $('#nsctc_process_doctor').val('');
    $('#nsctc_process_getSampleTime').val('');
    $('#nsctc_process_returnResultTime').val('');
    $('#nsctc_process_userReturnResultNSCTC').val('');
    $('#nsctc_process_address').val('');
    $('#nsctc_process_diagnostic').val('');
    $('#tbody-gridview-service').empty();
    $('#imageCDHA').empty();
    $('#nsctc_process_result').empty();
    $('#nsctc_process_suggest').empty();
    CKEDITOR.instances["nsctc_process_description"].setData("");
    Process_SetSelect2_03(true);
}

function Process_GetPatientInfo(id) {
    $.ajax({
        url: "/NSCTC_Process/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_patientInfo").html(result);
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            Process_SetSelect2_03();
            var loginId = $("#nsctc_process_userLoginId").val();
            var $sel = $("#nsctc_process_userReturnResultNSCTC");
            if (loginId && (!$sel.val() || $sel.val() === "")) {
                $sel.val(loginId).trigger("change");
            }

            $(document)
                .off("change", "#nsctc_process_userReturnResultNSCTC")
                .on("change", "#nsctc_process_userReturnResultNSCTC", Process_Load_SelectedDoctorInfo);
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
        url: "/NSCTC_Process/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_nsctc-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function Process_SaveResult() {
    var patientId = $('#nsctc_process_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var validate = Process_ValidateInput('process_patientInfo');
        if (validate) {
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
                var returnResultTime = $('#nsctc_process_returnResultTime').val();
                var userReturnResult = $('#nsctc_process_userReturnResultNSCTC').val();
                var description = CKEDITOR.instances['nsctc_process_description'].getData();
                var result = $('#nsctc_process_result').val();
                var suggest = $('#nsctc_process_suggest').val();

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
                    url: "/NSCTC_Process/SaveResult/",
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
            return false;
        }
    }
}

// --- GỌI IN 1 DỊCH VỤ (hành vi cũ), tách riêng thành hàm dùng lại ---
function _validPrintOne(resultCDHAId, opts, isMulti = false) {
    return new Promise(function (resolve) {
        var payload = {
            patientId: opts.patientId,
            resultCDHAId: resultCDHAId,
            returnResultTime: opts.returnResultTime,
            userReturnResult: opts.userReturnResult,
            description: opts.description || null,
            result: opts.result || null,
            suggest: opts.suggest || null,
            selectedImageIds: Array.isArray(opts.selectedImageIds) ? opts.selectedImageIds : null
        };
        if (isMulti) {
            $.ajax({
                url: "/NSCTC_Process/ValidPrintMultiple/",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                data: JSON.stringify(payload),
                success: function (resp) {
                    if (resp) {
                        var bytes = atob(resp);
                        var arr = new Uint8Array(bytes.length);
                        for (var i = 0; i < bytes.length; i++) arr[i] = bytes.charCodeAt(i);
                        var blob = new Blob([arr], { type: 'application/pdf;base64' });
                        window.open(URL.createObjectURL(blob));
                    }
                    resolve();
                },
                error: function () { resolve(); }
            });
        } else {
            $.ajax({
                url: "/NSCTC_Process/ValidPrint/",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                data: JSON.stringify(payload),
                success: function (resp) {
                    if (resp) {
                        var bytes = atob(resp);
                        var arr = new Uint8Array(bytes.length);
                        for (var i = 0; i < bytes.length; i++) arr[i] = bytes.charCodeAt(i);
                        var blob = new Blob([arr], { type: 'application/pdf;base64' });
                        window.open(URL.createObjectURL(blob));
                    }
                    resolve();
                },
                error: function () { resolve(); }
            });
        }
    });
}

function Process_ValidPrint(mode) {
    mode = mode || 'current';
    var patientId = $('#nsctc_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }
    var ok = Process_ValidateInput('process_patientInfo');
    if (!ok) return;
    var returnResultTime = $('#nsctc_process_returnResultTime').val();
    var userReturnResult = $('#nsctc_process_userReturnResultNSCTC').val();

    var resultIds = [];
    if (mode === 'current') {
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

    var isMulti = resultIds.length > 1;
    var description = isMulti ? null : (CKEDITOR.instances['nsctc_process_description'].getData() || null);
    var resultText = isMulti ? null : ($('#nsctc_process_result').val() || null);
    var suggest = isMulti ? null : ($('#nsctc_process_suggest').val() || null);
    var selectedImageIds = isMulti ? null : $(".nsctc-print-check:checked").map(function () { return $(this).val(); }).get();

    var opts = {
        patientId: patientId,
        returnResultTime: returnResultTime,
        userReturnResult: userReturnResult,
        description: description,
        result: resultText,
        suggest: suggest,
        selectedImageIds: selectedImageIds
    };

    $('#showWaitting').modal('show');

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
    var id = $('#nsctc_process_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        $.ajax({
            url: "/NSCTC_Process/GetSample?id= " + id,
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
        url: "/NSCTC_Process/Get_Count/",
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
    $('#nsctc_process_saveresult').show();
    $('#nsctc_process_result').attr('disabled', false);
    $('#nsctc_list_service .row-service').removeClass('multi-selected');
    $(".row-service .form-check-input").each(function () {
        var $chk = $(this);
        var resultCdhaId = String($chk.val());
        var isTarget = (resultCdhaId === String(id));
        $chk.prop("checked", isTarget);

        if (isTarget) {
            $('.row-service').removeClass('active');
            $chk.closest('.row-service').addClass('active');
            var serviceId = $chk.data("serviceid");
            Process_Load_ResultAndImage_ForService(id, serviceId);
        }
    });
}

function _normalizeSexFromUI() {
    var raw = ($("#nsctc_process_sex").val() || "").toString().trim().toUpperCase();
    if (raw === "NAM" || raw === "M" || raw === "1") return "M";
    return "F";
}

function _autoPickSampleForService(serviceId) {
    var $sel = $("#nsctc_process_sampleresult");
    if (!$sel.length) return;

    var gender = _normalizeSexFromUI();
    var accept = new Set([gender, "MF"]);

    var $options = $sel.find("option").filter(function () {
        var optService = $(this).data("serviceid");
        var optGender = ($(this).data("gender") || "").toString().toUpperCase();
        return String(optService) === String(serviceId) && accept.has(optGender);
    });

    if (!$options.length) {
        $("#nsctc_process_sampleresult").val("");
        $('#nsctc_process_sampleresult').select2({ placeholder: "-- Chọn --" });
        return;
    }

    var $best = $options.filter(function () {
        return ($(this).data("gender") || "").toString().toUpperCase() === gender;
    });
    if (!$best.length) $best = $options.filter(function () {
        return ($(this).data("gender") || "").toString().toUpperCase() === "MF";
    });

    var val = ($best.length ? $best.first().val() : $options.first().val()) || "";
    $sel.val(val).trigger("change");
}

function _setSampleSelectValue(sampleId, sampleName) {
    var $sel = $("#nsctc_process_sampleresult");
    if (!$sel.length) return;

    var has = $sel.find('option[value="' + sampleId + '"]').length > 0;
    if (!has) {
        $sel.append($('<option>', {
            value: sampleId,
            text: sampleName ? sampleName : ('Sample #' + sampleId)
        }));
    }
    $sel.val(String(sampleId)).trigger("change");
}

function Process_Load_ResultAndImage_ForService(id, serviceId) {
    $("#imageCDHA").empty();
    CKEDITOR.instances["nsctc_process_description"].setData("");
    $("#nsctc_process_result").val("");
    $("#nsctc_process_suggest").val("");
    $("#nsctc_process_sampleresult").val("");
    $('#nsctc_process_sampleresult').select2({ placeholder: "-- Chọn --" });

    $.ajax({
        url: "/NSCTC_Process/GetImageForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var html = '';
            var firstPayload = null;
            $.each(response, function (key, value) {
                if (!firstPayload && value) firstPayload = value;
                if (value.id) {
                    html += `
                        <div class="img-cell">
                            <img class="image-item"
                                 id="img_${value.id}"
                                 src="${value.name}"
                                 data-id="${value.id}"
                                 onclick="Process_GetImageCDHAId(${value.id}, this)" />
                            <label class="nsctc-print-ctrl">
                              <input type="checkbox"
                                     class="nsctc-print-check"
                                     value="${value.id}"
                                     checked />
                              Chọn in
                            </label>
                        </div>`;
                }
            });
            $("#imageCDHA").html(html);

            $(".nsctc-print-check").off("change").on("change", function () {
                $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
            });
            $(".nsctc-print-check").each(function () {
                $(this).closest(".img-cell").toggleClass("print-selected", this.checked);
            });

            var desc = "", res = "", suggest = "";
            if (firstPayload) {
                desc = firstPayload.description ?? "";
                res = firstPayload.result ?? "";
                suggest = firstPayload.suggest ?? "";
            }
            if (CKEDITOR.instances["nsctc_process_description"]) {
                CKEDITOR.instances["nsctc_process_description"].setData(desc);
            }
            $("#nsctc_process_result").val(res);
            $("#nsctc_process_suggest").val(suggest);

            var needAutoPick =
                (!desc || desc.trim() === "") &&
                (!res || res.trim() === "");

            if (needAutoPick && serviceId != null) {
                _autoPickSampleForService(serviceId);
            } else {
                $("#nsctc_process_sampleresult").val("");
                $('#nsctc_process_sampleresult').select2({ placeholder: "-- Chọn --" });
            }

            return { needAutoPick: !!needAutoPick };
        }
    });
}

function StartCamera() {
    $.ajax({
        url: "/NSCTC_Process/Check_SelectDevice/",
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
            SwalHelper.Toast.error("Không thể chọn thiết bị. Vui lòng kiểm tra lại!");
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
    document.getElementById("nsctc_process_startcamera").style.display = "none";
    document.getElementById("nsctc_process_snapshot").style.display = "unset";
    document.getElementById("nsctc_process_deletephoto").style.display = "unset";
    document.getElementById("nsctc_process_startvideo").style.display = "unset";
}

function Process_SelectDevice() {
    var deviceId = $('#nsctc_process_select_device_select').val();
    if (deviceId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn thiết bị !")
    }
    else {
        $.ajax({
            url: "/NSCTC_Process/SelectDevice?deviceId=" + deviceId,
            type: 'GET',
            dataType: 'text',
            success: function (result) {
                if (result == 'False') {
                    SwalHelper.Toast.error("Không thể chọn thiết bị. Kiểm tra lại !");
                }
                else {
                    StartCamera_01();
                }
            },
            error: function () {
                SwalHelper.Toast.error("Không thể chọn thiết bị. Kiểm tra lại !");
            }
        });
    }
}

function SnapShot() {
    var countImage = 0;
    $(".imageCDHA").each(function () {
        $(this).find(".image-item").each(function () {
            countImage++;
        })
    })

    if (countImage === 6) {
        SwalHelper.Toast.warning("Chỉ cho phép chụp tối đa 6 hình !");
    }
    else {
        var canvas = document.getElementById("canvas");
        canvas.width = 750;
        canvas.height = 567;
        canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
        var image_data_url = document.getElementById("canvas").toDataURL("image/png");
        var resultCDHAId = null;

        $(".row-service").each(function () {
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
                url: "/NSCTC_Process/SaveImageCDHA/",
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(data),
                dataType: 'text',
                success: function (response) {
                    if (response !== 'True') {
                        SwalHelper.Toast.error("Không thể lưu ảnh. Vui lòng kiểm tra lại!");
                    }
                    else {
                        Process_Load_ResultAndImage_ForService(resultCDHAId);
                    }
                }
            });
        }
        else {
            SwalHelper.Toast.warning("Vui lòng chọn dịch vụ !");
        }
    }
}

function DeletePhoto() {
    var imageCDHAId = $("#nsctc_process_deletephoto").val();
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
                    url: "/NSCTC_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
                    type: 'POST',
                    dataType: 'text',
                    success: function (response) {
                        if (response === "") {
                            SwalHelper.Alert.error('Lỗi', 'Không thể xoá ảnh. Vui lòng kiểm tra lại!');
                        }
                        else {
                            SwalHelper.Toast.success('Xóa ảnh thành công!');
                            Process_Load_ResultAndImage_ForService(response);
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
}

// Begin ghi video màn hình
let media_recorder = null;
let blobs_recorded = [];
let download_link = document.querySelector("#nsctc_process_downloadvideo_a");

function StartVideo() {
    document.getElementById("nsctc_process_startvideo").style.display = "none";
    document.getElementById("nsctc_process_stopvideo").style.display = "unset";

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
    document.getElementById("nsctc_process_stopvideo").style.display = "none";
    document.getElementById("nsctc_process_deletevideo").style.display = "unset";
    document.getElementById("nsctc_process_downloadvideo").style.display = "unset";
    media_recorder.stop();
}

function DeleteVideo() {
    document.getElementById("nsctc_process_startvideo").style.display = "unset";
    document.getElementById("nsctc_process_deletevideo").style.display = "none";
    document.getElementById("nsctc_process_downloadvideo").style.display = "none";
    media_recorder = null;
    blobs_recorded = [];
    download_link.href = null;
}

function DownloadVideo() {
    document.getElementById("nsctc_process_startvideo").style.display = "unset";
    document.getElementById("nsctc_process_deletevideo").style.display = "none";
    document.getElementById("nsctc_process_downloadvideo").style.display = "none";
    download_link.click();
    media_recorder = null;
    blobs_recorded = [];
    download_link.href = null;
}
// End ghi video màn hình

function Process_GetImageCDHAId(imageCDHAId, el) {
    $("#nsctc_process_deletephoto").val(imageCDHAId);
    $("#nsctc_process_xem").val(imageCDHAId);
    $(".image-item").removeClass("active");
    $(el).addClass("active");
}

function GetSampleForService(id) {
    $.ajax({
        url: "/NSCTC_Process/GetSampleForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            CKEDITOR.instances["nsctc_process_description"].setData(response.description);
            $("#nsctc_process_result").val(response.result);
            $("#nsctc_process_suggest").val(response.suggest);
        }
    });
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

function Process_Load_SelectedDoctorInfo() {
    var userId = $("#nsctc_process_userReturnResultNSCTC").val();
    if (!userId) {
        $("#nsctc_process_signerCCCD").empty();
        $("#nsctc_process_doctorInfo").empty();
        return;
    }
    $.ajax({
        url: "/NSCTC_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#nsctc_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
                return;
            }
            var cccd = (u.cccd || "");
            $("#nsctc_process_signerCCCD").val(cccd);
        },
        error: function () {
            $("#nsctc_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
        }
    });
}

// ========================== KÝ SỐ PDF (Form: NỘI SOI CỔ TỬ CUNG) ==========================
function Process_SignPdf() {
    var patientId = $('#nsctc_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

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

    var signerCCCD = $('#nsctc_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#nsctc_process_signerCCCD').focus();
        return;
    }

    var patientCode = $('#nsctc_process_patientId').val() || $('#nsctc_process_sid').val() || "";
    var patientName = $('#nsctc_process_patientName').val() || "";
    var doctorName = $('#nsctc_process_userReturnResultNSCTC option:selected').text() || $('#nsctc_process_userReturnResultNSCTC').val() || "";
    var performedAt = $('#nsctc_process_returnResultTime').val() || "";
    var serviceCode = "";

    $('#showWaitting').modal('show');
    var dataExport = {
        patientId: patientId,
        resultCDHAId: resultCDHAId,
        returnResultTime: $('#nsctc_process_returnResultTime').val(),
        userReturnResult: $('#nsctc_process_userReturnResultNSCTC').val(),
        description: CKEDITOR.instances['nsctc_process_description'].getData(),
        result: $('#nsctc_process_result').val(),
        suggest: $('#nsctc_process_suggest').val()
    };

    $.ajax({
        url: "/NSCTC_Process/ValidPrint/",
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

            var file = base64ToFile(base64Pdf, "temp.pdf", "application/pdf");
            var formData = new FormData();
            formData.append("signerCCCD", signerCCCD);
            formData.append("file", file, file.name);
            formData.append("formType", "NoiSoiCTC");
            formData.append("patientCode", patientCode);
            formData.append("patientName", patientName);
            formData.append("serviceCode", serviceCode);
            formData.append("doctorName", doctorName);
            if (performedAt) formData.append("performedAt", performedAt);

            $.ajax({
                url: "/api/ExternalSign/sign-pdf",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (resp) {
                    $('#showWaitting').modal('hide');
                    try {
                        var obj = (typeof resp === "string") ? JSON.parse(resp) : resp;
                        var signStoreId =
                            (obj && obj.data && (obj.data.id || obj.data.signStoreId)) ||
                            obj.id || obj.signStoreId || null;
                        signStoreId = signStoreId || "51402";
                        var keyResult = null;

                        if (signStoreId) {
                            $.ajax({
                                url: "/NSCTC_Process/SaveSignStoreIdForResultCDHA",
                                type: "POST",
                                dataType: "text",
                                data: { resultCDHAId: resultCDHAId, signStoreId: signStoreId },
                                success: function (res) {
                                    var objResult = JSON.parse(res);
                                    openSigned(signStoreId);
                                    if (objResult.success == true) {
                                        keyResult = objResult.keyResult;
                                        var targetText = doctorName;
                                        var statusNum = (obj && (obj.status === 1 || obj.status === "1")) ? 1 : 0;

                                        var digitalSignPayload = {
                                            referenceType: "NoiSoiCTC",
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
                                            success: function (r) { console.log("Lưu DigitalSign thành công: ", r); },
                                            error: function (f) { console.log("Lưu DigitalSign thất bại: ", f); }
                                        });

                                        if (obj && obj.fileUrl) { window.open(obj.fileUrl, "_blank"); return; }
                                        if (obj && (obj.base64Pdf || obj.base64)) { openBase64Pdf(obj.base64Pdf || obj.base64); return; }
                                    }
                                },
                                error: function (xhr) {
                                    console.error("SaveSignStoreIdForResultCDHA error:", xhr.responseText);
                                }
                            });
                        }
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
    mode = mode || 'current';
    var patientId = $('#nsctc_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    var resultIds = [];
    if (mode === 'current') {
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

    var signerCCCD = $('#nsctc_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#nsctc_process_signerCCCD').focus();
        return;
    }

    var patientCode = $('#nsctc_process_patientId').val() || $('#nsctc_process_sid').val() || "";
    var patientMaBenhAn = $('#nsctc_process_maBenhAn').val() || $('#nsctc_process_sid').val() || "";
    var patientName = $('#nsctc_process_patientName').val() || "";
    var doctorName = $('#nsctc_process_userReturnResultNSCTC option:selected').text() || $('#nsctc_process_userReturnResultNSCTC').val() || "";
    var doctorId = $('#nsctc_process_userReturnResultNSCTC').val();
    var performedAt = $('#nsctc_process_returnResultTime').val() || "";
    var formType = "NoiSoiCTC";

    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tạo file PDF...'); } catch (e) { }

    var pdfJobs = [];
    var chain = Promise.resolve();

    if (resultIds.length > 1) {
        resultIds.forEach(function (rid) {
            chain = chain.then(function () {
                return new Promise(function (resolve) {
                    var dataExport = {
                        patientId: patientId,
                        resultCDHAId: rid,
                        returnResultTime: performedAt,
                        userReturnResult: doctorId,
                        description: null,
                        result: null,
                        suggest: null
                    };
                    $.ajax({
                        url: "/NSCTC_Process/ValidPrintMultiple/",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        data: JSON.stringify(dataExport),
                        success: function (base64Pdf) {
                            if (base64Pdf) {
                                pdfJobs.push({ resultCDHAId: rid, base64Pdf: base64Pdf });
                            }
                            resolve();
                        },
                        error: function () { resolve(); }
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
                    returnResultTime: $('#nsctc_process_returnResultTime').val(),
                    userReturnResult: $('#nsctc_process_userReturnResultNSCTC').val(),
                    description: CKEDITOR.instances['nsctc_process_description'].getData(),
                    result: $('#nsctc_process_result').val(),
                    suggest: $('#nsctc_process_suggest').val()
                };
                console.log(dataExport);
                $.ajax({
                    url: "/NSCTC_Process/ValidPrint/",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    data: JSON.stringify(dataExport),
                    success: function (base64Pdf) {
                        if (base64Pdf) {
                            pdfJobs.push({ resultCDHAId: resultIds[0], base64Pdf: base64Pdf });
                        }
                        resolve();
                    },
                    error: function () { resolve(); }
                });
            });
        });
    }

    chain = chain.then(function () {
        console.log(pdfJobs);
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
        if (performedAt) formData.append("performedAt", "");

        pdfJobs.forEach(function (job) {
            var file = base64ToFile(job.base64Pdf, "temp.pdf", "application/pdf");
            formData.append("files", file, file.name);
            formData.append("resultIds", job.resultCDHAId);
        });

        return new Promise(function (resolve) {
            $.ajax({
                url: "/api/ExternalSign/sign-pdf-multi",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (resp) {
                    var items = [];
                    if (resp) {
                        if (Array.isArray(resp.items)) {
                            items = resp.items.filter(it => it && it.data.id);
                        } else if (resp.item && resp.item.id) {
                            items = [resp.item];
                        } else if (resp.data && resp.data.id) {
                            items = [resp];
                        }
                    }

                    if (!items || items.length === 0) {
                        resolve();
                        $('#showWaitting').modal('hide');
                        var errorMessage = resp.items[0].data.detail || "";
                        SwalHelper.Toast.error(`Xảy ra lỗi khi ký: ${errorMessage}`);
                        return;
                    }

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

                                if (!signStoreId) { resolveSave(); }
                                if (!resultId) { resolveSave(); return; }

                                $.ajax({
                                    url: "/NSCTC_Process/SaveSignStoreIdForResultCDHA",
                                    type: "POST",
                                    dataType: "text",
                                    data: { resultCDHAId: resultId, signStoreId: signStoreId },
                                    success: function (resText) {
                                        try {
                                            var res = JSON.parse(resText);
                                            if (res && res.success) {
                                                var keyResult = res.keyResult;
                                                var statusNum = (objDataResponse && (objDataResponse.status === 1 || objDataResponse.status === "1")) ? 1 : 0;
                                                var payload = {
                                                    referenceType: formType ?? "NoiSoiCTC",
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
                                                        resolveSave();
                                                    }
                                                });
                                            } else {
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
                    console.log("Thất bại: ", xhr);
                    $('#showWaitting').modal('hide');
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
    window.open(URL.createObjectURL(file));
}

function openSigned(id) {
    if (!id) {
        SwalHelper.Toast.warning("Chưa có signStoreId cho dịch vụ này!");
        return;
    }
    window.open("/api/ExternalSign/view-signed/" + encodeURIComponent(id), "_blank");
}

function isProbablyBase64(s) {
    if (!s || typeof s !== "string") return false;
    var re = /^[A-Za-z0-9+/=\s]+$/;
    return re.test(s) && s.length > 1000;
}

function Process_SaveDateToSession() {
    var fromDate = document.getElementById('nsctc_process_timeSearchFrom').value;
    var toDate = document.getElementById('nsctc_process_timeSearchTo').value;

    if (fromDate && toDate) {
        $.ajax({
            url: '/NSCTC_Process/SaveSearchDates',
            type: 'POST',
            data: { timeSearchFrom: fromDate, timeSearchTo: toDate },
            success: function (result) { }
        });
    }
}

// Tick/untick tất cả dịch vụ
$(document).on('change', '#nsctc_chk_all', function () {
    var checked = this.checked === true;
    var $items = $('#nsctc_list_service').find('.nsctc-chk-service');
    $items.prop('checked', checked);

    var marked = $items.filter(':checked').length;
    if (marked >= 2) {
        $items.each(function () {
            var $row = $(this).closest('.row-service');
            $row.removeClass('active');
            if (this.checked) $row.addClass('multi-selected');
            else $row.removeClass('multi-selected');
        });
    } else {
        var $row = $(this).closest('.row-service');
        $row.removeClass('active');
        $('#nsctc_list_service .row-service').removeClass('multi-selected');
    }

    if (checked) {
        $('#nsctc_process_saveresult').hide();
    } else {
        $('#nsctc_process_saveresult').show();
    }
});

// Khai báo guard toàn cục (một lần)
if (window.__nsctc_row_selecting === undefined) {
    window.__nsctc_row_selecting = false;
}

// Tick/untick từng dịch vụ riêng lẻ
$(document).on('change', '.nsctc-chk-service', function () {
    var $all = $('#nsctc_list_service').find('.nsctc-chk-service');
    var total = $all.length;
    var marked = $all.filter(':checked').length;

    var allChecked = total > 0 && marked === total;
    $('#nsctc_chk_all').prop('checked', allChecked);

    if (marked >= 2) {
        $('#nsctc_process_saveresult').hide();
        $('#imageCDHA').empty();
        CKEDITOR.instances.nsctc_process_description.setReadOnly(true);
        CKEDITOR.instances.nsctc_process_description.setData("");
        $('#nsctc_process_result').attr('disabled', true);
        $('#nsctc_process_result').val('');

        $all.each(function () {
            var $row = $(this).closest('.row-service');
            $row.removeClass('active');
            if (this.checked) $row.addClass('multi-selected');
            else $row.removeClass('multi-selected');
        });
    } else {
        $('#nsctc_process_saveresult').show();
        CKEDITOR.instances.nsctc_process_description.setReadOnly(false);
        $('#imageCDHA').empty();
        CKEDITOR.instances.nsctc_process_description.setData("");
        $('#nsctc_process_result').attr('disabled', false);
        $('#nsctc_list_service .row-service').removeClass('multi-selected');
    }

    if (marked === 1 && !window.__nsctc_row_selecting) {
        var $only = $all.filter(':checked').first();
        var resultId = $only.val();

        window.__nsctc_row_selecting = true;
        setTimeout(function () {
            try {
                Process_CheckedBoxOnRow(resultId);
            } finally {
                window.__nsctc_row_selecting = false;
            }
        }, 0);
    }
});

//***************************************************************************************** Return Result

function ReturnResult_Refresh() {
    $.ajax({
        url: "/NSCTC_ReturnResult/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
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
    var nsctc_returnresult_pidorseq = $("#nsctc_returnresult_pidorseq").val();
    var timeSearchFrom = $("#nsctc_returnresult_timeSearchFrom").val();
    var timeSearchTo = $("#nsctc_returnresult_timeSearchTo").val();
    $.ajax({
        url: "/NSCTC_ReturnResult/Search?" + "pidorseq=" + nsctc_returnresult_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#nsctc_returnresult_pidorseq").val('');
    $('#nsctc_returnresult_id').val('');
    $('#nsctc_returnresult_patientId').val('');
    $('#nsctc_returnresult_seq').val('');
    $('#nsctc_returnresult_sid').val('');
    $('#nsctc_returnresult_patientName').val('');
    $('#nsctc_returnresult_age').val('');
    $('#nsctc_returnresult_sex').val('');
    $('#nsctc_returnresult_obj').val('');
    $('#nsctc_returnresult_type').val('');
    $('#nsctc_returnresult_location').val('');
    $('#nsctc_returnresult_doctor').val('');
    $('#nsctc_returnresult_getSampleTime').val('');
    $('#nsctc_returnresult_returnResultTime').val('');
    $('#nsctc_returnresult_userReturnResultNSCTC').val('');
    $('#nsctc_returnresult_address').val('');
    $('#nsctc_returnresult_diagnostic').val('');
    $('#tbody-gridview-service').empty();
}

function ReturnResult_GetPatientInfo(id) {
    $.ajax({
        url: "/NSCTC_ReturnResult/GetPatientInfo?id=" + id,
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

function ReturnResult_Load_SelectedDoctorInfo() {
    var userId = $("#nsctc_returnresult_userLoginId").val();
    if (!userId) {
        $("#nsctc_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/NSCTC_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#nsctc_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            var cccd = (u.cccd || "");
            $("#nsctc_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#nsctc_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
        }
    });
}

function ReturnResult_GetListServiceForPatient(id) {
    $.ajax({
        url: "/NSCTC_ReturnResult/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_nsctc-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function ReturnResult_Invalid() {
    var id = $('#nsctc_returnresult_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    const selectedKeys = [];
    $('.nsctc-returnresult-chk-service:checked').each(function () {
        var keyResult = $(this).data('key-result');
        if (keyResult) {
            selectedKeys.push(keyResult);
        }
    });

    if (selectedKeys.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn ít nhất một dịch vụ!");
        return;
    }

    if (confirm('Bạn muốn InValid kết quả của bệnh nhân ?')) {
        $.ajax({
            url: "/NSCTC_ReturnResult/Invalid",
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'text',
            data: JSON.stringify({
                patientId: parseInt(id),
                keyResultList: selectedKeys,
                categoryCode: 'NSCTC'
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

function ReturnResult_Get_Count() {
    $.ajax({
        url: "/NSCTC_ReturnResult/Get_Count/",
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
    var patientId = $('#nsctc_returnresult_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    var resultCDHAId = "";
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            if ($(this).is(':checked')) {
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
        url: "/NSCTC_ReturnResult/Print?resultCDHAId=" + resultCDHAId,
        dataType: "text",
        type: "GET",
        success: function (response) {
            $('#showWaitting').modal('hide');

            if (response === "") {
                SwalHelper.Toast.error("In không thành công. Vui lòng kiểm tra lại!");
                return;
            }

            try {
                var byteCharacters = atob(response);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var blob = new Blob([byteArray], { type: 'application/pdf' });

                var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
                if (isMobile) {
                    var link = document.createElement('a');
                    link.href = URL.createObjectURL(blob);
                    link.download = 'KetQua_NoiSoiCTC_' + resultCDHAId + '_' + new Date().getTime() + '.pdf';
                    link.style.display = 'none';
                    document.body.appendChild(link);
                    link.click();
                    setTimeout(function () {
                        document.body.removeChild(link);
                        URL.revokeObjectURL(link.href);
                    }, 100);
                    SwalHelper.Toast.success('File PDF đã được tải xuống!');
                } else {
                    var fileURL = URL.createObjectURL(blob);
                    var newWindow = window.open(fileURL, '_blank');
                    if (!newWindow) {
                        var link = document.createElement('a');
                        link.href = fileURL;
                        link.download = 'KetQua_NoiSoiCTC_' + resultCDHAId + '.pdf';
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

function ReturnResult_CheckedBoxOnRow(id, event) {
    if (event && event.target.type === 'checkbox') {
        var $rows = $('.nsctc-returnresult-chk-service');
        var total = $rows.length;
        var marked = $rows.filter(':checked').length;
        var allChecked = total > 0 && marked === total;
        $('#nsctc_returnresult_chk_all').prop('checked', allChecked);

        var selectedCheckboxes = $(".row-service .form-check-input:checked");
        if (selectedCheckboxes.length === 1) {
            var $row = selectedCheckboxes.first().closest('.row-service');
            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
        } else if (selectedCheckboxes.length > 1) {
            const $containHinh = $('.image-returnresult');
            $containHinh.empty();
            CKEDITOR.instances["nsctc_returnresult_description"].setData("");
            $("#nsctc_returnresult_result").val("");
            $("#nsctc_returnresult_suggest").val("");
            ReturnResult_ToggleDigitalSignButtons(false);
        } else {
            const $containHinh = $('.image-returnresult');
            $containHinh.empty();
            CKEDITOR.instances["nsctc_returnresult_description"].setData("");
            $("#nsctc_returnresult_result").val("");
            $("#nsctc_returnresult_suggest").val("");
            ReturnResult_ToggleDigitalSignButtons(false);
        }
        return;
    }

    $(".row-service .form-check-input").each(function () {
        var $chk = $(this);
        var resultCdhaId = String($chk.val());
        var isTarget = (resultCdhaId === String(id));
        $chk.prop("checked", isTarget);

        if (isTarget) {
            ReturnResult_LoadImageForService(id);
            var $row = $chk.closest('.row-service');
            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
        }
    });

    var $rows = $('.nsctc-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;
    var allChecked = total > 0 && marked === total;
    $('#nsctc_returnresult_chk_all').prop('checked', allChecked);
}

$(document).on('change', '.nsctc-returnresult-chk-service', function () {
    var $rows = $('.nsctc-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    var allChecked = total > 0 && marked === total;
    $('#nsctc_returnresult_chk_all').prop('checked', allChecked);

    var selectedCheckboxes = $(".row-service .form-check-input:checked");

    if (selectedCheckboxes.length === 1) {
        var selectedId = selectedCheckboxes.first().val();
        ReturnResult_LoadImageForService(selectedId);
        var $row = selectedCheckboxes.first().closest('.row-service');
        var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
        ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
    } else if (selectedCheckboxes.length > 1) {
        const $containHinh = $('.image-returnresult');
        $containHinh.empty();
        CKEDITOR.instances["nsctc_returnresult_description"].setData("");
        $("#nsctc_returnresult_result").val("");
        $("#nsctc_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    } else {
        const $containHinh = $('.image-returnresult');
        $containHinh.empty();
        CKEDITOR.instances["nsctc_returnresult_description"].setData("");
        $("#nsctc_returnresult_result").val("");
        $("#nsctc_returnresult_suggest").val("");
        ReturnResult_ToggleDigitalSignButtons(false);
    }
});

function ReturnResult_LoadImageForService(id) {
    $("#image-returnresult").empty();
    CKEDITOR.instances["nsctc_returnresult_description"].setData("");
    $("#nsctc_returnresult_result").val("");
    $("#nsctc_returnresult_suggest").val("");
    $.ajax({
        url: "/NSCTC_ReturnResult/GetImageForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                if (value.id) {
                    $("#image-returnresult").append('<div class="image-item"><img class="image" id="' + value.id + '" src="' + value.name + '" onclick="ReturnResult_GetImageCDHAId(' + value.id + ', this)"  /></div>');
                }
                CKEDITOR.instances["nsctc_returnresult_description"].setData(value.description);
                $("#nsctc_returnresult_result").val(value.result);
                $("#nsctc_returnresult_suggest").val(value.suggest);
            });
        }
    });
}

function ReturnResult_ToggleDigitalSignButtons(isDigitallySigned) {
    var $printSignedBtn = $('#nsctc_returnresult_print_signed');
    if ($printSignedBtn.length) {
        if (isDigitallySigned) {
            $printSignedBtn.show().prop('disabled', false);
        } else {
            $printSignedBtn.hide().prop('disabled', true);
        }
    }

    var $invalidRemoveSignBtn = $('#nsctc_returnresult_invalid_removedigitalsign');
    if ($invalidRemoveSignBtn.length) {
        if (isDigitallySigned) {
            $invalidRemoveSignBtn.show().prop('disabled', false);
        } else {
            $invalidRemoveSignBtn.hide().prop('disabled', true);
        }
    }
}

function ReturnResult_GetImageCDHAId(imageCDHAId, img) {
    $('.image-returnresult img').removeClass('active');
    $(img).toggleClass('active');
    $("#nsctc_returnresult_xem").val(imageCDHAId);
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
    function openSigned(id) {
        if (!id) { SwalHelper.Toast.warning("Chưa có signStoreId cho dịch vụ này!"); return; }
        window.open("/api/ExternalSign/view-signed/" + encodeURIComponent(id), "_blank");
    }

    var resultId = "";
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            if ($(this).is(':checked')) {
                resultId = $(this).val();
            }
        });
    });

    if (!resultId) {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ để xem PDF đã ký!");
        return;
    }

    $.ajax({
        url: "/NSCTC_ReturnResult/GetSignStoreId",
        type: "GET",
        data: { resultId: resultId },
        dataType: "json",
        success: function (res) {
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

// ========================== KÝ SỐ PDF CHO TAB ĐÃ XONG (RETURN RESULT) ==========================
function ReturnResult_SignPdf_Multi(mode) {
    mode = mode || 'current';
    var patientId = $('#nsctc_returnresult_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    var resultIds = [];
    if (mode === 'current') {
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

    var signerCCCD = $('#nsctc_returnresult_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#nsctc_returnresult_signerCCCD').focus();
        return;
    }

    var patientCode = $('#nsctc_returnresult_patientId').val() || $('#nsctc_returnresult_sid').val() || "";
    var patientMaBenhAn = $('#nsctc_returnresult_maBenhAn').val() || $('#nsctc_returnresult_sid').val() || "";
    var patientName = $('#nsctc_returnresult_patientName').val() || "";
    var doctorName = $('#nsctc_returnresult_userReturnResultNSCTC option:selected').text() || $('#nsctc_returnresult_userReturnResultNSCTC').val() || "";
    var doctorId = $('#nsctc_returnresult_userLoginId').val();
    var performedAt = $('#nsctc_returnresult_returnResultTime').val() || "";
    var formType = "NoiSoiCTC";

    try { $('#showWaitting').modal('show'); updateLoadingText('Đang tải file PDF...'); } catch (e) { }

    var pdfJobs = [];
    var chain = Promise.resolve();

    resultIds.forEach(function (rid) {
        chain = chain.then(function () {
            return new Promise(function (resolve) {
                $.ajax({
                    url: "/NSCTC_ReturnResult/Print?resultCDHAId=" + rid,
                    type: "GET",
                    dataType: "text",
                    success: function (base64Pdf) {
                        if (base64Pdf) {
                            pdfJobs.push({ resultCDHAId: rid, base64Pdf: base64Pdf });
                        }
                        resolve();
                    },
                    error: function () {
                        console.error("Lỗi lấy PDF cho resultCDHAId:", rid);
                        resolve();
                    }
                });
            });
        });
    });

    chain = chain.then(function () {
        if (pdfJobs.length === 0) {
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.error("Không tạo được file PDF nào để ký.");
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
        if (performedAt) formData.append("performedAt", "");

        pdfJobs.forEach(function (job) {
            var file = base64ToFile(job.base64Pdf, "temp.pdf", "application/pdf");
            formData.append("files", file, file.name);
            formData.append("resultIds", job.resultCDHAId);
        });

        return new Promise(function (resolve) {
            $.ajax({
                url: "/api/ExternalSign/sign-pdf-multi",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (resp) {
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

                    if (!items || items.length === 0) {
                        resolve();
                        $('#showWaitting').modal('hide');
                        var messageError = resp?.items[0]?.error || "";
                        SwalHelper.Toast.error(`Xảy ra lỗi khi ký số: ${messageError}`);
                        return;
                    }

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

                                if (!signStoreId || !resultId) { resolveSave(); return; }

                                $.ajax({
                                    url: "/NSCTC_ReturnResult/SaveSignStoreIdForResultCDHA",
                                    type: "POST",
                                    dataType: "text",
                                    data: { resultCDHAId: resultId, signStoreId: signStoreId },
                                    success: function (resText) {
                                        try {
                                            var res = JSON.parse(resText);
                                            if (res && res.success) {
                                                var keyResult = res.keyResult;
                                                var statusNum = (objDataResponse && (objDataResponse.status === 1 || objDataResponse.status === "1")) ? 1 : 0;
                                                var payload = {
                                                    referenceType: formType ?? "NoiSoiCTC",
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
                    var msg = "Ký số thất bại.";
                    if (xhr && xhr.responseJSON) msg += "\n" + xhr.responseJSON.message;
                    SwalHelper.Toast.error(msg);
                    resolve();
                }
            });
        });
    });
}

function ReturnResult_Invalid_RemoveDigitalSign() {
    var id = $('#nsctc_returnresult_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

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
            url: "/NSCTC_ReturnResult/Invalid",
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'text',
            data: JSON.stringify({
                patientId: parseInt(id),
                keyResultList: selectedKeys,
                categoryCode: 'NSCTC'
            }),
            success: function (result) {
                if (result === 'True') {
                    SwalHelper.Toast.success("Invalid thành công và đã xóa hủy ký số!");
                    UpdateSignStatus(resultCDHAId);
                    ReturnResult_Refresh();
                    //ReturnResult_Get_Count();
                }
                else {
                    SwalHelper.Toast.error("Không thể Invalid. Vui lòng kiểm tra lại!");
                }
            },
            error: function (xhr) {
                SwalHelper.Toast.error("Không thể Invalid. Vui lòng kiểm tra lại!");
            }
        });
    }
}

function ReturnResult_SaveDateToSession() {
    var fromDate = document.getElementById('nsctc_returnresult_timeSearchFrom').value;
    var toDate = document.getElementById('nsctc_returnresult_timeSearchTo').value;

    if (fromDate && toDate) {
        $.ajax({
            url: '/NSCTC_ReturnResult/SaveSearchDates',
            type: 'POST',
            data: { timeSearchFrom: fromDate, timeSearchTo: toDate },
            success: function (result) { }
        });
    }
}

// Tick/untick tất cả cho ReturnResult
$(document).on('change', '#nsctc_returnresult_chk_all', function () {
    var checked = this.checked === true;
    $('.nsctc-returnresult-chk-service').prop('checked', checked);

    const $containHinh = $('.image-returnresult');
    $containHinh.empty();
    CKEDITOR.instances["nsctc_returnresult_description"].setData("");
    $("#nsctc_returnresult_result").val("");
    $("#nsctc_returnresult_suggest").val("");
    ReturnResult_ToggleDigitalSignButtons(false);
});

$(document).on('change', '.nsctc-returnresult-chk-service', function () {
    var $rows = $('.nsctc-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;
    var allChecked = total > 0 && marked === total;
    $('#nsctc_returnresult_chk_all').prop('checked', allChecked);
});

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