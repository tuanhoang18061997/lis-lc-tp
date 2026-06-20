using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class SampleBL
    {
        private readonly LABContext _db;
        public SampleBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Sample>> GetListSampleByCategory(string code)
        {
            if (string.IsNullOrEmpty(code))
                return await _db.Samples.ToListAsync();
            else
                return await _db.Samples.Where(p =>  p.Category.Code == code).ToListAsync();
        }

        public async Task<Sample> GetSample(long id)
        {
            return await _db.Samples.Where(p =>  p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Sample>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.Samples.ToListAsync();
            }
            else
            {
                return await _db.Samples.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<Sample>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Samples.ToListAsync();
                }
                else
                {
                    return await _db.Samples.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Samples.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.Samples.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<Sample> Get(string code)
        {
            return await _db.Samples.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(SampleModel sample)
        {
            try
            {
                var _obj = await _db.Samples.Where(p => p.Code == sample.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new Sample() { Code = sample.code, Name = sample.name, Description = sample.description, Result = sample.result, Suggest = sample.suggest, CategorieId = sample.categoryid, Gender = sample.gender, ServiceId = sample.serviceid == 0 ? null : sample.serviceid, Active = sample.active };
                    await _db.Samples.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = sample.name;
                    _obj.Description = sample.description;
                    _obj.Result = sample.result;
                    _obj.Suggest = sample.suggest;
                    _obj.CategorieId = sample.categoryid;
                    _obj.Gender = sample.gender;
                    _obj.ServiceId = sample.serviceid == 0 ? null : sample.serviceid;
                    _obj.Active = sample.active;
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
                var _obj = await _db.Samples.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.Samples.Remove(_obj);
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
