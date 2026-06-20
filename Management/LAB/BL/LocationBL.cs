using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class LocationBL
    {
        private readonly LABContext _db;
        public LocationBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Location>> GetListLocation(bool all = false)
        {
            if(all)
            {
                return await _db.Locations.ToListAsync();
            }
            else
            {
                return await _db.Locations.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<Location>> GetListLocationByValue(string value, bool all = false)
        {
            if(all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Locations.ToListAsync();
                }
                else
                {
                    return await _db.Locations.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Locations.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.Locations.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }
            
        }

        public async Task<Location> GetLocation(string code)
        {
            return await _db.Locations.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(LocationModel location)
        {
            try
            {
                var _location = await _db.Locations.Where(p => p.Code == location.code).FirstOrDefaultAsync();
                if (_location == null)
                {
                    _location = new Location() { Code = location.code, Name = location.name, Active = location.active};
                    await _db.Locations.AddAsync(_location);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _location.Name = location.name;
                    _location.Active = location.active;
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
                var _location = await _db.Locations.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_location != null)
                {
                    _db.Remove(_location);
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
