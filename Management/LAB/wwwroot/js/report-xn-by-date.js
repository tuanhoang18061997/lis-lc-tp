// XN theo ngày: dùng JSON + virtual rows. Không dùng PartialView chứa hàng nghìn <tr>.
(function () {
    "use strict";
    var columns = ["Ngày", "STT", "HỌ TÊN", "GIỚI", "TUỔI", "ĐỐI TƯỢNG", "ĐỊA CHỈ", "YÊU CẦU", "NGƯỜI ĐỌC"];
    var rowHeight = 35;
    var overscan = 9;
    var groups = [];
    var filterTimer = null;

    function el(tag, className, value) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (value !== undefined && value !== null) node.textContent = String(value);
        return node;
    }
    function norm(value) { return String(value || "").toLocaleLowerCase("vi-VN").normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/đ/g, "d").trim(); }
    function dateText(value) {
        var iso = String(value || "").substring(0, 10);
        return iso.length === 10 ? iso.substring(8, 10) + "/" + iso.substring(5, 7) + "/" + iso.substring(0, 4) : iso;
    }
    function scrollSpacer(count) {
        var tr = el("tr", "xn-date-spacer");
        var td = el("td");
        td.colSpan = 9;
        td.style.height = Math.max(0, count * rowHeight) + "px";
        tr.appendChild(td);
        return tr;
    }
    function dataRow(row, index, day) {
        var tr = el("tr", "xn-date-data-row");
        var values = [dateText(day), index + 1, row.patientName || "", row.gender || "", row.ageValue === null || row.ageValue === undefined ? "" : row.ageValue, row.objectName || "", row.address || "", row.serviceName || "", row.readerName || ""];
        values.forEach(function (value, i) { tr.appendChild(el("td", i === 1 ? "xn-date-stt" : "", value)); });
        return tr;
    }
    function renderWindow(group) {
        if (group.collapsed) return;
        var visibleHeight = group.viewport.clientHeight || 420;
        var scrollTop = group.viewport.scrollTop;
        var start = Math.max(0, Math.floor(scrollTop / rowHeight) - overscan);
        var count = Math.ceil(visibleHeight / rowHeight) + 2 * overscan;
        var end = Math.min(group.filtered.length, start + count);
        var frag = document.createDocumentFragment();
        if (start > 0) frag.appendChild(scrollSpacer(start));
        for (var i = start; i < end; i++) frag.appendChild(dataRow(group.filtered[i], i, group.date));
        if (end < group.filtered.length) frag.appendChild(scrollSpacer(group.filtered.length - end));
        group.tbody.replaceChildren(frag);
    }
    function setCollapsed(group, collapsed) {
        group.collapsed = collapsed;
        group.viewport.hidden = collapsed;
        group.toggle.textContent = collapsed ? "Mở rộng" : "Thu gọn";
        if (!collapsed) renderWindow(group);
    }
    function makeGroup(date, rows) {
        var container = el("section", "xn-date-group");
        var header = el("div", "xn-date-group-header");
        var title = el("strong", "xn-date-group-title", "NGÀY THỰC HIỆN: " + dateText(date));
        var badge = el("span", "xn-date-count", rows.length + " dòng");
        var toggle = el("button", "xn-date-toggle", "Thu gọn");
        toggle.type = "button";
        header.append(title, badge, toggle);
        var viewport = el("div", "xn-date-viewport");
        var table = el("table", "xn-date-table");
        var thead = el("thead");
        var tr = el("tr");
        columns.forEach(function (name) { tr.appendChild(el("th", "", name)); });
        thead.appendChild(tr);
        var tbody = el("tbody");
        table.append(thead, tbody);
        viewport.appendChild(table);
        container.append(header, viewport);
        var group = { date: date, rows: rows, filtered: rows.slice(), viewport: viewport, tbody: tbody, container: container, toggle: toggle, badge: badge, collapsed: false };
        var pending = false;
        viewport.addEventListener("scroll", function () {
            if (pending) return;
            pending = true;
            requestAnimationFrame(function () { pending = false; renderWindow(group); });
        }, { passive: true });
        toggle.addEventListener("click", function () { setCollapsed(group, !group.collapsed); });
        renderWindow(group);
        return group;
    }
    function filterRows() {
        var term = norm(document.getElementById("xn-date-patient").value);
        var gender = norm(document.getElementById("xn-date-gender").value);
        var obj = norm(document.getElementById("xn-date-object").value);
        var address = norm(document.getElementById("xn-date-address").value);
        var service = norm(document.getElementById("xn-date-service").value);
        var reader = norm(document.getElementById("xn-date-reader").value);
        var total = 0;
        groups.forEach(function (group) {
            group.filtered = group.rows.filter(function (r) {
                return (!term || norm(r.patientName).includes(term)) && (!gender || norm(r.gender) === gender) && (!obj || norm(r.objectName).includes(obj)) && (!address || norm(r.address).includes(address)) && (!service || norm(r.serviceName).includes(service)) && (!reader || norm(r.readerName).includes(reader));
            });
            group.container.hidden = group.filtered.length === 0;
            group.badge.textContent = group.filtered.length + " / " + group.rows.length + " dòng";
            group.viewport.scrollTop = 0;
            group.viewport.style.height = Math.min(420, 40 + group.filtered.length * rowHeight) + "px";
            total += group.filtered.length;
            renderWindow(group);
        });
        document.getElementById("xn-date-total").textContent = "Đang hiển thị " + total.toLocaleString("vi-VN") + " dòng";
    }
    function renderReport(data) {
        groups = [];
        var root = document.getElementById("report-result");
        root.replaceChildren();
        var wrapper = el("div", "xn-date-report");
        var toolbar = el("div", "xn-date-toolbar");
        function field(id, placeholder, values) {
            var node = values ? el("select", "xn-date-filter") : el("input", "xn-date-filter");
            node.id = id;
            if (values) values.forEach(function (choice) { var option = el("option", "", choice[1]); option.value = choice[0]; node.appendChild(option); });
            else { node.type = "search"; node.placeholder = placeholder; }
            node.setAttribute("aria-label", placeholder);
            node.addEventListener(values ? "change" : "input", function () { clearTimeout(filterTimer); filterTimer = setTimeout(filterRows, 130); });
            toolbar.appendChild(node);
        }
        field("xn-date-patient", "Tên khách hàng");
        field("xn-date-gender", "Giới tính", [["", "Mọi giới tính"], ["nam", "Nam"], ["nu", "Nữ"]]);
        field("xn-date-object", "Đối tượng");
        field("xn-date-address", "Địa chỉ");
        field("xn-date-service", "Dịch vụ");
        field("xn-date-reader", "Người đọc");
        var actions = el("div", "xn-date-actions");
        var openAll = el("button", "btn btn-outline-secondary btn-sm", "Mở tất cả"); openAll.type = "button";
        var closeAll = el("button", "btn btn-outline-secondary btn-sm", "Thu gọn tất cả"); closeAll.type = "button";
        var total = el("span", "xn-date-total", ""); total.id = "xn-date-total";
        openAll.addEventListener("click", function () { groups.forEach(function (g) { if (!g.container.hidden) setCollapsed(g, false); }); });
        closeAll.addEventListener("click", function () { groups.forEach(function (g) { setCollapsed(g, true); }); });
        actions.append(openAll, closeAll, total);
        wrapper.append(toolbar, actions);
        var list = el("div", "xn-date-groups");
        (data.dateGroups || []).forEach(function (day) { var group = makeGroup(day.date, day.rows || []); groups.push(group); list.appendChild(group.container); });
        if (!groups.length) list.appendChild(el("p", "xn-date-no-results", "Không có dữ liệu trong khoảng thời gian đã chọn."));
        wrapper.appendChild(list);
        root.appendChild(wrapper);
        filterRows();
    }
    window.Report_XNByDateClear = function () { groups = []; clearTimeout(filterTimer); };
    $(document).on("change.xnByDate", "#report-time-from, #report-time-to", function () {
        if ($("#report-type").val() !== "5") return;
        if (window.reportSearchRequest && window.reportSearchRequest.readyState !== 4) window.reportSearchRequest.abort();
        window.reportHasResults = false;
        Report_UpdateExportButton();
        Report_SetStatus("ready", "Bộ lọc đã thay đổi");
        Report_SetEmptyState("Cần tải lại báo cáo", "Nhấn Xem để tải dữ liệu theo khoảng thời gian mới.");
    });
    window.Report_XNByDateSearch = function (from, to) {
        if (!from || !to || from > to) { SwalHelper.Toast.warning("Vui lòng chọn khoảng thời gian hợp lệ!"); return; }
        if (window.reportSearchRequest && window.reportSearchRequest.readyState !== 4) window.reportSearchRequest.abort();
        window.reportHasResults = false;
        Report_UpdateExportButton();
        Report_SetStatus("loading", "Đang tải báo cáo theo ngày...");
        $("#report-search").prop("disabled", true);
        window.reportSearchRequest = $.ajax({
            url: "/Report_BaoCaoThongKe/GetXNByDateReport", type: "GET", dataType: "json", cache: false, global: false,
            data: { from: from, to: to },
            success: function (payload) {
                if ($("#report-type").val() !== "5") return;
                if (!payload || !payload.success || !Array.isArray(payload.dateGroups)) { Report_SetStatus("error", "Response không hợp lệ"); SwalHelper.Toast.error("Response báo cáo không hợp lệ."); return; }
                var count = payload.dateGroups.reduce(function (acc, group) { return acc + (group.rows || []).length; }, 0);
                if (count !== payload.totalRows) { Report_SetStatus("error", "Dữ liệu không đầy đủ"); SwalHelper.Toast.error("Tổng số dòng không khớp với dữ liệu."); return; }
                renderReport(payload);
                window.reportHasResults = count > 0;
                Report_SetStatus("loaded", count.toLocaleString("vi-VN") + " lượt dịch vụ / " + payload.dateGroups.length + " ngày");
                Report_UpdateExportButton();
            },
            error: function (xhr, status) {
                if (status === "abort" || $("#report-type").val() !== "5") return;
                Report_SetStatus("error", "Lỗi tải dữ liệu");
                Report_SetEmptyState("Không thể tải báo cáo theo ngày", "Kiểm tra kết nối, khoảng ngày hoặc log SQL Server.");
                SwalHelper.Toast.error("Không tải được báo cáo XN theo ngày.");
            },
            complete: function () { if ($("#report-type").val() === "5") $("#report-search").prop("disabled", false); }
        });
    };
})();
