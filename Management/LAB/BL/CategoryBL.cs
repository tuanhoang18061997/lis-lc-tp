using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class CategoryBL
    {
        private readonly LABContext _db;
        public CategoryBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<Category>> GetListCategoryByGroup(string groupCode, bool all = false)
        {
            if(all)
            {
                return await _db.Categories.Where(p => p.Group.Code == groupCode).ToListAsync();
            }
            else
            {
                return await _db.Categories.Where(p => p.Active == true && p.Group.Code == groupCode).ToListAsync();
            }
        }

        public async Task<List<Category>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.Categories.ToListAsync();
            }
            else
            {
                return await _db.Categories.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<Category>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Categories.ToListAsync();
                }
                else
                {
                    return await _db.Categories.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Categories.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.Categories.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<Category> Get(string code)
        {
            return await _db.Categories.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(CategoryModel category)
        {
            try
            {
                var _obj = await _db.Categories.Where(p => p.Code == category.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new Category() { Code = category.code, Name = category.name, Active = category.active };
                    await _db.Categories.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = category.name;
                    _obj.Active = category.active;
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
                var _obj = await _db.Categories.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.Categories.Remove(_obj);
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
