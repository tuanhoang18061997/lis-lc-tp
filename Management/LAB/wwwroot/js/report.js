$(document).ajaxStart(function () {
    AppLoading.show('Đang xử lý...');
});

$(document).ajaxStop(function () {
    AppLoading.hide(true);
});
let button = document.querySelector("#report-excel");
button.addEventListener("click", e => {
    let table = document.querySelector("#table-export-to-excel");
    TableToExcel.convert(table);
});
window.reportProcessCache = null;
// Disable/Enable service name filter based on report result
function toggleServiceNameInput() {
    var reportResult = $("#report-result");
    var serviceNameInput = $("#report-service-name");

    // Check if report-result has data (has table or meaningful content)
    var hasData = reportResult.find("table tbody tr").length > 0;

    if (hasData) {
        serviceNameInput.prop("disabled", false);
    } else {
        serviceNameInput.prop("disabled", true);
        serviceNameInput.val(""); // Clear input when disabled
    }
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
// Chỉ hiển thị bộ lọc danh mục khi chọn khoa Xét nghiệm
function toggleReportCategoryFilter() {
    var location = ($("#report-location").val() || "")
        .toString()
        .trim()
        .toUpperCase();

    var isXN = location === "XN";
    var $categoryWrapper = $("#report-category-wrapper");
    var $categorySelect = $("#report-category-code");

    if (isXN) {
        $categoryWrapper.show();

        // Load danh mục từ database
        Report_LoadXNCategories(false);
    } else {
        $categoryWrapper.hide();

        // Reset danh mục khi chuyển sang khoa khác
        $categorySelect
            .val("")
            .prop("disabled", true);
    }
}

// Filter table rows based on service name input
function filterServiceByName() {
    var filterValue = $("#report-service-name").val().toLowerCase().trim();
    var table = $("#report-result table tbody tr");

    table.each(function () {
        var row = $(this);
        // Assuming service name is in the first or second column, adjust index as needed
        var serviceName = row.find("td").eq(1).text().toLowerCase();

        if (serviceName.includes(filterValue) || filterValue === "") {
            row.show();
        } else {
            row.hide();
        }
    });
}

// Initialize on document ready
$(document).ready(function () {
    toggleServiceNameInput();
    toggleReportCategoryFilter();

    $("#report-service-name").on("keyup", function () {
        filterServiceByName();
    });
});

$(document).on("change", "#report-location", function () {
    toggleReportCategoryFilter();
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
    var validate = ValidateInput('report-option-search');

    if (!validate) {
        return;
    }

    var from = $("#report-time-from").val();
    var to = $("#report-time-to").val();
    var type = $("#report-type").val();

    var location = ($("#report-location").val() || "")
        .toString()
        .trim()
        .toUpperCase();

    // Chỉ lấy Category.Code khi khoa đang chọn là Xét nghiệm
    var categoryCode = "";

    if (location === "XN") {
        categoryCode = ($("#report-category-code").val() || "")
            .toString()
            .trim()
            .toUpperCase();
    }

    var userid = 20;

    console.log("Bộ lọc báo cáo:", {
        from: from,
        to: to,
        type: type,
        location: location,
        categoryCode: categoryCode,
        userid: userid
    });

    $.ajax({
        url: "/Report_BaoCaoThongKe/LC_Search",
        type: "GET",
        data: {
            from: from,
            to: to,
            type: type,
            location: location,
            userid: userid,
            categoryCode: categoryCode
        },
        dataType: "html",
        cache: false,
        success: function (result) {
            $('#showWaitting').modal('hide');
            $("#report-result").html(result);

            toggleServiceNameInput();
        },
        error: function (xhr) {
            $('#showWaitting').modal('hide');
            $("#report-result").empty();

            toggleServiceNameInput();

            console.error(
                "Không tải được dữ liệu báo cáo:",
                xhr.responseText
            );

            SwalHelper.Toast.error(
                "Không tải được dữ liệu báo cáo!"
            );
        }
    });
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
// Xuất Excel nội dung chi tiết trong modal
$(document).on("click", "#btn-export-detail", function () {
    var table = $("#modal-service-detail").find("table").first();
    if (table.length === 0) {
        SwalHelper.Toast.error("Không tìm thấy bảng dữ liệu để xuất!");
        return;
    }
    TableToExcel.convert(table[0], {
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

