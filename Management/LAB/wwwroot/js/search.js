$(function () {
    if (typeof window.BarcodeScan === 'undefined') return;

    // Gắn barcode cho ô input tìm PID/SEQ/Mã bệnh án
    window.BarcodeScan.register('#search_pidorseq', window.Search_Search, {
        minLen: 8,
        idleMs: 1000,
        triggerOnEnter: true,
        selectAfter: true,
        clearAfter: false
    });
    // Tự động gắn cho các input có class .barcode-input (dùng chung nhiều màn hình)
    window.BarcodeScan.autowire('.barcode-input');
});
// *********************************************************************************** Gắn hotkey Enter cho tìm kiếm
$(document).ready(function () {
    // Gắn Enter key cho các input field tìm kiếm của search
    $('#search_pidorseq, #search_timeSearchFrom, #search_timeSearchTo, #search_maDotKham').on('keydown', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            Process_Search();
        }
    });
});
// *********************************************************************************** Get sample
function Search_Refresh() {
    $.ajax({
        url: "/Search/Refresh/",
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            var date = new Date();
            var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
            $("#listPatient").html(result);
            $("#search_timeSearchFrom").val(today);
            $("#search_timeSearchTo").val(today);
            Search_ResetInput();
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

function Search_Search() {
    var search_pidorseq = $("#search_pidorseq").val();
    var timeSearchFrom = $("#search_timeSearchFrom").val();
    var timeSearchTo = $("#search_timeSearchTo").val();
    var maDotKham = $("#search_maDotKham").val();
    $.ajax({
        url: "/Search/Search?" + "pidorseq=" + search_pidorseq + "&timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo + "&maDotKham=" + encodeURIComponent(maDotKham),
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#listPatient").html(result);
            Search_LoadMaDotKhamList();
        },
        error: function () {
            $("#listPatient").empty();
        }
    });
}

// Thêm function để load danh sách MaDotKham
function Search_LoadMaDotKhamList() {
    var timeSearchFrom = $("#search_timeSearchFrom").val();
    var timeSearchTo = $("#search_timeSearchTo").val();

    if (!timeSearchFrom || !timeSearchTo) return;

    $.ajax({
        url: "/Search/GetMaDotKhamList?timeSearchFrom=" + timeSearchFrom + "&timeSearchTo=" + timeSearchTo,
        type: "GET",
        dataType: "json",
        cache: false,
        success: function (result) {
            var $select = $("#search_maDotKham");
            var currentValue = $select.val(); // Lưu giá trị đang chọn

            // Xóa các option cũ (trừ option đầu tiên "-- Tất cả --")
            $select.find('option:not(:first)').remove();

            // Thêm option "Khách lẻ" nếu có bệnh nhân không có MaDotKham
            if (result.hasKhachLe) {
                $select.append('<option value="khach-le">-- Khách lẻ --</option>');
            }

            // Thêm các option mới từ kết quả
            if (result.maDotKhamList && result.maDotKhamList.length > 0) {
                $.each(result.maDotKhamList, function (index, maDotKham) {
                    $select.append('<option value="' + maDotKham + '">' + maDotKham + '</option>');
                });
            }

            // Khôi phục giá trị đã chọn nếu vẫn tồn tại trong danh sách mới
            if (currentValue && $select.find('option[value="' + currentValue + '"]').length > 0) {
                $select.val(currentValue);
            }
        },
        error: function () {
            console.error("Không thể tải danh sách MaDotKham");
        }
    });
}

function Search_GetPatientInfo(id) {
    $.ajax({
        url: "/Search/GetPatientInfo?id=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#patientInfo").html(result);
            $(".list-group-item-action").removeClass("active");
            $(`.list-group-item-action[data-id='${id}']`).addClass("active");
            //Search_GetListServiceForPatient(id);
            Search_GetListServiceForPatientGroupedByCategory(id); // mở hàm này để full chức năng in kqxn theo nhóm theo danh mục và từng dịch vụ riêng lẽ
        },
        error: function () {
            $("#patientInfo").empty();
            $('#tbody-gridview-service').empty();
        }
    });
}


function Search_GetListServiceForPatient(id) {
    $.ajax({
        url: "/Search/GetServiceForPatient?patientId=" + id,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#search-right-service-gridview").html(result);
        },
        error: function () {
            $('#tbody-gridview-service').empty();
        }
    });
}

function Search_ResetInput() {
    var date = new Date();
    var today = date.getFullYear() + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + ("0" + date.getDate()).slice(-2);
    var time = ("0" + date.getHours()).slice(-2) + ":" + ("0" + date.getMinutes()).slice(-2);
    var dateTime = today + " " + time;
    $("#search_pidorseq").val('');
    $('#search_id').val('');
    $('#search_patientId').val('');
    $('#search_seq').val('');
    $('#search_sid').val('');
    $('#search_patientName').val('');
    $('#search_age').val('');
    $('#search_sex').val('');
    $('#search_obj').val('');
    $('#search_type').val('');
    $('#search_location').val('');
    $('#search_doctor').val('');
    $('#search_getSampleTime').val(dateTime);
    $('#search_address').val('');
    $('#search_diagnostic').val('');
    $('#select-category').val('');
    $('#select-service').val('');
    $('#tbody-gridview-service').empty();
}

function Search_Print(keyresultforhis) {
    if (keyresultforhis === "") {
        SwalHelper.Toast.error("Không thể in kết quả chỉ định này. Vui lòng kiểm tra lại !");
    }
    else {
        $.ajax({
            url: "/Search/Print?keyresultforhis=" + keyresultforhis,
            dataType: "text",
            type: "GET",
            success: function (response) {
                if (response === "") {
                    SwalHelper.Toast.error("Không thể in kết quả chỉ định này. Vui lòng kiểm tra lại !");
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
                SwalHelper.Toast.error("Không thể in kết quả chỉ định này. Vui lòng kiểm tra lại !");
            }
        });
    }
}

// Add this new function for printing category results
function Search_PrintCategory(categoryId, patientId) {
    if (!categoryId || !patientId) {
        SwalHelper.Toast.warning("Thông tin danh mục không hợp lệ!");
        return;
    }

    // Open the print page in a new window
    var printUrl = "/Search/PrintCategoryResults?categoryId=" + categoryId + "&patientId=" + patientId;
    var printWindow = window.open(printUrl, '_blank', 'width=1024,height=768,scrollbars=yes,resizable=yes');

    if (printWindow) {
        printWindow.focus();
    } else {
        SwalHelper.Toast.warning("Không thể mở cửa sổ in. Vui lòng kiểm tra trình duyệt chặn popup!");
    }
}

// Function to toggle between grouped and normal view
function Search_GetListServiceForPatientGroupedByCategory() {
    var patientId = $('#search_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    $.ajax({
        url: "/Search/GetServiceForPatientGroupedByCategory?patientId=" + patientId,
        type: "GET",
        dataType: "html",
        cache: false,
        success: function (result) {
            $("#search-right-service-gridview").html(result);
        },
        error: function () {
            SwalHelper.Toast.error("Không thể tải danh sách nhóm dịch vụ!");
        }
    });
}

// Function to switch back to normal view
function Search_NormalView() {
    var patientId = $('#search_id').val();
    if (!patientId) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
        return;
    }

    Search_GetListServiceForPatient(patientId);
}

// Add this new function for printing individual child service results
function Search_PrintChildService(serviceId, patientId) {
    if (!serviceId || !patientId) {
        SwalHelper.Toast.warning("Thông tin dịch vụ không hợp lệ!");
        return;
    }

    // Open the print page in a new window
    var printUrl = "/Search/PrintChildServiceResults?serviceId=" + serviceId + "&patientId=" + patientId;
    var printWindow = window.open(printUrl, '_blank', 'width=1024,height=768,scrollbars=yes,resizable=yes');

    if (printWindow) {
        printWindow.focus();
    } else {
        SwalHelper.Toast.warning("Không thể mở cửa sổ in. Vui lòng kiểm tra trình duyệt chặn popup!");
    }
}
// Add this new function for printing grouped XN results
function Search_PrintGroupedXN(keyresultforhis) {
    if (keyresultforhis === "") {
        SwalHelper.Toast.error("Không thể in kết quả chỉ định này. Vui lòng kiểm tra lại!");
        return;
    }

    // Call the existing print function for XN results
    Search_Print(keyresultforhis);
}

// *********************************************************************************** Download All Results
function Search_DownloadAllResults() {
    var id = $('#search_id').val();
    var maBenhAn = $('#search_maBenhAn').val();
    if (id === "" && maBenhAn === "") {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân!");
    }

    // Hiển thị loading trên button
    var $button = $("#search_downloadAllResults");
    var originalText = $button.html();
    $button.prop('disabled', true).html('<i class="spinner-border spinner-border-sm me-1"></i>Đang tải...');

    // Sử dụng fetch API để có error handling tốt hơn
    fetch("/Search/DownloadAllResults?patientId=" + id + "&maBenhAn=" + maBenhAn, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        }
    })
        .then(response =>
        {
            console.log(response);
            if (!response.ok) {
                if (response.status === 400) {
                    throw new Error('Bệnh nhân chưa có kết quả hoàn thành nào để tải.');
                } else if (response.status === 404) {
                    throw new Error('Không tìm thấy thông tin bệnh nhân.');
                } else if (response.status === 500) {
                    throw new Error('Có lỗi xảy ra khi tải kết quả. Vui lòng thử lại sau.');
                } else {
                    throw new Error('Lỗi từ server: ' + response.status);
                }
            }

            // Lấy filename từ header Content-Disposition nếu có
            var filename = 'KetQua_' + maBenhAn + '_' + new Date().getTime() + '.zip';
            var contentDisposition = response.headers.get('Content-Disposition');
            if (contentDisposition) {
                var filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }

            return response.blob().then(blob => ({ blob, filename }));
        })
        .then(({ blob, filename }) => {
            // Tạo download link và trigger download
            var url = window.URL.createObjectURL(blob);
            var $link = $('<a>').attr({
                href: url,
                download: filename,
                style: 'display: none'
            }).appendTo('body');

            $link[0].click();

            // Clean up
            $link.remove();
            window.URL.revokeObjectURL(url);

            // Hiển thị thông báo thành công
            var patientName = $("#search_patientName").val() || 'bệnh nhân';
            SwalHelper.Toast.success("Tải kết quả thành công cho " + patientName + "!\nFile: " + filename);
        })
        .catch(error => {
            console.error('Download error:', error);
            SwalHelper.Toast.error(error.message);
        })
        .finally(() => {
            // Restore button về trạng thái ban đầu
            $button.prop('disabled', false).html(originalText);
        });
}

// Alternative: Simple version using direct link (backup method)
function Search_DownloadAllResults_Simple(patientId) {
    if (!patientId || patientId <= 0) {
        SwalHelper.Toast.warning("Vui lòng chọn bệnh nhân trước khi tải kết quả!");
        return;
    }

    // Hiển thị loading
    var $button = $("#search_downloadAllResults");
    var originalText = $button.html();
    $button.prop('disabled', true).html('<i class="spinner-border spinner-border-sm me-1"></i>Đang tải...');

    // Tạo URL download
    var downloadUrl = "/Search/DownloadAllResults?patientId=" + patientId;

    // Tạo invisible link để download file
    var $link = $('<a>').attr({
        href: downloadUrl,
        download: '',
        target: '_blank',
        style: 'display: none'
    }).appendTo('body');

    // Trigger download
    $link[0].click();

    // Clean up
    setTimeout(function () {
        $link.remove();
    }, 1000);

    // Restore button sau 3 giây
    setTimeout(function () {
        $button.prop('disabled', false).html(originalText);
    }, 3000);
}

// *********************************************************************************** Download All Patients Results
function Search_DownloadAllPatientsResults() {
    // Validate maDotKham selection
    var maDotKham = $("#search_maDotKham").val();
    if (!maDotKham || maDotKham === "" || maDotKham === "all") {
        SwalHelper.Toast.warning("Vui lòng chọn đợt khám cụ thể trước khi download!");
        return;
    }
    // Lấy danh sách tất cả bệnh nhân từ DOM
    var patientIds = [];
    $('#listPatient .list-group-item-action').each(function () {
        var patientId = $(this).data('id');
        if (patientId) {
            patientIds.push(patientId);
        }
    });

    if (patientIds.length === 0) {
        SwalHelper.Toast.warning("Không có bệnh nhân nào trong danh sách!");
        return;
    }

    // Xác nhận trước khi download
    Swal.fire({
        title: 'Xác nhận download',
        text: `Bạn có chắc muốn tải kết quả xét nghiệm của ${patientIds.length} bệnh nhân?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Đồng ý',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            executeDownloadAllXN(patientIds, maDotKham);
        }
    });
}

function executeDownloadAllXN(patientIds, maDotKham) {
    // Hiển thị loading modal
    $('#showWaitting').modal('show');

    // Đợi modal hiển thị xong rồi mới update text
    setTimeout(function () {
        updateLoadingText('Đang chuẩn bị tải kết quả xét nghiệm...');
    }, 100);

    var $button = $("#search_downloadAllPatients");
    var originalText = $button.html();
    $button.prop('disabled', true).html('<i class="spinner-border spinner-border-sm me-1"></i>Đang xử lý...');

    var sessionId = null;
    var progressInterval = null;
    var totalPatients = patientIds.length;

    fetch("/Search/DownloadAllPatientsResults", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            patientIds: patientIds,
            maDotKham: maDotKham
        })
    })
        .then(response => {
            // Lấy session ID từ header
            sessionId = response.headers.get('X-Session-Id');
            console.log('Session ID received:', sessionId); // Debug log

            // Bắt đầu polling progress nếu có session ID
            if (sessionId) {
                progressInterval = setInterval(function () {
                    fetch("/Search/GetDownloadProgress?sessionId=" + sessionId)
                        .then(res => res.json())
                        .then(data => {
                            console.log('Progress data:', data); // Debug log
                            if (data.total > 0) {
                                updateLoadingText(`${data.status} - ${data.current}/${data.total} (${data.percentage}%)`);

                                // Dừng polling nếu đã hoàn thành
                                if (data.status === "Hoàn thành" || data.percentage >= 100) {
                                    clearInterval(progressInterval);
                                    console.log('Progress completed, stopping interval');
                                }
                            }
                        })
                        .catch(err => console.error('Progress error:', err));
                }, 500); // Update mỗi 500ms
            } else {
                console.warn('No session ID found in response headers');
                // Nếu không có session ID, hiển thị thông tin cơ bản
                updateLoadingText(`Đang xử lý kết quả xét nghiệm... (${totalPatients} bệnh nhân)`);
            }

            if (!response.ok) {
                if (response.status === 400) {
                    return response.text().then(text => { throw new Error(text); });
                } else if (response.status === 404) {
                    throw new Error('Không tìm thấy kết quả xét nghiệm nào.');
                } else if (response.status === 500) {
                    throw new Error('Có lỗi xảy ra khi tải kết quả. Vui lòng thử lại sau.');
                } else {
                    throw new Error('Lỗi từ server: ' + response.status);
                }
            }

            var filename = maDotKham + '_XN_' + formatDateTimeForFilename(new Date()) + '.zip';
            var contentDisposition = response.headers.get('Content-Disposition');
            if (contentDisposition) {
                var filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }

            updateLoadingText('Đang tạo file ZIP...');
            return response.blob().then(blob => ({ blob, filename }));
        })
        .then(({ blob, filename }) => {
            // Dừng polling
            if (progressInterval) {
                clearInterval(progressInterval);
            }

            updateLoadingText('Đang chuẩn bị download...');

            var url = window.URL.createObjectURL(blob);
            var $link = $('<a>').attr({
                href: url,
                download: filename,
                style: 'display: none'
            }).appendTo('body');

            $link[0].click();
            $link.remove();
            window.URL.revokeObjectURL(url);

            $('#showWaitting').modal('hide');
            SwalHelper.Toast.success("Tải kết quả thành công!\nTổng: " + totalPatients + " bệnh nhân\nFile: " + filename);
        })
        .catch(error => {
            if (progressInterval) {
                clearInterval(progressInterval);
            }
            console.error('Download error:', error);
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.error(error.message || 'Có lỗi xảy ra khi tải kết quả!');
        })
        .finally(() => {
            $button.prop('disabled', false).html(originalText);
        });
}
// *********************************************************************************** Download All Patients CDHA Results
function Search_DownloadAllPatientsCDHAResults() {
    // Validate maDotKham selection
    var maDotKham = $("#search_maDotKham").val();
    if (!maDotKham || maDotKham === "" || maDotKham === "all") {
        SwalHelper.Toast.warning("Vui lòng chọn đợt khám cụ thể trước khi download!");
        return;
    }
    // Lấy danh sách tất cả bệnh nhân từ DOM
    var patientIds = [];
    $('#listPatient .list-group-item-action').each(function () {
        var patientId = $(this).data('id');
        if (patientId) {
            patientIds.push(patientId);
        }
    });

    if (patientIds.length === 0) {
        SwalHelper.Toast.warning("Không có bệnh nhân nào trong danh sách!");
        return;
    }

    // Xác nhận trước khi download
    Swal.fire({
        title: 'Xác nhận download CDHA',
        text: `Bạn có chắc muốn tải kết quả CDHA (SA, SAT, XQ, DDT, NS) của ${patientIds.length} bệnh nhân?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#17a2b8',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Đồng ý',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            executeDownloadAllCDHA(patientIds, maDotKham);
        }
    });
}

function executeDownloadAllCDHA(patientIds, maDotKham) {
    // Hiển thị loading modal
    $('#showWaitting').modal('show');

    // Đợi modal hiển thị xong rồi mới update text
    setTimeout(function () {
        updateLoadingText('Đang chuẩn bị tải kết quả CDHA...');
    }, 100);

    var $button = $("#search_downloadAllPatientsCDHA");
    var originalText = $button.html();
    $button.prop('disabled', true).html('<i class="spinner-border spinner-border-sm me-1"></i>Đang xử lý...');

    var sessionId = null;
    var progressInterval = null;
    var totalPatients = patientIds.length;

    fetch("/Search/DownloadAllPatientsCDHAResults", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            patientIds: patientIds,
            maDotKham: maDotKham
        })
    })
        .then(response => {
            // Lấy session ID từ header
            sessionId = response.headers.get('X-Session-Id');
            console.log('Session ID received:', sessionId); // Debug log

            // Bắt đầu polling progress nếu có session ID
            if (sessionId) {
                progressInterval = setInterval(function () {
                    fetch("/Search/GetDownloadProgressCDHA?sessionId=" + sessionId)
                        .then(res => res.json())
                        .then(data => {
                            console.log('Progress data:', data); // Debug log
                            if (data.total > 0) {
                                updateLoadingText(`${data.status} - ${data.current}/${data.total} (${data.percentage}%)`);

                                // Dừng polling nếu đã hoàn thành
                                if (data.status === "Hoàn thành" || data.percentage >= 100) {
                                    clearInterval(progressInterval);
                                    console.log('Progress completed, stopping interval');
                                }
                            }
                        })
                        .catch(err => console.error('Progress error:', err));
                }, 500); // Update mỗi 500ms
            } else {
                console.warn('No session ID found in response headers');
                // Nếu không có session ID, hiển thị thông tin cơ bản
                updateLoadingText(`Đang xử lý kết quả CDHA... (${totalPatients} bệnh nhân)`);
            }

            if (!response.ok) {
                if (response.status === 400) {
                    return response.text().then(text => { throw new Error(text); });
                } else if (response.status === 404) {
                    throw new Error('Không tìm thấy kết quả CDHA nào.');
                } else if (response.status === 500) {
                    throw new Error('Có lỗi xảy ra khi tải kết quả. Vui lòng thử lại sau.');
                } else {
                    throw new Error('Lỗi từ server: ' + response.status);
                }
            }

            // Lấy filename từ header Content-Disposition
            var filename = maDotKham + '_CDHA_' + formatDateTimeForFilename(new Date()) + '.zip';
            var contentDisposition = response.headers.get('Content-Disposition');
            if (contentDisposition) {
                var filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }

            updateLoadingText('Đang tạo file ZIP CDHA...');
            return response.blob().then(blob => ({ blob, filename }));
        })
        .then(({ blob, filename }) => {
            // Dừng polling
            if (progressInterval) {
                clearInterval(progressInterval);
            }

            updateLoadingText('Đang chuẩn bị download...');

            // Tạo download link và trigger download
            var url = window.URL.createObjectURL(blob);
            var $link = $('<a>').attr({
                href: url,
                download: filename,
                style: 'display: none'
            }).appendTo('body');

            $link[0].click();

            // Clean up
            $link.remove();
            window.URL.revokeObjectURL(url);

            // Ẩn loading modal
            $('#showWaitting').modal('hide');

            // Hiển thị thông báo thành công
            SwalHelper.Toast.success("Tải kết quả CDHA thành công!\nTổng: " + totalPatients + " bệnh nhân\nFile: " + filename);
        })
        .catch(error => {
            if (progressInterval) {
                clearInterval(progressInterval);
            }
            console.error('Download CDHA error:', error);
            $('#showWaitting').modal('hide');
            SwalHelper.Toast.error(error.message || 'Có lỗi xảy ra khi tải kết quả CDHA!');
        })
        .finally(() => {
            // Restore button về trạng thái ban đầu
            $button.prop('disabled', false).html(originalText);
        });
}

// *********************************************************************************** Helper function for date formatting
function formatDateTimeForFilename(date) {
    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var year = date.getFullYear();
    var hours = ("0" + date.getHours()).slice(-2);
    var minutes = ("0" + date.getMinutes()).slice(-2);
    var seconds = ("0" + date.getSeconds()).slice(-2);

    return day + month + year + hours + minutes + seconds;
}