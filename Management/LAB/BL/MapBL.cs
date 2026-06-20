using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class MapBL
    {
        private readonly LABContext _db;
        public MapBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Map>> GetMapByDeviceId(long deviceId)
        {
            return await _db.Maps.Where(p => p.DeviceId == deviceId).ToListAsync();
        }

        public async Task<Map> GetMap(long id)
        {
            return await _db.Maps.Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(MapModel map)
        {
            try
            {
                if(map != null)
                {
                    var _obj = await _db.Maps.Where(p => p.Id == map.id).FirstOrDefaultAsync();
                    if (_obj == null)
                    {
                        _obj = new Map();
                        _obj.DeviceId = map.deviceid;
                        _obj.TestCodeId = map.testcodeid;
                        _obj.TestcodeIn = map.testcodein;
                        _obj.TestcodeIn2 = map.testcodein2;
                        _obj.Note = map.note;
                        _obj.FormatNumber = map.formatnumber;
                        await _db.Maps.AddAsync(_obj);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        _obj.TestCodeId = map.testcodeid;
                        _obj.TestcodeIn = map.testcodein;
                        _obj.TestcodeIn2 = map.testcodein2;
                        _obj.Note = map.note;
                        _obj.FormatNumber = map.formatnumber;
                        await _db.SaveChangesAsync();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(List<Map01Model> lstMap01)
        {
            try
            {
                if (lstMap01 != null && lstMap01.Count > 0)
                {
                    foreach(var map in lstMap01)
                    {
                        var _obj = await _db.Maps.Where(p => p.Id == map.id).FirstOrDefaultAsync();
                        if (_obj != null)
                        {
                            _db.Maps.Remove(_obj);
                        }
                    }
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
