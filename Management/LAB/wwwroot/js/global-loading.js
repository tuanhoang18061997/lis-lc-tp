window.AppLoading = (function () {
    var overlayId = 'globalLoadingOverlay';
    var textId = 'globalLoadingText';
    var counter = 0;

    function getOverlay() {
        return document.getElementById(overlayId);
    }

    function getTextElement() {
        return document.getElementById(textId);
    }

    function show(message) {
        var overlay = getOverlay();
        var textElement = getTextElement();

        if (!overlay) return;

        counter++;

        if (textElement) {
            textElement.textContent = message || 'Đang xử lý, vui lòng chờ...';
        }

        overlay.classList.remove('d-none');
        document.body.classList.add('loading-active');
    }

    function hide(force) {
        var overlay = getOverlay();
        if (!overlay) return;

        if (force === true) {
            counter = 0;
        } else {
            counter = Math.max(0, counter - 1);
        }

        if (counter === 0) {
            overlay.classList.add('d-none');
            document.body.classList.remove('loading-active');
        }
    }

    function reset() {
        counter = 0;
        var overlay = getOverlay();
        if (!overlay) return;

        overlay.classList.add('d-none');
        document.body.classList.remove('loading-active');
    }

    return {
        show: show,
        hide: hide,
        reset: reset
    };
})();