using Management.Models.SignServer;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Management.Interfaces
{
    public interface ISignServerService
    {
        /// <summary>
        /// Gửi pdfFile + signerCccd (signUserId) tới MySign.
        /// Trả về raw response string từ MySign.
        /// </summary>
        /// Gửi PDF sang MySign. Trả về raw JSON (string). Controller sẽ trích response.data.
        Task<string> SignPdfAsync(
            string signerCccd,
            IFormFile pdfFile,
            SignContext context,
            string? targetTextOverride = null,
            CancellationToken cancellationToken = default);

        // Trả về URL xem PDF đã ký (không chèn token)
        string GetSignedPdfViewUrl(string signStoreId);
    }

}
