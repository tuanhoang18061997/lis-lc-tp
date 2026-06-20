// ===============================
// BarcodeScan - Service quét barcode dùng chung
// ===============================
(function (w, $) {
    if (!w || !$) return;

    const _timers = new WeakMap();
    const _attached = new WeakSet();
    const _defaults = {
        minLen: 4,            // Độ dài tối thiểu để coi là barcode hợp lệ
        idleMs: 200,          // Thời gian "ngưng nhập" để tự trigger nếu scanner không gửi Enter
        triggerOnEnter: true, // Gặp phím Enter thì trigger ngay
        selectAfter: true,    // Sau khi trigger sẽ select toàn bộ text (quét tiếp cho nhanh)
        clearAfter: false     // Sau khi trigger thì xóa input
    };

    function _resolveCallback(cbOrName) {
        if (typeof cbOrName === 'function') return cbOrName;
        if (typeof cbOrName === 'string' && cbOrName.trim() && typeof w[cbOrName] === 'function') {
            return w[cbOrName];
        }
        return null;
    }

    function _readOptionsFromAttrs(el) {
        const o = {};
        if (el.hasAttribute('data-scan-minlen')) o.minLen = +el.getAttribute('data-scan-minlen') || _defaults.minLen;
        if (el.hasAttribute('data-scan-idle')) o.idleMs = +el.getAttribute('data-scan-idle') || _defaults.idleMs;
        if (el.hasAttribute('data-scan-enter')) o.triggerOnEnter = el.getAttribute('data-scan-enter') !== 'false';
        if (el.hasAttribute('data-scan-select')) o.selectAfter = el.getAttribute('data-scan-select') !== 'false';
        if (el.hasAttribute('data-scan-clear')) o.clearAfter = el.getAttribute('data-scan-clear') === 'true';
        return o;
    }

    function _mergeOptions(a, b) {
        return $.extend({}, _defaults, a || {}, b || {});
    }

    function _trigger(el, cb, opts) {
        const v = (el.value || '').trim();
        if (!v || v.length < opts.minLen) return;
        try { cb && cb(v, el); } catch (e) { console.warn('Barcode callback error:', e); }
        if (opts.clearAfter) el.value = '';
        if (opts.selectAfter && !opts.clearAfter) setTimeout(() => el.select(), 0);
    }

    function _attach(el, cb, options) {
        if (!el || _attached.has(el)) return;
        const opts = _mergeOptions(_readOptionsFromAttrs(el), options);
        const callback = _resolveCallback(cb) || _resolveCallback(el.getAttribute('data-scan-callback'));

        if (!callback) {
            console.warn('BarcodeScan: thiếu callback cho', el);
            return;
        }

        const onKeydown = function (e) {
            clearTimeout(_timers.get(el));
            if (opts.triggerOnEnter && e.key === 'Enter') {
                e.preventDefault();
                _trigger(el, callback, opts);
                return;
            }
            const t = setTimeout(() => _trigger(el, callback, opts), opts.idleMs);
            _timers.set(el, t);
        };

        const onPaste = function () {
            clearTimeout(_timers.get(el));
            setTimeout(() => _trigger(el, callback, opts), 50);
        };

        const onFocus = function () {
            if (opts.selectAfter) setTimeout(() => el.select(), 0);
        };

        // Lưu handlers để có thể gỡ sau này
        el._barcodeHandlers = { onKeydown, onPaste, onFocus };
        el.addEventListener('keydown', onKeydown);
        el.addEventListener('paste', onPaste);
        el.addEventListener('focus', onFocus);

        _attached.add(el);
    }

    function _detach(el) {
        if (!el || !_attached.has(el) || !el._barcodeHandlers) return;
        clearTimeout(_timers.get(el));
        el.removeEventListener('keydown', el._barcodeHandlers.onKeydown);
        el.removeEventListener('paste', el._barcodeHandlers.onPaste);
        el.removeEventListener('focus', el._barcodeHandlers.onFocus);
        delete el._barcodeHandlers;
        _attached.delete(el);
    }

    // Public API
    const API = {
        /**
         * Đăng ký cho 1 selector hoặc element
         * @param {string|Element|jQuery} selector
         * @param {function|string} callback - hàm hoặc tên hàm global (ví dụ 'GetSample_Search')
         * @param {object} options - {minLen, idleMs, triggerOnEnter, selectAfter, clearAfter}
         */
        register: function (selector, callback, options) {
            const $els = selector && selector.jquery ? selector : $(selector);
            $els.each(function () { _attach(this, callback, options); });
            // Tự động attach cho các element tạo mới phù hợp selector khi focus
            $(document).on('focus', selector, function () { _attach(this, callback, options); });
        },

        /**
         * Tự động gắn theo data-attributes
         * - data-scan-callback="TenHamGlobal"
         * - data-scan-minlen / data-scan-idle / data-scan-enter / data-scan-select / data-scan-clear
         */
        autowire: function (selector) {
            selector = selector || '[data-scan-callback]';
            this.register(selector, null, null);
        },

        /** Gỡ đăng ký */
        unregister: function (selector) {
            const $els = selector && selector.jquery ? selector : $(selector);
            $els.each(function () { _detach(this); });
        }
    };

    w.BarcodeScan = API;

})(window, window.jQuery);
