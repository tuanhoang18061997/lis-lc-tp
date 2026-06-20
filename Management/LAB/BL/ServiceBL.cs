using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class ServiceBL
    {
        private readonly LABContext _db;
        public ServiceBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Service>> GetListServiceByGroup(string code, bool all = false)
        {
            if (all)
            {
                return await _db.Services.Where(p => p.Category.Group.Code == code).OrderBy( p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
            }
            else
            {
                return await _db.Services.Where(p => p.Active == true && p.Category.Group.Code == code).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
            }
        }

        public async Task<List<Service>> GetListServiceByCategory(long? categoryId, bool all = false)
        {
            if (all)
            {
                if (categoryId == null)
                    return await _db.Services.OrderBy( p => p.Name).ToListAsync();
                else
                    return await _db.Services.Where(p => p.CategoryId == categoryId).OrderBy(p => p.Name).ToListAsync();
            }
            else
            {
                if (categoryId == null)
                    return await _db.Services.Where(p => p.Active == true).OrderBy(p => p.Name).ToListAsync();
                else
                    return await _db.Services.Where(p => p.Active == true && p.CategoryId == categoryId).OrderBy(p => p.Name).ToListAsync();
            }
        }

        public async Task<List<Service>> GetListServiceByCategory(string code, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(code))
                    return await _db.Services.OrderBy(p => p.Name).ToListAsync();
                else
                    return await _db.Services.Where(p => p.Category.Code == code).OrderBy(p => p.Name).ToListAsync();
            }
            else
            {
                if (string.IsNullOrEmpty(code))
                    return await _db.Services.Where(p => p.Active == true).OrderBy(p => p.Name).ToListAsync();
                else
                    return await _db.Services.Where(p => p.Active == true && p.Category.Code == code).OrderBy(p => p.Name).ToListAsync();
            }
        }

        public async Task<List<Service>> GetListServiceByGroupCLS()
        {
            return await _db.Services.Where(p => p.Active == true && p.Category.Group.Id == 2).OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<List<Service>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.Services.OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
            }
            else
            {
                return await _db.Services.Where(p => p.Active == true).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
            }
        }

        public async Task<List<Service>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Services.OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
                else
                {
                    return await _db.Services.Where(p => p.Name.Contains(value)).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Services.Where(p => p.Active == true).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
                else
                {
                    return await _db.Services.Where(p => p.Active == true && p.Name.Contains(value)).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
            }

        }

        public async Task<List<Service>> GetListByGroupCodeAndValue(string code, string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Services.Where(p => p.Category.Group.Code == code).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
                else
                {
                    return await _db.Services.Where(p => p.Category.Group.Code == code && p.Name.Contains(value)).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Services.Where(p => p.Active == true && p.Category.Group.Code == code).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
                else
                {
                    return await _db.Services.Where(p => p.Active == true && p.Name.Contains(value) && p.Category.Group.Code == code).OrderBy(p => p.Category.PrintOrder).ThenBy(p => p.Name).ToListAsync();
                }
            }

        }

        public async Task<Service> Get(string code)
        {
            return await _db.Services.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(ServiceModel service)
        {
            try
            {
                var _obj = await _db.Services.Where(p => p.Code == service.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new Service() { Code = service.code, Name = service.name, CategoryId = service.categoryid, PrintSampleId = service.printsampleid, PrintOrder = service.printorder, ProcessType = service.processtype, Active = service.active };
                    await _db.Services.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = service.name;
                    _obj.CategoryId = service.categoryid;
                    _obj.PrintSampleId = service.printsampleid;
                    _obj.PrintOrder = service.printorder;
                    _obj.ProcessType = service.processtype;
                    _obj.Active = service.active;
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
                var _obj = await _db.Services.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.Services.Remove(_obj);
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

        public async Task<Service> GetById(long? id)
        {
            return await _db.Services.Where(p => p.Id == id).FirstOrDefaultAsync();
        }
    }
}
