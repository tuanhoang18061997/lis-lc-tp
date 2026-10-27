$(document).ajaxStart(function () {
    AppLoading.show('Đang xử lý...');
});

$(document).ajaxStop(function () {
    AppLoading.hide(true);
});
window.reportProcessCache = null;
// Filter theo tên dịch vụ chỉ áp dụng bảng tổng hợp loại 1.
function toggleServiceNameInput() {
    var enabled = $("#report-type").val() === "1" && $("#report-result #table-export-to-excel tbody tr").length > 0;
    $("#report-service-name").prop("disabled", !enabled);
    if (!enabled) $("#report-service-name").val("");
}

// Load danh mục xét nghiệm từ database
function Report_LoadXNCategories(forceReload) {
    var $categorySelect = $("#report-category-code");

    if ($categorySelect.length === 0) {
        return;
    }

    // Không gọi lại API nếu danh mục đã được load
    if (!forceReload && $categorySelect.data("loaded") === true) {
        $categorySelect.prop("disabled", false);
        return;
    }

    $categorySelect
        .prop("disabled", true)
        .empty()
        .append(new Option("-- Đang tải danh mục --", ""));

    $.ajax({
        url: "/Report_BaoCaoThongKe/GetXNReportCategories",
        type: "GET",
        dataType: "json",
        cache: false,
        success: function (response) {
            $categorySelect.empty();

            $categorySelect.append(
                new Option("-- Tất cả danh mục --", "")
            );

            if (Array.isArray(response) && response.length > 0) {
                response.forEach(function (item) {
                    // Hỗ trợ cả camelCase và PascalCase
                    var code = item.code || item.Code || "";
                    var name = item.name || item.Name || code;

                    if (code) {
                        $categorySelect.append(
                            new Option(name, code)
                        );
                    }
                });

                $categorySelect.data("loaded", true);
                $categorySelect.prop("disabled", false);
            } else {
                $categorySelect.append(
                    new Option("-- Không có dữ liệu danh mục --", "")
                );

                $categorySelect.prop("disabled", true);
            }
        },
        error: function (xhr) {
            console.error(
                "Không tải được danh mục xét nghiệm:",
                xhr.responseText
            );

            $categorySelect
                .empty()
                .append(
                    new Option("-- Không tải được danh mục --", "")
                )
                .prop("disabled", true);

            SwalHelper.Toast.error(
                "Không tải được danh mục xét nghiệm!"
            );
        }
    });
}
// Danh mục chỉ có tác dụng với báo cáo Theo dịch vụ của khoa XN.
function toggleReportCategoryFilter() {
    var isXNService = $("#report-type").val() === "XN-SERVICE-DETAIL" || ($("#report-type").val() === "1" && $("#report-location").val() === "XN");
    $("#report-category-wrapper").prop("hidden", !isXNService);
    if (isXNService) Report_LoadXNCategories(false);
    else $("#report-category-code").val("").prop("disabled", true);
}

function filterServiceByName() {
    var filterValue = String($("#report-service-name").val() || "").toLocaleLowerCase().trim();
    $("#report-result #table-export-to-excel tbody tr").each(function () {
        var row = $(this);
        if (row.hasClass("tr-content")) return;
        var serviceName = row.find("td").eq(1).text().toLocaleLowerCase();
        row.toggle(!filterValue || serviceName.includes(filterValue));
    });
}

var reportSearchRequest = null;
var reportHasResults = false;
var reportXNRows = [];
var reportXNVirtual = null;
var reportXNExporting = false;
var reportTitleDescriptions = { "1": "Theo dịch vụ", "2": "Theo số ca", "3": "Công thực hiện", "4": "XN theo dịch vụ", "5": "XN theo ngày", "XN-SERVICE-DETAIL": "Thống kê dịch vụ XN" };

function Report_SetEmptyState(title, message) {
    $("#report-result").empty().append($("<div>", { "class": "report-empty-state" })
        .append($("<i>", { "class": "bi bi-clipboard-data", "aria-hidden": "true" }))
        .append($("<h3>").text(title || "Chưa có dữ liệu hiển thị"))
        .append($("<p>").text(message || "Chọn thời gian và nhấn Xem báo cáo để tải dữ liệu.")));
}

function Report_SetStatus(status, label) {
    var note = label || (status === "loaded" ? "Đã tải kết quả" : status === "loading" ? "Đang tải dữ liệu" : status === "error" ? "Lỗi tải dữ liệu" : "Chưa tải dữ liệu");
    $("#report-result-note").text(note);
}

function Report_UpdateExportButton() {
    var type = $("#report-type").val();
    var canExport = reportHasResults && !reportXNExporting && ["1", "2", "3", "4", "5", "XN-SERVICE-DETAIL"].includes(type);
    $("#report-excel").prop("disabled", !canExport);
}

function Report_ApplyMode(type) {
    var item = $(".report-menu-item").filter(function () { return $(this).attr("data-report-type") === type; }).first();
    if (!item.length) return;
    if (reportSearchRequest && reportSearchRequest.readyState !== 4) reportSearchRequest.abort();
    reportSearchRequest = null;
    reportHasResults = false;
    reportXNRows = [];
    reportXNVirtual = null;
    if (typeof Report_XNByDateClear === "function") Report_XNByDateClear();
    $("#report-result").removeClass("report-result--xn");
    window.reportProcessCache = null;
    $("#report-type").val(type);
    $(".report-menu-item").removeClass("active").attr("aria-pressed", "false");
    item.addClass("active").attr("aria-pressed", "true");
    $("#report-current-title").text(item.attr("data-title") || reportTitleDescriptions[type]);
    $("#report-current-group").text(item.closest("[data-report-group]").find(".report-menu-label").text().includes("XÉT NGHIỆM") ? "Xét nghiệm" : "Tổng hợp");
    var requiresLocation = ["1", "2", "3"].includes(type);
    $("#report-location-wrapper").prop("hidden", !requiresLocation);
    $("#report-location").toggleClass("valid", requiresLocation).prop("required", requiresLocation);
    $("#report-service-wrapper").prop("hidden", type !== "1");
    $("#report-madotkham-wrapper, #report-xn-completed-wrapper").prop("hidden", type !== "XN-SERVICE-DETAIL");
    $("#report-search").prop("disabled", false);
    toggleReportCategoryFilter();
    toggleServiceNameInput();
    Report_UpdateExportButton();
    Report_SetStatus("ready", "Sẵn sàng");
    if (type === "XN-SERVICE-DETAIL") {
        Report_SetEmptyState("Chọn Mã Đợt Khám để xem báo cáo", "Báo cáo hiển thị đầy đủ 15 cột. Dữ liệu của đợt khám sẽ được tải toàn bộ, không phân trang.");
        Report_SetStatus("ready", "Chưa tải dữ liệu");
        $("#report-excel").attr("title", "Xuất đầy đủ 15 cột theo template Excel");
    } else {
        $("#report-excel").attr("title", "Xuất Excel");
        Report_SetEmptyState("Chưa có dữ liệu hiển thị", "Chọn khoảng thời gian và nhấn “Xem báo cáo” để tải dữ liệu.");
    }
    $("#report-shell").removeClass("report-nav-open");
    Report_SyncSidebarToggle();
}

function Report_InitMaDotKham() {
    var select = $("#report-ma-dot-kham");
    if (!select.length || typeof $.fn.select2 !== "function") {
        $("#report-madotkham-hint").text("Không khởi tạo được bộ lọc Select2.");
        return;
    }
    var url = "/Report_BaoCaoThongKe/GetMaDotKhamOptions";
    select.select2({
        width: "100%", placeholder: "-- Chọn Mã Đợt Khám --", allowClear: true, minimumInputLength: 0, dropdownCssClass: "report-madotkham-dropdown", ajax: {
            url: url, type: "GET", dataType: "json", delay: 300, global: false,
            data: function (params) { return { term: params.term || "" }; },
            processResults: function (response) { return { results: Array.isArray(response) ? response : [] }; },
            cache: true
        }
    });
    // Dropdown được Select2 gắn ngoài report-shell; thêm class khi mở để CSS không ảnh hưởng Select2 của module khác.
    select.on("select2:open", function () { $(".select2-container--open .select2-dropdown").addClass("report-madotkham-dropdown"); });
    select.on("select2:select select2:clear", function () { select.next(".select2-container").find(".select2-selection__rendered").attr("title", select.find("option:selected").text() || "Chọn Mã Đợt Khám"); });
    // Nạp trước 100 mã; người dùng vẫn tìm kiếm được các mã cũ ở API.
    $.ajax({
        url: url, type: "GET", dataType: "json", global: false, cache: true,
        success: function (items) {
            if (!Array.isArray(items)) return;
            items.forEach(function (item) { if (item.id && !select.find("option").filter(function () { return this.value === item.id; }).length) select.append(new Option(item.text, item.id)); });
            select.trigger("change.select2");
        },
        error: function (xhr) {
            console.error("Lỗi lấy danh sách Mã Đợt Khám:", xhr.responseText);
            $("#report-madotkham-hint").text("Không tải được mã gần đây. Có thể nhập từ khóa để thử lại.");
        }
    });
}

function Report_SyncSidebarToggle() {
    var mobile = window.matchMedia("(max-width: 900px)").matches;
    var expanded = mobile ? $("#report-shell").hasClass("report-nav-open") : !$("#report-shell").hasClass("report-nav-collapsed");
    var label = expanded ? "Thu gọn danh mục báo cáo" : "Mở danh mục báo cáo";
    $("#report-menu-toggle").attr({ "aria-expanded": expanded ? "true" : "false", "aria-label": label, "title": label }).find("i").attr("class", expanded ? "bi bi-layout-sidebar-inset" : "bi bi-layout-sidebar");
    $("#report-sidebar-collapse").attr("aria-expanded", expanded ? "true" : "false");
}

function Report_FilterMenu() {
    var term = String($("#report-menu-search").val() || "").toLocaleLowerCase().trim();
    var found = 0;
    $("[data-report-group]").each(function () {
        var count = 0;
        $(this).find(".report-menu-item").each(function () {
            var matched = $(this).text().toLocaleLowerCase().includes(term);
            $(this).prop("hidden", !matched);
            if (matched) { count++; found++; }
        });
        $(this).prop("hidden", count === 0);
    });
    $("#report-menu-empty").prop("hidden", found > 0);
}

$(document).ready(function () {
    if (!$("#report-shell").length) return;
    Report_ApplyMode("1");
    Report_InitMaDotKham();
    $("#report-menu-search").on("input", Report_FilterMenu);
    $("#report-service-name").on("input", filterServiceByName);
    $("#report-location").on("change", toggleReportCategoryFilter);
    $("#report-time-from, #report-time-to, #report-category-code, #report-xn-completed, #report-ma-dot-kham").on("change", Report_XNInvalidateChangedFilter);
    $("#report-menu").on("click", ".report-menu-item", function () { Report_ApplyMode($(this).attr("data-report-type")); });
    $("#report-menu-toggle, #report-sidebar-collapse").on("click", function () {
        var mobile = window.matchMedia("(max-width: 900px)").matches;
        $("#report-shell").toggleClass(mobile ? "report-nav-open" : "report-nav-collapsed");
        Report_SyncSidebarToggle();
    });
    $(window).on("resize", Report_SyncSidebarToggle);
    Report_SyncSidebarToggle();
    $("#report-reset").on("click", function () {
        var today = new Date();
        var isoDate = today.getFullYear() + "-" + String(today.getMonth() + 1).padStart(2, "0") + "-" + String(today.getDate()).padStart(2, "0");
        $("#report-time-from, #report-time-to").val(isoDate);
        $("#report-location").val("XN");
        $("#report-category-code, #report-ma-dot-kham").val("");
        $("#report-xn-completed").val("true");
        $("#report-ma-dot-kham").trigger("change.select2");
        $("#report-service-name").val("");
        Report_ApplyMode($("#report-type").val());
    });
    $("#report-excel").on("click", Report_Excel);
});

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

// ****************************************************************************** Data
function Report_Search() {
    var validate = ValidateInput('report-option-search');
    if (validate) {
        var from = $("#report-time-from").val();
        var to = $("#report-time-to").val();
        var type = $("#report-type").val();
        var location = $("#report-location").val();
        //var userid = $("#report-user").val();
        //var hospitalId = $("#report-hospital").val();
        var userid = 20;
        var hospitalId = null;

        $.ajax({
            url: "/Report_BaoCaoThongKe/Search?" + "from=" + from + "&to=" + to + "&type=" + type + "&location=" + location + "&userid=" + userid + "&hospitalId=" + hospitalId,
            type: "GET",
            dataType: "html",
            cache: false,
            success: function (result) {
                $("#report-result").html(result);
            },
            error: function () {
                $("#report-result").empty();
            }
        });
    }
}

function Report_Search_New() {
    var from = $("#report-time-from").val();
    var to = $("#report-time-to").val();
    var type = $("#report-type").val();
    var requiresLocation = ["1", "2", "3"].includes(type);
    var location = requiresLocation ? String($("#report-location").val() || "").trim().toUpperCase() : "";
    if (type === "XN-SERVICE-DETAIL") { Report_XNServiceStatisticsSearch(from, to); return; }
    if (type === "5") { Report_XNByDateSearch(from, to); return; }
    if (!from || !to || from > to) { SwalHelper.Toast.warning("Vui lòng chọn khoảng thời gian hợp lệ!"); return; }
    if (requiresLocation && !location) { SwalHelper.Toast.warning("Vui lòng chọn chuyên khoa!"); $("#report-location").focus(); return; }
    var categoryCode = type === "1" && location === "XN" ? String($("#report-category-code").val() || "").trim().toUpperCase() : "";
    if (reportSearchRequest && reportSearchRequest.readyState !== 4) reportSearchRequest.abort();
    reportHasResults = false;
    Report_UpdateExportButton();
    Report_SetStatus("loading", "Đang tải");
    $("#report-search").prop("disabled", true);
    reportSearchRequest = $.ajax({
        url: "/Report_BaoCaoThongKe/LC_Search", type: "GET", dataType: "html", cache: false,
        data: { from: from, to: to, type: type, location: location, userid: 20, categoryCode: categoryCode },
        success: function (html) {
            if ($("#report-type").val() !== type) return;
            $("#report-result").html(html);
            reportHasResults = true;
            Report_SetStatus("loaded", "Đã tải");
            toggleServiceNameInput();
            Report_UpdateExportButton();
        },
        error: function (xhr, status) {
            if (status === "abort" || $("#report-type").val() !== type) return;
            Report_SetStatus("error", "Lỗi tải dữ liệu");
            Report_SetEmptyState("Không tải được báo cáo", "Kiểm tra kết nối hoặc thử lại với khoảng thời gian khác.");
            SwalHelper.Toast.error("Không tải được dữ liệu báo cáo!");
        },
        complete: function () { if ($("#report-type").val() === type) $("#report-search").prop("disabled", false); }
    });
}

function Report_Excel() {
    var type = $("#report-type").val();
    if (type === "XN-SERVICE-DETAIL") { Report_XNExportExcel(); return; }
    if (!reportHasResults) { SwalHelper.Toast.warning("Vui lòng xem báo cáo trước khi xuất Excel!"); return; }
    if (type === "5") {
        var from = encodeURIComponent($("#report-time-from").val());
        var to = encodeURIComponent($("#report-time-to").val());
        window.open("/Report_BaoCaoThongKe/ExportPatientServicesByDateXN?fromtime=" + from + "&totime=" + to, "_blank");
        return;
    }
    if (typeof TableToExcel === "undefined") { SwalHelper.Toast.error("Chưa tải được thư viện xuất Excel!"); return; }
    if (type === "4") {
        var exported = document.createElement("table");
        var rowCount = 0;
        $("#report-result .service-report-block:visible").each(function () {
            var table = $(this).find("table.grouped-service-table").first();
            var rows = table.find("tbody tr.service-data-row:visible");
            if (!table.length || !rows.length) return;
            var header = document.createElement("tr");
            var title = document.createElement("th");
            title.textContent = "Dịch vụ: " + String($(this).attr("data-service-name") || "");
            title.colSpan = table.find("thead tr").first().find("th,td").length || 1;
            header.appendChild(title);
            exported.appendChild(header);
            table.find("thead tr").each(function () { exported.appendChild($(this).clone()[0]); });
            rows.each(function () { exported.appendChild($(this).clone()[0]); rowCount++; });
        });
        if (!rowCount) { SwalHelper.Toast.warning("Không có dòng xét nghiệm nào đang hiển thị để xuất!"); return; }
        TableToExcel.convert(exported, { name: "KhachHangXNTheoDichVu.xlsx", sheet: { name: "Theo dich vu" } });
        return;
    }
    var table = document.querySelector("#report-result #table-export-to-excel");
    if (!table) { SwalHelper.Toast.warning("Báo cáo chưa có bảng dữ liệu để xuất!"); return; }
    TableToExcel.convert(table, { name: "BaoCao_" + type + ".xlsx", sheet: { name: "Bao cao" } });
}

// REPORT-06: Export server-side bang ClosedXML, KHONG export bang virtual scroll trong DOM.
async function Report_XNExportExcel() {
    if (reportXNExporting) return;
    if (!reportHasResults || !reportXNRows.length) { SwalHelper.Toast.warning("Vui lòng Xem báo cáo trước khi xuất Excel!"); return; }
    var from = $("#report-time-from").val(), to = $("#report-time-to").val();
    var maDotKham = String($("#report-ma-dot-kham").val() || "").trim();
    if (!from || !to || from > to || !maDotKham) { SwalHelper.Toast.warning("Vui lòng chọn ngày và Mã Đợt Khám hợp lệ!"); return; }
    var params = new URLSearchParams({ from: from, to: to, maDotKham: maDotKham, onlyCompleted: $("#report-xn-completed").val() === "true" ? "true" : "false", timeMode: "GETSAMPLE" });
    var categoryCode = String($("#report-category-code").val() || "").trim();
    if (categoryCode) params.set("categoryCode", categoryCode);
    reportXNExporting = true; Report_UpdateExportButton();
    Report_SetStatus("loading", "Đang tạo file Excel...");
    try {
        var response = await fetch("/Report_BaoCaoThongKe/ExportXNServiceStatisticsExcel?" + params.toString(), { method: "GET", credentials: "same-origin", cache: "no-store" });
        if (!response.ok) {
            var error = await response.json().catch(function () { return null; });
            throw new Error(error && error.message ? error.message : "Xuất Excel thất bại (HTTP " + response.status + ").");
        }
        var contentType = response.headers.get("content-type") || "";
        if (!contentType.includes("spreadsheetml.sheet")) throw new Error("Server không trả về file Excel hợp lệ.");
        var blob = await response.blob();
        var downloadUrl = URL.createObjectURL(blob);
        var link = document.createElement("a");
        link.href = downloadUrl;
        link.download = "ThongKeDichVuXN_" + from.replace(/-/g, "") + "_" + to.replace(/-/g, "") + ".xlsx";
        document.body.appendChild(link); link.click(); link.remove();
        setTimeout(function () { URL.revokeObjectURL(downloadUrl); }, 60000);
        if ($("#report-type").val() === "XN-SERVICE-DETAIL" && reportHasResults) Report_SetStatus("loaded", reportXNRows.length.toLocaleString("vi-VN") + " lượt dịch vụ");
    } catch (error) {
        if ($("#report-type").val() === "XN-SERVICE-DETAIL") Report_SetStatus("error", "Lỗi xuất Excel");
        SwalHelper.Toast.error(error.message || "Không thể xuất Excel!");
    } finally {
        reportXNExporting = false; Report_UpdateExportButton();
    }
}

// REPORT-05: 15 cột Excel, giữ dữ liệu đầy đủ từ API và chỉ dựng DOM các dòng đang xem.
var reportXNColumns = [
    { key: "stt", label: "STT", width: 54 }, { key: "id", label: "ID", width: 108 }, { key: "sid", label: "SID", width: 108 },
    { key: "namSinh", label: "Năm sinh", width: 85 }, { key: "gioiTinh", label: "Giới tính", width: 84 },
    { key: "loaiMau", label: "Loại mẫu", width: 130 }, { key: "tenBenhNhan", label: "Tên Bệnh Nhân", width: 205 },
    { key: "nguoiNhap", label: "Người Nhập", width: 160 }, { key: "nguoiDuyet", label: "Người Duyệt", width: 174 },
    { key: "inTime", label: "InTime", width: 150 }, { key: "trangThai", label: "Trạng thái", width: 132 },
    { key: "tenXN", label: "Tên XN", width: 270 }, { key: "nhomXN", label: "Nhóm XN", width: 145 },
    { key: "tenBacSiKTV", label: "Tên Bác Sĩ/KTV", width: 180 }, { key: "maHopDong", label: "Mã Hợp Đồng", width: 285 }
];

function Report_XNTimeText(value) {
    if (!value) return "";
    var text = String(value);
    var match = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/.exec(text);
    return match ? match[3] + "/" + match[2] + "/" + match[1] + " " + match[4] + ":" + match[5] : text;
}

function Report_XNInvalidateChangedFilter() {
    if ($("#report-type").val() !== "XN-SERVICE-DETAIL") return;
    if (reportSearchRequest && reportSearchRequest.readyState !== 4) reportSearchRequest.abort();
    reportXNRows = []; reportXNVirtual = null; reportHasResults = false;
    $("#report-result").removeClass("report-result--xn");
    Report_UpdateExportButton(); Report_SetStatus("ready", "Bộ lọc đã thay đổi");
    Report_SetEmptyState("Bộ lọc đã thay đổi", "Nhấn Xem để tải báo cáo theo điều kiện vừa chọn.");
    $("#report-search").prop("disabled", false);
}

function Report_XNServiceStatisticsSearch(from, to) {
    if (!from || !to || from > to) { SwalHelper.Toast.warning("Vui lòng chọn khoảng thời gian hợp lệ!"); return; }
    var maDotKham = String($("#report-ma-dot-kham").val() || "").trim();
    if (!maDotKham) { SwalHelper.Toast.warning("Vui lòng chọn Mã Đợt Khám!"); if (typeof $.fn.select2 === "function" && $("#report-ma-dot-kham").data("select2")) $("#report-ma-dot-kham").select2("open"); else $("#report-ma-dot-kham").focus(); return; }
    if (reportSearchRequest && reportSearchRequest.readyState !== 4) reportSearchRequest.abort();
    reportHasResults = false;
    reportXNRows = [];
    reportXNVirtual = null;
    Report_UpdateExportButton();
    $("#report-result").removeClass("report-result--xn");
    Report_SetStatus("loading", "Đang tải dữ liệu...");
    Report_SetEmptyState("Đang truy vấn báo cáo...", "Dữ liệu đợt khám được tải đầy đủ. Có thể mất thêm thời gian nếu đoàn khám lớn.");
    $("#report-search").prop("disabled", true);
    var payload = { from: from, to: to, maDotKham: maDotKham, onlyCompleted: $("#report-xn-completed").val() === "true", timeMode: "GETSAMPLE" };
    var categoryCode = String($("#report-category-code").val() || "").trim();
    if (categoryCode) payload.categoryCode = categoryCode;
    reportSearchRequest = $.ajax({
        url: "/Report_BaoCaoThongKe/GetXNServiceStatistics", type: "GET", dataType: "json", data: payload, global: false, cache: false, timeout: 120000,
        success: function (response) {
            if ($("#report-type").val() !== "XN-SERVICE-DETAIL" || $("#report-ma-dot-kham").val() !== maDotKham) return;
            if (!response || response.success !== true || !Array.isArray(response.rows)) { Report_SetStatus("error", "Response không hợp lệ"); Report_SetEmptyState("Dữ liệu báo cáo không hợp lệ", "API không trả về rows theo cấu trúc đã thống nhất."); SwalHelper.Toast.error("Cấu trúc API không hợp lệ!"); return; }
            reportXNRows = response.rows;
            var totalRows = Number(response.totalRows);
            if (!Number.isSafeInteger(totalRows) || totalRows < 0 || totalRows !== reportXNRows.length) {
                reportXNRows = []; Report_SetStatus("error", "Số dòng API không khớp");
                Report_SetEmptyState("Dữ liệu trả về không đầy đủ", "API trả về số dòng khác totalRows; không hiển thị báo cáo để tránh thiếu dữ liệu.");
                SwalHelper.Toast.error("Dữ liệu trả về không đầy đủ. Kiểm tra API báo cáo!");
                return;
            }
            reportHasResults = totalRows > 0;
            Report_UpdateExportButton();
            if (!totalRows) { Report_SetStatus("loaded", "0 dòng"); Report_SetEmptyState("Không tìm thấy dữ liệu", "Kiểm tra khoảng ngày, Mã Đợt Khám và trạng thái đã chọn."); return; }
            Report_XNRenderTable();
            Report_SetStatus("loaded", totalRows.toLocaleString("vi-VN") + " lượt dịch vụ");
        },
        error: function (xhr, status) {
            if (status === "abort" || $("#report-type").val() !== "XN-SERVICE-DETAIL") return;
            var msg = xhr.responseJSON && xhr.responseJSON.message ? xhr.responseJSON.message : (status === "timeout" ? "Truy vấn quá thời gian cho phép." : "Không thể tải dữ liệu báo cáo.");
            Report_SetStatus("error", "Lỗi tải dữ liệu");
            Report_SetEmptyState("Không tải được báo cáo", msg);
            SwalHelper.Toast.error(msg);
        },
        complete: function () { if ($("#report-type").val() === "XN-SERVICE-DETAIL") $("#report-search").prop("disabled", false); }
    });
}

function Report_XNRenderTable() {
    var root = document.getElementById("report-result");
    if (!root) return;
    root.classList.add("report-result--xn");
    root.replaceChildren();
    var wrap = document.createElement("div"); wrap.className = "report-xn-scroll"; wrap.id = "report-xn-scroll"; wrap.setAttribute("role", "region");
    wrap.setAttribute("aria-label", "Bảng thống kê dịch vụ xét nghiệm, cuộn để xem toàn bộ"); wrap.tabIndex = 0;
    var table = document.createElement("table"); table.className = "report-xn-table"; table.style.width = reportXNColumns.reduce(function (sum, col) { return sum + col.width; }, 0) + "px"; table.setAttribute("aria-rowcount", String(reportXNRows.length + 1));
    var cols = document.createElement("colgroup");
    reportXNColumns.forEach(function (col) { var c = document.createElement("col"); c.style.width = col.width + "px"; cols.appendChild(c); });
    table.appendChild(cols);
    var thead = document.createElement("thead"), hr = document.createElement("tr");
    reportXNColumns.forEach(function (col) { var th = document.createElement("th"); th.scope = "col"; th.textContent = col.label; hr.appendChild(th); });
    thead.appendChild(hr); table.appendChild(thead);
    var tbody = document.createElement("tbody"); tbody.id = "report-xn-body"; table.appendChild(tbody); wrap.appendChild(table); root.appendChild(wrap);
    reportXNVirtual = { wrap: wrap, tbody: tbody, start: -1, rowHeight: 37, buffer: 12, frame: null };
    wrap.addEventListener("scroll", Report_XNQueueVirtualRender, { passive: true });
    Report_XNRenderVirtualRows();
}

function Report_XNQueueVirtualRender() {
    var virtual = reportXNVirtual;
    if (!virtual || virtual.frame !== null) return;
    virtual.frame = window.requestAnimationFrame(function () { virtual.frame = null; if (reportXNVirtual === virtual) Report_XNRenderVirtualRows(); });
}

function Report_XNRenderVirtualRows() {
    var v = reportXNVirtual;
    if (!v) return;
    var total = reportXNRows.length;
    var start = Math.max(0, Math.floor(v.wrap.scrollTop / v.rowHeight) - v.buffer);
    var count = Math.ceil(v.wrap.clientHeight / v.rowHeight) + v.buffer * 2;
    var end = Math.min(total, start + count);
    if (v.start === start && v.end === end) return;
    v.start = start; v.end = end;
    var fragment = document.createDocumentFragment();
    if (start) fragment.appendChild(Report_XNSpacer(start * v.rowHeight));
    for (var i = start; i < end; i++) {
        var row = document.createElement("tr"); row.className = "report-xn-data-row"; row.setAttribute("aria-rowindex", String(i + 2));
        reportXNColumns.forEach(function (col) {
            var td = document.createElement("td");
            var value = col.key === "inTime" ? Report_XNTimeText(reportXNRows[i][col.key]) : reportXNRows[i][col.key];
            var safeText = value === null || value === undefined ? "" : String(value);
            td.textContent = safeText; td.title = safeText; row.appendChild(td);
        });
        fragment.appendChild(row);
    }
    if (end < total) fragment.appendChild(Report_XNSpacer((total - end) * v.rowHeight));
    v.tbody.replaceChildren(fragment);
}

function Report_XNSpacer(height) {
    var tr = document.createElement("tr"); tr.className = "report-xn-spacer"; tr.style.height = height + "px";
    var td = document.createElement("td"); td.colSpan = reportXNColumns.length; td.style.height = height + "px"; tr.appendChild(td);
    return tr;
}

// Hàm gọi khi user click 1 dòng dịch vụ trong bảng báo cáo
function Report_ViewServiceDetail(serviceId) {
    var from = $("#report-time-from").val();
    var to = $("#report-time-to").val();
    var location = $("#report-location").val();

    if (!serviceId || !from || !to) {
        SwalHelper.Toast.warning("Thiếu thông tin dịch vụ hoặc thời gian!");
        return;
    }
    console.log(from);
    console.log(to);
    // Hiển thị modal chờ
    $('#showWaitting').modal('show');
    console.log()
    if (location == "XN") {
        $.ajax({
            url: "/Report_BaoCaoThongKe/GetPatientResultByServiceXN",
            type: "GET",
            data: {
                serviceId: serviceId,
                fromtime: from,
                totime: to
            },
            dataType: "html",
            cache: false,
            success: function (result) {
                $('#showWaitting').modal('hide');

                // Load nội dung partial vào modal
                $("#report-service-detail").html(result);

                // Cập nhật tiêu đề modal (nếu partial có serviceName)
                var title = $(result).find("#service-name-title").text() || "Chi tiết dịch vụ";
                $("#serviceDetailLabel").text(title);

                // Mở modal
                $("#modal-service-detail").modal("show");
            },
            error: function () {
                $('#showWaitting').modal('hide');
                SwalHelper.Toast.error("Không tải được dữ liệu chi tiết dịch vụ!");
            }
        });
    } else {
        $.ajax({
            url: "/Report_BaoCaoThongKe/GetPatientResultByServiceCDHA",
            type: "GET",
            data: {
                serviceId: serviceId,
                fromtime: from,
                totime: to,
                location: location
            },
            dataType: "html",
            cache: false,
            success: function (result) {
                $('#showWaitting').modal('hide');

                // Load nội dung partial vào modal
                $("#report-service-detail").html(result);

                // Cập nhật tiêu đề modal (nếu partial có serviceName)
                var title = $(result).find("#service-name-title").text() || "Chi tiết dịch vụ";
                $("#serviceDetailLabel").text(title);

                // Mở modal
                $("#modal-service-detail").modal("show");
            },
            error: function () {
                $('#showWaitting').modal('hide');
                SwalHelper.Toast.error("Không tải được dữ liệu chi tiết dịch vụ!");
            }
        });
    }

}

// Xuất Excel CHỈ những dòng đang hiển thị trong modal
$(document).on("click", "#btn-export-detail", function () {

    var $table = $("#modal-service-detail").find("table").first();

    if ($table.length === 0) {
        SwalHelper.Toast.error("Không tìm thấy bảng dữ liệu để xuất!");
        return;
    }

    var $visibleRows = $table.find("tbody tr:visible");

    if ($visibleRows.length === 0) {
        SwalHelper.Toast.warning("Không có dữ liệu đang hiển thị để xuất!");
        return;
    }

    // Clone table, không tác động giao diện hiện tại
    var $exportTable = $table.clone();

    // Xóa dữ liệu cũ trong clone
    $exportTable.find("tbody").empty();

    // Copy đúng các dòng hiện đang hiển thị
    $visibleRows.each(function () {
        $exportTable.find("tbody").append(
            $(this).clone()
        );
    });

    // Đánh lại STT từ 1 -> N
    $exportTable.find("tbody tr").each(function (index) {
        $(this).find("td").first().text(index + 1);
    });

    TableToExcel.convert($exportTable[0], {
        name: "ChiTietDichVu.xlsx",
        sheet: {
            name: "KQ Xet Nghiem"
        }
    });
});

// Hàm xem chi tiết danh sách bệnh nhân
function Report_ViewPatientDetail(total) {
    if (total <= 0) {
        SwalHelper.Toast.warning("Không có dữ liệu bệnh nhân!");
        return;
    }

    var from = $("#report-time-from").val();
    var to = $("#report-time-to").val();
    var location = $("#report-location").val();

    if (!from || !to) {
        SwalHelper.Toast.warning("Thiếu thông tin thời gian!");
        return;
    }

    // Hiển thị modal chờ
    $('#showWaitting').modal('show');

    $.ajax({
        url: "/Report_BaoCaoThongKe/GetPatientDetail",
        type: "GET",
        data: {
            fromtime: from,
            totime: to,
            location: location
        },
        dataType: "html",
        cache: false,
        success: function (result) {
            $('#showWaitting').modal('hide');

            // Load nội dung vào modal
            $("#report-patient-detail").html(result);

            // Mở modal
            $("#modal-patient-detail").modal("show");
        },
        error: function () {
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.error("Không tải được dữ liệu chi tiết bệnh nhân!");
        }
    });
}

// Hàm xuất Excel chi tiết bệnh nhân
function Report_ExportPatientDetailToExcel() {
    // Export nội dung bảng trong modal ra Excel
    var table = $("#report-patient-detail table");
    if (table.length === 0) {
        SwalHelper.Toast.warning("Không có dữ liệu để xuất!");
        return;
    }

    TableToExcel.convert(table[0], {
        name: "ChiTietBenhNhan.xlsx",
        sheet: {
            name: "DanhSachBenhNhan"
        }
    });
}

function Report_PDF() {
    var validate = ValidateInput('report-option-search');
    if (validate) {
        $('#showWaitting').modal('show');
        var from = $("#report-time-from").val();
        var to = $("#report-time-to").val();
        var type = $("#report-type").val();
        var location = $("#report-location").val();
        //var userid = $("#report-user").val();
        //var hospitalId = $("#report-hospital").val();
        var userid = 20;
        var hospitalId = null;

        $.ajax({
            url: "/Report_BaoCaoThongKe/Export_Pdf?" + "from=" + from + "&to=" + to + "&type=" + type + "&location=" + location + "&userid=" + userid + "&hospitalId=" + hospitalId,
            type: "GET",
            dataType: "text",
            success: function (response) {
                $('#showWaitting').modal('hide');
                if (response === "") {
                    SwalHelper.Toast.error("Valid & In không thành công. Vui lòng kiểm tra lại!");
                }
                else {
                    var byteCharacters = atob(response);
                    var byteNumbers = new Array(byteCharacters.length);
                    for (var i = 0; i < byteCharacters.length; i++) {
                        byteNumbers[i] = byteCharacters.charCodeAt(i);
                    }
                    var byteArray = new Uint8Array(byteNumbers);
                    var file = new Blob([byteArray], { type: 'application/pdf;base64' });
                    var fileURL = URL.createObjectURL(file);
                    window.open(fileURL);
                }
            },
            error: function () {
                $('#showWaitting').modal('hide');
                SwalHelper.Toast.error("Valid & In không thành công. Vui lòng kiểm tra lại!");
            }
        });
    }
}

function getMaDotKham() {
    return $('#report-madotkham').val();
}
function Report_BenhNhanThucHienDichVu_Search() {
    var maDotKham = getMaDotKham();
    if (!maDotKham) {
        alert('Vui lòng nhập MaDotKham.');
        return;
    }

    // Nếu trong project đã có hàm show/hide loading riêng thì dùng lại
    if (typeof ShowLoading === 'function') {
        ShowLoading();
    }

    $.ajax({
        url: "/Report_BenhNhanThucHienDichVu/GetBenhNhanThucHienDichVu",
        type: "GET",
        data: {
            maDotKham: maDotKham
        },
        dataType: "html",
        cache: false,
        success: function (result) {
            $('#showWaitting').modal('hide');

            // Load nội dung vào modal
            $('#report-result').html(result);
        },
        error: function () {
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.error("Không tải được dữ liệu chi tiết bệnh nhân!");
        }
    });
    //$.get('@Url.Action("GetBenhNhanThucHienDichVu", "Report_BenhNhanThucHienDichVuController")',
    //    { maDotKham: maDotKham })
    //    .done(function (html) {
    //        $('#report-result').html(html);
    //    })
    //    .always(function () {
    //        if (typeof HideLoading === 'function') {
    //            HideLoading();
    //        }
    //    });
}

function Report_BenhNhanThucHienDichVu_PDF() {
    var maDotKham = getMaDotKham();
    if (!maDotKham) {
        alert('Vui lòng nhập MaDotKham.');
        return;
    }
    // Controller action export PDF (bạn sẽ viết)
    window.location.href =
        '@Url.Action("Export_Pdf", "Report_BenhNhanThucHienDichVu")'
        + '?maDotKham=' + encodeURIComponent(maDotKham);
}

function Report_BenhNhanThucHienDichVu_Excel(fileType) {
    var maDotKham = getMaDotKham();
    if (!maDotKham) {
        alert('Vui lòng nhập MaDotKham.');
        return;
    }
    // Controller action export Excel (bạn sẽ viết)
    window.location.href =
        '@Url.Action("ExportBenhNhanThucHienDichVuExcel", "Report_BenhNhanThucHienDichVu")'
        + '?maDotKham=' + encodeURIComponent(maDotKham)
        + '&fileType=' + encodeURIComponent(fileType || 'xlsx');
}

function Report_BuildPatientDetailRowHtml(patients, colSpan) {
    if (!patients || patients.length === 0) {
        return `
            <tr class="patient-detail-row">
                <td colspan="${colSpan || 6}" class="text-center text-muted">
                    Không có dữ liệu bệnh nhân
                </td>
            </tr>
        `;
    }

    var rowsHtml = "";
    for (var i = 0; i < patients.length; i++) {
        var p = patients[i];
        rowsHtml += `
            <tr>
                <td>${i + 1}</td>
                <td>${p.PatientID || ""}</td>
                <td>${p.PatientName || ""}</td>
                <td>${p.MaDotKham || ""}</td>
                <td>${p.Sid || ""}</td>
                <td>${p.Seq || ""}</td>
                <td>${p.MaBenhAn || ""}</td>
                <td>${p.InsertTimeText || ""}</td>
            </tr>
        `;
    }

    return `
        <div style="padding:10px 20px; background:#fafafa;">
            <table class="table table-bordered table-sm mb-0" style="width:100%;">
                <thead>
                    <tr>
                        <th style="width: 50px;">STT</th>
                        <th style="width: 120px;">Mã BN</th>
                        <th style="width: 300px;">Họ tên</th>
                        <th>Mã Đợt khám</th>
                        <th style="width: 150px;">SID</th>
                        <th style="width: 80px;">SEQ</th>
                        <th style="width: 120px;">Mã bệnh án</th>
                        <th style="width: 150px;">Ngày tiếp nhận</th>
                    </tr>
                </thead>
                <tbody>
                    ${rowsHtml}
                </tbody>
            </table>
        </div>
    `;
}
function Report_FindServiceFromCache(doctorId, serviceId) {
    if (!window.reportProcessCache || !Array.isArray(window.reportProcessCache)) {
        return null;
    }

    var doctor = window.reportProcessCache.find(function (d) {
        return String(d.DoctorID) === String(doctorId);
    });

    if (!doctor || !doctor.Services) {
        return null;
    }

    var service = doctor.Services.find(function (s) {
        return String(s.ServiceID) === String(serviceId);
    });

    return service || null;
}
// --------------- Report theo công thực hiện --------------------
// Hàm toggle hiển thị danh sách bệnh nhân theo dịch vụ trong báo cáo theo Công thực hiện
function Report_TogglePatientsByService(
    doctorId,
    serviceId,
    rowElement) {

    var detailRow =
        $("#detail-row-" + doctorId + "-" + serviceId);

    var icon =
        $(rowElement).find(".toggle-icon");

    detailRow.toggle();

    if (detailRow.is(":visible")) {
        icon.css("transform", "rotate(90deg)");
    }
    else {
        icon.css("transform", "rotate(0deg)");
    }

}