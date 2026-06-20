// ============================================================================
// TDCN GetSample JS
// Tách từ tdcn.js hiện tại, giữ nguyên tên hàm global để Razor onclick cũ không bị gãy.
// ============================================================================

// *********************************************************************************** Get sample

// Kiểm tra nhập liệu trên form => Trường nào có class = valid thì sẽ kiểm tra rỗng và bắt nhập
function GetSample_ValidateInput(id) {
    var flag = true;
    $("#" + id).find(".valid").each(function (i, e) {
        var value = $(e).val();
        if (value === "" || !value) {
            if (flag) {
                if (e.id === "tdcn_getsample_location") {
                    $('#tdcn_getsample_location').select2('focus');
                }
                else if (e.id === "tdcn_getsample_doctor") {
                    $('#tdcn_getsample_doctor').select2('focus');
                }
                else if (e.id === "tdcn_getsample_userReturnResultTDCN") {
                    $('#tdcn_getsample_userReturnResultTDCN').select2('focus');
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
        url: "/TDCN_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#tdcn_getsample_timeSearchFrom").val(today);
            //$("#tdcn_getsample_timeSearchTo").val(today);
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
        url: "/TDCN_GetSample/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#tdcn_getsample_timeSearchFrom").val(today);
            $("#tdcn_getsample_timeSearchTo").val(today);
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function GetSample_Search() {
    var tdcn_getsample_pidorseq = $("#tdcn_getsample_pidorseq").val();
    var timeSearchFrom = $("#tdcn_getsample_timeSearchFrom").val();
    var timeSearchTo = $("#tdcn_getsample_timeSearchTo").val();
    $.ajax({
        url: "/TDCN_GetSample/Search?" + "pidorseq=" + tdcn_getsample_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
$("#tdcn_getsample_savepatient").hide();
$("#tdcn_getsample_cancelpatient").hide();
function GetSample_HideButton(_new, _save, _delete, _cancel, _addservice, _getsample) {
    if (_new == 1) {
        $("#tdcn_getsample_newpatient").hide();
    }
    else {
        $("#tdcn_getsample_newpatient").show();
    }

    if (_save == 1) {
        $("#tdcn_getsample_savepatient").hide();
    }
    else {
        $("#tdcn_getsample_savepatient").show();
    }

    if (_delete == 1) {
        $("#tdcn_getsample_deletepatient").hide();
    }
    else {
        $("#tdcn_getsample_deletepatient").show();
    }

    if (_cancel == 1) {
        $("#tdcn_getsample_cancelpatient").hide();
    }
    else {
        $("#tdcn_getsample_cancelpatient").show();
    }

    if (_addservice == 1) {
        $("#tdcn_getsample_addService").hide();
    }
    else {
        $("#tdcn_getsample_addService").show();
    }

    if (_getsample == 1) {
        $("#tdcn_getsample_processresult").hide();
    }
    else {
        $("#tdcn_getsample_processresult").show();
    }
}


function GetSample_GetPatientInfo(id) {
    GetSample_HideButton(false, true, false, true, false, false);
    $.ajax({
        url: "/TDCN_GetSample/GetPatientInfo?id=" + id,
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
        url: "/TDCN_GetSample/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#tdcn-right-service-gridview").html(result);
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
    $("#tdcn_getsample_pidorseq").val('');
    $('#tdcn_getsample_id').val('');
    $('#tdcn_getsample_patientId').val('');
    $('#tdcn_getsample_seq').val('');
    $('#tdcn_getsample_sid').val('');
    $('#tdcn_getsample_patientName').val('');
    $('#tdcn_getsample_age').val('');
    $('#tdcn_getsample_sex').val('');
    $('#tdcn_getsample_obj').val('');
    $('#tdcn_getsample_type').val('');
    $('#tdcn_getsample_location').val('');
    $('#tdcn_getsample_doctor').val('');
    $('#tdcn_getsample_getSampleTime').val(dateTime);
    $('#tdcn_getsample_address').val('');
    $('#tdcn_getsample_diagnostic').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
    GetSample_SetSelect2_01(false);
}

function GetSample_NewPatient() {
    $('#tdcn_getsample_patientId').focus();
    GetSample_HideButton(true, false, true, false, true, true);
    GetSample_ResetInput();
}


function GetSample_DeletePatient() {
    var id = $('#tdcn_getsample_id').val();
    if (id == '') {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var choice = confirm("Bạn muốn xoá bệnh nhân và tất cả chỉ định xét nghiệm?");
        if (choice) {
            $.ajax({
                url: "/TDCN_GetSample/DeletePatientAndService?id=" + id,
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
    var id = $('#tdcn_getsample_id').val();
    if (id == '') {
        GetSample_ResetInput()
    }
    else {
        GetSample_GetPatientInfo(id);
    }
}

function GetSample_GetSID(seq) {
    $.ajax({
        url: "/TDCN_GetSample/GetSID?seq=" + seq,
        type: "GET",
        dataType: "text",
        cache: false,
        success: function (result) {
            $('#tdcn_getsample_sid').val(result);
        }
    });
}

function GetSample_SavePatient() {
    var validate = GetSample_ValidateInput('patientInfo');
    if (validate) {
        var id = $('#tdcn_getsample_id').val();
        var patientId = $('#tdcn_getsample_patientId').val();
        var seq = $('#tdcn_getsample_seq').val();
        var sid = $('#tdcn_getsample_sid').val();
        var patientName = $('#tdcn_getsample_patientName').val();
        var age = $('#tdcn_getsample_age').val();
        var sex = $('#tdcn_getsample_sex').val();
        var obj = $('#tdcn_getsample_obj').val();
        var type = $('#tdcn_getsample_type').val();
        var location = $('#tdcn_getsample_location').val();
        var doctor = $('#tdcn_getsample_doctor').val();
        var getSampleTime = $('#tdcn_getsample_getSampleTime').val();
        var address = $('#tdcn_getsample_address').val();
        var diagnostic = $('#tdcn_getsample_diagnostic').val();
        var category = $('#select-category').val();
        var service = $('#select-service').val();

        $.ajax({
            url: "/TDCN_GetSample/SavePatient?id= " + id + "&&patientId=" + patientId + "&&seq=" + seq + "&&sid=" + sid + "&&patientName=" + patientName + "&&age=" + age + "&&sex=" + sex + "&&obj=" + obj + "&&type=" + type + "&&location=" + location + "&&doctor=" + doctor + "&&getSampleTime=" + getSampleTime + "&&address=" + address + "&&diagnostic=" + diagnostic + "&&service=" + service,
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
    var id = $('#tdcn_getsample_id').val();
    if (id === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        var getSampleTime = $('#tdcn_getsample_getSampleTime').val();
        $.ajax({
            url: "/TDCN_GetSample/ProcessResult?id= " + id + "&getSampleTime=" + getSampleTime,
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
        url: "/TDCN_GetSample/ProcessResultAll",
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
function GetSample_AddService() {
    var id = $('#tdcn_getsample_id').val();
    if (id === "") {
        $('#addServiceForm').modal('hide');
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }
    else {
        GetSample_SetSelect2_02();
    }
}

function GetSample_DeleteServiceForPatient(idResultCDHA) {
    var patientId = $('#tdcn_getsample_id').val();
    $.ajax({
        url: "/TDCN_GetSample/DeleteServiceForPatient?id=" + idResultCDHA,
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
    var serviceId = $('#tdcn_getsample_service').val();
    var patientId = $('#tdcn_getsample_id').val();
    var doctorId = $('#tdcn_getsample_doctor').val();
    if (serviceId === "" || patientId === "") {
        SwalHelper.Toast.warning("Chỉ định dịch vụ không thành công. Vui lòng kiểm tra lại !")
    }
    else {
        $.ajax({
            url: "/TDCN_GetSample/AddServiceForPatient?patientId=" + patientId + "&&serviceId=" + serviceId + "&&doctorId=" + doctorId,
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
        url: "/TDCN_GetSample/Get_Count/",
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
    var fromDate = document.getElementById('tdcn_getsample_timeSearchFrom').value;
    var toDate = document.getElementById('tdcn_getsample_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/tdcn_GetSample/SaveSearchDates',
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
    var pid = $('#tdcn_getsample_patientId').val();
    var pidTablePatient = $('#tdcn_getsample_id').val();
    var pName = $('#tdcn_getsample_patientName').val();
    var pMaBenhAn = $('#tdcn_getsample_maBenhAn').val();

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
    $.get('/TDCN_GetSample/GetExternalResultFiles', { pMaBenhAn: pMaBenhAn, pId: pId, patientId: patientId }, function (res) {
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
