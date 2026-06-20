using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class ServiceTestBL
    {
        private readonly LABContext _db;
        public ServiceTestBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<ServiceTest>> GetListServiceTestByServiceId(long serviceId)
        {
            var _lstServiceTest = await _db.ServiceTests.Where(p => p.ServiceId == serviceId).OrderBy(p => p.TestCode.PrintOrder).ToListAsync();
            return _lstServiceTest;
        }

        public async Task<bool> Save(List<ServiceTestModel> lstServiceTest)
        {
            try
            {
                if (lstServiceTest != null && lstServiceTest.Count > 0)
                {
                    foreach (var item in lstServiceTest)
                    {
                        var _obj = await _db.ServiceTests.Where(p => p.ServiceId == item.serviceid && p.TestCodeId == item.testcodeid).FirstOrDefaultAsync();
                        if (_obj == null)
                        {
                            await _db.ServiceTests.AddAsync(new ServiceTest { ServiceId = item.serviceid, TestCodeId = item.testcodeid });
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

        public async Task<bool> Delete(List<ServiceTestModel> lstServiceTest)
        {
            try
            {
                if (lstServiceTest != null && lstServiceTest.Count > 0)
                {
                    foreach(var item in lstServiceTest)
                    {
                        var _obj = await _db.ServiceTests.Where(p => p.ServiceId == item.serviceid && p.TestCodeId == item.testcodeid).FirstOrDefaultAsync();
                        if (_obj != null)
                        {
                            _db.ServiceTests.Remove(_obj);
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
