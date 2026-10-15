(function () {
    'use strict';

    const state = { rows: [], selected: new Set(), busy: false, searching: false };
    const token = () => $('input[name="__RequestVerificationToken"]').val();

    function escapeHtml(value) {
        return $('<div>').text(value == null ? '' : value).html();
    }

    function formatDate(value) {
        if (!value) return '';

        const date = new Date(value);
        return Number.isNaN(date.getTime())
            ? ''
            : date.toLocaleString('vi-VN');
    }

    function statusOf(row) {
        if (!row.isLockedByDate) {
            return {
                text: 'Chưa khóa theo ngày',
                css: 'status-today',
                icon: 'bi-unlock'
            };
        }

        if (row.hasActiveUnlock && row.unlockWasUsed) {
            return {
                text: 'Đang cho phép sửa',
                css: 'status-editing',
                icon: 'bi-pencil-square'
            };
        }

        if (row.hasActiveUnlock) {
            return {
                text: 'Đã mở - chờ Invalid',
                css: 'status-open',
                icon: 'bi-unlock-fill'
            };
        }

        return {
            text: 'Đã khóa',
            css: 'status-locked',
            icon: 'bi-lock-fill'
        };
    }

    function actionButton(row, index) {
        if (row.hasActiveUnlock) {
            return `
                <button class="btn btn-sm btn-lock" data-index="${index}">
                    <i class="bi bi-lock-fill"></i> Khóa
                </button>`;
        }

        if (row.isLockedByDate) {
            return `
                <button class="btn btn-sm btn-unlock" data-index="${index}">
                    <i class="bi bi-key-fill"></i> Mở khóa
                </button>`;
        }

        // Trạng thái "Chưa khóa theo ngày" đã đủ rõ, không thêm nhãn thao tác thừa.
        return '';
    }

    function getServiceNames(row) {
        const serviceNames = Array.isArray(row.serviceNames) && row.serviceNames.length > 0
            ? row.serviceNames
            : (row.serviceName ? [row.serviceName] : []);

        return [...new Set(
            serviceNames
                .map(service => (service || '').trim())
                .filter(Boolean)
        )];
    }

    function renderPatientInfo(row) {
        return `
            <div class="patient-info">
                <div class="patient-name" title="${escapeHtml(row.patientName)}">
                    ${escapeHtml(row.patientName)}
                </div>

                <div class="patient-meta">
                    <span class="patient-meta-item">
                        <span class="patient-meta-label">Mã BN:</span>
                        <span class="patient-code-value">${escapeHtml(row.patientCode)}</span>
                    </span>

                    <span class="patient-meta-separator">•</span>

                    <span class="patient-meta-item">
                        <span class="patient-meta-label">BA/SID:</span>
                        <span class="patient-record-value">${escapeHtml(row.medicalRecordCode)}</span>
                    </span>
                </div>
                ${row.maDotKham ? `
                <div class="patient-meta">
                    <span class="patient-meta-item">
                        <span class="patient-meta-label">Đợt khám</span>
                        <span class="patient-record-value">${escapeHtml(row.maDotKham)}</span>
                    </span>
                </div>
                ` : ''}
            </div>`;
    }

    function renderServiceGrid(row, mode) {
        const services = getServiceNames(row);
        const isModal = mode === 'modal';

        if (services.length === 0) {
            return '<span class="text-muted">Không có dịch vụ</span>';
        }

        const gridClass = isModal ? 'swal-service-grid' : 'service-grid';
        const badgeClass = isModal ? 'swal-service-badge' : 'service-badge';
        const numberClass = isModal ? 'swal-service-number' : 'service-badge-number';
        const nameClass = isModal ? 'swal-service-name' : 'service-badge-name';

        return `
            <div class="${gridClass}">
                ${services.map((service, index) => `
                    <div class="${badgeClass}" title="${escapeHtml(service)}">
                        <span class="${numberClass}">${index + 1}</span>
                        <span class="${nameClass}">${escapeHtml(service)}</span>
                    </div>
                `).join('')}
            </div>`;
    }

    function renderUnlockSummary(row) {
        const serviceCount = getServiceNames(row).length;

        return `
            <div class="swal-form-summary">
                <div class="swal-patient-info">
                    <strong>${escapeHtml(row.patientName)}</strong>
                    <div class="swal-patient-meta">
                        <span>Mã BN: <b>${escapeHtml(row.patientCode)}</b></span>
                        <span>BA/SID: <b>${escapeHtml(row.medicalRecordCode)}</b></span>
                    </div>
                </div>

                <div class="swal-module-row">
                    <span class="module-badge">${escapeHtml(row.moduleCode)}</span>
                    <span class="swal-service-count">${serviceCount} dịch vụ</span>
                </div>

                ${renderServiceGrid(row, 'modal')}
            </div>`;
    }

    function visibleIndexes() {
        // Filter cột hiện dùng .toggle() trên TR. Không đọc state.rows toàn bộ.
        return $('#tool-table tbody tr').filter(function () {
            return this.style.display !== 'none';
        }).map(function () {
            return Number($(this).attr('data-index'));
        }).get().filter(Number.isInteger);
    }

    function updateSelectionUi() {
        const visible = visibleIndexes();
        const visibleSet = new Set(visible);
        // Không giữ lựa chọn ẩn khi đổi filter: batch không bao giờ unlock dòng bị ẩn.
        for (const index of state.selected) {
            if (!visibleSet.has(index)) state.selected.delete(index);
        }

        $('#tool-table tbody .tool-row-select').each(function () {
            const index = Number($(this).data('index'));
            $(this).prop('checked', state.selected.has(index));
            $(this).closest('tr').toggleClass('tool-row-selected', state.selected.has(index));
        });

        const selectedCount = state.selected.size;
        const totalVisible = visible.length;
        const allCheckbox = $('#tool-select-all');
        allCheckbox
            .prop('checked', totalVisible > 0 && selectedCount === totalVisible)
            .prop('indeterminate', selectedCount > 0 && selectedCount < totalVisible)
            .prop('disabled', state.busy || state.searching || totalVisible === 0);

        $('#tool-selected-count').text(`Đã chọn ${selectedCount} / ${totalVisible} dòng đang hiển thị`);
        $('#tool-bulk-count').text(`(${selectedCount})`);
        $('#tool-bulk-unlock').prop('disabled', state.busy || state.searching || selectedCount === 0);
        $('#tool-count').text(`${totalVisible} / ${state.rows.length} kết quả`);
        $('#tool-empty').toggleClass('d-none', totalVisible !== 0);
    }

    function setBusy(busy) {
        state.busy = busy;
        $('#tool-search, #tool-refresh, #tool-keyword, #tool-module, #tool-from-date, #tool-to-date')
            .prop('disabled', busy);
        $('.column-filters input, #tool-table .btn-lock, #tool-table .btn-unlock, .tool-row-select')
            .prop('disabled', busy);
        updateSelectionUi();
    }

    function renderRows(rows) {
        const $body = $('#tool-table tbody').empty();

        rows.forEach((row, index) => {
            const status = statusOf(row);
            const reason = row.unlockReason
                ? `<div class="unlock-note" title="${escapeHtml(row.unlockReason)}">${escapeHtml(row.unlockedByName)}: ${escapeHtml(row.unlockReason)}</div>`
                : '';

            $body.append(`
                <tr data-index="${index}">
                    <td class="select-cell">
                        <input type="checkbox" class="form-check-input tool-checkbox tool-row-select"
                               data-index="${index}" aria-label="Chọn dòng ${index + 1}" />
                    </td>
                    <td class="stt-cell">${index + 1}</td>

                    <td class="status-cell">
                        <div class="status-main">
                            <span class="status-badge ${status.css}">
                                <i class="bi ${status.icon}"></i> ${status.text}
                            </span>
                            ${actionButton(row, index)}
                        </div>
                        ${reason}
                    </td>

                    <td class="patient-info-cell">
                        ${renderPatientInfo(row)}
                    </td>

                    <td class="service-cell">
                        ${renderServiceGrid(row, 'table')}
                    </td>

                    <td class="module-cell">
                        <span class="module-badge">${escapeHtml(row.moduleCode)}</span>
                    </td>

                    <td class="return-time-cell">${formatDate(row.returnResultTime)}</td>
                    <td class="expire-time-cell">${row.hasActiveUnlock ? formatDate(row.unlockExpireAt) : ''}</td>
                </tr>`);
        });

        updateSelectionUi();
    }

    function errorMessage(xhr) {
        return xhr.responseJSON?.message
            || xhr.responseText
            || 'Không thực hiện được thao tác.';
    }

    function search() {
        if (state.busy || state.searching) return;
        const fromDate = $('#tool-from-date').val();
        const toDate = $('#tool-to-date').val();

        if (!fromDate || !toDate) {
            Swal.fire(
                'Thiếu ngày',
                'Vui lòng chọn đủ từ ngày và đến ngày trả kết quả.',
                'warning'
            );
            return;
        }

        state.searching = true;
        updateSelectionUi();
        $('#tool-search')
            .prop('disabled', true)
            .html('<span class="spinner-border spinner-border-sm"></span> Đang tìm');

        $.getJSON('/ToolAdmin/Search', {
            keyword: $('#tool-keyword').val(),
            moduleCode: $('#tool-module').val(),
            fromDate: fromDate,
            toDate: toDate
        })
            .done(function (response) {
                state.rows = response.data || [];
                state.selected.clear();
                renderRows(state.rows);
                applyColumnFilters();
            })
            .fail(function (xhr) {
                Swal.fire('Không thể tìm kiếm', errorMessage(xhr), 'error');
            })
            .always(function () {
                state.searching = false;
                $('#tool-search')
                    .prop('disabled', false)
                    .html('<i class="bi bi-search"></i> Tìm kiếm');
                updateSelectionUi();
            });
    }

    async function unlock(index) {
        if (state.busy || state.searching) return;
        const row = state.rows[index];

        const result = await Swal.fire({
            title: 'Mở khóa kết quả',
            html: `
                ${renderUnlockSummary(row)}

                <label class="swal-form-label">
                    Lý do mở khóa <span class="text-danger">*</span>
                </label>

                <textarea
                    id="unlock-reason"
                    class="swal2-textarea"
                    maxlength="1000"
                    placeholder="Nhập lý do cần sửa kết quả..."
                ></textarea>

                <label class="swal-form-label">Thời gian cho phép (phút)</label>

                <input
                    id="unlock-minutes"
                    type="number"
                    min="5"
                    max="1440"
                    value="60"
                    class="swal2-input"
                >`,
            showCancelButton: true,
            confirmButtonText: 'Mở khóa',
            cancelButtonText: 'Hủy',
            focusConfirm: false,
            preConfirm: function () {
                const reason = $('#unlock-reason').val().trim();
                const minutes = parseInt($('#unlock-minutes').val(), 10);

                if (reason.length < 5) {
                    Swal.showValidationMessage('Lý do phải có ít nhất 5 ký tự.');
                    return false;
                }

                if (!Number.isInteger(minutes) || minutes < 5 || minutes > 1440) {
                    Swal.showValidationMessage('Thời gian phải từ 5 đến 1440 phút.');
                    return false;
                }

                return {
                    reason: reason,
                    durationMinutes: minutes
                };
            }
        });

        if (!result.isConfirmed) return;

        const payload = {
            targetType: row.targetType,
            patientId: row.patientTableId,
            resultCDHAId: row.resultCDHAId,
            moduleCode: row.moduleCode,
            reason: result.value.reason,
            durationMinutes: result.value.durationMinutes
        };

        $.ajax({
            url: '/ToolAdmin/Unlock',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            headers: { RequestVerificationToken: token() },
            data: JSON.stringify(payload)
        })
            .done(function (response) {
                Swal.fire('Đã mở khóa', response.message, 'success');
                search();
            })
            .fail(function (xhr) {
                Swal.fire('Không thể mở khóa', errorMessage(xhr), 'error');
            });
    }

    async function lock(index) {
        if (state.busy || state.searching) return;
        const row = state.rows[index];

        const result = await Swal.fire({
            title: 'Khóa lại kết quả?',
            text: 'User sẽ không thể Invalid hoặc tiếp tục lưu thay đổi sau khi khóa.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Khóa lại',
            cancelButtonText: 'Hủy'
        });

        if (!result.isConfirmed) return;

        $.ajax({
            url: '/ToolAdmin/Lock',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            headers: { RequestVerificationToken: token() },
            data: JSON.stringify({
                targetType: row.targetType,
                patientId: row.patientTableId,
                resultCDHAId: row.resultCDHAId,
                moduleCode: row.moduleCode,
                reason: 'Admin khóa lại từ Tools Admin'
            })
        })
            .done(function (response) {
                Swal.fire('Đã khóa', response.message, 'success');
                search();
            })
            .fail(function (xhr) {
                Swal.fire('Không thể khóa', errorMessage(xhr), 'error');
            });
    }

    function applyColumnFilters() {
        const filters = {};

        $('.column-filters input').each(function () {
            const columnIndex = parseInt($(this).data('column'), 10);
            const filterValue = ($(this).val() || '').toLowerCase().trim();

            if (!Number.isNaN(columnIndex)) {
                filters[columnIndex] = filterValue;
            }
        });

        $('#tool-table tbody tr').each(function () {
            const $row = $(this);

            const visible = Object.entries(filters).every(([columnIndex, filterValue]) => {
                if (!filterValue) return true;

                const $cell = $row.children().eq(Number(columnIndex));
                // Chỉ lọc theo tên trạng thái, KHÔNG lấy text của nút Khóa/Mở khóa
                // hay ghi chú. Nếu không, nhập "khóa" sẽ khớp nhầm mọi dòng.
                const cellText = Number(columnIndex) === 2
                    ? $cell.find('.status-badge').text().toLowerCase()
                    : $cell.text().toLowerCase();

                return cellText.includes(filterValue);
            });

            $row.toggle(visible);
        });
        updateSelectionUi();
    }

    function createTarget(row) {
        return {
            targetType: row.targetType,
            patientId: row.patientTableId,
            resultCDHAId: row.resultCDHAId,
            moduleCode: row.moduleCode
        };
    }

    async function bulkUnlock() {
        if (state.busy || state.searching || state.selected.size === 0) return;
        // Chốt snapshot các dòng đang hiện. Nếu filter thay đổi thì selection đã tự clear dòng ẩn.
        const selectedIndexes = visibleIndexes().filter(index => state.selected.has(index));
        if (selectedIndexes.length === 0) return;
        const selectedRows = selectedIndexes.map(index => state.rows[index]);
        const readyRows = selectedRows.filter(row => row.isLockedByDate && !row.hasActiveUnlock);
        const readyCount = readyRows.length;
        const otherCount = selectedRows.length - readyCount;

        const confirmation = await Swal.fire({
            title: `Mở khóa ${selectedRows.length} dòng đã chọn`,
            html: `
                <div class="swal-batch-summary">
                    <div><strong>${readyCount}</strong> dòng có thể mở khóa theo dữ liệu đang hiển thị.</div>
                    ${otherCount > 0 ? `<div class="text-muted">${otherCount} dòng đã mở hoặc chưa bị khóa sẽ được bỏ qua, không gọi mở khóa lại.</div>` : ''}
                    <div class="small text-muted mt-2">XN mở theo lần xét nghiệm; CDHA mở theo từng dịch vụ. Các dòng được xử lý độc lập.</div>
                </div>
                <label class="swal-form-label" for="bulk-unlock-reason">Lý do mở khóa <span class="text-danger">*</span></label>
                <textarea id="bulk-unlock-reason" class="swal2-textarea" maxlength="1000"
                          placeholder="Nhập lý do cần sửa kết quả..."></textarea>
                <label class="swal-form-label" for="bulk-unlock-minutes">Thời gian cho phép (phút)</label>
                <input id="bulk-unlock-minutes" type="number" min="5" max="1440" value="60" class="swal2-input">`,
            showCancelButton: true,
            confirmButtonText: `Mở khóa ${selectedRows.length} dòng`,
            cancelButtonText: 'Hủy',
            focusConfirm: false,
            preConfirm: function () {
                const reason = ($('#bulk-unlock-reason').val() || '').trim();
                const minutes = Number($('#bulk-unlock-minutes').val());
                if (reason.length < 5 || reason.length > 1000) {
                    Swal.showValidationMessage('Lý do phải có từ 5 đến 1000 ký tự.');
                    return false;
                }
                if (!Number.isInteger(minutes) || minutes < 5 || minutes > 1440) {
                    Swal.showValidationMessage('Thời gian phải từ 5 đến 1440 phút.');
                    return false;
                }
                return { reason, durationMinutes: minutes };
            }
        });
        if (!confirmation.isConfirmed) return;

        const batchSize = 25; // chia lượt, không gửi một request hàng nghìn dòng.
        let successful = 0, failed = 0, skipped = otherCount, processed = 0;
        const errors = [];
        let interrupted = '';

        setBusy(true);
        const $button = $('#tool-bulk-unlock');
        try {
            for (let offset = 0; offset < readyRows.length; offset += batchSize) {
                const chunkRows = readyRows.slice(offset, offset + batchSize);
                $button.html(`<span class="spinner-border spinner-border-sm"></span> Đang xử lý ${processed}/${readyRows.length}`);
                let response;
                try {
                    response = await $.ajax({
                        url: '/ToolAdmin/BatchUnlock',
                        method: 'POST',
                        contentType: 'application/json; charset=utf-8',
                        headers: { RequestVerificationToken: token() },
                        data: JSON.stringify({
                            items: chunkRows.map(createTarget),
                            reason: confirmation.value.reason,
                            durationMinutes: confirmation.value.durationMinutes
                        })
                    });
                } catch (xhr) {
                    // Request lỗi có thể đã ghi DB một phần, KHÔNG tự retry.
                    interrupted = errorMessage(xhr);
                    break;
                }
                if (!response || response.success !== true || !Array.isArray(response.results)) {
                    interrupted = 'Phản hồi API không hợp lệ.';
                    break;
                }
                for (const result of response.results) {
                    const index = Number(result.index);
                    const row = chunkRows[index];
                    if (!row || !Number.isInteger(index)) continue;
                    if (result.success) successful++;
                    else if (result.skipped) skipped++;
                    else failed++;
                    if (!result.success && !result.skipped) {
                        errors.push(`${row.patientName || row.patientCode} (${row.moduleCode}): ${result.message || 'Không rõ lỗi'}`);
                    }
                }
                processed += chunkRows.length;
            }
        } finally {
            setBusy(false);
            $button.html('<i class="bi bi-key-fill"></i> Mở khóa các dòng đã chọn <span id="tool-bulk-count"></span>');
            updateSelectionUi();
        }

        const pending = readyRows.length - processed;
        const details = errors.slice(0, 15).map(value => `<li>${escapeHtml(value)}</li>`).join('');
        const resultHtml = `
            <div class="swal-batch-results">
                <div>Thành công: <strong>${successful}</strong></div>
                <div>Bỏ qua: <strong>${skipped}</strong></div>
                <div>Thất bại: <strong>${failed}</strong></div>
                ${pending > 0 ? `<div>Chưa xác nhận xử lý: <strong>${pending}</strong></div>` : ''}
                ${interrupted ? `<p class="text-danger mt-2">Lỗi giao tiếp: ${escapeHtml(interrupted)}. Có thể một phần dữ liệu đã được cập nhật; tải lại kiểm tra trước khi thao tác tiếp.</p>` : ''}
                ${details ? `<div class="mt-2"><strong>Chi tiết lỗi:</strong><ul>${details}</ul></div>` : ''}
                ${errors.length > 15 ? `<div>Còn ${errors.length - 15} lỗi chưa hiển thị.</div>` : ''}
            </div>`;
        await Swal.fire({
            title: interrupted || failed ? 'Hoàn tất có cảnh báo' : 'Đã xử lý mở khóa hàng loạt',
            html: resultHtml,
            icon: interrupted || failed ? 'warning' : 'success',
            confirmButtonText: 'Đóng và tải lại'
        });
        search();
    }

    $(function () {
        $('#tool-search, #tool-refresh').on('click', search);

        $('#tool-keyword').on('keydown', function (event) {
            if (event.key === 'Enter') {
                search();
            }
        });

        $('.column-filters input').on('input', function () {
            if (!state.busy) applyColumnFilters();
        });

        $('#tool-select-all').on('change', function () {
            if (state.busy || state.searching) return;
            const indexes = visibleIndexes();
            indexes.forEach(index => this.checked ? state.selected.add(index) : state.selected.delete(index));
            updateSelectionUi();
        });

        $('#tool-table').on('change', '.tool-row-select', function () {
            if (state.busy || state.searching) return;
            const index = Number($(this).data('index'));
            if (!Number.isInteger(index)) return;
            if (this.checked) state.selected.add(index);
            else state.selected.delete(index);
            updateSelectionUi();
        });

        $('#tool-bulk-unlock').on('click', bulkUnlock);

        $('#tool-table').on('click', '.btn-unlock', function () {
            unlock(parseInt($(this).data('index'), 10));
        });

        $('#tool-table').on('click', '.btn-lock', function () {
            lock(parseInt($(this).data('index'), 10));
        });

        search();
    });
})();
