using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class ObjectBL
    {
        private readonly LABContext _db;
        public ObjectBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Object>> GetListObject(bool all = false)
        {
            if (all)
            {
                return await _db.Objects.ToListAsync();
            }
            else
            {
                return await _db.Objects.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<Object>> GetListObjectByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Objects.ToListAsync();
                }
                else
                {
                    return await _db.Objects.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Objects.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.Objects.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<Object> GetObject(string code)
        {
            return await _db.Objects.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(ObjectModel obj)
        {
            try
            {
                var _object = await _db.Objects.Where(p => p.Code == obj.code).FirstOrDefaultAsync();
                if (_object == null)
                {
                    _object = new Object() { Code = obj.code, Name = obj.name, Active = obj.active };
                    await _db.Objects.AddAsync(_object);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _object.Name = obj.name;
                    _object.Active = obj.active;
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
                var _obj = await _db.Objects.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.Remove(_obj);
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
