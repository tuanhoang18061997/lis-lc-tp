using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class PrintSampleBL
    {
        private readonly LABContext _db;
        public PrintSampleBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<PrintSample>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.PrintSamples.ToListAsync();
            }
            else
            {
                return await _db.PrintSamples.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<PrintSample>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.PrintSamples.ToListAsync();
                }
                else
                {
                    return await _db.PrintSamples.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.PrintSamples.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.PrintSamples.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<PrintSample> Get(string code)
        {
            return await _db.PrintSamples.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Delete(string code)
        {
            try
            {
                var _obj = await _db.PrintSamples.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.PrintSamples.Remove(_obj);
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
