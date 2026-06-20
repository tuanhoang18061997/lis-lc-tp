using Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Management.BL
{
    public class HospitalBL
    {
        private readonly LABContext _db;
        public HospitalBL(LABContext db)
        {
            _db = db;
        }

        public async Task CreateHostpital()
        {
            var _lstHospital = await _db.Hospitals.ToListAsync();
            if (_lstHospital == null || _lstHospital.Count <= 0)
            {
                await _db.Hospitals.AddAsync(
                    new Hospital()
                    {
                        Code = "BVTEST",
                        Name = "Bệnh Viện Test",
                        NameEN = "Test Hospital",
                        Address = "60 Nguyễn Oanh, Phường 10, Quận Gò Vấp, TP.HCM",
                        Email = "test@gmail.com",
                        Website = "test.com",
                        Phone = "0986.929.984",
                    }
               );
                await _db.SaveChangesAsync();
            }
        }

        public async Task<Hospital> GetHospital()
        {
            var _hospital = await _db.Hospitals.Where(p => p.Default == 1).FirstOrDefaultAsync();
            return _hospital;
        }

        public async Task<Hospital> GetHospitalByCode(string code)
        {
            var _hospital = await _db.Hospitals.Where(p => p.Code == code).FirstOrDefaultAsync();
            return _hospital;
        }

        public async Task<Hospital> GetHospitalById(long id)
        {
            var _hospital = await _db.Hospitals.Where(p => p.Id == id).FirstOrDefaultAsync();
            return _hospital;
        }

        public async Task<List<Hospital>> GetListHospital()
        {
            var _lstHospital = await _db.Hospitals.OrderBy(p => p.Id).ToListAsync();
            return _lstHospital;
        }

        public async Task<bool> Save(HospitalModel hospital)
        {
            try
            {
                var _hostpital = await _db.Hospitals.Where(p => p.Code == hospital.code).FirstOrDefaultAsync();
                if (_hostpital == null)
                {
                    _hostpital = new Hospital();
                    _hostpital.Code = hospital.code;
                    _hostpital.Name = hospital.name;
                    _hostpital.NameEN = hospital.nameen;
                    _hostpital.Address = hospital.address;
                    _hostpital.Phone = hospital.phone;
                    _hostpital.Website = hospital.website;
                    _hostpital.Email = hospital.email;
                    _hostpital.Logo = hospital.logo;

                    await _db.Hospitals.AddRangeAsync(_hostpital);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _hostpital.Name = hospital.name;
                    _hostpital.NameEN = hospital.nameen;
                    _hostpital.Address = hospital.address;
                    _hostpital.Phone = hospital.phone;
                    _hostpital.Website = hospital.website;
                    _hostpital.Email = hospital.email;
                    _hostpital.Logo = hospital.logo;

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
                var _hostpital = await _db.Hospitals.Where(p => p.Code == code).FirstOrDefaultAsync();
                if(_hostpital != null)
                {
                    _db.Hospitals.Remove(_hostpital);
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
