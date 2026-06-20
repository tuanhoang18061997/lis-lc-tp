using Management.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Management.BL
{
    public class PortalLinkBL
    {
        private readonly LABContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PortalLinkBL(
            LABContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PortalLinkResult> BuildPortalLinkAsync(string maBenhAn, string fromDate = null, string toDate = null)
        {
            if (string.IsNullOrWhiteSpace(maBenhAn))
            {
                return new PortalLinkResult
                {
                    Success = false,
                    Message = "Mã bệnh án không hợp lệ"
                };
            }

            var patient = await GetPatientByMaBenhAn(maBenhAn);
            if (patient == null)
            {
                return new PortalLinkResult
                {
                    Success = false,
                    Message = $"Không tìm thấy bệnh nhân: {maBenhAn}"
                };
            }

            var secretKey = _configuration["HISIntegration:HISApiKey"] ?? "LisSecretKeyForHISLeanCare2025!@#$%";
            var expiryDays = _configuration.GetValue<int>("HISIntegration:TokenExpiryDays", 1);

            var expiryDate = DateTime.Now.AddDays(expiryDays);
            var token = GenerateJwtToken(maBenhAn, secretKey, expiryDate);

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return new PortalLinkResult
                {
                    Success = false,
                    Message = "Không lấy được HttpContext để tạo portal URL"
                };
            }

            var portalUrl = $"{request.Scheme}://{request.Host}/Portal/ViewResults?token={token}&maBenhAn={maBenhAn}";

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                portalUrl += $"&fromDate={fromDate}";
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                portalUrl += $"&toDate={toDate}";
            }

            return new PortalLinkResult
            {
                Success = true,
                Token = token,
                PortalUrl = portalUrl,
                ExpiryDate = expiryDate,
                Message = "Tạo link portal thành công"
            };
        }

        private async Task<Patient> GetPatientByMaBenhAn(string maBenhAn)
        {
            return await Task.FromResult(
                _context.Patients.FirstOrDefault(x => x.MaBenhAn == maBenhAn)
            );
        }

        private string GenerateJwtToken(string maBenhAn, string secretKey, DateTime expiryDate)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("maBenhAn", maBenhAn)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiryDate,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class PortalLinkResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string PortalUrl { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Message { get; set; }
    }
}