// ============================================================================
// TDCN ReturnResult JS
// Tách từ tdcn.js hiện tại, giữ nguyên tên hàm global để Razor onclick cũ không bị gãy.
// ============================================================================

//***************************************************************************************** Return Result

function ReturnResult_Refresh() {
    $.ajax({
        url: "/TDCN_ReturnResult/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            //var date = new Date();
            //var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            //$("#tdcn_returnresult_timeSearchFrom").val(today);
            //$("#tdcn_returnresult_timeSearchTo").val(today);
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
    var tdcn_process_pidorseq = $("#tdcn_returnresult_pidorseq").val();
    var timeSearchFrom = $("#tdcn_returnresult_timeSearchFrom").val();
    var timeSearchTo = $("#tdcn_returnresult_timeSearchTo").val();
    $.ajax({
        url: "/TDCN_ReturnResult/Search?" + "pidorseq=" + tdcn_process_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
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
    $("#tdcn_returnresult_pidorseq").val('');
    $('#tdcn_returnresult_id').val('');
    $('#tdcn_returnresult_patientId').val('');
    $('#tdcn_returnresult_seq').val('');
    $('#tdcn_returnresult_sid').val('');
    $('#tdcn_returnresult_patientName').val('');
    $('#tdcn_returnresult_age').val('');
    $('#tdcn_returnresult_sex').val('');
    $('#tdcn_returnresult_obj').val('');
    $('#tdcn_returnresult_type').val('');
    $('#tdcn_returnresult_location').val('');
    $('#tdcn_returnresult_doctor').val('');
    $('#tdcn_returnresult_getSampleTime').val('');
    $('#tdcn_returnresult_returnResultTime').val('');
    $('#tdcn_returnresult_location').val('');
    $('#tdcn_returnresult_doctor').val('');
    $('#tdcn_returnresult_userReturnResultTDCN').val('');
    $('#tdcn_returnresult_address').val('');
    $('#tdcn_returnresult_diagnostic').val('');
    $('#tbody-gridview-service').empty();
}

function ReturnResult_GetPatientInfo(id) {
    $.ajax({
        url: "/TDCN_ReturnResult/GetPatientInfo?id=" + id,
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
    var userId = $("#tdcn_returnresult_userLoginId").val();
    console.log(userId);
    if (!userId) {
        $("#tdcn_returnresult_signerCCCD").empty();
        return;
    }
    $.ajax({
        url: "/TDCN_ReturnResult/GetUserInfo?userId=" + userId,
        type: "GET",
        dataType: "json",
        success: function (u) {
            console.log(u);
            if (!u) {
                $("#tdcn_returnresult_signerCCCD").text("Không lấy được thông tin CCCD.");
                return;
            }
            // Hiển thị nhẹ ở UI
            var cccd = (u.cccd || "");
            $("#tdcn_returnresult_signerCCCD").val(cccd);
        },
        error: function () {
            $("#tdcn_returnresult_signerCCCD").text("Không lấy đượcthông tin CCCD.");
        },
    });
}
function ReturnResult_GetListServiceForPatient(id) {
    $.ajax({
        url: "/TDCN_ReturnResult/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#returnresult_tdcn-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function ReturnResult_Invalid() {
    var patientId = $('#tdcn_returnresult_id').val();

    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    // Lấy ResultCDHA.Id của các dịch vụ đang được chọn.
    // Backend sẽ tự lấy KeyResultForHis từ DB theo ResultCDHA.Id.
    var resultIds = [];

    $('.tdcn-returnresult-chk-service:checked').each(function () {
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
        url: "/TDCN_ReturnResult/Invalid",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify({
            patientId: parseInt(patientId, 10),
            resultIds: resultIds,
            categoryCode: "TDCN"
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
            SwalHelper.Toast.error(
                xhr.responseText || "Không thể Invalid. Vui lòng kiểm tra lại!"
            );
        }
    });
}

function ReturnResult_Invalid_RemoveDigitalSign() {
    var patientId = $('#tdcn_returnresult_id').val();

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

            // Giữ lại id để UpdateSignStatus chạy như luồng cũ.
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
        url: "/TDCN_ReturnResult/Invalid",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        data: JSON.stringify({
            patientId: parseInt(patientId, 10),
            resultIds: resultIds,
            categoryCode: "TDCN"
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
            SwalHelper.Toast.error(
                xhr.responseText || "Không thể Invalid. Vui lòng kiểm tra lại!"
            );
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
        url: "/tdcn_ReturnResult/Get_Count/",
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
    var patientId = $('#tdcn_returnresult_id').val();
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
        url: "/tdcn_ReturnResult/Print?resultCDHAId=" + resultCDHAId,
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
                    link.download = 'KetQua_tdcn_' + resultCDHAId + '_' + new Date().getTime() + '.pdf';
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
                        link.download = 'KetQua_tdcn_' + resultCDHAId + '.pdf';
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

/**
 * Đóng hàm phía dưới lại dùng cho version lấy Mô tả, kết quả, gợi ý theo dịch vụ
 * gồm các hàm:
 * - ReturnResult_CheckedBoxOnRow
 * - document.on('change', '.tdcn-returnresult-chk-service', ...)
 * - ReturnResult_HandleServiceSelectionChange
 */
//function ReturnResult_CheckedBoxOnRow(id, event) {
//    // Nếu click trực tiếp vào checkbox thì không xử lý (để người dùng tự chọn multiple)
//    if (event && event.target.type === 'checkbox') {
//        // Chỉ cập nhật trạng thái "chọn tất cả"
//        var $rows = $('.tdcn-returnresult-chk-service');
//        var total = $rows.length;
//        var marked = $rows.filter(':checked').length;
//        var allChecked = total > 0 && marked === total;
//        $('#tdcn_returnresult_chk_all').prop('checked', allChecked);

//        // Nếu chỉ có 1 checkbox được chọn sau khi click
//        var selectedCheckboxes = $(".row-service .form-check-input:checked");
//        if (selectedCheckboxes.length === 1) {
//            var $row = selectedCheckboxes.first().closest('.row-service');
//            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
//            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
//        } else if (selectedCheckboxes.length > 1) {
//            // Clear chi tiết khi chọn nhiều
//            const $containHinh = $('.contain-hinh');
//            $containHinh.empty();
//            CKEDITOR.instances["tdcn_returnresult_description"].setData("");
//            $("#tdcn_returnresult_result").val("");
//            $("#tdcn_returnresult_suggest").val("");
//            ReturnResult_ToggleDigitalSignButtons(false);
//        } else {
//            // Clear khi không có gì được chọn
//            const $containHinh = $('.contain-hinh');
//            $containHinh.empty();
//            CKEDITOR.instances["tdcn_returnresult_description"].setData("");
//            $("#tdcn_returnresult_result").val("");
//            $("#tdcn_returnresult_suggest").val("");
//            ReturnResult_ToggleDigitalSignButtons(false);
//        }
//        return;
//    }

//    // Click vào row (không phải checkbox) → chỉ chọn 1 dịch vụ và hiển thị chi tiết
//    $(".row-service .form-check-input").each(function () {
//        var $chk = $(this);
//        var resultCdhaId = String($chk.val());
//        var isTarget = (resultCdhaId === String(id));
//        $chk.prop("checked", isTarget);

//        if (isTarget) {
//            ReturnResult_LoadImageForService(id);

//            // Check digital signature status and toggle buttons
//            var $row = $chk.closest('.row-service');
//            var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
//            ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
//        }
//    });

//    // Update checkAll status
//    var $rows = $('.tdcn-returnresult-chk-service');
//    var total = $rows.length;
//    var marked = $rows.filter(':checked').length;
//    var allChecked = total > 0 && marked === total;
//    $('#tdcn_returnresult_chk_all').prop('checked', allChecked);
//}

// Event handler cho checkbox để chỉ xử lý việc chọn multiple


//$(document).on('change', '.tdcn-returnresult-chk-service', function () {
//    var $rows = $('.tdcn-returnresult-chk-service');
//    var total = $rows.length;
//    var marked = $rows.filter(':checked').length;

//    // Update trạng thái "chọn tất cả"
//    var allChecked = total > 0 && marked === total;
//    $('#tdcn_returnresult_chk_all').prop('checked', allChecked);

//    // Xử lý hiển thị chi tiết
//    var selectedCheckboxes = $(".row-service .form-check-input:checked");

//    if (selectedCheckboxes.length === 1) {
//        // Chỉ 1 được chọn → hiển thị chi tiết
//        var selectedId = selectedCheckboxes.first().val();
//        ReturnResult_LoadImageForService(selectedId);

//        var $row = selectedCheckboxes.first().closest('.row-service');
//        var hasDigitalSign = $row.find('.digital-sign-icon').length > 0;
//        ReturnResult_ToggleDigitalSignButtons(hasDigitalSign);
//    } else if (selectedCheckboxes.length > 1) {
//        // Nhiều hơn 1 → clear chi tiết
//        const $containHinh = $('.contain-hinh');
//        $containHinh.empty();
//        CKEDITOR.instances["tdcn_returnresult_description"].setData("");
//        $("#tdcn_returnresult_result").val("");
//        $("#tdcn_returnresult_suggest").val("");
//        ReturnResult_ToggleDigitalSignButtons(false);
//    } else {
//        // Không có gì được chọn → clear
//        const $containHinh = $('.contain-hinh');
//        $containHinh.empty();
//        CKEDITOR.instances["tdcn_returnresult_description"].setData("");
//        $("#tdcn_returnresult_result").val("");
//        $("#tdcn_returnresult_suggest").val("");
//        ReturnResult_ToggleDigitalSignButtons(false);
//    }
//});


/**
 * Hàm phía dưới dùng để lấy pdf theo từng dịch vụ, khi click vào checkbox
 * bao gồm 
 * - ReturnResult_CheckedBoxOnRow, 
 * - document.on('change', '.tdcn-returnresult-chk-service', ...),
 * - ReturnResult_HandleServiceSelectionChange
 * - ReturnResult_LoadValidatedPdfForService
 * - ReturnResult_ClearPdfPreview
 * - ReturnResult_SetPdfPreview
 */
function ReturnResult_CheckedBoxOnRow(id, event) {
    // Nếu click trực tiếp vào checkbox thì để checkbox tự đổi trạng thái, sau đó xử lý theo số lượng được chọn.
    if (event && event.target && event.target.type === 'checkbox') {
        setTimeout(function () {
            ReturnResult_HandleServiceSelectionChange();
        }, 0);
        return;
    }

    // Click vào row: chỉ chọn 1 dịch vụ và load PDF đã Valid.
    $('.row-service').removeClass('active');
    $('.row-service .form-check-input').each(function () {
        var $chk = $(this);
        var isTarget = String($chk.val()) === String(id);
        $chk.prop('checked', isTarget);
        if (isTarget) {
            $chk.closest('.row-service').addClass('active');
        }
    });

    $('#tdcn_returnresult_chk_all').prop('checked', false);
    ReturnResult_LoadValidatedPdfForService(id);
}
// Event handler cho checkbox để chỉ xử lý việc chọn multiple
$(document).on('change', '.tdcn-returnresult-chk-service', function () {
    ReturnResult_HandleServiceSelectionChange();
});

function ReturnResult_HandleServiceSelectionChange() {
    var $rows = $('.tdcn-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    $('#tdcn_returnresult_chk_all').prop('checked', total > 0 && marked === total);
    $('.row-service').removeClass('active');

    if (marked === 1) {
        var $only = $rows.filter(':checked').first();
        $only.closest('.row-service').addClass('active');
        ReturnResult_LoadValidatedPdfForService($only.val());
        return;
    }

    if (marked > 1) {
        ReturnResult_ClearPdfPreview('Đã chọn nhiều dịch vụ. Chọn một dịch vụ cụ thể để xem PDF kết quả.');
    } else {
        ReturnResult_ClearPdfPreview('Chọn một dịch vụ bên trái để xem lại file PDF kết quả.');
    }

    ReturnResult_ToggleDigitalSignButtons(false);
}
function ReturnResult_ClearPdfPreview(message) {
    $('#tdcn_returnresult_selected_result_id').val('');
    $('#tdcn_returnresult_signStoreId').val('');
    $('#tdcn_returnresult_pdf_meta').text(message || 'Chọn một dịch vụ bên trái để xem lại file PDF kết quả.');
    $('#tdcn_returnresult_pdf_preview').attr('src', '').hide();
    $('#tdcn_returnresult_pdf_empty').show();
    $('#tdcn_returnresult_pdf_open')
        .attr('href', 'javascript:void(0)')
        .addClass('disabled')
        .removeAttr('download');
    $('#tdcn_returnresult_pdf_reload').prop('disabled', true);
}

function ReturnResult_SetPdfPreview(url, fileName, keyResultForHis) {
    var cacheBustUrl = url + (url.indexOf('?') >= 0 ? '&' : '?') + 't=' + new Date().getTime();
    $('#tdcn_returnresult_pdf_preview').attr('src', cacheBustUrl).show();
    $('#tdcn_returnresult_pdf_empty').hide();
    $('#tdcn_returnresult_pdf_open')
        .attr('href', cacheBustUrl)
        .removeClass('disabled')
        .attr('download', fileName || 'ket-qua-tdcn.pdf');
    $('#tdcn_returnresult_pdf_reload').prop('disabled', false);
    $('#tdcn_returnresult_pdf_meta').text((keyResultForHis || '') + (fileName ? ' • ' + fileName : ''));
}

function ReturnResult_LoadValidatedPdfForService(resultCDHAId) {
    if (!resultCDHAId) {
        ReturnResult_ClearPdfPreview('Không xác định được dịch vụ cần xem PDF.');
        return;
    }

    $('#tdcn_returnresult_selected_result_id').val(resultCDHAId);
    $('#tdcn_returnresult_pdf_meta').text('Đang tải PDF kết quả...');
    $('#tdcn_returnresult_pdf_preview').attr('src', '').hide();
    $('#tdcn_returnresult_pdf_empty').show();
    $('#tdcn_returnresult_pdf_reload').prop('disabled', true);
    $('#tdcn_returnresult_pdf_open').attr('href', 'javascript:void(0)').addClass('disabled');

    $.ajax({
        url: '/TDCN_ReturnResult/GetValidatedPdf',
        type: 'GET',
        dataType: 'json',
        data: { resultCDHAId: resultCDHAId },
        success: function (res) {
            if (!res || res.success !== true) {
                $('#tdcn_returnresult_signStoreId').val('');
                ReturnResult_ClearPdfPreview(res && res.message ? res.message : 'Không tải được PDF kết quả.');
                ReturnResult_ToggleDigitalSignButtons(false);
                return;
            }

            $('#tdcn_returnresult_signStoreId').val(res.signStoreId || '');
            ReturnResult_SetPdfPreview(res.url, res.fileName, res.keyResultForHis);
            ReturnResult_ToggleDigitalSignButtons(res.isDigitallySigned === true);
        },
        error: function () {
            $('#tdcn_returnresult_signStoreId').val('');
            ReturnResult_ClearPdfPreview('Lỗi khi tải PDF kết quả. Vui lòng kiểm tra lại.');
            ReturnResult_ToggleDigitalSignButtons(false);
        }
    });
}

function ReturnResult_ReloadSelectedPdf() {
    var resultId = $('#tdcn_returnresult_selected_result_id').val();
    if (!resultId) {
        SwalHelper.Toast.warning('Vui lòng chọn dịch vụ để reload PDF.');
        return;
    }
    ReturnResult_LoadValidatedPdfForService(resultId);
}
function ReturnResult_LoadImageForService(id) {
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["tdcn_returnresult_description"].setData("");
    $("#tdcn_returnresult_result").val("");
    $("#tdcn_returnresult_suggest").val("");
    $.ajax({
        url: "/tdcn_ReturnResult/Get_Description_Result_Suggest_ForService?id=" + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $.each(response, function (key, value) {
                if (value.id) {
                    const $hinh = $('<div class="hinh"></div>');
                    $hinh.append('<img id="' + value.id + '" src="' + value.name + '" onclick="ReturnResult_Hinh(' + value.id + ', this)"/>');
                    $containHinh.append($hinh);
                }
                CKEDITOR.instances["tdcn_returnresult_description"].setData(value.description);
                $("#tdcn_returnresult_result").val(value.result);
                $("#tdcn_returnresult_suggest").val(value.suggest);
            });
        }
    });
}

// Helper function to toggle digital signature buttons
function ReturnResult_ToggleDigitalSignButtons(isDigitallySigned) {
    console.log('Digital signature status:', isDigitallySigned);

    // Toggle "Tải PDF đã ký" button
    var $printSignedBtn = $('#tdcn_returnresult_print_signed');
    if ($printSignedBtn.length) {
        if (isDigitallySigned) {
            $printSignedBtn.show().prop('disabled', false);
        } else {
            $printSignedBtn.hide().prop('disabled', true);
        }
    }

    // Toggle "Invalid & Xóa Ký Số" button  
    var $invalidRemoveSignBtn = $('#tdcn_returnresult_invalid_removedigitalsign');
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
    $("#tdcn_returnresult_xem").val(imageCDHAId);
}

function ReturnResult_ViewSignedPdf() {
    var signStoreId = $('#tdcn_returnresult_signStoreId').val();

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
        url: "/tdcn_ReturnResult/GetSignStoreId",
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
    var patientId = $('#tdcn_returnresult_id').val();
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
    var signerCCCD = $('#tdcn_returnresult_signerCCCD').val();
    if (!signerCCCD || $.trim(signerCCCD) === "") {
        SwalHelper.Toast.warning("Vui lòng nhập CCCD người ký (Viettel MySign)!");
        $('#tdcn_returnresult_signerCCCD').focus();
        return;
    }

    // Lấy context để backend đặt tên file đẹp theo template
    var patientCode = $('#tdcn_returnresult_patientId').val() || $('#tdcn_returnresult_sid').val() || "";
    var patientMaBenhAn = $('#tdcn_returnresult_maBenhAn').val() || $('#tdcn_returnresult_sid').val() || "";
    var patientName = $('#tdcn_returnresult_patientName').val() || "";
    var doctorName = $('#tdcn_returnresult_userReturnResultTDCN option:selected').text() || $('#tdcn_returnresult_userReturnResultTDCN').val() || "";
    var doctorId = $('#tdcn_returnresult_userLoginId').val();
    var performedAt = $('#tdcn_returnresult_returnResultTime').val() || ""; // thời điểm trả kết quả
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
                    url: "/tdcn_ReturnResult/Print?resultCDHAId=" + rid,
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
                                    url: "/tdcn_ReturnResult/SaveSignStoreIdForResultCDHA",
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
    var fromDate = document.getElementById('tdcn_returnresult_timeSearchFrom').value;
    var toDate = document.getElementById('tdcn_returnresult_timeSearchTo').value;

    if (fromDate && toDate) {
        // Save to session via AJAX call when dates change
        $.ajax({
            url: '/tdcn_ReturnResult/SaveSearchDates',
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
$(document).on('change', '#tdcn_returnresult_chk_all', function () {
    var checked = this.checked === true;
    // chỉ tick các dịch vụ đang hiển thị
    $('.tdcn-returnresult-chk-service').prop('checked', checked);

    // Nếu check all thì load kết quả của dịch vụ đầu tiên
    const $containHinh = $('.contain-hinh');
    $containHinh.empty();
    CKEDITOR.instances["tdcn_returnresult_description"].setData("");
    $("#tdcn_returnresult_result").val("");
    $("#tdcn_returnresult_suggest").val("");
    ReturnResult_ToggleDigitalSignButtons(false);
});

// Nếu người dùng tick/untick từng dịch vụ, đồng bộ lại trạng thái "chọn tất cả" cho ReturnResult
$(document).on('change', '.tdcn-returnresult-chk-service', function () {
    var $rows = $('.tdcn-returnresult-chk-service');
    var total = $rows.length;
    var marked = $rows.filter(':checked').length;

    // Nếu tất cả đều check => check header; ngược lại bỏ check header
    var allChecked = total > 0 && marked === total;
    $('#tdcn_returnresult_chk_all').prop('checked', allChecked);
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

function ReturnResult_ChuyenDangThucHien() {
    var patientId = $('#tdcn_returnresult_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning('Vui lòng chọn bệnh nhân!');
        return false;
    }

    if (!confirm('Xác nhận chuyển bệnh nhân sang trạng thái Đã thực hiện?')) {
        return false;
    }

    try { $('#showWaitting').modal('show'); } catch (e) { }

    $.ajax({
        url: '/TDCN_ReturnResult/ChuyenDangThucHien?patientId=' + patientId,
        contentType: 'application/json; charset=utf-8',
        dataType: 'text',
        type: 'POST',
        success: function (res) {
            try { $('#showWaitting').modal('hide'); } catch (e) { }

            if (!res || res.success !== true) {
                SwalHelper.Toast.error(res && res.message ? res.message : 'Cập nhật trạng thái không thành công.');
                return;
            }

            SwalHelper.Toast.success(res.message || 'Đã cập nhật trạng thái Đang thực hiện.');
            ReturnResult_Refresh();
            ReturnResult_Get_Count();
        },
        error: function () {
            try { $('#showWaitting').modal('hide'); } catch (e) { }
            SwalHelper.Toast.error('Không thể kết nối server để cập nhật trạng thái.');
        }
    });

    return true;
}
