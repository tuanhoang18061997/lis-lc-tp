using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Management.Controllers
{
    [Route("api/his")]
    [ApiController]
    public class SharedAPIController : ControllerBase
    {
        private readonly APIBL _apiBL;
        private readonly LABContext _db;

        public SharedAPIController()
        {
            _apiBL = new APIBL();
            _db = new LABContext();
        }

        /// <summary>
        /// API để HIS cập nhật thông tin bệnh nhân
        /// Chỉ cần truyền PatientId và TicketId (required) + các field cần update
        /// </summary>
        /// <param name="request">Thông tin bệnh nhân cần cập nhật (chỉ truyền field cần update)</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPost("update-patient")]
        public async Task<IActionResult> UpdatePatientInfo([FromBody] UpdatePatientRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Dữ liệu yêu cầu không hợp lệ",
                        ErrorCode = "INVALID_REQUEST"
                    });
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.PatientId))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "PatientId là bắt buộc",
                        ErrorCode = "MISSING_REQUIRED_FIELDS"
                    });
                }

                var result = await _apiBL.UpdatePatientFromHIS(request);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi hệ thống: {ex.Message}",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// API để HIS lấy thông tin bệnh nhân
        /// </summary>
        [HttpGet("get-patient/{patientId}/{ticketId}")]
        public async Task<IActionResult> GetPatientInfo(string patientId, string ticketId)
        {
            try
            {
                var patient = await _db.Patients
                    .Include(p => p.Doctor)
                    .Include(p => p.Object)
                    .Include(p => p.Location)
                    .FirstOrDefaultAsync(p => p.PatientId == patientId && p.TicketId == ticketId);

                if (patient == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin bệnh nhân",
                        ErrorCode = "PATIENT_NOT_FOUND"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Lấy thông tin bệnh nhân thành công",
                    Data = new
                    {
                        patient.Sid,
                        patient.PatientId,
                        patient.TicketId,
                        patient.PatientName,
                        patient.Sex,
                        patient.Age,
                        patient.Address,
                        patient.Diagnostic,
                        DoctorName = patient.Doctor?.Name,
                        ObjectName = patient.Object?.Name,
                        LocationName = patient.Location?.Name,
                        patient.InsertTime,
                        patient.MaBenhAn,
                        patient.MaDotKham
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi hệ thống: {ex.Message}",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// API để HIS cập nhật dịch vụ không thực hiện
        /// </summary>
        [HttpPost("cancel-service")]
        public async Task<IActionResult> CancelService([FromBody] UpdateServiceNotPerformedRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Dữ liệu yêu cầu không hợp lệ",
                        ErrorCode = "INVALID_REQUEST"
                    });
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.TicketItemId))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "TicketItemId là bắt buộc",
                        ErrorCode = "MISSING_TICKET_ITEM_ID"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ServiceCode))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "ServiceCode là bắt buộc",
                        ErrorCode = "MISSING_SERVICE_CODE"
                    });
                }

                var result = await _apiBL.UpdateServiceNotPerformed(request);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi hệ thống: {ex.Message}",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// API để HIS cập nhật nhiều dịch vụ không thực hiện cùng lúc
        /// </summary>
        [HttpPost("cancel-services-batch")]
        public async Task<IActionResult> CancelServicesBatch([FromBody] List<UpdateServiceNotPerformedRequest> requests)
        {
            try
            {
                if (requests == null || requests.Count == 0)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Danh sách yêu cầu không hợp lệ",
                        ErrorCode = "INVALID_REQUEST"
                    });
                }

                var result = await _apiBL.UpdateMultipleServicesNotPerformed(requests);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi hệ thống: {ex.Message}",
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }
    }
}