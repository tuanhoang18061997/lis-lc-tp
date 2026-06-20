// ============================================================================
// TDCN Process JS
// Tách từ tdcn.js hiện tại, giữ nguyên tên hàm global để Razor onclick cũ không bị gãy.
// ============================================================================

//***************************************************************************************** Process



// ============================== PROCESS MODE WRAPPERS ==============================
// Dùng cho màn _TDCN_Process_ListService_Custom đang có 2 UI:
// - normal: nhập mô tả/kết luận/đề nghị
// - uploadPdf: upload file PDF theo từng dịch vụ
// Giữ nguyên các hàm cũ bên dưới để không làm gãy các onclick/Razor cũ.
function TDCN_Process_GetResultMode() {
    var mode = $('#tdcn_process_result_mode').val()
        || window.TDCN_PROCESS_RESULT_MODE
        || localStorage.getItem('TDCN_PROCESS_RESULT_MODE')
        || 'normal';

    return mode === 'uploadPdf' ? 'uploadPdf' : 'normal';
}

function TDCN_Process_IsUploadPdfMode() {
    return TDCN_Process_GetResultMode() === 'uploadPdf';
}

function TDCN_Process_SaveByMode() {
    if (TDCN_Process_IsUploadPdfMode()) {
        return Process_SavePDFResult();
    }

    return Process_SaveResult();
}

function TDCN_Process_ValidPrintByMode() {
    if (TDCN_Process_IsUploadPdfMode()) {
        return Process_ValidPrintUploadPdf();
    }

    return Process_ValidPrint();
}

function TDCN_Process_SignPdfByMode(mode) {
    // Hiện tại Process_SignPdf_Multi vẫn là luồng ký số dùng chung.
    // Sau khi backend hỗ trợ ký PDF upload đã lưu theo KeyResultForHis riêng,
    // có thể tách nhánh uploadPdf tại đây mà không cần sửa lại Razor button.
    return Process_SignPdf_Multi(mode || 'selected');
}
// ============================ END PROCESS MODE WRAPPERS ============================

// ============================== UNIFIED SERVICE LIST HELPERS ==============================
function TDCN_Process_GetSelectedServiceIds() {
    var ids = [];
    $('#tdcn_list_service .tdcn-chk-service:checked').each(function () {
        var value = $(this).val();
        if (value) ids.push(value);
    });
    return ids;
}

function TDCN_Process_GetSelectedServiceId() {
    var hiddenId = $('#tdcn_upload_pdf_resultCDHAId').val();
    if (hiddenId) return hiddenId;

    var ids = TDCN_Process_GetSelectedServiceIds();
    return ids.length > 0 ? ids[0] : '';
}

function TDCN_Process_SetSharedServiceSelection(resultId) {
    var idText = String(resultId || '');
    $('#tdcn_upload_pdf_resultCDHAId').val(idText);
    $('#tdcn_list_service .row-service').removeClass('active');
    $('#tdcn_list_service .tdcn-chk-service').each(function () {
        var $chk = $(this);
        var isTarget = String($chk.val()) === idText;
        $chk.prop('checked', isTarget);
        if (isTarget) {
            $chk.closest('.row-service').addClass('active');
        }
    });

    var $checks = $('#tdcn_list_service .tdcn-chk-service');
    $('#tdcn_chk_all').prop('checked', $checks.length > 0 && $checks.filter(':checked').length === $checks.length);
}
// ============================ END UNIFIED SERVICE LIST HELPERS ============================


function Process_ValidateInput(id) {
    var flag = true;
    $("#" + id).find(".valid").each(function (i, e) {
        var value = $(e).val();
        if (value === "" || !value) {
            if (flag) {
                if (e.id === "tdcn_process_userReturnResultTDCN") {
                    $('#tdcn_process_userReturnResultTDCN').select2('focus');
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
        url: "/TDCN_Process/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#tdcn_process_timeSearchFrom").val(today);
            //$("#tdcn_process_timeSearchTo").val(today);

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
    var tdcn_process_pidorseq = $("#tdcn_process_pidorseq").val();
    var timeSearchFrom = $("#tdcn_process_timeSearchFrom").val();
    var timeSearchTo = $("#tdcn_process_timeSearchTo").val();
    $.ajax({
        url: "/TDCN_Process/Search?" + "pidorseq=" + tdcn_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#tdcn_process_pidorseq").val('');
    $('#tdcn_process_id').val('');
    $('#tdcn_process_patientId').val('');
    $('#tdcn_process_seq').val('');
    $('#tdcn_process_sid').val('');
    $('#tdcn_process_patientName').val('');
    $('#tdcn_process_age').val('');
    $('#tdcn_process_sex').val('');
    $('#tdcn_process_obj').val('');
    $('#tdcn_process_type').val('');
    $('#tdcn_process_location').val('');
    //$('#tdcn_process_doctor').val('');
    $('#tdcn_process_getSampleTime').val('');
    $('#tdcn_process_returnResultTime').val('');
    $('#tdcn_process_location').val('');
    //$('#tdcn_process_doctor').val('');
    $('#tdcn_process_userReturnResultTDCN').val('');
    $('#tdcn_process_address').val('');
    $('#tdcn_process_diagnostic').val('');
    $('#tbody-gridview-service').empty();
    $('#imageCDHA').empty();
    $('#tdcn_process_result').empty();
    $('#tdcn_process_suggest').empty();
    $('#tdcn_text_key').empty();
    CKEDITOR.instances["tdcn_process_description"].setData("");
    Process_SetSelect2_03(true);
}

function Process_GetPatientInfo(id) {
    $.ajax({
        url: "/TDCN_Process/Check_SelectDevice/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            if (result === "False") {
                $('#addDeviceForm').modal('show');
            } else {
                $.ajax({
                    url: "/TDCN_Process/GetPatientInfo?id=" + id,
                    type: "GET",
                    dataType: "html",
                    cache: false,
                    success: function (result) {
                        $("#process_patientInfo").html(result);
                        $(".list-group-item-action").removeClass("active");
                        $(`.list-group-item-action[data-id='${id}']`).addClass("active");
                        Process_SetSelect2_03();
                        // Auto-chọn bác sĩ = user đang login (nếu chưa có giá trị)
                        var loginId = $("#tdcn_process_userLoginId").val();
                        console.log(loginId);
                        var $sel = $("#tdcn_process_userReturnResultTDCN");
                        if (loginId && (!$sel.val() || $sel.val() === "")) {
                            $sel.val(loginId).trigger("change");
                        }

                        // Gắn handler đổi bác sĩ
                        $(document)
                            .off("change", "#tdcn_process_userReturnResultTDCN")
                            .on("change", "#tdcn_process_userReturnResultTDCN", Process_Load_SelectedDoctorInfo);

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
    })

}

function Process_GetListServiceForPatient(id) {
    $.ajax({
        url: "/TDCN_Process/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#process_tdcn-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}


function Process_SaveResult() {
    var patientId = $('#tdcn_process_id').val();
    if (patientId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var validate = Process_ValidateInput('process_patientInfo');
        if (validate) {

            // Lấy ServiceId
            var resultCDHAId = TDCN_Process_GetSelectedServiceId();

            if (resultCDHAId === "") {
                SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
            }
            else {
                var returnResultTime = $('#tdcn_process_returnResultTime').val();
                var userReturnResult = $('#tdcn_process_userReturnResultTDCN').val();
                var description = CKEDITOR.instances['tdcn_process_description'].getData();
                var result = $('#tdcn_process_result').val();
                var suggest = $('#tdcn_process_suggest').val();

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
                    url: "/TDCN_Process/SaveResult/",
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
    var deviceId = $('#tdcn_getsample_select_device_select').val();
    if (deviceId === "") {
        SwalHelper.Toast.warning("Vui lòng chọn thiết bị !")
    }
    else {
        $.ajax({
            url: "/TDCN_Process/SelectDevice?deviceId=" + deviceId,
            type: 'GET',
            dataType: 'text',
            success: function (result) {
                if (result === 'False') {
                    SwalHelper.Toast.error("Không thể chọn thiết bị. Kiểm tra lại !");
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
        url: "/TDCN_Process/Check_SelectDevice/",
        type: 'GET',
        dataType: 'text',
        success: function (result) {
            if (result === "False") {
                $('#addDeviceForm').modal('show');
            }
            else {
                var patientId = $('#tdcn_process_id').val();
                if (patientId === "") {
                    SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
                }
                else {
                    var validate = Process_ValidateInput('process_patientInfo');
                    if (validate) {

                        // Lấy ServiceId
                        var resultCDHAId = TDCN_Process_GetSelectedServiceId();

                        if (resultCDHAId === "") {
                            SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
                        }
                        else {
                            $('#showWaitting').modal('show');
                            var returnResultTime = $('#tdcn_process_returnResultTime').val();
                            var userReturnResult = $('#tdcn_process_userReturnResultTDCN').val();
                            var description = CKEDITOR.instances['tdcn_process_description'].getData();
                            var result = $('#tdcn_process_result').val();
                            var suggest = $('#tdcn_process_suggest').val();

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
                                url: "/TDCN_Process/ValidPrint/",
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


// ============================== UPLOAD PDF UI HELPERS ==============================
window.TDCN_UPLOAD_PDF_OBJECT_URL = window.TDCN_UPLOAD_PDF_OBJECT_URL || null;
window.TDCN_UPLOAD_PDF_PREVIEW_URL = window.TDCN_UPLOAD_PDF_PREVIEW_URL || null;
window.TDCN_UPLOAD_PDF_FILE = window.TDCN_UPLOAD_PDF_FILE || null;

function TDCN_Process_ShowSelectPatientWarning() {
    if (window.SwalHelper && SwalHelper.Toast && typeof SwalHelper.Toast.warning === 'function') {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
        return;
    }
    if (window.Swal && typeof Swal.fire === 'function') {
        Swal.fire({ icon: 'warning', title: 'Thông báo', text: 'Vui lòng chọn bệnh nhân!' });
        return;
    }
    alert('Vui lòng chọn bệnh nhân!');
}

function TDCN_Process_HasSelectedPatient() {
    var patientDbId = ($('#tdcn_process_id').val() || '').toString().trim();
    var patientCode = ($('#tdcn_process_patientId').val() || '').toString().trim();
    return patientDbId !== '' || patientCode !== '';
}

function TDCN_Process_EnsurePatientBeforePdfAction(e) {
    if (TDCN_Process_HasSelectedPatient()) return true;
    if (e) {
        e.preventDefault();
        e.stopPropagation();
    }
    TDCN_Process_ShowSelectPatientWarning();
    return false;
}

function TDCN_Process_FormatFileSize(bytes) {
    if (!bytes) return '0 KB';
    var units = ['B', 'KB', 'MB', 'GB'];
    var size = bytes;
    var unit = 0;
    while (size >= 1024 && unit < units.length - 1) {
        size = size / 1024;
        unit++;
    }
    return size.toFixed(unit === 0 ? 0 : 2) + ' ' + units[unit];
}

function TDCN_Process_HandleUploadPdfFile(file) {
    if (!file) return;
    if (!TDCN_Process_HasSelectedPatient()) {
        TDCN_Process_ClearUploadPdf();
        TDCN_Process_ShowSelectPatientWarning();
        return;
    }

    window.TDCN_UPLOAD_PDF_FILE = file;
    var fileName = (file.name || '').toLowerCase();
    if (file.type !== 'application/pdf' && !fileName.endsWith('.pdf')) {
        SwalHelper.Toast.warning('Vui lòng chọn đúng file PDF.');
        TDCN_Process_ClearUploadPdf();
        return;
    }

    if (window.TDCN_UPLOAD_PDF_OBJECT_URL) {
        URL.revokeObjectURL(window.TDCN_UPLOAD_PDF_OBJECT_URL);
    }

    window.TDCN_UPLOAD_PDF_OBJECT_URL = URL.createObjectURL(file);
    window.TDCN_UPLOAD_PDF_PREVIEW_URL = window.TDCN_UPLOAD_PDF_OBJECT_URL;
    $('#tdcn_pdf_preview_embed').attr('src', window.TDCN_UPLOAD_PDF_OBJECT_URL).show();
    $('#tdcn_pdf_preview_empty').hide();
    $('#tdcn_pdf_file_name').text(file.name);
    $('#tdcn_pdf_file_size').text(TDCN_Process_FormatFileSize(file.size));
    $('#tdcn_pdf_file_info').show();
}

function TDCN_Process_ClearUploadPdf() {
    if (window.TDCN_UPLOAD_PDF_OBJECT_URL) {
        URL.revokeObjectURL(window.TDCN_UPLOAD_PDF_OBJECT_URL);
        window.TDCN_UPLOAD_PDF_OBJECT_URL = null;
    }
    window.TDCN_UPLOAD_PDF_FILE = null;
    window.TDCN_UPLOAD_PDF_PREVIEW_URL = null;
    $('#tdcn_process_result_pdf').val('');
    $('#tdcn_pdf_preview_embed').attr('src', '').hide();
    $('#tdcn_pdf_preview_empty').show();
    $('#tdcn_pdf_file_info').hide();
    $('#tdcn_pdf_file_name').text('');
    $('#tdcn_pdf_file_size').text('');
}

function TDCN_Process_OpenUploadPdfNewTab() {
    var url = window.TDCN_UPLOAD_PDF_OBJECT_URL || window.TDCN_UPLOAD_PDF_PREVIEW_URL || $('#tdcn_pdf_preview_embed').attr('src');
    if (!url) {
        SwalHelper.Toast.warning('Chưa có file PDF để mở.');
        return;
    }
    window.open(url, '_blank');
}

$(document)
    .off('click.tdcnUploadPdfGuard', '.tdcn-pdf-choose-btn, #tdcn_process_result_pdf')
    .on('click.tdcnUploadPdfGuard', '.tdcn-pdf-choose-btn, #tdcn_process_result_pdf', function (e) {
        if (!TDCN_Process_EnsurePatientBeforePdfAction(e)) return false;
    });

$(document)
    .off('click.tdcnUploadPdfDropzone', '#tdcn_pdf_dropzone')
    .on('click.tdcnUploadPdfDropzone', '#tdcn_pdf_dropzone', function (e) {
        if (!TDCN_Process_EnsurePatientBeforePdfAction(e)) return false;
        if ($(e.target).closest('label, input').length) return;
        $('#tdcn_process_result_pdf').trigger('click');
    });

$(document)
    .off('dragover.tdcnUploadPdf dragleave.tdcnUploadPdf drop.tdcnUploadPdf', '#tdcn_pdf_dropzone')
    .on('dragover.tdcnUploadPdf', '#tdcn_pdf_dropzone', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('dragover');
    })
    .on('dragleave.tdcnUploadPdf', '#tdcn_pdf_dropzone', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');
    })
    .on('drop.tdcnUploadPdf', '#tdcn_pdf_dropzone', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');
        if (!TDCN_Process_EnsurePatientBeforePdfAction(e)) return false;
        var files = e.originalEvent.dataTransfer && e.originalEvent.dataTransfer.files;
        if (!files || !files.length) return;
        try {
            var dt = new DataTransfer();
            dt.items.add(files[0]);
            document.getElementById('tdcn_process_result_pdf').files = dt.files;
        } catch (ex) { }
        TDCN_Process_HandleUploadPdfFile(files[0]);
    });
// ============================ END UPLOAD PDF UI HELPERS ============================

function Process_GetUploadPdfSelectedResultId() {
    var resultCDHAId = $('#tdcn_upload_pdf_resultCDHAId').val();
    if (resultCDHAId) return resultCDHAId;

    return TDCN_Process_GetSelectedServiceId();
}

function Process_GetUploadPdfFile() {
    if (window.TDCN_UPLOAD_PDF_FILE) {
        return window.TDCN_UPLOAD_PDF_FILE;
    }

    var input = document.getElementById('tdcn_process_result_pdf');
    if (input && input.files && input.files.length > 0) {
        return input.files[0];
    }

    return null;
}
function Process_GetUploadPdfSelectedResultIds() {
    var ids = TDCN_Process_GetSelectedServiceIds();

    if (ids.length === 0) {
        var hiddenId = $('#tdcn_upload_pdf_resultCDHAId').val();
        if (hiddenId) ids.push(hiddenId);
    }

    return ids;
}

function Process_GetUploadPdfSelectedResultIdStrict() {
    var ids = Process_GetUploadPdfSelectedResultIds();

    if (ids.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
        return '';
    }

    if (ids.length > 1) {
        SwalHelper.Toast.warning("Chỉ được chọn 1 dịch vụ cho mỗi file PDF. Vui lòng bỏ chọn các dịch vụ còn lại!");
        return '';
    }

    return ids[0];
}
function Process_SavePDFResult() {
    var patientId = $('#tdcn_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return false;
    }

    var validate = Process_ValidateInput('process_patientInfo');
    if (!validate) return false;

    var resultCDHAId = Process_GetUploadPdfSelectedResultIdStrict();
    if (!resultCDHAId) {
        return false;
    }

    var pdfFile = Process_GetUploadPdfFile();
    if (!pdfFile) {
        SwalHelper.Toast.warning("Vui lòng chọn file PDF kết quả!");
        return false;
    }

    var fileName = (pdfFile.name || '').toLowerCase();
    if (pdfFile.type !== 'application/pdf' && !fileName.endsWith('.pdf')) {
        SwalHelper.Toast.warning("File kết quả phải là định dạng PDF!");
        return false;
    }

    var returnResultTime = $('#tdcn_process_returnResultTime').val();
    var userReturnResult = $('#tdcn_process_userReturnResultTDCN').val();

    // UI upload PDF không nhập mô tả/kết luận/đề nghị, nhưng vẫn gửi field để giữ đúng contract ResultCDHAModel.
    var description = '';
    var result = '';
    var suggest = '';

    if (window.CKEDITOR && CKEDITOR.instances && CKEDITOR.instances['tdcn_process_description']) {
        description = CKEDITOR.instances['tdcn_process_description'].getData() || '';
    } else if ($('#tdcn_process_description').length) {
        description = $('#tdcn_process_description').val() || '';
    }

    if ($('#tdcn_process_result').length) {
        result = $('#tdcn_process_result').val() || '';
    }

    if ($('#tdcn_process_suggest').length) {
        suggest = $('#tdcn_process_suggest').val() || '';
    }

    var fd = new FormData();
    fd.append('patientId', patientId);
    fd.append('resultCDHAId', resultCDHAId);
    fd.append('returnResultTime', returnResultTime || '');
    fd.append('userReturnResult', userReturnResult || '');
    fd.append('description', description);
    fd.append('result', result);
    fd.append('suggest', suggest);
    fd.append('resultPdf', pdfFile, pdfFile.name || 'tdcn-result.pdf');

    $('#showWaitting').modal('show');

    $.ajax({
        url: "/TDCN_Process/SavePDFResult/",
        data: fd,
        processData: false,
        contentType: false,
        dataType: "json",
        type: "POST",
        success: function (res) {
            $('#showWaitting').modal('hide');

            if (!res || res.success !== true) {
                SwalHelper.Toast.error(res && res.message ? res.message : "Lưu PDF không thành công. Vui lòng kiểm tra lại!");
                return;
            }

            SwalHelper.Toast.success("Lưu PDF kết quả thành công.");

            // Reload lại danh sách dịch vụ để cập nhật trạng thái file PDF của từng dịch vụ.
            Process_GetListServiceForPatient(patientId);

            // Clear file sau khi lưu để tránh user chọn dịch vụ khác rồi lưu nhầm cùng PDF.
            if (typeof TDCN_Process_ClearUploadPdf === 'function') {
                TDCN_Process_ClearUploadPdf();
            }
        },
        error: function () {
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.error("Lưu PDF không thành công. Vui lòng kiểm tra lại!");
        }
    });

    return true;
}
function Process_ValidPrintUploadPdf() {
    $.ajax({
        url: "/TDCN_Process/Check_SelectDevice/",
        type: 'GET',
        dataType: 'text',
        success: function (deviceResult) {
            if (deviceResult === "False") {
                $('#addDeviceForm').modal('show');
                return;
            }

            var patientId = $('#tdcn_process_id').val();
            if (!patientId) {
                SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
                return;
            }

            var validate = Process_ValidateInput('process_patientInfo');
            if (!validate) return false;

            var resultCDHAId = Process_GetUploadPdfSelectedResultId();
            if (!resultCDHAId) {
                SwalHelper.Toast.warning("Vui lòng chọn dịch vụ!");
                return;
            }

            var pdfFile = Process_GetUploadPdfFile();
            if (!pdfFile) {
                SwalHelper.Toast.warning("Vui lòng chọn file PDF kết quả!");
                return;
            }

            var fileName = (pdfFile.name || '').toLowerCase();
            if (pdfFile.type !== 'application/pdf' && !fileName.endsWith('.pdf')) {
                SwalHelper.Toast.warning("File kết quả phải là định dạng PDF!");
                return;
            }

            var returnResultTime = $('#tdcn_process_returnResultTime').val();
            var userReturnResult = $('#tdcn_process_userReturnResultTDCN').val();

            // UI Upload PDF không dùng CKEditor/Kết luận/Đề nghị, nhưng vẫn gửi field để giữ logic update ResultCDHA cũ.
            var description = '';
            var result = '';
            var suggest = '';

            if (window.CKEDITOR && CKEDITOR.instances && CKEDITOR.instances['tdcn_process_description']) {
                description = CKEDITOR.instances['tdcn_process_description'].getData() || '';
            } else if ($('#tdcn_process_description').length) {
                description = $('#tdcn_process_description').val() || '';
            }

            if ($('#tdcn_process_result').length) {
                result = $('#tdcn_process_result').val() || '';
            }

            if ($('#tdcn_process_suggest').length) {
                suggest = $('#tdcn_process_suggest').val() || '';
            }

            var fd = new FormData();
            fd.append('patientId', patientId);
            fd.append('resultCDHAId', resultCDHAId);
            fd.append('returnResultTime', returnResultTime || '');
            fd.append('userReturnResult', userReturnResult || '');
            fd.append('description', description);
            fd.append('result', result);
            fd.append('suggest', suggest);
            fd.append('resultPdf', pdfFile, pdfFile.name || 'tdcn-result.pdf');

            $('#showWaitting').modal('show');

            $.ajax({
                url: "/TDCN_Process/ValidPrintUploadedPdf/",
                data: fd,
                processData: false,
                contentType: false,
                dataType: "text",
                type: "POST",
                success: function (response) {
                    $('#showWaitting').modal('hide');
                    if (!response) {
                        SwalHelper.Toast.error("Valid & In không thành công. Vui lòng kiểm tra lại!");
                        return;
                    }

                    Process_Refresh();
                    Process_Get_Count();

                    try {
                        var pureBase64 = response.indexOf('base64,') >= 0
                            ? response.substring(response.indexOf('base64,') + 7)
                            : response;
                        var byteCharacters = atob(pureBase64);
                        var byteNumbers = new Array(byteCharacters.length);
                        for (var i = 0; i < byteCharacters.length; i++) {
                            byteNumbers[i] = byteCharacters.charCodeAt(i);
                        }
                        var byteArray = new Uint8Array(byteNumbers);
                        var file = new Blob([byteArray], { type: 'application/pdf' });
                        var fileURL = URL.createObjectURL(file);
                        window.open(fileURL, '_blank');
                    } catch (e) {
                        SwalHelper.Toast.error("Không thể mở file PDF sau khi Valid.");
                    }
                },
                error: function () {
                    $('#showWaitting').modal('hide');
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại!");
                }
            });
        },
        error: function () {
            SwalHelper.Toast.error("Không thể kiểm tra thiết bị. Vui lòng kiểm tra lại!");
        }
    });
}

function Process_GetSample() {
    var id = $('#tdcn_process_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        $.ajax({
            url: "/TDCN_Process/GetSample?id= " + id,
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
        url: "/TDCN_Process/Get_Count/",
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

// ============================== PROCESS SERVICE AUTO MODE ==============================
// Click dịch vụ ở bất kỳ bảng nào cũng đi qua Process_CheckedBoxOnRow.
// Hàm này load dữ liệu đã lưu trước, sau đó tự quyết định mode:
// - Có Description hoặc Result  => mode normal
// - Không có Description/Result => mode uploadPdf
function Process_CheckedBoxOnRow(id) {
    if (!id) {
        SwalHelper.Toast.warning("Không xác định được dịch vụ!");
        return false;
    }

    // Luôn select lại dòng ở bảng normal trước để các hàm Save/Valid normal lấy đúng resultCDHAId.
    TDCN_Process_SelectNormalServiceOnly(id);

    // Quan trọng: không quyết định mode bằng UI đang visible, mà quyết định bằng data thật của dịch vụ.
    Process_LoadImageForService(id);
    return true;
}

function TDCN_Process_ApplyResultMode(mode) {
    mode = mode === 'uploadPdf' ? 'uploadPdf' : 'normal';

    if (typeof window.TDCN_Process_SetResultMode === 'function') {
        window.TDCN_Process_SetResultMode(mode);
        return;
    }

    // Fallback nếu script trong _TDCN_Process_ListService_Custom chưa được execute.
    window.TDCN_PROCESS_RESULT_MODE = mode;
    try { localStorage.setItem('TDCN_PROCESS_RESULT_MODE', mode); } catch (e) { }
    $('#tdcn_process_result_mode').val(mode);

    if (mode === 'uploadPdf') {
        $('#tdcn-process-mode-normal').hide();
        $('#tdcn-process-mode-upload-pdf').show();
        $('#tdcn_mode_btn_normal').removeClass('btn-primary active').addClass('btn-outline-primary');
        $('#tdcn_mode_btn_upload_pdf').removeClass('btn-outline-primary').addClass('btn-primary active');
    } else {
        $('#tdcn-process-mode-upload-pdf').hide();
        $('#tdcn-process-mode-normal').show();
        $('#tdcn_mode_btn_upload_pdf').removeClass('btn-primary active').addClass('btn-outline-primary');
        $('#tdcn_mode_btn_normal').removeClass('btn-outline-primary').addClass('btn-primary active');
    }
}

function TDCN_Process_SelectNormalServiceOnly(resultId) {
    TDCN_Process_SetSharedServiceSelection(resultId);
}

function TDCN_Process_SyncUploadPdfServiceSelection(resultId, row) {
    TDCN_Process_SetSharedServiceSelection(resultId);
}

function TDCN_Process_GetFirstResultPayload(response) {
    if (!response) return null;
    if ($.isArray(response)) {
        return response.length > 0 ? response[0] : null;
    }
    return response;
}

function TDCN_Process_NormalizeText(value) {
    return (value === null || value === undefined) ? '' : String(value).trim();
}

function TDCN_Process_HasNormalResultData(response) {
    var payload = TDCN_Process_GetFirstResultPayload(response);
    if (!payload) return false;

    var description = TDCN_Process_NormalizeText(payload.description || payload.Description);
    var result = TDCN_Process_NormalizeText(payload.result || payload.Result);

    return description !== '' || result !== '';
}

function TDCN_Process_SetNormalEditorValue(description, result, suggest) {
    description = description || '';
    result = result || '';
    suggest = suggest || '';

    if (window.CKEDITOR && CKEDITOR.instances && CKEDITOR.instances['tdcn_process_description']) {
        CKEDITOR.instances['tdcn_process_description'].setData(description);
    } else {
        $('#tdcn_process_description').val(description);
    }

    $('#tdcn_process_result').val(result);
    $('#tdcn_process_suggest').val(suggest);
}

function TDCN_Process_ClearUploadPdfPreviewMessage(message) {
    $('#tdcn_pdf_preview_embed').attr('src', '').hide();
    $('#tdcn_pdf_preview_empty')
        .html(message || '<i class="bi bi-file-earmark-pdf"></i><div>Chọn file PDF để preview kết quả.</div>')
        .show();
}

function TDCN_Process_SetUploadPdfPreview(url, fileName, keyResultForHis) {
    if (!url) {
        TDCN_Process_ClearUploadPdfPreviewMessage('<i class="bi bi-file-earmark-pdf"></i><div>Dịch vụ này chưa có file PDF đã lưu.</div>');
        return;
    }

    var cacheBustUrl = url + (url.indexOf('?') >= 0 ? '&' : '?') + 't=' + new Date().getTime();
    window.TDCN_UPLOAD_PDF_PREVIEW_URL = cacheBustUrl;
    $('#tdcn_pdf_preview_embed').attr('src', cacheBustUrl).show();
    $('#tdcn_pdf_preview_empty').hide();

    if (fileName) {
        $('#tdcn_pdf_file_name').text(fileName);
        $('#tdcn_pdf_file_size').text(keyResultForHis || 'PDF đã lưu');
        $('#tdcn_pdf_file_info').show();
    }
}

function TDCN_Process_LoadUploadPdfPreviewForService(resultCDHAId) {
    if (!resultCDHAId) {
        TDCN_Process_ClearUploadPdfPreviewMessage('<i class="bi bi-file-earmark-pdf"></i><div>Không xác định được dịch vụ cần preview PDF.</div>');
        return;
    }

    TDCN_Process_ClearUploadPdfPreviewMessage('<i class="bi bi-hourglass-split"></i><div>Đang kiểm tra file PDF đã lưu...</div>');

    $.ajax({
        url: '/TDCN_ReturnResult/GetValidatedPdf',
        type: 'GET',
        dataType: 'json',
        data: { resultCDHAId: resultCDHAId },
        success: function (res) {
            if (!res || res.success !== true) {
                TDCN_Process_ClearUploadPdfPreviewMessage('<i class="bi bi-file-earmark-pdf"></i><div>' + ((res && res.message) ? res.message : 'Dịch vụ này chưa có file PDF đã lưu. Vui lòng chọn file PDF để upload.') + '</div>');
                return;
            }

            TDCN_Process_SetUploadPdfPreview(res.url, res.fileName, res.keyResultForHis);
        },
        error: function () {
            TDCN_Process_ClearUploadPdfPreviewMessage('<i class="bi bi-file-earmark-pdf"></i><div>Không tải được file PDF đã lưu. Vui lòng chọn file PDF để upload.</div>');
        }
    });
}

function TDCN_Process_ShowUploadPdfModeForService(resultCDHAId) {
    TDCN_Process_ApplyResultMode('uploadPdf');
    TDCN_Process_SyncUploadPdfServiceSelection(resultCDHAId);

    // Khi chuyển dịch vụ, clear file local cũ để tránh lưu nhầm PDF của dịch vụ trước.
    if (typeof window.TDCN_Process_ClearUploadPdf === 'function') {
        window.TDCN_Process_ClearUploadPdf();
    } else {
        window.TDCN_UPLOAD_PDF_FILE = null;
        $('#tdcn_process_result_pdf').val('');
        $('#tdcn_pdf_file_info').hide();
    }

    TDCN_Process_LoadUploadPdfPreviewForService(resultCDHAId);
}

function Process_LoadImageForService(id) {
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    TDCN_Process_SetNormalEditorValue('', '', '');

    $.ajax({
        url: "/TDCN_Process/Get_Description_Result_Suggest_ForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var hasNormalData = TDCN_Process_HasNormalResultData(response);
            var payload = TDCN_Process_GetFirstResultPayload(response) || {};

            if (hasNormalData) {
                TDCN_Process_ApplyResultMode('normal');
                TDCN_Process_SelectNormalServiceOnly(id);

                $.each(($.isArray(response) ? response : [response]), function (key, value) {
                    if (value && value.id) {
                        const $hinh = $('<div class="hinh"></div>');
                        $hinh.append('<img id="' + value.id + '" src="' + value.name + '" onclick="Process_Hinh(' + value.id + ', this)"/>');
                        $containHinh.append($hinh);
                    }
                });

                TDCN_Process_SetNormalEditorValue(
                    payload.description || payload.Description || '',
                    payload.result || payload.Result || '',
                    payload.suggest || payload.Suggest || ''
                );
                return;
            }

            // Không có mô tả/kết luận => coi là dịch vụ upload PDF.
            TDCN_Process_ShowUploadPdfModeForService(id);
        },
        error: function () {
            SwalHelper.Toast.error('Không tải được dữ liệu kết quả của dịch vụ.');
        }
    });
}

// Fallback nếu script trong partial UploadPdf không được execute khi partial được inject bằng ajax.
if (typeof window.TDCN_Process_SelectUploadPdfService !== 'function') {
    window.TDCN_Process_SelectUploadPdfService = function (resultId, row) {
        TDCN_Process_SyncUploadPdfServiceSelection(resultId, row);
    };
}

// Danh sách dịch vụ đã được gom về một bảng duy nhất #tdcn_list_service.
// Click row dùng inline Process_CheckedBoxOnRow(id), không cần event riêng cho upload table.
// ============================ END PROCESS SERVICE AUTO MODE ============================

function GetSampleForService(id) {
    $.ajax({
        url: "/TDCN_Process/GetSampleForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            CKEDITOR.instances["tdcn_process_description"].setData(response.description);
            $("#tdcn_process_result").val(response.result);
            $("#tdcn_process_suggest").val(response.suggest);
        }
    });
}

function Process_Hinh(imageCDHAId, img) {
    $('.contain-hinh img').removeClass('selected');
    $(img).toggleClass('selected');
    $("#tdcn_process_delete").val(imageCDHAId);
    $("#tdcn_process_xem").val(imageCDHAId);
}

function UploadHinh(fileInput) {
    if (fileInput.files.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn file!");
        return;
    }

    var resultCDHAId = TDCN_Process_GetSelectedServiceId();

    if (resultCDHAId) {

        const formData = new FormData();
        formData.append("file", fileInput.files[0]);
        formData.append("resultCDHAId", resultCDHAId);

        $.ajax({
            url: "/TDCN_Process/UploadHinh/",
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
    var imageCDHAId = $("#tdcn_process_delete").val();
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
                    url: "/TDCN_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
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
    //        url: "/TDCN_Process/DeleteImageCDHA?imageCDHAId=" + imageCDHAId,
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
    var userId = $("#tdcn_process_userReturnResultTDCN").val();
    if (!userId) {
        $("#tdcn_process_signerCCCD").empty();
        $("#tdcn_process_doctorInfo").empty();
        return;
    }
    console.log(userId);
    $.ajax({
        url: "/TDCN_Process/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            if (!u) {
                $("#tdcn_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
                return;
            }
            console.log(u);
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#tdcn_process_signerCCCD").val(cccd);
        },
        error: function () {
            $("#tdcn_process_signerCCCD").text("Không lấy được thông tin bác sĩ.");
        }
    });
}
function Process_SignPdf_Multi(mode) {
    // mode: 'current' (mặc định), 'selected', 'all'
    mode = mode || 'current';
    var patientId = $('#tdcn_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Thu thập danh sách resultIds theo mode
    var resultIds = [];
    if (mode === 'current') {
        var rid = TDCN_Process_GetSelectedServiceId();
        if (rid) resultIds.push(rid);
    } else if (mode === 'selected') {
        resultIds = TDCN_Process_GetSelectedServiceIds();
    } else if (mode === 'all') {
        $('#tdcn_list_service .tdcn-chk-service').each(function () {
            var v = $(this).val();
            if (v) resultIds.push(v);
        });
    }

    if (resultIds.length === 0) {
        SwalHelper.Toast.warning("Vui lòng chọn ít nhất 1 dịch vụ để ký.");
        return;
    }

    // Lấy CCCD người ký (bác sĩ)
    var signerCCCD = $('#tdcn_process_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#tdcn_process_signerCCCD').focus();
        return;
    }
    console.log(signerCCCD);

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#tdcn_process_patientId').val() || $('#tdcn_process_sid').val() || "";
    var patientMaBenhAn = $('#tdcn_process_maBenhAn').val() || $('#tdcn_process_sid').val() || "";
    var patientName = $('#tdcn_process_patientName').val() || "";
    var doctorName = $('#tdcn_process_userReturnResultTDCN option:selected').text() || $('#tdcn_process_userReturnResultTDCN').val() || "";
    var doctorId = $('#tdcn_process_userReturnResultTDCN').val();
    var performedAt = $('#tdcn_process_returnResultTime').val() || ""; // thời điểm trả kết quả
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
                        url: "/TDCN_Process/ValidPrintMultiple/",
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
                    returnResultTime: $('#tdcn_process_returnResultTime').val(),
                    userReturnResult: $('#tdcn_process_userReturnResultTDCN').val(),
                    description: CKEDITOR.instances['tdcn_process_description'].getData(),
                    result: $('#tdcn_process_result').val(),
                    suggest: $('#tdcn_process_suggest').val()
                };
                console.log(resultIds[0], "====>", dataExport);
                $.ajax({
                    url: "/TDCN_Process/ValidPrint/",
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
                                    url: "/TDCN_Process/SaveSignStoreIdForResultCDHA",
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
function TDCN_TestPortalLink() {
    var patientId = $('#tdcn_process_id').val();
    var maBenhAn = $('#tdcn_process_maBenhAn').val(); // Hidden field we'll add

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
    var fromDate = document.getElementById('tdcn_process_timeSearchFrom').value;
    var toDate = document.getElementById('tdcn_process_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/tdcn_Process/SaveSearchDates',
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
$(document).on('change', '#tdcn_chk_all', function () {
    var checked = this.checked === true;
    // chỉ tick các dịch vụ đang hiển thị
    $('#tdcn_list_service').find('.tdcn-chk-service').prop('checked', checked);

    // Ẩn/hiện nút Lưu dựa trên trạng thái check all
    //if (checked) {
    //    $('#tdcn_process_saveresult').hide();
    //} else {
    //    $('#tdcn_process_saveresult').show();
    //}
});

// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả"
$(document).on('change', '.tdcn-chk-service', function () {
    var $rows = $('#tdcn_list_service').find('.tdcn-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Nếu tất cả đều check => check header; ngược lại bỏ check header
    var allChecked = total > 0 && marked === total;
    $('#tdcn_chk_all').prop('checked', allChecked);

    //// Ẩn/hiện nút Lưu dựa trên trạng thái check all
    //if (allChecked) {
    //    $('#tdcn_process_saveresult').hide();
    //} else {
    //    $('#tdcn_process_saveresult').show();
    //}
});

// ============================== IMPORT kết quả NGOÀI (PDF) ==============================

// Mở modal + đổ patientId hiện chọn
var currentImportedExternalFileId = 0;
var currentImportedExternalFileName = '';
function Process_ShowImportModal() {
    var pid = $('#tdcn_process_patientId').val();
    var pidTablePatient = $('#tdcn_process_id').val();
    var pName = $('#tdcn_process_patientName').val();
    var pMaBenhAn = $('#tdcn_process_maBenhAn').val();

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
    $.get('/TDCN_GetSample/GetExternalResultFiles', { pMaBenhAn: pMaBenhAn, pId: pId, pidTablePatient: pidTablePatient }, function (res) {
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
        url: '/TDCN_GetSample/DeleteExternalResultFile',
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

    $.post('/TDCN_GetSample/SaveExternalResultFileVisibility', {
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
        url: "/TDCN_GetSample/ImportExternalResultPdf",
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
// ============================================================================
// TDCN Process - SaveResultAndGenPDF + Đã thực hiện
// Dán vào tdcn.process.js sau nhóm PROCESS MODE WRAPPERS hoặc trước Process_SaveResult().
// ============================================================================

function TDCN_Process_GetSelectedResultIdForNormal() {
    if (typeof TDCN_Process_GetSelectedServiceId === 'function') {
        return TDCN_Process_GetSelectedServiceId();
    }

    var resultCDHAId = '';
    $('#tdcn_list_service .tdcn-chk-service:checked').each(function () {
        if (!resultCDHAId) resultCDHAId = $(this).val();
    });

    if (!resultCDHAId) {
        $('.row-service .form-check-input:checked').each(function () {
            if (!resultCDHAId) resultCDHAId = $(this).val();
        });
    }

    return resultCDHAId;
}

function TDCN_Process_BuildNormalResultPayload() {
    var patientId = $('#tdcn_process_id').val();
    var resultCDHAId = TDCN_Process_GetSelectedResultIdForNormal();
    var returnResultTime = $('#tdcn_process_returnResultTime').val();
    var userReturnResult = $('#tdcn_process_userReturnResultTDCN').val();

    var description = '';
    if (window.CKEDITOR && CKEDITOR.instances && CKEDITOR.instances['tdcn_process_description']) {
        description = CKEDITOR.instances['tdcn_process_description'].getData() || '';
    } else if ($('#tdcn_process_description').length) {
        description = $('#tdcn_process_description').val() || '';
    }

    return {
        patientId: patientId,
        resultCDHAId: resultCDHAId,
        returnResultTime: returnResultTime,
        userReturnResult: userReturnResult,
        description: description,
        result: $('#tdcn_process_result').val() || '',
        suggest: $('#tdcn_process_suggest').val() || ''
    };
}

// ============================================================================
// TDCN Process - SaveResultAndGenPDF theo mode
// Button "Lưu & tạo PDF" gọi hàm này.
// - mode normal: gọi /TDCN_Process/SaveResultAndGenPDF/ để lưu mô tả/kết luận và render PDF
// - mode uploadPdf: gọi Process_SavePDFResult() để lưu chính file PDF user upload qua /TDCN_Process/SavePDFResult/
// ============================================================================

function TDCN_Process_GetSelectedResultIdForSaveGenPdf() {
    if (typeof TDCN_Process_GetSelectedServiceId === 'function') {
        var sharedId = TDCN_Process_GetSelectedServiceId();
        if (sharedId) return sharedId;
    }

    var hiddenUploadId = $('#tdcn_upload_pdf_resultCDHAId').val();
    if (hiddenUploadId) return hiddenUploadId;

    var resultCDHAId = '';

    $('#tdcn_list_service .tdcn-chk-service:checked').each(function () {
        if (!resultCDHAId) resultCDHAId = $(this).val();
    });

    if (!resultCDHAId) {
        $('#tdcn_upload_pdf_list_service .tdcn-upload-pdf-chk-service:checked').each(function () {
            if (!resultCDHAId) resultCDHAId = $(this).val();
        });
    }

    if (!resultCDHAId) {
        $('.row-service .form-check-input:checked').each(function () {
            if (!resultCDHAId) resultCDHAId = $(this).val();
        });
    }

    return resultCDHAId;
}

function TDCN_Process_BuildNormalResultPayloadForSaveGenPdf() {
    var description = '';
    if (window.CKEDITOR && CKEDITOR.instances && CKEDITOR.instances['tdcn_process_description']) {
        description = CKEDITOR.instances['tdcn_process_description'].getData() || '';
    } else if ($('#tdcn_process_description').length) {
        description = $('#tdcn_process_description').val() || '';
    }

    return {
        patientId: $('#tdcn_process_id').val(),
        resultCDHAId: TDCN_Process_GetSelectedResultIdForSaveGenPdf(),
        returnResultTime: $('#tdcn_process_returnResultTime').val(),
        userReturnResult: $('#tdcn_process_userReturnResultTDCN').val(),
        description: description,
        result: $('#tdcn_process_result').val() || '',
        suggest: $('#tdcn_process_suggest').val() || ''
    };
}
function Process_SaveResultAndGenPDF() {
    var patientId = $('#tdcn_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
        return false;
    }

    var validate = Process_ValidateInput('process_patientInfo');
    if (!validate) return false;

    // Quan trọng: cùng một button nhưng phải chạy đúng theo mode hiện tại.
    // Upload PDF không được gọi SaveResultAndGenPDF vì endpoint đó render PDF từ description/result/suggest.
    if (typeof TDCN_Process_IsUploadPdfMode === 'function' && TDCN_Process_IsUploadPdfMode()) {
        return Process_SavePDFResult();
    }

    var data = TDCN_Process_BuildNormalResultPayloadForSaveGenPdf();
    if (!data.resultCDHAId) {
        SwalHelper.Toast.warning('Vui lòng chọn dịch vụ!');
        return false;
    }

    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: '/TDCN_Process/SaveResultAndGenPDF/',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        success: function (res) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }

            if (!res || res.success !== true) {
                SwalHelper.Toast.error(res && res.message ? res.message : 'Lưu kết quả và tạo PDF không thành công.');
                return;
            }

            SwalHelper.Toast.success('Đã lưu kết quả và tạo PDF thành công.');

            // Nếu controller trả URL PDF thì preview luôn file vừa tạo.
            if (typeof TDCN_Process_SetUploadPdfPreview === 'function' && res.url) {
                TDCN_Process_SetUploadPdfPreview(res.url, res.fileName, res.keyResultForHis);
            }
        },
        error: function () {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            SwalHelper.Toast.error('Không thể kết nối server để lưu kết quả và tạo PDF.');
        }
    });

    return true;
}

function TDCN_Process_GetAllServiceIdsForPatient() {
    var ids = [];
    var hiddenIds = ($('#tdcn_all_result_cdha_ids').val() || '').toString().trim();

    if (hiddenIds) {
        hiddenIds.split(',').forEach(function (value) {
            value = (value || '').toString().trim();
            if (value && ids.indexOf(value) === -1) ids.push(value);
        });
    }

    // Fallback nếu template chưa có hidden input hoặc đang dùng template cũ.
    if (ids.length === 0) {
        $('#tdcn_list_service .tdcn-chk-service').each(function () {
            var value = ($(this).val() || '').toString().trim();
            if (value && ids.indexOf(value) === -1) ids.push(value);
        });
    }

    if (ids.length === 0) {
        $('#tdcn_list_service .row-service').each(function () {
            var value = ($(this).data('result-id') || '').toString().trim();
            if (value && ids.indexOf(value) === -1) ids.push(value);
        });
    }

    return ids;
}

function TDCN_Process_BuildAllResultPayloadForMarkDone() {
    var patientId = $('#tdcn_process_id').val();
    var returnResultTime = $('#tdcn_process_returnResultTime').val();
    var userReturnResult = $('#tdcn_process_userReturnResultTDCN').val();
    var resultIds = TDCN_Process_GetAllServiceIdsForPatient();

    return resultIds.map(function (resultId) {
        return {
            patientId: patientId,
            resultCDHAId: resultId,
            returnResultTime: returnResultTime,
            userReturnResult: userReturnResult,
            description: '',
            result: '',
            suggest: ''
        };
    });
}

function Process_MarkPatientAsDone() {
    var patientId = $('#tdcn_process_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
        return false;
    }

    var validate = Process_ValidateInput('process_patientInfo');
    if (!validate) return false;

    var dataList = TDCN_Process_BuildAllResultPayloadForMarkDone();
    if (!dataList || dataList.length === 0) {
        SwalHelper.Toast.warning('Bệnh nhân chưa có dịch vụ TDCN để cập nhật.');
        return false;
    }

    if (!confirm('Xác nhận chuyển bệnh nhân sang trạng thái Đã thực hiện?')) {
        return false;
    }

    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: '/TDCN_Process/MarkPatientAsDone/',
        data: JSON.stringify(dataList),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        success: function (res) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }

            if (!res || res.success !== true) {
                SwalHelper.Toast.error(res && res.message ? res.message : 'Cập nhật trạng thái không thành công.');
                return;
            }

            SwalHelper.Toast.success(res.message || ('Đã cập nhật trạng thái Đã thực hiện cho ' + dataList.length + ' dịch vụ.'));
            Process_Refresh();
            Process_Get_Count();
        },
        error: function () {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            SwalHelper.Toast.error('Không thể kết nối server để cập nhật trạng thái.');
        }
    });

    return true;
}

