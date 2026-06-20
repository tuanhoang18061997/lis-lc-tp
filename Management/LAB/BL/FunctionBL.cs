using Management.Models;
using Microsoft.EntityFrameworkCore;
using Type = Management.Models.Type;

namespace Management.BL
{
    public class FunctionBL
    {
        private readonly LABContext _db;
        public FunctionBL(LABContext db)
        {
            _db = db;
        }
        public async Task CreateFunction()
        {
            var _lstFunction = await _db.Functions.ToListAsync();
            if (_lstFunction == null || _lstFunction.Count <= 0)
            {
                await _db.Functions.AddRangeAsync(
                    new Function() { Code = FunctionModel.XN, Name = "Xét nghiệm", Sort = 1 },
                    new Function() { Code = FunctionModel.SA, Name = "Siêu âm", Sort = 2 },
                    new Function() { Code = FunctionModel.SAT, Name = "Siêu âm tim", Sort = 3 },
                    new Function() { Code = FunctionModel.XQ, Name = "X-Quang", Sort = 4 },
                    new Function() { Code = FunctionModel.NS, Name = "Nội soi", Sort = 5 },
                    new Function() { Code = FunctionModel.Report, Name = "Báo cáo", Sort = 6 },
                    new Function() { Code = FunctionModel.Search, Name = "Tìm kiếm KQ", Sort = 7 },
                    new Function() { Code = FunctionModel.Settup, Name = "Cấu hình", Sort = 8 },

                    new Function() { Code = FunctionModel.User, Name = "Người dùng", Sort = 9 },
                    new Function() { Code = FunctionModel.Hospital, Name = "Bệnh viện", Sort = 10 },
                    new Function() { Code = FunctionModel.Location, Name = "Khoa phòng", Sort = 11 },
                    new Function() { Code = FunctionModel.Object, Name = "Đối tượng", Sort = 12 },
                    new Function() { Code = FunctionModel.Doctor, Name = "Bác sĩ", Sort = 13 },

                    new Function() { Code = FunctionModel.TestType, Name = "Loại mẫu", Sort = 14 },
                    new Function() { Code = FunctionModel.Category, Name = "Danh mục", Sort = 15 },
                    new Function() { Code = FunctionModel.Service, Name = "Dịch vụ", Sort = 16 },
                    new Function() { Code = FunctionModel.TestCode, Name = "Mã XN", Sort = 17 },
                    new Function() { Code = FunctionModel.ServiceTest, Name = "Dịch vụ & Mã XN", Sort = 18 },
                    new Function() { Code = FunctionModel.Device, Name = "Thiết bị", Sort = 19 },
                    new Function() { Code = FunctionModel.Map, Name = "Mã XN máy", Sort = 20 },
                    new Function() { Code = FunctionModel.Sample, Name = "Mẫu kết quả", Sort = 21 },

                    new Function() { Code = FunctionModel.ManageData, Name = "Dữ liệu", Sort = 22 },
                    new Function() { Code = FunctionModel.ManageConfigSystem, Name = "Hệ thống", Sort = 23 }
               );
                await _db.SaveChangesAsync();
            }
        }
    }
}
