using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class TestTypeBL
    {
        private readonly LABContext _db;
        public TestTypeBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<TestType>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.TestTypes.ToListAsync();
            }
            else
            {
                return await _db.TestTypes.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<TestType>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.TestTypes.ToListAsync();
                }
                else
                {
                    return await _db.TestTypes.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.TestTypes.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.TestTypes.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<TestType> Get(string code)
        {
            return await _db.TestTypes.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(TestTypeModel testType)
        {
            try
            {
                var _obj = await _db.TestTypes.Where(p => p.Code == testType.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new TestType() { Code = testType.code, Name = testType.name, Active = testType.active };
                    await _db.TestTypes.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = testType.name;
                    _obj.Active = testType.active;
                    await _db.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string code)
        {
            try
            {
                var _obj = await _db.TestTypes.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.TestTypes.Remove(_obj);
                    await _db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
