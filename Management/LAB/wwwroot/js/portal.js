/**
 * Portal.js - JavaScript functions for LIS Portal
 * Handles image viewing, token generation, and portal interactions
 */

// Global variables
const PORTAL = {
    config: {
        apiBaseUrl: '/Portal',
        dateFormat: 'yyyyMMdd'
    }
};

/**
 * View images for a specific result
 * @param {number} resultId - The ID of the result to view images for
 */
async function viewImages(resultId) {
    try {
        showLoading('Đang tải hình ảnh...');
        
        const token = generateTokenForResult(resultId);
        const response = await fetch(`${PORTAL.config.apiBaseUrl}/ViewResultDetail?token=${token}&resultId=${resultId}`);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        
        if (data.success && data.data.images && data.data.images.length > 0) {
            displayImagesInModal(data.data.images);
        } else {
            showAlert('Không có hình ảnh để hiển thị', 'info');
        }
    } catch (error) {
        console.error('Error loading images:', error);
        showAlert('Lỗi khi tải hình ảnh. Vui lòng thử lại.', 'error');
    } finally {
        hideLoading();
    }
}

/**
 * Display images in modal
 * @param {Array} images - Array of image objects
 */
function displayImagesInModal(images) {
    let imageHtml = '<div class="image-gallery">';
    
    images.forEach((img, index) => {
        imageHtml += `
            <div class="image-item" data-index="${index}">
                <div class="image-wrapper">
                    <img src="${img.fullPath}" 
                         alt="Hình ảnh kết quả ${index + 1}" 
                         class="img-fluid portal-image" 
                         onclick="openImageFullscreen('${img.fullPath}')"
                         loading="lazy" />
                    <div class="image-overlay">
                        <button class="btn btn-sm btn-light" onclick="openImageFullscreen('${img.fullPath}')">
                            <i class="fas fa-expand"></i> Phong to
                        </button>
                        <button class="btn btn-sm btn-light" onclick="downloadImage('${img.fullPath}', 'ketqua_${img.id}')">
                            <i class="fas fa-download"></i> Tải xuống
                        </button>
                    </div>
                </div>
                <div class="image-caption">
                    Hình ảnh ${index + 1}
                </div>
            </div>
        `;
    });
    
    imageHtml += '</div>';
    
    document.getElementById('modalImageContent').innerHTML = imageHtml;
    
    // Show modal using Bootstrap 5
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    modal.show();
}

/**
 * Open image in fullscreen
 * @param {string} imagePath - Path to the image
 */
function openImageFullscreen(imagePath) {
    const fullscreenModal = `
        <div class="modal fade" id="fullscreenImageModal" tabindex="-1">
            <div class="modal-dialog modal-fullscreen">
                <div class="modal-content bg-dark">
                    <div class="modal-header border-0">
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body d-flex align-items-center justify-content-center">
                        <img src="${imagePath}" class="img-fluid" style="max-height: 90vh; max-width: 90vw;" />
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing fullscreen modal if any
    const existingModal = document.getElementById('fullscreenImageModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Add new modal to body
    document.body.insertAdjacentHTML('beforeend', fullscreenModal);
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('fullscreenImageModal'));
    modal.show();
    
    // Remove modal after hiding
    document.getElementById('fullscreenImageModal').addEventListener('hidden.bs.modal', function() {
        this.remove();
    });
}

/**
 * Download image
 * @param {string} imagePath - Path to the image
 * @param {string} filename - Filename for download
 */
function downloadImage(imagePath, filename) {
    try {
        const link = document.createElement('a');
        link.href = imagePath;
        link.download = filename + '.jpg';
        link.target = '_blank';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } catch (error) {
        console.error('Error downloading image:', error);
        showAlert('Không thể tải xuống hình ảnh', 'error');
    }
}

/**
 * Generate token for result viewing
 * @param {number} resultId - The result ID
 * @returns {string} Generated token
 */
function generateTokenForResult(resultId) {
    // Simple token generation - should match server-side logic
    const today = new Date();
    const dateString = today.getFullYear().toString() + 
                      (today.getMonth() + 1).toString().padStart(2, '0') + 
                      today.getDate().toString().padStart(2, '0');
    
    const tokenData = resultId + dateString;
    return btoa(tokenData);
}

/**
 * Generate token for patient access (for HIS integration)
 * @param {string} maBenhAn - Patient medical record number
 * @param {number} validDays - Number of days the token should be valid
 * @returns {Promise<Object>} Token generation result
 */
async function generatePatientToken(maBenhAn, validDays = 7) {
    try {
        showLoading('Đang tạo token...');
        
        const response = await fetch(`${PORTAL.config.apiBaseUrl}/GenerateToken?maBenhAn=${encodeURIComponent(maBenhAn)}&validDays=${validDays}`);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        
        if (data.token) {
            return {
                success: true,
                token: data.token,
                portalUrl: data.portalUrl,
                maBenhAn: data.maBenhAn,
                expiryDate: data.expiryDate
            };
        } else {
            throw new Error('Invalid response from server');
        }
    } catch (error) {
        console.error('Error generating token:', error);
        return {
            success: false,
            error: error.message
        };
    } finally {
        hideLoading();
    }
}

/**
 * Open portal for a patient (for HIS integration)
 * @param {string} maBenhAn - Patient medical record number
 */
async function openPortalForPatient(maBenhAn) {
    if (!maBenhAn) {
        showAlert('Vui lòng cung cấp mã bệnh án', 'warning');
        return;
    }
    
    try {
        const result = await generatePatientToken(maBenhAn);
        
        if (result.success) {
            // Open portal in new window/tab
            window.open(result.portalUrl, '_blank', 'width=1200,height=800,scrollbars=yes,resizable=yes');
        } else {
            // Fallback to test URL
            const testUrl = `${window.location.origin}/Portal/Test?maBenhAn=${encodeURIComponent(maBenhAn)}`;
            window.open(testUrl, '_blank', 'width=1200,height=800,scrollbars=yes,resizable=yes');
        }
    } catch (error) {
        console.error('Error opening portal:', error);
        showAlert('Không thể mở portal. Vui lòng thử lại.', 'error');
    }
}

/**
 * Copy portal URL to clipboard
 * @param {string} maBenhAn - Patient medical record number
 */
async function copyPortalUrl(maBenhAn) {
    try {
        const result = await generatePatientToken(maBenhAn);
        
        if (result.success) {
            await navigator.clipboard.writeText(result.portalUrl);
            showAlert('Đã sao chép link portal vào clipboard', 'success');
        } else {
            const testUrl = `${window.location.origin}/Portal/Test?maBenhAn=${encodeURIComponent(maBenhAn)}`;
            await navigator.clipboard.writeText(testUrl);
            showAlert('Đã sao chép link test vào clipboard', 'info');
        }
    } catch (error) {
        console.error('Error copying URL:', error);
        showAlert('Không thể sao chép URL', 'error');
    }
}

/**
 * Print current page
 */
function printPortal() {
    window.print();
}

/**
 * Show loading indicator
 * @param {string} message - Loading message
 */
function showLoading(message = 'Đang tải...') {
    let loadingElement = document.getElementById('portalLoading');
    
    if (!loadingElement) {
        loadingElement = document.createElement('div');
        loadingElement.id = 'portalLoading';
        loadingElement.className = 'portal-loading';
        loadingElement.innerHTML = `
            <div class="loading-overlay">
                <div class="loading-content">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="loading-text">${message}</div>
                </div>
            </div>
        `;
        document.body.appendChild(loadingElement);
    } else {
        loadingElement.querySelector('.loading-text').textContent = message;
        loadingElement.style.display = 'block';
    }
}

/**
 * Hide loading indicator
 */
function hideLoading() {
    const loadingElement = document.getElementById('portalLoading');
    if (loadingElement) {
        loadingElement.style.display = 'none';
    }
}

/**
 * Show alert message
 * @param {string} message - Alert message
 * @param {string} type - Alert type (success, error, warning, info)
 */
function showAlert(message, type = 'info') {
    const alertTypes = {
        success: 'alert-success',
        error: 'alert-danger',
        warning: 'alert-warning',
        info: 'alert-info'
    };
    
    const alertClass = alertTypes[type] || alertTypes.info;
    const iconClass = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-info-circle'
    }[type] || 'fa-info-circle';
    
    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show portal-alert" role="alert">
            <i class="fas ${iconClass} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Remove existing alerts
    document.querySelectorAll('.portal-alert').forEach(alert => alert.remove());
    
    // Add new alert to top of portal container
    const portalContainer = document.querySelector('.portal-container');
    if (portalContainer) {
        portalContainer.insertAdjacentHTML('afterbegin', alertHtml);
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            const alert = document.querySelector('.portal-alert');
            if (alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    } else {
        // Fallback to browser alert
        SwalHelper.Toast.info(message);
    }
}

/**
 * Format date for display
 * @param {Date|string} date - Date to format
 * @param {string} format - Format pattern
 * @returns {string} Formatted date
 */
function formatDate(date, format = 'dd/MM/yyyy') {
    const d = new Date(date);
    if (isNaN(d.getTime())) return '';
    
    const day = d.getDate().toString().padStart(2, '0');
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const year = d.getFullYear();
    const hours = d.getHours().toString().padStart(2, '0');
    const minutes = d.getMinutes().toString().padStart(2, '0');
    
    return format
        .replace('dd', day)
        .replace('MM', month)
        .replace('yyyy', year)
        .replace('HH', hours)
        .replace('mm', minutes);
}

/**
 * View single image in modal
 * @param {string} imageUrl - URL of the image to view
 */
function viewImageInModal(imageUrl) {
    displaySingleImageInModal(imageUrl);
}

/**
 * Display single image in modal
 * @param {string} imageUrl - URL of the image
 */
function displaySingleImageInModal(imageUrl) {
    console.log(imageUrl);
    const imageHtml = `
        <div class="single-image-viewer">
            <div class="image-container">
                <img src="${imageUrl}" 
                     alt="Hình ảnh kết quả" 
                     class="img-fluid single-image" 
                     onclick="openImageFullscreen('${imageUrl}')"
                     style="max-width: 100%; max-height: 70vh; cursor: pointer;" />
            </div>
            <div class="image-actions mt-3 text-center">
                <button class="btn btn-primary me-2" onclick="openImageFullscreen('${imageUrl}')">
                    <i class="fas fa-expand"></i> Xem toàn màn hình
                </button>
                <button class="btn btn-secondary" onclick="downloadImage('${imageUrl}', 'ketqua_${Date.now()}')">
                    <i class="fas fa-download"></i> Tải xuống
                </button>
            </div>
        </div>
    `;
    
    document.getElementById('modalImageContent').innerHTML = imageHtml;
    document.querySelector('#imageModal .modal-title').textContent = 'Xem hình ảnh';
    
    // Show modal using Bootstrap 5
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    modal.show();
}

/**
 * Initialize portal when document is ready
 */
document.addEventListener('DOMContentLoaded', function() {
    console.log('Portal.js initialized');
    
    // Add CSS for loading and image gallery
    const style = document.createElement('style');
    style.textContent = `
        .portal-loading {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            z-index: 9999;
            display: none;
        }
        
        .loading-overlay {
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100%;
        }
        
        .loading-content {
            text-align: center;
            color: white;
        }
        
        .loading-text {
            margin-top: 10px;
            font-size: 14px;
        }
        
        .image-gallery {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            padding: 20px 0;
        }
        
        .image-item {
            position: relative;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        
        .image-wrapper {
            position: relative;
            overflow: hidden;
        }
        
        .portal-image {
            width: 100%;
            height: auto;
            transition: transform 0.3s ease;
            cursor: pointer;
        }
        
        .portal-image:hover {
            transform: scale(1.05);
        }
        
        .image-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.7);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            opacity: 0;
            transition: opacity 0.3s ease;
        }
        
        .image-item:hover .image-overlay {
            opacity: 1;
        }
        
        .image-caption {
            padding: 10px;
            text-align: center;
            background: #f8f9fa;
            font-size: 12px;
            color: #666;
        }
        
        .portal-alert {
            margin-bottom: 20px;
            z-index: 1050;
        }
        
        @media (max-width: 768px) {
            .image-gallery {
                grid-template-columns: 1fr;
                gap: 15px;
            }
            
            .image-overlay {
                opacity: 1;
                background: rgba(0, 0, 0, 0.5);
            }
        }

        .single-image-viewer {
            text-align: center;
            padding: 20px;
        }
        
        .single-image {
            max-width: 100%;
            height: auto;
            border-radius: 8px;
            transition: transform 0.3s ease;
        }
        
        .single-image:hover {
            transform: scale(1.05);
        }
        
        .image-actions {
            margin-top: 15px;
        }
        
        .image-actions .btn {
            min-width: 120px;
        }
    `;
    document.head.appendChild(style);
});

// Export functions for external use (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        viewImages,
        generateTokenForResult,
        generatePatientToken,
        openPortalForPatient,
        copyPortalUrl,
        printPortal,
        showAlert,
        formatDate
    };
}