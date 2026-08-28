(function () {
    'use strict';

    const state = { rows: [] };
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

        return '<span class="text-muted small">Không cần mở</span>';
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

    function renderRows(rows) {
        const $body = $('#tool-table tbody').empty();

        rows.forEach((row, index) => {
            const status = statusOf(row);
            const reason = row.unlockReason
                ? `<div class="unlock-note" title="${escapeHtml(row.unlockReason)}">${escapeHtml(row.unlockedByName)}: ${escapeHtml(row.unlockReason)}</div>`
                : '';

            $body.append(`
                <tr>
                    <td class="stt-cell">${index + 1}</td>

                    <td class="status-cell">
                        <span class="status-badge ${status.css}">
                            <i class="bi ${status.icon}"></i> ${status.text}
                        </span>
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
                    <td class="action-cell">${actionButton(row, index)}</td>
                </tr>`);
        });

        $('#tool-count').text(`${rows.length} kết quả`);
        $('#tool-empty').toggleClass('d-none', rows.length !== 0);
    }

    function errorMessage(xhr) {
        return xhr.responseJSON?.message
            || xhr.responseText
            || 'Không thực hiện được thao tác.';
    }

    function search() {
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
                renderRows(state.rows);
                applyColumnFilters();
            })
            .fail(function (xhr) {
                Swal.fire('Không thể tìm kiếm', errorMessage(xhr), 'error');
            })
            .always(function () {
                $('#tool-search')
                    .prop('disabled', false)
                    .html('<i class="bi bi-search"></i> Tìm kiếm');
            });
    }

    async function unlock(index) {
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

                const cellText = $row
                    .children()
                    .eq(Number(columnIndex))
                    .text()
                    .toLowerCase();

                return cellText.includes(filterValue);
            });

            $row.toggle(visible);
        });
    }

    $(function () {
        $('#tool-search, #tool-refresh').on('click', search);

        $('#tool-keyword').on('keydown', function (event) {
            if (event.key === 'Enter') {
                search();
            }
        });

        $('.column-filters input').on('input', applyColumnFilters);

        $('#tool-table').on('click', '.btn-unlock', function () {
            unlock(parseInt($(this).data('index'), 10));
        });

        $('#tool-table').on('click', '.btn-lock', function () {
            lock(parseInt($(this).data('index'), 10));
        });

        search();
    });
})();
