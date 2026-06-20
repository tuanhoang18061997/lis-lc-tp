using Management.Models;
using Microsoft.EntityFrameworkCore;
using Type = Management.Models.Type;

namespace Management.BL
{
    public class TypeBL
    {
        private readonly LABContext _db;
        public TypeBL(LABContext db)
        {
            _db = db;
        }
        public async Task CreateTypes()
        {
            var _lstType = await _db.Types.ToListAsync();
            if (_lstType == null || _lstType.Count <= 0)
            {
                await _db.Types.AddRangeAsync(
                    new Type() { Code = UserTypeModel.XN, Name = "XN"},
                    new Type() { Code = UserTypeModel.SA, Name = "SA"},
                    new Type() { Code = UserTypeModel.SAT, Name = "SA Tim" },
                    new Type() { Code = UserTypeModel.NS, Name = "NS" },
                    new Type() { Code = UserTypeModel.XQ, Name = "XQ" }
               );
                await _db.SaveChangesAsync();
            }
        }
    }
}
