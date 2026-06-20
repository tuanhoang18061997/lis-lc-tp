using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class DoctorBL
    {
        private readonly LABContext _db;
        public DoctorBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Doctor>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.Doctors.ToListAsync();
            }
            else
            {
                return await _db.Doctors.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<Doctor>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Doctors.ToListAsync();
                }
                else
                {
                    return await _db.Doctors.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Doctors.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.Doctors.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<Doctor> Get(string code)
        {
            return await _db.Doctors.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(DoctorModel doctor)
        {
            try
            {
                var _obj = await _db.Doctors.Where(p => p.Code == doctor.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new Doctor() { Code = doctor.code, Name = doctor.name, Active = doctor.active };
                    await _db.Doctors.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = doctor.name;
                    _obj.Active = doctor.active;
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
                var _obj = await _db.Doctors.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.Doctors.Remove(_obj);
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
