// ============================================================================
// Password Validation Utilities (dùng chung cho toàn bộ project)
// ============================================================================

var PasswordValidation = (function () {

    /**
     * Kiểm tra chuỗi có chứa ký tự liên tục tăng/giảm không
     * ví dụ: abc, bcd, 123, 321, cba...
     * @param {string} str - chuỗi cần kiểm tra
     * @param {number} minSeqLength - độ dài chuỗi liên tục tối thiểu để chặn
     * @returns {boolean}
     */
    function hasSequentialChars(str, minSeqLength) {
        if (!str || str.length < minSeqLength) return false;

        var s = str.toLowerCase();

        for (var i = 0; i <= s.length - minSeqLength; i++) {
            var part = s.substring(i, i + minSeqLength);

            // chỉ kiểm tra với chữ cái hoặc số
            if (/^[a-z]+$/.test(part) || /^[0-9]+$/.test(part)) {
                var isAsc = true;
                var isDesc = true;

                for (var j = 0; j < part.length - 1; j++) {
                    var current = part.charCodeAt(j);
                    var next = part.charCodeAt(j + 1);

                    if (next !== current + 1) {
                        isAsc = false;
                    }

                    if (next !== current - 1) {
                        isDesc = false;
                    }
                }

                if (isAsc || isDesc) {
                    return true;
                }
            }
        }

        return false;
    }

    /**
     * Kiểm tra password có đạt tất cả rule hay không
     * @param {string} password - mật khẩu cần kiểm tra
     * @param {string} [username] - tên đăng nhập (tuỳ chọn, dùng để kiểm tra không chứa username)
     * @returns {boolean}
     */
    function isPasswordValid(password, username) {
        // Bắt buộc nhập
        if (!password || password.length === 0) return false;

        // Tối thiểu 8 ký tự
        if (password.length < 8) return false;

        // Ký tự thường
        if (!/[a-z]/.test(password)) return false;

        // Ký tự hoa
        if (!/[A-Z]/.test(password)) return false;

        // Ký tự số
        if (!/[0-9]/.test(password)) return false;

        // Ký tự đặc biệt
        if (!/[!@#$%^&*()_\-+=\[{\]};:'",<.>\/?\\|`~]/.test(password)) return false;

        // Không chứa username
        if (username && password.toLowerCase().includes(username.toLowerCase())) return false;

        // Không chứa chuỗi ký tự liên tục
        if (hasSequentialChars(password, 3)) return false;

        return true;
    }

    /**
     * Đặt trạng thái rule (pass/fail) lên element
     * @param {string} element - jQuery selector
     * @param {boolean} isValid
     */
    function setRule(element, isValid) {
        if (isValid) {
            $(element)
                .removeClass("rule-fail")
                .addClass("rule-ok");
        } else {
            $(element)
                .removeClass("rule-ok")
                .addClass("rule-fail");
        }
    }

    /**
     * Validate realtime và cập nhật UI cho các rule element
     * @param {string} password - mật khẩu
     * @param {string} [username] - tên đăng nhập
     * @param {object} [selectors] - tuỳ chỉnh selector cho từng rule (tuỳ chọn)
     */
    function validateRealtime(password, username, selectors) {
        var sel = $.extend({
            required: "#required-pass",
            minLength: "#minLength-pass",
            lower: "#character-lower",
            upper: "#character-upper",
            number: "#character-isNumber",
            special: "#character-special",
            notIncludeUsername: "#notIncluded-username-pass",
            noSequential: "#khong-chua-chuoi-ky-tu-lien-tuc-pass"
        }, selectors);

        setRule(sel.required, password && password.length > 0);
        setRule(sel.minLength, password.length >= 8);
        setRule(sel.lower, /[a-z]/.test(password));
        setRule(sel.upper, /[A-Z]/.test(password));
        setRule(sel.number, /[0-9]/.test(password));
        setRule(sel.special, /[!@#$%^&*()_\-+=\[{\]};:'",<.>\/?\\|`~]/.test(password));

        if (username) {
            var valid = !password.toLowerCase().includes(username.toLowerCase());
            setRule(sel.notIncludeUsername, valid);
        }

        setRule(sel.noSequential, !hasSequentialChars(password, 3));
    }

    /**
     * Kiểm tra và cập nhật trạng thái khớp giữa password mới và nhập lại password
     * @param {string} password - mật khẩu mới
     * @param {string} confirmPassword - nhập lại mật khẩu mới
     * @param {string} [selector] - jQuery selector cho element hiển thị trạng thái (default: "#password-match")
     * @returns {boolean}
     */
    function checkPasswordMatch(password, confirmPassword, selector) {
        selector = selector || "#password-match";
        var isMatch = password.length > 0
            && confirmPassword.length > 0
            && password === confirmPassword;
        setRule(selector, isMatch);
        return isMatch;
    }

    /**
     * Toggle ẩn/hiện password cho 1 input
     * @param {string} inputSelector - jQuery selector cho input password
     * @param {string} iconSelector - jQuery selector cho icon element
     * @param {string} [showClass] - class icon khi hiện password (default: "bi-eye-slash-fill")
     * @param {string} [hideClass] - class icon khi ẩn password (default: "bi-eye-fill")
     */
    function togglePassword(inputSelector, iconSelector, showClass, hideClass) {
        var input = $(inputSelector);
        var icon = $(iconSelector);
        showClass = showClass || "bi-eye-slash-fill";
        hideClass = hideClass || "bi-eye-fill";

        if (input.attr("type") === "password") {
            input.attr("type", "text");
            icon.removeClass(hideClass).addClass(showClass);
        } else {
            input.attr("type", "password");
            icon.removeClass(showClass).addClass(hideClass);
        }
    }

    /**
     * Toggle ẩn/hiện password theo context element (dùng cho nhiều ô password trên cùng 1 trang)
     * Tự tìm input và icon trong cùng .input-group cha
     * @param {HTMLElement} toggleElement - element được click (span.toggle-password)
     * @param {string} [showClass] - class icon khi hiện password (default: "bi-eye")
     * @param {string} [hideClass] - class icon khi ẩn password (default: "bi-eye-slash")
     */
    function togglePasswordByElement(toggleElement, showClass, hideClass) {
        var $parent = $(toggleElement).closest('.input-group');
        var $input = $parent.find('input');
        var $icon = $(toggleElement).find('i');
        showClass = showClass || "bi-eye";
        hideClass = hideClass || "bi-eye-slash";

        if ($input.attr("type") === "password") {
            $input.attr("type", "text");
            $icon.removeClass(hideClass).addClass(showClass);
        } else {
            $input.attr("type", "password");
            $icon.removeClass(showClass).addClass(hideClass);
        }
    }

    // Public API
    return {
        hasSequentialChars: hasSequentialChars,
        isPasswordValid: isPasswordValid,
        setRule: setRule,
        validateRealtime: validateRealtime,
        checkPasswordMatch: checkPasswordMatch,
        togglePassword: togglePassword,
        togglePasswordByElement: togglePasswordByElement
    };

})();