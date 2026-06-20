using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class DeviceBL
    {
        private readonly LABContext _db;
        public DeviceBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Device>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.Devices.ToListAsync();
            }
            else
            {
                return await _db.Devices.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<Device>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Devices.ToListAsync();
                }
                else
                {
                    return await _db.Devices.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Devices.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.Devices.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<Device> Get(string code)
        {
            return await _db.Devices.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<Device> Get(long id)
        {
            return await _db.Devices.Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(DeviceModel device)
        {
            try
            {
                var _obj = await _db.Devices.Where(p => p.Code == device.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new Device() { Code = device.code, Name = device.name, Protocol = device.protocol, ProcessQc = device.processqc, CodeBHYT = device.codebhyt, Active = device.active };
                    await _db.Devices.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = device.name;
                    _obj.Protocol = device.protocol;
                    _obj.ProcessQc = device.processqc;
                    _obj.CodeBHYT = device.codebhyt;
                    _obj.Active = device.active;
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
                var _obj = await _db.Devices.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.Devices.Remove(_obj);
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
