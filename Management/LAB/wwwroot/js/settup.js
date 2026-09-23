// Tool tip cho control c?a form
//$(function () {
//    $('[data-toggle="tooltip"]').tooltip()
//})


// *********************************************************************************** Select 2
$(function () {
    GetSample_SetSelect2_01(true);
    Map_SetSelect2();
    $(document).ajaxStart(function () {
        AppLoading.show('Đang xử lý...');
    });

    $(document).ajaxStop(function () {
        AppLoading.hide(true);
    });
})

function GetSample_SetSelect2_02() {
    $(document).ready(function () {
        $('#ns_getsample_service').select2({
            dropdownParent: $('#addServiceForm'),
            placeholder: "-- Chọn dịch vụ --"
        });
    });
    $('#ns_getsample_service').val('');
}

function GetSample_SetSelect2_01(isLoadPage) {
    if (isLoadPage) {
        $('#ns_getsample_location').val("");
    }
    $(document).ready(function () {
        $('#ns_getsample_location').select2({
            placeholder: "-- Ch?n --"
        });
    });

    if (isLoadPage) {
        $('#ns_getsample_doctor').val("");
    }
    $(document).ready(function () {
        $('#ns_getsample_doctor').select2({
            placeholder: "-- Ch?n --"
        });
    });


    if (isLoadPage) {
        $('#ns_process_sampleresult').val("");
    }
    $(document).ready(function () {
        $('#ns_process_sampleresult').select2({
            placeholder: "-- Ch?n --"
        });
    });
}

function Process_SetSelect2_03() {
    $(document).ready(function () {
        $('#map-testcode').select2({
            placeholder: "-- Ch?n --"
        });
    });
}

function Map_SetSelect2() {
    $(document).ready(function () {
        $('#map-addtestcode-testcode').select2({
            dropdownParent: $('#addTestCode'),
            placeholder: "-- Ch?n m? xét nghiệm --"
        });
    });
}

function ValidateInput(id) {
    var flag = true;
    $("#" + id).find(".valid").each(function (i, e) {
        var value = $(e).val();
        if (value === "" || !value) {
            if (flag) {
                $(e).focus();
            }
            flag = false;
        }
    })
    return flag;
}

// ****************************************************************************** User

function User_HideButton(_new, _save, _delete, _cancel, _function) {
    if (_new == 1) {
        $("#user-new").hide();
    }
    else {
        $("#user-new").show();
    }

    if (_save == 1) {
        $("#user-save").hide();
    }
    else {
        $("#user-save").show();
    }

    if (_delete == 1) {
        $("#user-delete").hide();
    }
    else {
        $("#user-delete").show();
    }

    if (_cancel == 1) {
        $("#user-cancel").hide();
    }
    else {
        $("#user-cancel").show();
    }

    if (_function == 1) {
        $("#user-function").hide();
    }
    else {
        $("#user-function").show();
    }
}

function User_ValidateRequired() {
    var isValid = true;
    var messages = [];

    // Mã người dùng
    var code = $("#user-code").val();
    if (!code || code.trim() === "") {
        isValid = false;
        messages.push("Mã người dùng");
    }

    // Tên người dùng
    var name = $("#user-name").val();
    console.log(name);
    if (!name || name.trim() === "") {
        isValid = false;
        messages.push("Tên người dùng");
    }

    // Mật khẩu
    var pass = $("#user-pass").val();
    if (!pass || pass.trim() === "") {
        isValid = false;
        messages.push("Mật khẩu");
    }

    // Khoa phòng - ít nhất 1 checkbox được chọn
    var hasType = false;
    $(".div-input").find(".type").each(function () {
        if ($(this).prop('checked') === true) {
            hasType = true;
            return false; // break
        }
    });
    if (!hasType) {
        isValid = false;
        messages.push("Khoa phòng");
    }

    if (!isValid) {
        SwalHelper.Toast.warning("Vui lòng nhập đầy đủ thông tin: " + messages.join(", "));

        // Focus vào field trống đầu tiên
        if (!code || code.trim() === "") {
            $("#user-code").focus();
        } else if (!name || name.trim() === "") {
            $("#user-name").focus();
        } else if (!pass || pass.trim() === "") {
            $("#user-pass").focus();
        }
    }

    return isValid;
}
function User_ResetInput() {
    $("#user-code").val("");
    $("#user-name").val("");
    $("#user-pass").val("");
    $("#user-mabhyt").val("");
    $("#user-cks").val("");
    $("#user-cccd").val("");
    $("#user-his-employee-id").val("");
    $('input:checkbox').removeAttr('checked');
    User_ResetSignatureImage();
}

function User_Search(value) {
    $.ajax({
        url: "/User/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#user-settup-left-list").html(result);
        },
        error: function () {
            $("#user-settup-left-list").empty();
        }
    });
}

function User_CheckedBoxOnRow(code) {
    $(".row-service").each(function () { // L?y value tr?n t?ng Row
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                User_LoadInfo(code);
                User_LoadFunction(code);
                User_HideButton(false, true, false, true, false);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function User_LoadList() {
    $.ajax({
        url: "/User/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#user-settup-left-list").html(result);
        },
        error: function () {
            $("#user-settup-left-list").empty();
        }
    });
}

function User_LoadInfo(code) {
    $.ajax({
        url: "/User/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#user-settup-right-info").html(result);
            User_BindPasswordValidation();
        },
        error: function () {
            $("#user-settup-right-info").empty();
        }
    });
}

function User_LoadFunction(code) {
    $.ajax({
        url: "/User/GetFunction?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#user-settup-function").html(result);
        },
        error: function () {
            $("#user-settup-function").empty();
        }
    });
}

function User_New() {
    $("#user-code").prop('disabled', false);
    $("#user-code").focus();
    User_ResetInput();
    User_HideButton(true, false, true, false, true);
    User_BindPasswordValidation();
}

function User_BindPasswordValidation() {
    $("#user-pass").off("input").on("input", function () {
        User_ValidatePasswordRealtime();
    });
    // Run validation immediately to reflect current state of the password field
    User_ValidatePasswordRealtime();
}

// OLD — remove this:
$(document).ready(function () {
    $("#user-pass").on("input", function () {
        User_ValidatePasswordRealtime();
    });
});

function User_Save() {
    // Kiểm tra required fields
    if (!User_ValidateRequired()) {
        return;
    }

    // Kiểm tra rule password trước khi lưu
    if (!User_IsPasswordValid()) {
        User_ValidatePasswordRealtime();
        $("#user-pass").focus();
        SwalHelper.Toast.warning("Mật khẩu chưa đạt yêu cầu. Vui lòng kiểm tra lại quy tắc mật khẩu");
        return;
    }

    var code = $("#user-code").val();
    var name = $("#user-name").val();
    var pass = $("#user-pass").val();
    var mabhyt = $("#user-mabhyt").val();
    var active = $("#user-active").prop('checked');
    var type = "";
    var cks = $("#user-cks").val();
    var cccd = $("#user-cccd").val();
    var hisEmployeeIdRaw = $("#user-his-employee-id").val();
    var hisEmployeeId = (hisEmployeeIdRaw === null || hisEmployeeIdRaw.trim() === "")
        ? null
        : parseInt(hisEmployeeIdRaw, 10);

    $(".div-input").each(function () {
        $(this).find(".type").each(function () {
            var checked = $(this).prop('checked');
            if (checked === true) {
                type = type + $(this).val() + ";";
            }
        })
    })

    // Ki?m tra xem c? file ?nh ch? k? du?c ch?n kh?ng
    var signatureFile = User_GetSignatureImageData();

    if (signatureFile) {
        // N?u c? file upload, s? d?ng FormData
        var formData = new FormData();
        formData.append('code', code);
        formData.append('name', name);
        formData.append('pass', pass);
        formData.append('mabhyt', mabhyt);
        formData.append('active', active);
        formData.append('type', type);
        formData.append('cks', cks);
        formData.append('cccd', cccd);
        formData.append('hisEmployeeId', hisEmployeeId ?? '');
        formData.append('signatureImage', signatureFile);
        // Th?m t?n file d? format
        formData.append('signatureFileName', cks);
        console.log(formData);
        $.ajax({
            url: "/User/Save/",
            data: formData,
            contentType: false,
            processData: false,
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#user-code").prop('disabled', true);
                    SwalHelper.Toast.success("Lưu thành công.");
                    User_LoadList();
                    User_HideButton(false, true, false, true, false);
                    // Reset file input sau khi save th?nh c?ng
                    $("#user-signature-file").val('');
                    // C?p nh?t preview v?i file d? luu
                    User_UpdateSignaturePreview();
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    } else {
        // N?u kh?ng c? file upload, gi? nguy?n c?ch g?i JSON nhu cu
        var data = {
            code: code,
            name: name,
            pass: pass,
            mabhyt: mabhyt,
            active: active,
            type: type,
            cks: cks,
            cccd: cccd,
            hisEmployeeId: hisEmployeeId
        };

        $.ajax({
            url: "/User/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#user-code").prop('disabled', true);
                    SwalHelper.Toast.success("Lưu thành công.");
                    User_LoadList();
                    User_HideButton(false, true, false, true, false);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function User_Delete() {
    var choice = confirm("Bạn muốn xóa ngu?i d?ng n?y ?");
    if (choice) {
        $("#user-code").prop('disabled', true);
        var code = $("#user-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn người dùng cần xóa");
        }
        else {
            $.ajax({
                url: "/User/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        User_ResetInput();
                        User_LoadList();
                        User_HideButton(false, true, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function User_Cancel() {
    $("#user-code").prop('disabled', true);
    var code = $("#user-code").val();
    if (code === "" || !code) {
        User_ResetInput();
        User_HideButton(false, true, true, true, true);
    }
    else {
        User_LoadInfo(code);
        User_HideButton(false, true, false, true, false);
    }
}

function User_SaveUserFunction() {
    var code = $('#user-code').val();
    if (code === "" || !code) {
        SwalHelper.Toast.warning("Vui lòng chọn người dùng để phân quyền!");
    }
    else {
        var functions = "";
        $(".user-function-content").each(function () {
            $(this).find(".function").each(function () {
                var checked = $(this).prop('checked');
                if (checked === true) {
                    functions = functions + $(this).val() + ";";
                }
            })
        })

        var data = {
            code: code,
            functions: functions
        };

        $.ajax({
            url: "/User/Save_UserFunction/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'False') {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function User_UserFunction() {
    $("#user-code").prop('disabled', true);
    var code = $("#user-code").val();
    if (code === "" || !code) {
        $('#addFunction').modal('hide');
        SwalHelper.Toast.warning("Vui lòng phân quyền người dùng !");
    }
}

// ************************ Thêm rule khi tạo mật khẩu người dùng ***************************
function User_TogglePassword() {
    PasswordValidation.togglePassword("#user-pass", "#password-eye", "bi-eye-slash-fill", "bi-eye-fill");
}

function User_SetRule(element, isValid) {
    PasswordValidation.setRule(element, isValid);
}

function User_ValidatePasswordRealtime() {
    var password = $("#user-pass").val();
    var username = $("#user-code").val();
    PasswordValidation.validateRealtime(password, username);
}

function User_HasSequentialChars(str, minSeqLength) {
    return PasswordValidation.hasSequentialChars(str, minSeqLength);
}

function User_IsPasswordValid() {
    var password = $("#user-pass").val();
    var username = $("#user-code").val();
    return PasswordValidation.isPasswordValid(password, username);
}

$(document).ready(function () {

    $("#user-pass").on("input", function () {

        User_ValidatePasswordRealtime();

    });

});

function User_ValidatePasswordRealtime() {
    var password = $("#user-pass").val();
    var username = $("#user-code").val();
    PasswordValidation.validateRealtime(password, username);
}

function User_HasSequentialChars(str, minSeqLength) {
    return PasswordValidation.hasSequentialChars(str, minSeqLength);
}

function User_IsPasswordValid() {
    var password = $("#user-pass").val();
    var username = $("#user-code").val();
    return PasswordValidation.isPasswordValid(password, username);
}
// ****************************************************************************** Signature Management

function User_SelectSignatureImage() {
    $("#user-signature-file").click();
}

function User_OnSignatureImageSelected(input) {
    if (input.files && input.files[0]) {
        var file = input.files[0];

        // Validate file type
        if (!file.type.match('image.*')) {
            SwalHelper.Toast.warning("Vui l?ng ch?n file h?nh ?nh!");
            return;
        }

        // Validate file size (max 5MB)
        if (file.size > 5 * 1024 * 1024) {
            SwalHelper.Toast.warning("K?ch thu?c file kh?ng du?c vu?t qu? 5MB!");
            return;
        }

        // Format filename based on user name
        var userName = $("#user-name").val().trim();
        console.log(userName);
        if (userName) {
            var formattedFileName = User_FormatSignatureFileName(userName);
            $("#user-cks").val(formattedFileName);
        }

        var reader = new FileReader();
        reader.onload = function (e) {
            // Hide placeholder and show image
            $("#user-signature-placeholder").hide();
            $("#user-signature-preview").attr('src', e.target.result).show();
            User_HideButton(true, false, true, false, true);
        };
        reader.readAsDataURL(file);
    }
}

function User_FormatSignatureFileName(userName) {
    // Remove Vietnamese accents and special characters
    var formatted = User_RemoveVietnameseAccents(userName);

    // Remove all spaces and special characters, keep only letters and numbers
    formatted = formatted.replace(/[^a-zA-Z0-9]/g, '');

    // Convert to lowercase and add suffix
    return formatted.toLowerCase() + '_cks.png';
}

function User_RemoveVietnameseAccents(str) {
    var defaultDiacriticsRemovalMap = [
        { 'base': 'A', 'letters': /[\u0041\u24B6\uFF21\u00C0\u00C1\u00C2\u1EA6\u1EA4\u1EAA\u1EA8\u00C3\u0100\u0102\u1EB0\u1EAE\u1EB4\u1EB2\u0226\u01E0\u00C4\u01DE\u1EA2\u00C5\u01FA\u01CD\u0200\u0202\u1EA0\u1EAC\u1EB6\u1E00\u0104\u023A\u2C6F]/g },
        { 'base': 'AA', 'letters': /[\uA732]/g },
        { 'base': 'AE', 'letters': /[\u00C6\u01FC\u01E2]/g },
        { 'base': 'AO', 'letters': /[\uA734]/g },
        { 'base': 'AU', 'letters': /[\uA736]/g },
        { 'base': 'AV', 'letters': /[\uA738\uA73A]/g },
        { 'base': 'AY', 'letters': /[\uA73C]/g },
        { 'base': 'B', 'letters': /[\u0042\u24B7\uFF22\u1E02\u1E04\u1E06\u0243\u0182\u0181]/g },
        { 'base': 'C', 'letters': /[\u0043\u24B8\uFF23\u0106\u0108\u010A\u010C\u00C7\u1E08\u0187\u023B\uA73E]/g },
        { 'base': 'D', 'letters': /[\u0044\u24B9\uFF24\u1E0A\u010E\u1E0C\u1E10\u1E12\u1E0E\u0110\u018B\u018A\u0189\uA779]/g },
        { 'base': 'DZ', 'letters': /[\u01F1\u01C4]/g },
        { 'base': 'Dz', 'letters': /[\u01F2\u01C5]/g },
        { 'base': 'E', 'letters': /[\u0045\u24BA\uFF25\u00C8\u00C9\u00CA\u1EC0\u1EBE\u1EC4\u1EC2\u1EBC\u0112\u1E14\u1E16\u0114\u0116\u00CB\u1EBA\u011A\u0204\u0206\u1EB8\u1EC6\u0228\u1E1C\u0118\u1E18\u1E1A\u0190\u018E]/g },
        { 'base': 'F', 'letters': /[\u0046\u24BB\uFF26\u1E1E\u0191\uA77B]/g },
        { 'base': 'G', 'letters': /[\u0047\u24BC\uFF27\u01F4\u011C\u1E20\u011E\u0120\u01E6\u0122\u01E4\u0193\uA7A0\uA77D\uA77E]/g },
        { 'base': 'H', 'letters': /[\u0048\u24BD\uFF28\u0124\u1E22\u1E26\u021E\u1E24\u1E28\u1E2A\u0126\u2C67\u2C75\uA78D]/g },
        { 'base': 'I', 'letters': /[\u0049\u24BE\uFF29\u00CC\u00CD\u00CE\u0128\u012A\u012C\u0130\u00CF\u1E2E\u1EC8\u01CF\u0208\u020A\u1ECA\u012E\u1E2C\u0197]/g },
        { 'base': 'J', 'letters': /[\u004A\u24BF\uFF2A\u0134\u0248]/g },
        { 'base': 'K', 'letters': /[\u004B\u24C0\uFF2B\u1E30\u01E8\u1E32\u0136\u1E34\u0198\u2C69\uA740\uA742\uA744\uA7A2]/g },
        { 'base': 'L', 'letters': /[\u004C\u24C1\uFF2C\u013F\u0139\u013D\u1E36\u1E38\u013B\u1E3C\u1E3A\u0141\u023D\u2C62\u2C60\uA748\uA746\uA780]/g },
        { 'base': 'LJ', 'letters': /[\u01C7]/g },
        { 'base': 'Lj', 'letters': /[\u01C8]/g },
        { 'base': 'M', 'letters': /[\u004D\u24C2\uFF2D\u1E3E\u1E40\u1E42\u2C6E\u019C]/g },
        { 'base': 'N', 'letters': /[\u004E\u24C3\uFF2E\u01F8\u0143\u00D1\u1E44\u0147\u1E46\u0145\u1E4A\u1E48\u0220\u019D\uA790\uA7A4]/g },
        { 'base': 'NJ', 'letters': /[\u01CA]/g },
        { 'base': 'Nj', 'letters': /[\u01CB]/g },
        { 'base': 'O', 'letters': /[\u004F\u24C4\uFF2F\u00D2\u00D3\u00D4\u1ED2\u1ED0\u1ED6\u1ED4\u00D5\u1E4C\u022C\u1E4E\u014C\u1E50\u1E52\u014E\u022E\u0230\u00D6\u022A\u1ECE\u0150\u01D1\u020C\u020E\u01A0\u1EDC\u1EDA\u1EE0\u1EDE\u1EE2\u1ECC\u1ED8\u01EA\u01EC\u00D8\u01FE\u0186\u019F\uA74A\uA74C]/g },
        { 'base': 'OI', 'letters': /[\u01A2]/g },
        { 'base': 'OO', 'letters': /[\uA74E]/g },
        { 'base': 'OU', 'letters': /[\u0222]/g },
        { 'base': 'P', 'letters': /[\u0050\u24C5\uFF30\u1E54\u1E56\u01A4\u2C63\uA750\uA752\uA754]/g },
        { 'base': 'Q', 'letters': /[\u0051\u24C6\uFF31\uA756\uA758\u024A]/g },
        { 'base': 'R', 'letters': /[\u0052\u24C7\uFF32\u0154\u1E58\u0158\u0210\u0212\u1E5A\u1E5C\u0156\u1E5E\u024C\u2C64\uA75A\uA7A6\uA782]/g },
        { 'base': 'S', 'letters': /[\u0053\u24C8\uFF33\u1E9E\u015A\u1E64\u015C\u1E60\u0160\u1E66\u1E62\u1E68\u0218\u015E\u2C7E\uA7A8\uA784]/g },
        { 'base': 'T', 'letters': /[\u0054\u24C9\uFF34\u1E6A\u0164\u1E6C\u021A\u0162\u1E70\u1E6E\u0166\u01AC\u01AE\u023E\uA786]/g },
        { 'base': 'TZ', 'letters': /[\uA728]/g },
        { 'base': 'U', 'letters': /[\u0055\u24CA\uFF35\u00D9\u00DA\u00DB\u0168\u1E78\u016A\u1E7A\u016C\u00DC\u01DB\u01D7\u01D5\u01D9\u1EE6\u016E\u0170\u01D3\u0214\u0216\u01AF\u1EEA\u1EE8\u1EEE\u1EEC\u1EF0\u1EE4\u1E72\u0172\u1E76\u1E74\u0244]/g },
        { 'base': 'V', 'letters': /[\u0056\u24CB\uFF36\u1E7C\u1E7E\u01B2\uA75E\u0245]/g },
        { 'base': 'VY', 'letters': /[\uA760]/g },
        { 'base': 'W', 'letters': /[\u0057\u24CC\uFF37\u1E80\u1E82\u0174\u1E86\u1E84\u1E88\u2C72]/g },
        { 'base': 'X', 'letters': /[\u0058\u24CD\uFF38\u1E8A\u1E8C]/g },
        { 'base': 'Y', 'letters': /[\u0059\u24CE\uFF39\u1EF2\u00DD\u0176\u1EF8\u0232\u1E8E\u0178\u1EF6\u1EF4\u01B3\u024E\u1EFE]/g },
        { 'base': 'Z', 'letters': /[\u005A\u24CF\uFF3A\u0179\u1E90\u017B\u017D\u1E92\u1E94\u01B5\u0224\u2C7F\u2C6B\uA762]/g },
        { 'base': 'a', 'letters': /[\u0061\u24D0\uFF41\u1E9A\u00E0\u00E1\u00E2\u1EA7\u1EA5\u1EAB\u1EA9\u00E3\u0101\u0103\u1EB1\u1EAF\u1EB5\u1EB3\u0227\u01E1\u00E4\u01DF\u1EA3\u00E5\u01FB\u01CE\u0201\u0203\u1EA1\u1EAD\u1EB7\u1E01\u0105\u2C65\u0250]/g },
        { 'base': 'aa', 'letters': /[\uA733]/g },
        { 'base': 'ae', 'letters': /[\u00E6\u01FD\u01E3]/g },
        { 'base': 'ao', 'letters': /[\uA735]/g },
        { 'base': 'au', 'letters': /[\uA737]/g },
        { 'base': 'av', 'letters': /[\uA739\uA73B]/g },
        { 'base': 'ay', 'letters': /[\uA73D]/g },
        { 'base': 'b', 'letters': /[\u0062\u24D1\uFF42\u1E03\u1E05\u1E07\u0180\u0183\u0253]/g },
        { 'base': 'c', 'letters': /[\u0063\u24D2\uFF43\u0107\u0109\u010B\u010D\u00E7\u1E09\u0188\u023C\uA73F\u2184]/g },
        { 'base': 'd', 'letters': /[\u0064\u24D3\uFF44\u1E0B\u010F\u1E0D\u1E11\u1E13\u1E0F\u0111\u018C\u0256\u0257\uA77A]/g },
        { 'base': 'dz', 'letters': /[\u01F3\u01C6]/g },
        { 'base': 'e', 'letters': /[\u0065\u24D4\uFF45\u00E8\u00E9\u00EA\u1EC1\u1EBF\u1EC5\u1EC3\u1EBD\u0113\u1E15\u1E17\u0115\u0117\u00EB\u1EBB\u011B\u0205\u0207\u1EB9\u1EC7\u0229\u1E1D\u0119\u1E19\u1E1B\u0247\u025B\u01DD]/g },
        { 'base': 'f', 'letters': /[\u0066\u24D5\uFF46\u1E1F\u0192\uA77C]/g },
        { 'base': 'g', 'letters': /[\u0067\u24D6\uFF47\u01F5\u011D\u1E21\u011F\u0121\u01E7\u0123\u01E5\u0260\uA7A1\u1D79\uA77F]/g },
        { 'base': 'h', 'letters': /[\u0068\u24D7\uFF48\u0125\u1E23\u1E27\u021F\u1E25\u1E29\u1E2B\u1E96\u0127\u2C68\u2C76\u0265]/g },
        { 'base': 'hv', 'letters': /[\u0195]/g },
        { 'base': 'i', 'letters': /[\u0069\u24D8\uFF49\u00EC\u00ED\u00EE\u0129\u012B\u012D\u00EF\u1E2F\u1EC9\u01D0\u0209\u020B\u1ECB\u012F\u1E2D\u0268\u0131]/g },
        { 'base': 'j', 'letters': /[\u006A\u24D9\uFF4A\u0135\u01F0\u0249]/g },
        { 'base': 'k', 'letters': /[\u006B\u24DA\uFF4B\u1E31\u01E9\u1E33\u0137\u1E35\u0199\u2C6A\uA741\uA743\uA745\uA7A3]/g },
        { 'base': 'l', 'letters': /[\u006C\u24DB\uFF4C\u0140\u013A\u013E\u1E37\u1E39\u013C\u1E3D\u1E3B\u017F\u0142\u019A\u026B\u2C61\uA749\uA781\uA747]/g },
        { 'base': 'lj', 'letters': /[\u01C9]/g },
        { 'base': 'm', 'letters': /[\u006D\u24DC\uFF4D\u1E3F\u1E41\u1E43\u0271\u026F]/g },
        { 'base': 'n', 'letters': /[\u006E\u24DD\uFF4E\u01F9\u0144\u00F1\u1E45\u0148\u1E47\u0146\u1E4B\u1E49\u019E\u0272\u0149\uA791\uA7A5]/g },
        { 'base': 'nj', 'letters': /[\u01CC]/g },
        { 'base': 'o', 'letters': /[\u006F\u24DE\uFF4F\u00F2\u00F3\u00F4\u1ED3\u1ED1\u1ED7\u1ED5\u00F5\u1E4D\u022D\u1E4F\u014D\u1E51\u1E53\u014F\u022F\u0231\u00F6\u022B\u1ECF\u0151\u01D2\u020D\u020F\u01A1\u1EDD\u1EDB\u1EE1\u1EDF\u1EE3\u1ECD\u1ED9\u01EB\u01ED\u00F8\u01FF\u0254\uA74B\uA74D\u0275]/g },
        { 'base': 'oi', 'letters': /[\u01A3]/g },
        { 'base': 'ou', 'letters': /[\u0223]/g },
        { 'base': 'oo', 'letters': /[\uA74F]/g },
        { 'base': 'p', 'letters': /[\u0070\u24DF\uFF50\u1E55\u1E57\u01A5\u1D7D\uA751\uA753\uA755]/g },
        { 'base': 'q', 'letters': /[\u0071\u24E0\uFF51\u024B\uA757\uA759]/g },
        { 'base': 'r', 'letters': /[\u0072\u24E1\uFF52\u0155\u1E59\u0159\u0211\u0213\u1E5B\u1E5D\u0157\u1E5F\u024D\u027D\uA75B\uA7A7\uA783]/g },
        { 'base': 's', 'letters': /[\u0073\u24E2\uFF53\u00DF\u015B\u1E65\u015D\u1E61\u0161\u1E67\u1E63\u1E69\u0219\u015F\u023F\uA7A9\uA785\u1E9B]/g },
        { 'base': 't', 'letters': /[\u0074\u24E3\uFF54\u1E6B\u1E97\u0165\u1E6D\u021B\u0163\u1E71\u1E6F\u0167\u01AD\u0288\u2C66\uA787]/g },
        { 'base': 'tz', 'letters': /[\uA729]/g },
        { 'base': 'u', 'letters': /[\u0075\u24E4\uFF55\u00F9\u00FA\u00FB\u0169\u1E79\u016B\u1E7B\u016D\u00FC\u01DC\u01D8\u01D6\u01DA\u1EE7\u016F\u0171\u01D4\u0215\u0217\u01B0\u1EEB\u1EE9\u1EEF\u1EED\u1EF1\u1EE5\u1E73\u0173\u1E77\u1E75\u0289]/g },
        { 'base': 'v', 'letters': /[\u0076\u24E5\uFF56\u1E7D\u1E7F\u028B\uA75F\u028C]/g },
        { 'base': 'vy', 'letters': /[\uA761]/g },
        { 'base': 'w', 'letters': /[\u0077\u24E6\uFF57\u1E81\u1E83\u0175\u1E87\u1E85\u1E98\u1E89\u2C73]/g },
        { 'base': 'x', 'letters': /[\u0078\u24E7\uFF58\u1E8B\u1E8D]/g },
        { 'base': 'y', 'letters': /[\u0079\u24E8\uFF59\u1EF3\u00FD\u0177\u1EF9\u0233\u1E8F\u00FF\u1EF7\u1E99\u1EF5\u01B4\u024F\u1EFF]/g },
        { 'base': 'z', 'letters': /[\u007A\u24E9\uFF5A\u017A\u1E91\u017C\u017E\u1E93\u1E95\u01B6\u0225\u0240\u2C6C\uA763]/g }
    ];

    for (var i = 0; i < defaultDiacriticsRemovalMap.length; i++) {
        str = str.replace(defaultDiacriticsRemovalMap[i].letters, defaultDiacriticsRemovalMap[i].base);
    }

    return str;
}
function User_UpdateSignaturePreview() {
    var cksValue = $("#user-cks").val();
    if (cksValue && cksValue.trim() !== "") {
        // Hide placeholder and show image
        $("#user-signature-placeholder").hide();
        $("#user-signature-preview").attr('src', '/images/cks/' + cksValue.trim()).show();
    } else {
        // Show placeholder and hide image
        $("#user-signature-placeholder").show();
        $("#user-signature-preview").hide();
    }
}
function User_ShowAddSignatureText() {
    // Called when image fails to load (onerror)
    $("#user-signature-placeholder").show();
    $("#user-signature-preview").hide();
}

function User_GetSignatureImageData() {
    var fileInput = $("#user-signature-file")[0];
    if (fileInput.files && fileInput.files[0]) {
        return fileInput.files[0];
    }
    return null;
}

function User_ResetSignatureImage() {
    $("#user-signature-placeholder").show();
    $("#user-signature-preview").hide();
    $("#user-signature-file").val('');
}


// ****************************************************************************** Hospital
function Hospital_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Hospital_LoadInfo(code);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Hospital_LoadList() {
    $.ajax({
        url: "/Hospital/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#hospital-settup-left-list").html(result);
        },
        error: function () {
            $("#hospital-settup-left-list").empty();
        }
    });
}

function Hospital_LoadInfo(code) {
    $.ajax({
        url: "/Hospital/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#hospital-settup-right-info").html(result);
        },
        error: function () {
            $("#hospital-settup-right-info").empty();
        }
    });
}

function Hospital_Save() {
    var validate = ValidateInput('hospital-settup-right-info');
    if (validate) {
        var code = $("#hospital-code").val();
        var name = $("#hospital-name").val();
        var nameen = $("#hospital-nameen").val();
        var address = $("#hospital-address").val();
        var phone = $("#hospital-phone").val();
        var website = $("#hospital-website").val();
        var email = $("#hospital-email").val();
        var logo = $("#hospital-logo").val();

        var data = {
            code: code,
            name: name,
            nameen: nameen,
            address: address,
            phone: phone,
            website: website,
            email: email,
            logo: logo
        };

        $.ajax({
            url: "/Hospital/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#hospital-code").prop('disabled', true);
                    Hospital_LoadList();
                    Hospital_HideButton(false, true, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Hospital_Cancel() {
    $("#hospital-code").prop('disabled', true);
    var code = $("#hospital-code").val();
    Hospital_LoadInfo(code);

    if (code) {
        Hospital_LoadInfo(code);
        Hospital_HideButton(false, true, true);
    }
    else {
        Hospital_ResetInput();
        Hospital_HideButton(false, true, true);
    }
}

function Hospital_New() {
    $("#hospital-code").prop('disabled', false);
    $("#hospital-code").focus();
    Hospital_ResetInput();
    Hospital_HideButton(true, false, false);
}

function Hospital_ResetInput() {
    $("#hospital-code").val("");
    $("#hospital-name").val("");
    $("#hospital-nameen").val("");
    $("#hospital-address").val("");
    $("#hospital-phone").val("");
    $("#hospital-website").val("");
    $("#hospital-email").val("");
    $("#hospital-logo").val("");
}

function Hospital_HideButton(_new, _save, _cancel) {
    if (_new == 1) {
        $("#hospital-new").hide();
    }
    else {
        $("#hospital-new").show();
    }

    if (_save == 1) {
        $("#hospital-save").hide();
    }
    else {
        $("#hospital-save").show();
    }

    if (_cancel == 1) {
        $("#hospital-cancel").hide();
    }
    else {
        $("#hospital-cancel").show();
    }
}


// ****************************************************************************** Location

function Location_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#location-new").hide();
    }
    else {
        $("#location-new").show();
    }

    if (_save == 1) {
        $("#location-save").hide();
    }
    else {
        $("#location-save").show();
    }

    if (_delete == 1) {
        $("#location-delete").hide();
    }
    else {
        $("#location-delete").show();
    }

    if (_cancel == 1) {
        $("#location-cancel").hide();
    }
    else {
        $("#location-cancel").show();
    }
}

function Location_ResetInput() {
    $("#location-code").val("");
    $("#location-name").val("");
    $('input:checkbox').removeAttr('checked');
}

function Location_Search(value) {
    $.ajax({
        url: "/Location/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#location-settup-left-list").html(result);
        },
        error: function () {
            $("#location-settup-left-list").empty();
        }
    });
}

function Location_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Location_LoadInfo(code);
                Location_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Location_LoadList() {
    $.ajax({
        url: "/Location/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#location-settup-left-list").html(result);
        },
        error: function () {
            $("#location-settup-left-list").empty();
        }
    });
}

function Location_LoadInfo(code) {
    $.ajax({
        url: "/Location/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#location-settup-right-info").html(result);
        },
        error: function () {
            $("#location-settup-right-info").empty();
        }
    });
}

function Location_New() {
    $("#location-code").prop('disabled', false);
    $("#location-code").focus();
    Location_ResetInput();
    Location_HideButton(true, false, true, false);
}

function Location_Save() {
    var validate = ValidateInput('location-settup-right-info');
    if (validate) {
        var code = $("#location-code").val();
        var name = $("#location-name").val();
        var active = $("#location-active").prop('checked');

        var data = {
            code: code,
            name: name,
            active: active
        };

        $.ajax({
            url: "/Location/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#location-code").prop('disabled', true);
                    Location_LoadList();
                    Location_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Location_Delete() {
    var choice = confirm("Bạn muốn xóa khoa ph?ng n?y ?");
    if (choice) {
        $("#location-code").prop('disabled', true);
        var code = $("#location-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn người dùng cần xóa");
        }
        else {
            $.ajax({
                url: "/Location/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Location_ResetInput();
                        Location_LoadList();
                        Location_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Location_Cancel() {
    $("#location-code").prop('disabled', true);
    var code = $("#location-code").val();
    if (code === "" || !code) {
        Location_ResetInput();
        Location_HideButton(false, true, true, true);
    }
    else {
        Location_LoadInfo(code);
        Location_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** Object

function Object_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#object-new").hide();
    }
    else {
        $("#object-new").show();
    }

    if (_save == 1) {
        $("#object-save").hide();
    }
    else {
        $("#object-save").show();
    }

    if (_delete == 1) {
        $("#object-delete").hide();
    }
    else {
        $("#object-delete").show();
    }

    if (_cancel == 1) {
        $("#object-cancel").hide();
    }
    else {
        $("#object-cancel").show();
    }
}

function Object_ResetInput() {
    $("#object-code").val("");
    $("#object-name").val("");
    $('input:checkbox').removeAttr('checked');
}

function Object_Search(value) {
    $.ajax({
        url: "/Object/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#object-settup-left-list").html(result);
        },
        error: function () {
            $("#object-settup-left-list").empty();
        }
    });
}

function Object_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Object_LoadInfo(code);
                Object_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Object_LoadList() {
    $.ajax({
        url: "/Object/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#object-settup-left-list").html(result);
        },
        error: function () {
            $("#object-settup-left-list").empty();
        }
    });
}

function Object_LoadInfo(code) {
    $.ajax({
        url: "/Object/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#object-settup-right-info").html(result);
        },
        error: function () {
            $("#object-settup-right-info").empty();
        }
    });
}

function Object_New() {
    $("#object-code").prop('disabled', false);
    $("#object-code").focus();
    Object_ResetInput();
    Object_HideButton(true, false, true, false);
}

function Object_Save() {
    var validate = ValidateInput('object-settup-right-info');
    if (validate) {
        var code = $("#object-code").val();
        var name = $("#object-name").val();
        var active = $("#object-active").prop('checked');

        var data = {
            code: code,
            name: name,
            active: active
        };

        $.ajax({
            url: "/Object/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#object-code").prop('disabled', true);
                    Object_LoadList();
                    Object_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Object_Delete() {
    var choice = confirm("Bạn muốn xóa khoa ph?ng n?y ?");
    if (choice) {
        $("#object-code").prop('disabled', true);
        var code = $("#object-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn người dùng cần xóa");
        }
        else {
            $.ajax({
                url: "/Object/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Object_ResetInput();
                        Object_LoadList();
                        Object_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Object_Cancel() {
    $("#object-code").prop('disabled', true);
    var code = $("#object-code").val();
    if (code === "" || !code) {
        Object_ResetInput();
        Object_HideButton(false, true, true, true);
    }
    else {
        Object_LoadInfo(code);
        Object_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** Doctor

function Doctor_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#doctor-new").hide();
    }
    else {
        $("#doctor-new").show();
    }

    if (_save == 1) {
        $("#doctor-save").hide();
    }
    else {
        $("#doctor-save").show();
    }

    if (_delete == 1) {
        $("#doctor-delete").hide();
    }
    else {
        $("#doctor-delete").show();
    }

    if (_cancel == 1) {
        $("#doctor-cancel").hide();
    }
    else {
        $("#doctor-cancel").show();
    }
}

function Doctor_ResetInput() {
    $("#doctor-code").val("");
    $("#doctor-name").val("");
    $('input:checkbox').removeAttr('checked');
}

function Doctor_Search(value) {
    $.ajax({
        url: "/Doctor/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#doctor-settup-left-list").html(result);
        },
        error: function () {
            $("#doctor-settup-left-list").empty();
        }
    });
}

function Doctor_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Doctor_LoadInfo(code);
                Doctor_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Doctor_LoadList() {
    $.ajax({
        url: "/Doctor/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#doctor-settup-left-list").html(result);
        },
        error: function () {
            $("#doctor-settup-left-list").empty();
        }
    });
}

function Doctor_LoadInfo(code) {
    $.ajax({
        url: "/Doctor/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#doctor-settup-right-info").html(result);
        },
        error: function () {
            $("#doctor-settup-right-info").empty();
        }
    });
}

function Doctor_New() {
    $("#doctor-code").prop('disabled', false);
    $("#doctor-code").focus();
    Doctor_ResetInput();
    Doctor_HideButton(true, false, true, false);
}

function Doctor_Save() {
    var validate = ValidateInput('doctor-settup-right-info');
    if (validate) {
        var code = $("#doctor-code").val();
        var name = $("#doctor-name").val();
        var active = $("#doctor-active").prop('checked');

        var data = {
            code: code,
            name: name,
            active: active
        };

        $.ajax({
            url: "/Doctor/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#doctor-code").prop('disabled', true);
                    Doctor_LoadList();
                    Doctor_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Doctor_Delete() {
    var choice = confirm("Bạn muốn xóa bác sĩ này ?");
    if (choice) {
        $("#doctor-code").prop('disabled', true);
        var code = $("#doctor-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn người dùng cần xóa");
        }
        else {
            $.ajax({
                url: "/Doctor/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Doctor_ResetInput();
                        Doctor_LoadList();
                        Doctor_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Doctor_Cancel() {
    $("#doctor-code").prop('disabled', true);
    var code = $("#doctor-code").val();
    if (code === "" || !code) {
        Doctor_ResetInput();
        Doctor_HideButton(false, true, true, true);
    }
    else {
        Doctor_LoadInfo(code);
        Doctor_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** TestType

function TestType_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#testtype-new").hide();
    }
    else {
        $("#testtype-new").show();
    }

    if (_save == 1) {
        $("#testtype-save").hide();
    }
    else {
        $("#testtype-save").show();
    }

    if (_delete == 1) {
        $("#testtype-delete").hide();
    }
    else {
        $("#testtype-delete").show();
    }

    if (_cancel == 1) {
        $("#testtype-cancel").hide();
    }
    else {
        $("#testtype-cancel").show();
    }
}

function TestType_ResetInput() {
    $("#testtype-code").val("");
    $("#testtype-name").val("");
    $('input:checkbox').removeAttr('checked');
}

function TestType_Search(value) {
    $.ajax({
        url: "/TestType/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#testtype-settup-left-list").html(result);
        },
        error: function () {
            $("#testtype-settup-left-list").empty();
        }
    });
}

function TestType_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                TestType_LoadInfo(code);
                TestType_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function TestType_LoadList() {
    $.ajax({
        url: "/TestType/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#testtype-settup-left-list").html(result);
        },
        error: function () {
            $("#testtype-settup-left-list").empty();
        }
    });
}

function TestType_LoadInfo(code) {
    $.ajax({
        url: "/TestType/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#testtype-settup-right-info").html(result);
        },
        error: function () {
            $("#testtype-settup-right-info").empty();
        }
    });
}

function TestType_New() {
    $("#testtype-code").prop('disabled', false);
    $("#testtype-code").focus();
    TestType_ResetInput();
    TestType_HideButton(true, false, true, false);
}

function TestType_Save() {
    var validate = ValidateInput('testtype-settup-right-info');
    if (validate) {
        var code = $("#testtype-code").val();
        var name = $("#testtype-name").val();
        var active = $("#testtype-active").prop('checked');

        var data = {
            code: code,
            name: name,
            active: active
        };

        $.ajax({
            url: "/TestType/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#testtype-code").prop('disabled', true);
                    TestType_LoadList();
                    TestType_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function TestType_Delete() {
    var choice = confirm("Bạn muốn xóa lo?i m?u n?y ?");
    if (choice) {
        $("#testtype-code").prop('disabled', true);
        var code = $("#testtype-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn người dùng cần xóa");
        }
        else {
            $.ajax({
                url: "/TestType/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        TestType_ResetInput();
                        TestType_LoadList();
                        TestType_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function TestType_Cancel() {
    $("#testtype-code").prop('disabled', true);
    var code = $("#testtype-code").val();
    if (code === "" || !code) {
        TestType_ResetInput();
        TestType_HideButton(false, true, true, true);
    }
    else {
        TestType_LoadInfo(code);
        TestType_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** Category

function Category_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#category-new").hide();
    }
    else {
        $("#category-new").show();
    }

    if (_save == 1) {
        $("#category-save").hide();
    }
    else {
        $("#category-save").show();
    }

    if (_delete == 1) {
        $("#category-delete").hide();
    }
    else {
        $("#category-delete").show();
    }

    if (_cancel == 1) {
        $("#category-cancel").hide();
    }
    else {
        $("#category-cancel").show();
    }
}

function Category_ResetInput() {
    $("#category-code").val("");
    $("#category-name").val("");
    $('input:checkbox').removeAttr('checked');
}

function Category_Search(value) {
    $.ajax({
        url: "/Category/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#category-settup-left-list").html(result);
        },
        error: function () {
            $("#category-settup-left-list").empty();
        }
    });
}

function Category_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Category_LoadInfo(code);
                Category_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Category_LoadList() {
    $.ajax({
        url: "/Category/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#category-settup-left-list").html(result);
        },
        error: function () {
            $("#category-settup-left-list").empty();
        }
    });
}

function Category_LoadInfo(code) {
    $.ajax({
        url: "/Category/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#category-settup-right-info").html(result);
        },
        error: function () {
            $("#category-settup-right-info").empty();
        }
    });
}

function Category_New() {
    $("#category-code").prop('disabled', false);
    $("#category-code").focus();
    Category_ResetInput();
    Category_HideButton(true, false, true, false);
}

function Category_Save() {
    var validate = ValidateInput('category-settup-right-info');
    if (validate) {
        var code = $("#category-code").val();
        var name = $("#category-name").val();
        var active = $("#category-active").prop('checked');

        var data = {
            code: code,
            name: name,
            active: active
        };

        $.ajax({
            url: "/Category/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#category-code").prop('disabled', true);
                    Category_LoadList();
                    Category_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Category_Delete() {
    var choice = confirm("Bạn muốn xóa danh m?c n?y ?");
    if (choice) {
        $("#category-code").prop('disabled', true);
        var code = $("#category-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn người dùng cần xóa");
        }
        else {
            $.ajax({
                url: "/Category/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Category_ResetInput();
                        Category_LoadList();
                        Category_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Category_Cancel() {
    $("#category-code").prop('disabled', true);
    var code = $("#category-code").val();
    if (code === "" || !code) {
        Category_ResetInput();
        Category_HideButton(false, true, true, true);
    }
    else {
        Category_LoadInfo(code);
        Category_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** Service

$(function () {
    Service_SetSelect2(true);
})

function Service_SetSelect2(isLoadPage) {
    if (isLoadPage) {
        $('#service-category').val("");
    }
    $(document).ready(function () {
        $('#service-category').select2({
            placeholder: "-- Ch?n --"
        });
    });

    if (isLoadPage) {
        $('#service-printsample').val("");
    }
    $(document).ready(function () {
        $('#service-printsample').select2({
            placeholder: "-- Ch?n --"
        });
    });
}

function Service_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#service-new").hide();
    }
    else {
        $("#service-new").show();
    }

    if (_save == 1) {
        $("#service-save").hide();
    }
    else {
        $("#service-save").show();
    }

    if (_delete == 1) {
        $("#service-delete").hide();
    }
    else {
        $("#service-delete").show();
    }

    if (_cancel == 1) {
        $("#service-cancel").hide();
    }
    else {
        $("#service-cancel").show();
    }
}

function Service_ResetInput() {
    $("#service-code").val("");
    $("#service-name").val("");
    $('#service-category').val('');
    $('#service-printsample').val('');
    $('#service-printorder').val('');
    $('#service-processtype').val('');
    $("#service-active").prop("checked", false);
    $('input:checkbox').removeAttr('checked');
    Service_SetSelect2(false);
}

function Service_Search(value) {
    $.ajax({
        url: "/Service/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#service-settup-left-list").html(result);
        },
        error: function () {
            $("#service-settup-left-list").empty();
        }
    });
}

function Service_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Service_LoadInfo(code);
                Service_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Service_LoadList() {
    $.ajax({
        url: "/Service/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#service-settup-left-list").html(result);
        },
        error: function () {
            $("#service-settup-left-list").empty();
        }
    });
}

function Service_LoadInfo(code) {
    $.ajax({
        url: "/Service/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#service-settup-right-info").html(result);
            Service_SetSelect2(false);
        },
        error: function () {
            $("#service-settup-right-info").empty();
        }
    });
}

function Service_New() {
    $("#service-code").prop('disabled', false);
    $("#service-code").focus();
    Service_ResetInput();
    Service_HideButton(true, false, true, false);
}

function Service_Save() {
    var validate = ValidateInput('service-settup-right-info');
    if (validate) {
        var code = $("#service-code").val();
        var name = $("#service-name").val();
        var categoryid = $("#service-category").val();
        var printsampleid = $("#service-printsample").val();
        var processtype = $("#service-processtype").prop('checked');
        var printorder = $("#service-printorder").val();
        var active = $("#service-active").prop('checked');

        var data = {
            code: code,
            name: name,
            categoryid: categoryid,
            printsampleid: printsampleid,
            processtype: processtype,
            printorder: printorder,
            active: active
        };

        $.ajax({
            url: "/Service/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#service-code").prop('disabled', true);
                    Service_LoadList();
                    Service_HideButton(false, true, false, true);
                    Service_SetSelect2(false);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Service_Delete() {
    var choice = confirm("Bạn muốn xóa dịch vụ này ?");
    if (choice) {
        $("#service-code").prop('disabled', true);
        var code = $("#service-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn người dùng cần xóa");
        }
        else {
            $.ajax({
                url: "/Service/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Service_ResetInput();
                        Service_LoadList();
                        Service_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Service_Cancel() {
    $("#service-code").prop('disabled', true);
    var code = $("#service-code").val();
    if (code === "" || !code) {
        Service_ResetInput();
        Service_HideButton(false, true, true, true);
    }
    else {
        Service_LoadInfo(code);
        Service_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** Device

function Device_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#device-new").hide();
    }
    else {
        $("#device-new").show();
    }

    if (_save == 1) {
        $("#device-save").hide();
    }
    else {
        $("#device-save").show();
    }

    if (_delete == 1) {
        $("#device-delete").hide();
    }
    else {
        $("#device-delete").show();
    }

    if (_cancel == 1) {
        $("#device-cancel").hide();
    }
    else {
        $("#device-cancel").show();
    }
}

function Device_ResetInput() {
    $("#device-code").val("");
    $("#device-name").val("");
    $("#device-protocol").val("");
    $("#device-codebhyt").val("");
    $('input:checkbox').removeAttr('checked');
}

function Device_Search(value) {
    $.ajax({
        url: "/Device/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#device-settup-left-list").html(result);
        },
        error: function () {
            $("#device-settup-left-list").empty();
        }
    });
}

function Device_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Device_LoadInfo(code);
                Device_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Device_LoadList() {
    $.ajax({
        url: "/Device/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#device-settup-left-list").html(result);
        },
        error: function () {
            $("#device-settup-left-list").empty();
        }
    });
}

function Device_LoadInfo(code) {
    $.ajax({
        url: "/Device/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#device-settup-right-info").html(result);
        },
        error: function () {
            $("#device-settup-right-info").empty();
        }
    });
}

function Device_New() {
    $("#device-code").prop('disabled', false);
    $("#device-code").focus();
    Device_ResetInput();
    Device_HideButton(true, false, true, false);
}

function Device_Save() {
    var validate = ValidateInput('device-settup-right-info');
    if (validate) {
        var code = $("#device-code").val();
        var name = $("#device-name").val();
        var protocol = $("#device-protocol").val();
        var codebhyt = $("#device-codebhyt").val();
        var processqc = $("#device-processqc").prop('checked');
        var active = $("#device-active").prop('checked');

        var data = {
            code: code,
            name: name,
            protocol: protocol,
            codebhyt: codebhyt,
            processqc: processqc,
            active: active
        };

        $.ajax({
            url: "/Device/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#device-code").prop('disabled', true);
                    Device_LoadList();
                    Device_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Device_Delete() {
    var choice = confirm("Bạn muốn xóa thiết bị này ?");
    if (choice) {
        $("#device-code").prop('disabled', true);
        var code = $("#device-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui l?ng ch?n thi?t b? c?n xo?");
        }
        else {
            $.ajax({
                url: "/Device/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Device_ResetInput();
                        Device_LoadList();
                        Device_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Device_Cancel() {
    $("#device-code").prop('disabled', true);
    var code = $("#device-code").val();
    if (code === "" || !code) {
        Device_ResetInput();
        Device_HideButton(false, true, true, true);
    }
    else {
        Device_LoadInfo(code);
        Device_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** Sample

function Sample_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#sample-new").hide();
    }
    else {
        $("#sample-new").show();
    }

    if (_save == 1) {
        $("#sample-save").hide();
    }
    else {
        $("#sample-save").show();
    }

    if (_delete == 1) {
        $("#sample-delete").hide();
    }
    else {
        $("#sample-delete").show();
    }

    if (_cancel == 1) {
        $("#sample-cancel").hide();
    }
    else {
        $("#sample-cancel").show();
    }
}

function Sample_ResetInput() {
    $("#sample-code").val("");
    $("#sample-name").val("");
    CKEDITOR.instances["sample-description"].setData("");
    $("#sample-result").val("");
    $("#sample-suggest").val("");
    $("#sample-category").val("");
    $('input:checkbox').removeAttr('checked');
}

function Sample_Search(value) {
    $.ajax({
        url: "/Sample/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#sample-settup-left-list").html(result);
        },
        error: function () {
            $("#sample-settup-left-list").empty();
        }
    });
}

function Sample_CheckedBoxOnRow(code) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Sample_LoadInfo(code);
                Sample_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Sample_LoadList() {
    $.ajax({
        url: "/Sample/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#sample-settup-left-list").html(result);
        },
        error: function () {
            $("#sample-settup-left-list").empty();
        }
    });
}

function Sample_LoadInfo(code) {
    $.ajax({
        url: "/Sample/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#sample-settup-right-info").html(result);
        },
        error: function () {
            $("#sample-settup-right-info").empty();
        }
    });
}

function Sample_New() {
    CKEDITOR.replace("sample-description", { height: '240px' });
    $("#sample-code").prop('disabled', false);
    $("#sample-code").focus();
    $("#sample-active").prop("checked", true);
    Sample_ResetInput();
    Sample_HideButton(true, false, true, false);
}

function Sample_Save() {
    var validate = ValidateInput('sample-settup-right-info');
    if (validate) {
        var code = $("#sample-code").val();
        var name = $("#sample-name").val();
        var description = CKEDITOR.instances['sample-description'].getData();
        var result = $("#sample-result").val();
        var suggest = $("#sample-suggest").val();
        var categoryid = $("#sample-category").val();
        var active = $("#sample-active").prop('checked');
        var gender = $("input[name='sample-gender']:checked").val();
        var serviceid = $("#sample-service").val();

        var data = {
            code: code,
            name: name,
            description: description,
            result: result,
            suggest: suggest,
            categoryid: categoryid,
            gender: gender,
            serviceid: serviceid ? parseInt(serviceid) : 0,
            active: active
        };
        $.ajax({
            url: "/Sample/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#sample-code").prop('disabled', true);
                    Sample_LoadList();
                    Sample_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Sample_Delete() {
    var choice = confirm("Bạn muốn xóa mẫu kết quả này ?");
    if (choice) {
        $("#sample-code").prop('disabled', true);
        var code = $("#sample-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui l?ng ch?n m?u k?t qu? c?n xo?");
        }
        else {
            $.ajax({
                url: "/Sample/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Sample_ResetInput();
                        Sample_LoadList();
                        Sample_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Sample_Cancel() {
    $("#sample-code").prop('disabled', true);
    var code = $("#sample-code").val();
    if (code === "" || !code) {
        Sample_ResetInput();
        Sample_HideButton(false, true, true, true);
    }
    else {
        Sample_LoadInfo(code);
        Sample_HideButton(false, true, false, true);
    }
}

// ****************************************************************************** TestCode

$(function () {
    TestCode_SetSelect2(true);
})

function TestCode_SetSelect2(isLoadPage) {
    if (isLoadPage) {
        $('#testcode-category').val("");
    }
    $(document).ready(function () {
        $('#testcode-category').select2({
            placeholder: "-- Ch?n --"
        });
    });

    if (isLoadPage) {
        $('#testcode-testtype').val("");
    }
    $(document).ready(function () {
        $('#testcode-testtype').select2({
            placeholder: "-- Ch?n --"
        });
    });
}

function TestCode_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#testcode-new").hide();
    }
    else {
        $("#testcode-new").show();
    }

    if (_save == 1) {
        $("#testcode-save").hide();
    }
    else {
        $("#testcode-save").show();
    }

    if (_delete == 1) {
        $("#testcode-delete").hide();
    }
    else {
        $("#testcode-delete").show();
    }

    if (_cancel == 1) {
        $("#testcode-cancel").hide();
    }
    else {
        $("#testcode-cancel").show();
    }
}

function TestCode_ResetInput() {
    $("#testcode-code").val("");
    $("#testcode-name").val("");
    $('#testcode-testtype').val('');
    $('#testcode-category').val('');
    $('#testcode-normalrangem').val('');
    $('#testcode-normalrangef').val('');
    $('#testcode-normalresult').val('');
    $('#testcode-lowerlimit').val('');
    $('#testcode-higherlimit').val('');
    $('#testcode-lowerlimit-f').val('');
    $('#testcode-higherlimit-f').val('');
    $('#testcode-unit').val('');
    $('#testcode-printorder').val('');
    $('#testcode-codebhyt').val('');
    $('#testcode-namebhyt').val('');
    $('input:checkbox').removeAttr('checked');
    TestCode_SetSelect2(false);
}

function TestCode_Search(value) {
    $.ajax({
        url: "/TestCode/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#testcode-settup-left-list").html(result);
        },
        error: function () {
            $("#testcode-settup-left-list").empty();
        }
    });
}

function TestCode_CheckedBoxOnRow(id) {
    $(".row-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == id) {
                $(this).prop("checked", true);
                TestCode_LoadInfo(id);
                TestCode_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function TestCode_LoadList() {
    $.ajax({
        url: "/TestCode/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#testcode-settup-left-list").html(result);
        },
        error: function () {
            $("#testcode-settup-left-list").empty();
        }
    });
}

function TestCode_LoadInfo(id) {
    $.ajax({
        url: "/TestCode/GetInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#testcode-settup-right-info").html(result);
            TestCode_SetSelect2(false);
        },
        error: function () {
            $("#testcode-settup-right-info").empty();
        }
    });
}

function TestCode_New() {
    $("#testcode-code").prop('disabled', false);
    $("#testcode-code").focus();
    TestCode_ResetInput();
    TestCode_HideButton(true, false, true, false);
}

function TestCode_Save() {
    var validate = ValidateInput('testcode-settup-right-info');
    if (validate) {
        var code = $("#testcode-code").val();
        var name = $("#testcode-name").val();
        var testtypeid = $('#testcode-testtype').val();
        var categoryid = $('#testcode-category').val();
        var normalrangem = $('#testcode-normalrangem').val();
        var normalrangef = $('#testcode-normalrangef').val();
        var normalresult = $('#testcode-normalresult').val();
        var lowerlimit = $('#testcode-lowerlimit').val();
        var higherlimit = $('#testcode-higherlimit').val();
        var lowerlimitf = $('#testcode-lowerlimit-f').val();
        var higherlimitf = $('#testcode-higherlimit-f').val();
        var unit = $('#testcode-unit').val();
        var printorder = $('#testcode-printorder').val();
        var codebhyt = $('#testcode-codebhyt').val();
        var namebhyt = $('#testcode-namebhyt').val();
        var istesthead = $("#testcode-istesthead").prop('checked');
        var istestchild = $("#testcode-istestchild").prop('checked');
        var active = $("#testcode-active").prop('checked');

        var data = {
            code: code,
            name: name,
            testtypeid: testtypeid,
            categoryid: categoryid,
            normalrangem: normalrangem,
            normalrangef: normalrangef,
            normalresult: normalresult,
            lowerlimit: lowerlimit,
            higherlimit: higherlimit,
            lowerlimitf: lowerlimitf,
            higherlimitf: higherlimitf,
            unit: unit,
            printorder: printorder,
            codebhyt: codebhyt,
            namebhyt: namebhyt,
            istesthead: istesthead,
            istestchild: istestchild,
            active: active
        };

        $.ajax({
            url: "/TestCode/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#testcode-code").prop('disabled', true);
                    TestCode_LoadList();
                    TestCode_HideButton(false, true, false, true);
                    TestCode_SetSelect2(false);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function TestCode_Delete() {
    var choice = confirm("Bạn muốn xóa mã xét nghiệm này ?");
    if (choice) {
        $("#testcode-code").prop('disabled', true);
        var code = $("#testcode-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui l?ng ch?nm? xét nghiệm c?n xo?");
        }
        else {
            $.ajax({
                url: "/TestCode/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        TestCode_ResetInput();
                        TestCode_LoadList();
                        TestCode_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function TestCode_Cancel() {
    $("#testcode-code").prop('disabled', true);
    var code = $("#testcode-code").val();
    if (code === "" || !code) {
        TestCode_ResetInput();
        TestCode_HideButton(false, true, true, true);
    }
    else {
        TestCode_LoadInfo(code);
        TestCode_HideButton(false, true, false, true);
    }
}


// ****************************************************************************** ServiceTest

function ServiceTest_Search(value) {
    $.ajax({
        url: "/ServiceTest/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#servicetest-settup-left-list").html(result);
        },
        error: function () {
            $("#servicetest-settup-left-list").empty();
        }
    });
}

function ServiceTest_Search_AddTestCode(value) {
    $.ajax({
        url: "/ServiceTest/Search_AddTestCode?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#servicetest-table-addtestcode").html(result);
        },
        error: function () {
            $("#servicetest-table-addtestcode").empty();
        }
    });
}

function ServiceTest_Service_CheckedBoxOnRow(id) {
    $(".row-servicetest-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == id) {
                $(this).prop("checked", true);
                ServiceTest_LoadInfo(id);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function ServiceTest_TestCode_CheckedBoxOnRow(id) {
    $(".row-servicetest-testcode").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            var checked = $(this).prop('checked');
            if (value == id) {
                if (checked == true) {
                    $(this).prop("checked", false);
                }
                else {
                    $(this).prop("checked", true);
                }
            }
        })
    })
}

function ServiceTest_LoadInfo(id) {
    $.ajax({
        url: "/ServiceTest/GetInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#servicetest-settup-right-list").html(result);
        },
        error: function () {
            $("#servicetest-settup-right-list").empty();
        }
    });
}

function ServiceTest_Delete() {
    var choice = confirm("Bạn muốn xóa mã xét nghiệm này ?");
    if (choice) {
        var data = [];
        var serviceid = "";
        $(".row-servicetest-service").each(function () {
            $(this).find(".form-check-input").each(function () {
                var checked = $(this).prop('checked');
                if (checked == true) {
                    serviceid = $(this).val();
                }
            })
        })

        $(".row-servicetest-testcode").each(function () {
            $(this).find(".form-check-input").each(function () {
                var checked = $(this).prop('checked');
                if (checked == true) {
                    data.push({ serviceid: serviceid, testcodeid: $(this).val() });
                }
            })
        })


        if (data.length == 0) {
            SwalHelper.Toast.warning("Vui lòng chọn mã xét nghiệm cần xóa");
        }
        else {
            $.ajax({
                url: "/ServiceTest/Delete/",
                data: JSON.stringify(data),
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                type: "POST",
                success: function (result) {
                    if (result == 'True') {
                        ServiceTest_LoadInfo(serviceid);
                    }
                    else {
                        SwalHelper.Toast.warning("Dữ liệu đã được sử dụng nên không thể xoá");
                    }
                },
                error: function () {
                    SwalHelper.Toast.warning("Dữ liệu đã được sử dụng nên không thể xoá");
                }
            });
        }
    }
}

function ServiceTest_AddTestCode_CheckedBoxOnRow(id) {
    $(".row-servicetest-addtestcode").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            var checked = $(this).prop('checked');
            if (value == id) {
                if (checked == true) {
                    $(this).prop("checked", false);
                }
                else {
                    $(this).prop("checked", true);
                }
            }
        })
    })
}

function ServiceTest_Save() {
    var data = [];
    var serviceid = "";
    $(".row-servicetest-service").each(function () {
        $(this).find(".form-check-input").each(function () {
            var checked = $(this).prop('checked');
            if (checked == true) {
                serviceid = $(this).val();
            }
        })
    })

    $(".row-servicetest-addtestcode").each(function () {
        $(this).find(".form-check-input").each(function () {
            var checked = $(this).prop('checked');
            if (checked == true) {
                data.push({ serviceid: serviceid, testcodeid: $(this).val() });
            }
        })
    })

    if (data.length == 0) {
        SwalHelper.Toast.warning("Vui lòng chọn mã xét nghiệm !");
    }
    else {
        $.ajax({
            url: "/ServiceTest/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    ServiceTest_LoadInfo(serviceid);
                    // G?i h?m refresh d? b? checkbox c?c testcode dang ch?n
                    ServiceTest_Refresh_AddTestCode();
                }
                else {
                    SwalHelper.Toast.warning("M? xét nghiệm d? du?c ch?n. Vui l?ng ki?m tra l?i");
                    ServiceTest_Refresh_AddTestCode();
                }
            },
            error: function () {
                SwalHelper.Toast.warning("M? xét nghiệm d? du?c ch?n. Vui l?ng ki?m tra l?i");
                ServiceTest_Refresh_AddTestCode();
            }
        });
    }
}

function ServiceTest_Refresh_AddTestCode() {
    $(".row-servicetest-addtestcode").each(function () {
        $(this).find(".form-check-input").each(function () {
            $(this).prop("checked", false);
        })
    })
}

// ****************************************************************************** Map

function Map_Device_CheckedBoxOnRow(id) {
    $(".row-map-device").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == id) {
                $(this).prop("checked", true);
                Map_LoadInfo(id);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Map_TestCode_CheckedBoxOnRow(id) {
    $(".row-map-testcode").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            var checked = $(this).prop('checked');
            if (value == id) {
                if (checked == true) {
                    $(this).prop("checked", false);
                }
                else {
                    $(this).prop("checked", true);
                }
            }
        })
    })
}

function Map_LoadInfo(id) {
    $.ajax({
        url: "/Map/GetInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#map-settup-right-list").html(result);
        },
        error: function () {
            $("#map-settup-right-list").empty();
        }
    });
}

function Map_Delete() {
    var choice = confirm("Bạn muốn xóa mã xét nghiệm này ?");
    if (choice) {
        var deviceid = "";
        $(".row-map-device").each(function () {
            $(this).find(".form-check-input").each(function () {
                var checked = $(this).prop('checked');
                if (checked == true) {
                    deviceid = $(this).val();
                }
            })
        })

        var data = [];
        $(".row-map-testcode").each(function () {
            $(this).find(".form-check-input").each(function () {
                var checked = $(this).prop('checked');
                if (checked == true) {
                    data.push({ id: $(this).val() });
                }
            })
        })

        if (data.length == 0) {
            SwalHelper.Toast.warning("Vui lòng chọn mã xét nghiệm cần xóa");
        }
        else {
            $.ajax({
                url: "/Map/Delete/",
                data: JSON.stringify(data),
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                type: "POST",
                success: function (result) {
                    if (result == 'True') {
                        Map_LoadInfo(deviceid);
                    }
                    else {
                        SwalHelper.Toast.warning("Dữ liệu đã được sử dụng nên không thể xoá");
                    }
                },
                error: function () {
                    SwalHelper.Toast.warning("Dữ liệu đã được sử dụng nên không thể xoá");
                }
            });
        }
    }
}

function Map_AddTestCode_New() {
    var deviceid = "";
    $(".row-map-device").each(function () {
        $(this).find(".form-check-input").each(function () {
            var checked = $(this).prop('checked');
            if (checked == true) {
                deviceid = $(this).val();
            }
        })
    })

    if (deviceid === "" || !deviceid) {
        $('#addTestCode').modal('hide');
        SwalHelper.Toast.warning("Vui lòng chọn thiết bị !");
    }
    else {
        Map_SetSelect2();
        $("#map-addtestcode-id").val('');
        $("#map-addtestcode-testcode").val('');
        $("#map-addtestcode-testcodein").val('');
        $("#map-addtestcode-testcodein2").val('');
        $("#map-addtestcode-note").val('');
        $("#map-addtestcode-formatnumber").val('');
    }
}


function Map_AddTestCode_Edit() {
    var id = "";
    $(".row-map-testcode").each(function () {
        $(this).find(".form-check-input").each(function () {
            var checked = $(this).prop('checked');
            if (checked == true) {
                id = $(this).val();
            }
        })
    })

    if (id === "" || !id) {
        $('#addTestCode').modal('hide');
        SwalHelper.Toast.warning("Vui l?ng ch?n m? xét nghiệm m?y c?n s?a !");
    }
    else {
        Map_SetSelect2();
        $.ajax({
            url: "/Map/GetMap?id=" + id,
            type: "GET",
            dataType: "json",
            success: function (result) {
                $("#map-addtestcode-id").val(result.id);
                $('#map-addtestcode-testcode').val(result.testcodeid);
                $('#map-addtestcode-testcode').trigger('change');
                $("#map-addtestcode-testcodein").val(result.testcodein);
                $("#map-addtestcode-testcodein2").val(result.testcodein2);
                $("#map-addtestcode-note").val(result.note);
                $("#map-addtestcode-formatnumber").val(result.formatnumber);
            }
        });
    }
}

function Map_Save() {
    var validate = ValidateInput('map-addtestcode-content');
    if (validate) {
        var deviceid = "";
        $(".row-map-device").each(function () {
            $(this).find(".form-check-input").each(function () {
                var checked = $(this).prop('checked');
                if (checked == true) {
                    deviceid = $(this).val();
                }
            })
        })

        var id = $("#map-addtestcode-id").val();
        if (id === "" || !id) id = 0;
        var testcodeid = $("#map-addtestcode-testcode").val();
        var testcodein = $("#map-addtestcode-testcodein").val();
        var testcodein2 = $("#map-addtestcode-testcodein2").val();
        var note = $("#map-addtestcode-note").val();
        var formatnumber = $("#map-addtestcode-formatnumber").val();
        var data = { id: id, deviceid: deviceid, testcodeid: testcodeid, testcodein: testcodein, testcodein2: testcodein2, note: note, formatnumber: formatnumber };

        $.ajax({
            url: "/Map/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    Map_LoadInfo(deviceid);
                }
                else {
                    SwalHelper.Toast.warning("Không thể lưu. Vui lòng kiểm tra lại!");
                }
            },
            error: function () {
                SwalHelper.Toast.warning("Không thể lưu. Vui lòng kiểm tra lại!");
            }
        });
    }
    else {
        SwalHelper.Toast.warning("Không thể lưu. Vui lòng kiểm tra lại!");
    }
}


// ****************************************************************************** Setting

function Setting_HideButton(_new, _save, _delete, _cancel) {
    if (_new == 1) {
        $("#setting-new").hide();
    }
    else {
        $("#setting-new").show();
    }

    if (_save == 1) {
        $("#setting-save").hide();
    }
    else {
        $("#setting-save").show();
    }

    if (_delete == 1) {
        $("#setting-delete").hide();
    }
    else {
        $("#setting-delete").show();
    }

    if (_cancel == 1) {
        $("#setting-cancel").hide();
    }
    else {
        $("#setting-cancel").show();
    }
}

function Setting_ResetInput() {
    $("#setting-code").val("");
    $("#setting-name").val("");
    $("#setting-value").val("");
}

function Setting_Search(value) {
    $.ajax({
        url: "/Setting/Search?value=" + value,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#setting-settup-left-list").html(result);
        },
        error: function () {
            $("#setting-settup-left-list").empty();
        }
    });
}

function Setting_CheckedBoxOnRow(code) {
    $(".row-setting").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == code) {
                $(this).prop("checked", true);
                Setting_LoadInfo(code);
                Setting_HideButton(false, true, false, true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function Setting_LoadList() {
    $.ajax({
        url: "/Setting/GetList/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#setting-settup-left-list").html(result);
        },
        error: function () {
            $("#setting-settup-left-list").empty();
        }
    });
}

function Setting_LoadInfo(code) {
    $.ajax({
        url: "/Setting/GetInfo?code=" + code,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#setting-settup-right-info").html(result);
        },
        error: function () {
            $("#setting-settup-right-info").empty();
        }
    });
}

function Setting_New() {
    $("#setting-code").prop('disabled', false);
    $("#setting-code").focus();
    Setting_ResetInput();
    Setting_HideButton(true, false, true, false);
}

function Setting_Save() {
    var validate = ValidateInput('setting-settup-right-info');
    if (validate) {
        var code = $("#setting-code").val();
        var name = $("#setting-name").val();
        var value = $("#setting-value").val();

        var data = {
            code: code,
            name: name,
            value: value
        };

        $.ajax({
            url: "/Setting/Save/",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    $("#setting-code").prop('disabled', true);
                    Setting_LoadList();
                    Setting_HideButton(false, true, false, true);
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}

function Setting_Delete() {
    var choice = confirm("Bạn muốn xóa c?u h?nh n?y ?");
    if (choice) {
        $("#setting-code").prop('disabled', true);
        var code = $("#setting-code").val();
        if (code === "" || !code) {
            SwalHelper.Toast.warning("Vui lòng chọn cấu hình cần xoá");
        }
        else {
            $.ajax({
                url: "/Setting/Delete?code=" + code,
                type: "POST",
                dataType: "text",
                success: function (result) {
                    if (result == 'True') {
                        Setting_ResetInput();
                        Setting_LoadList();
                        Setting_HideButton(false, true, true, true);
                    }
                    else {
                        SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                    }
                },
                error: function () {
                    SwalHelper.Toast.error("Dữ liệu đã được sử dụng nên không thể xoá. Chỉ có thể InActive");
                }
            });
        }
    }
}

function Setting_Cancel() {
    $("#setting-code").prop('disabled', true);
    var code = $("#setting-code").val();
    if (code === "" || !code) {
        Setting_ResetInput();
        Setting_HideButton(false, true, true, true);
    }
    else {
        Setting_LoadInfo(code);
        Setting_HideButton(false, true, false, true);
    }
}

function Setting_Start_AutoTask() {
    $.ajax({
        url: "/Setting/Setting_Start_AutoTask/",
        type: "GET",
        dataType: "text",
        success: function (result) {
            if (result == 'False') {
                SwalHelper.Toast.warning("Không thể Start Auto Task. Vui lòng kiểm tra lại!");
            }
            else {
                SwalHelper.Toast.warning("Start Auto Task thành công !");
            }
        },
        error: function () {
            SwalHelper.Toast.warning("Không thể Start Auto Task. Vui lòng kiểm tra lại!");
        }
    });
}

function Setting_Stop_AutoTask() {
    $.ajax({
        url: "/Setting/Setting_Stop_AutoTask/",
        type: "GET",
        dataType: "text",
        success: function (result) {
            if (result == 'False') {
                SwalHelper.Toast.warning("Không thể Stop Auto Task. Vui lòng kiểm tra lại!");
            }
            else {
                SwalHelper.Toast.warning("Stop Auto Task thành công !");
            }
        },
        error: function () {
            SwalHelper.Toast.warning("Không thể Stop Auto Task. Vui lòng kiểm tra lại!");
        }
    });
}

// ****************************************************************************** Data

function ResultStandard_CheckedBoxOnRow(id) {
    $(".row-resultstandard").each(function () {
        $(this).find(".form-check-input").each(function () {
            var value = $(this).val();
            if (value == id) {
                $(this).prop("checked", true);
            }
            else {
                $(this).prop("checked", false);
            }
        })

    })
}

function ResultStandard_Search() {
    var from = $("#resultstandard-from").val();
    var to = $("#resultstandard-to").val();
    var deviceid = $("#resultstandard-device").val();
    var seq = $("#resultstandard-seq").val();

    $.ajax({
        url: "/Data/Search?" + "from=" + from + "&to=" + to + "&deviceid=" + deviceid + "&seq=" + seq,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#resultstandard-settup-left-list").html(result);
        },
        error: function () {
            $("#resultstandard-settup-left-list").empty();
        }
    });
}

function ResultStandard_UpdateStatus() {
    var from = $("#resultstandard-from").val();
    var to = $("#resultstandard-to").val();
    var seq = $("#resultstandard-seq").val();
    var status = $("#resultstandard-status").val();

    if (seq == "" || !seq || status == "" || !status) {
        SwalHelper.Toast.warning("Vui lòng nhập Seq và Status cần cập nhật !");
    }
    else {
        $.ajax({
            url: "/Data/UpdateStatus?from=" + from + "&to=" + to + "&seq=" + seq + "&status=" + status,
            dataType: "text",
            type: "POST",
            success: function (result) {
                if (result == 'True') {
                    ResultStandard_Search();
                }
                else {
                    SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
                }
            },
            error: function () {
                SwalHelper.Toast.error("Lưu không thành công. Vui lòng kiểm tra lại");
            }
        });
    }
}