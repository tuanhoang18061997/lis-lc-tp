using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class ResultStandardBL
    {
        private readonly LABContext _db;
        public ResultStandardBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<ResultStandard>> GetList(DateTime from, DateTime to)
        {
            return await _db.ResultStandards.Where(p => p.InsertTime > from && p.InsertTime < to).ToListAsync();
        }

        public async Task<List<ResultStandard>> Search(DateTime from, DateTime to, long? deviceId, string seq)
        {
            if (deviceId != null && !string.IsNullOrEmpty(seq))
            {
                return await _db.ResultStandards.Where(p => p.InsertTime > from && p.InsertTime < to && p.DeviceId == deviceId && p.Seq == seq).ToListAsync();
            }
            else if (deviceId == null && !string.IsNullOrEmpty(seq))
            {
                return await _db.ResultStandards.Where(p => p.InsertTime > from && p.InsertTime < to && p.Seq == seq).ToListAsync();
            }
            else if (deviceId != null && string.IsNullOrEmpty(seq))
            {
                return await _db.ResultStandards.Where(p => p.InsertTime > from && p.InsertTime < to && p.DeviceId == deviceId).ToListAsync();
            }
            else
            {
                return await _db.ResultStandards.Where(p => p.InsertTime > from && p.InsertTime < to).ToListAsync();
            }
        }

        public async Task<bool> UpdateStatus(DateTime from, DateTime to, string seq, string status)
        {
            try
            {
                var _lstItem = await _db.ResultStandards.Where(p => p.InsertTime > from && p.InsertTime < to && p.Seq == seq).ToListAsync();
                if (_lstItem != null && _lstItem.Count > 0)
                {
                    foreach(var item in _lstItem)
                    {
                        item.Status = status;
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
