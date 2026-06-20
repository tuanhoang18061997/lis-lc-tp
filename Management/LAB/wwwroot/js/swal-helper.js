/* swal-helper.js
 * SweetAlert2 wrapper utilities for reusable alerts, toasts, and confirms
 * Requires: SweetAlert2
 */

const SwalHelper = (() => {
    // ========== Toast Configuration ==========
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    // ========== Toast Methods ==========
    function showToast(type, message, timer = 1000) {
        return Toast.fire({
            icon: type,
            title: message,
            timer: timer
        });
    }

    function toastSuccess(message, timer = 1000) {
        return showToast('success', message, timer);
    }

    function toastError(message, timer = 1000) {
        return showToast('error', message, timer);
    }

    function toastWarning(message, timer = 1000) {
        return showToast('warning', message, timer);
    }

    function toastInfo(message, timer = 1000) {
        return showToast('info', message, timer);
    }

    // ========== Alert Methods ==========
    function showAlert(type, title, message = '') {
        return Swal.fire({
            icon: type,
            title: title,
            text: message,
            confirmButtonText: 'OK',
            confirmButtonColor: '#3085d6'
        });
    }

    function alertSuccess(title, message = '') {
        return showAlert('success', title, message);
    }

    function alertError(title, message = '') {
        return showAlert('error', title, message);
    }

    function alertWarning(title, message = '') {
        return showAlert('warning', title, message);
    }

    function alertInfo(title, message = '') {
        return showAlert('info', title, message);
    }

    // ========== Confirm Dialog ==========
    function confirm(title, message = '', options = {}) {
        const defaults = {
            title: title,
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Hủy',
            reverseButtons: true
        };

        return Swal.fire({ ...defaults, ...options });
    }

    // ========== Prompt Dialog (Input) ==========
    function prompt(title, inputType = 'text', options = {}) {
        const defaults = {
            title: title,
            input: inputType,
            inputPlaceholder: 'Nhập thông tin...',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            inputValidator: (value) => {
                if (!value) {
                    return 'Vui lòng nhập thông tin!';
                }
            }
        };

        return Swal.fire({ ...defaults, ...options });
    }

    // ========== Loading Dialog ==========
    function showLoading(title = 'Đang xử lý...', message = '') {
        return Swal.fire({
            title: title,
            text: message,
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    }

    function hideLoading() {
        Swal.close();
    }

    // ========== Custom Alert with HTML ==========
    function showCustom(options) {
        return Swal.fire(options);
    }

    // ========== Auto-close Alert ==========
    function showAutoClose(type, title, timer = 2000) {
        return Swal.fire({
            icon: type,
            title: title,
            timer: timer,
            showConfirmButton: false,
            timerProgressBar: true
        });
    }

    // Public API
    return {
        // Toast
        Toast: {
            show: showToast,
            success: toastSuccess,
            error: toastError,
            warning: toastWarning,
            info: toastInfo
        },
        // Alert
        Alert: {
            show: showAlert,
            success: alertSuccess,
            error: alertError,
            warning: alertWarning,
            info: alertInfo
        },
        // Confirm
        confirm: confirm,
        // Prompt
        prompt: prompt,
        // Loading
        Loading: {
            show: showLoading,
            hide: hideLoading
        },
        // Auto-close
        autoClose: showAutoClose,
        // Custom
        custom: showCustom,
        // Direct access to Swal
        Swal: Swal
    };
})();

// Expose to window for global access
window.SwalHelper = SwalHelper;