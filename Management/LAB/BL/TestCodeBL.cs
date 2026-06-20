using Management.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class TestCodeBL
    {
        private readonly LABContext _db;
        public TestCodeBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<TestCode>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.TestCodes.ToListAsync();
            }
            else
            {
                return await _db.TestCodes.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task<List<TestCode>> GetListByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.TestCodes.ToListAsync();
                }
                else
                {
                    return await _db.TestCodes.Where(p => p.Name.Contains(value)).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.TestCodes.Where(p => p.Active == true).ToListAsync();
                }
                else
                {
                    return await _db.TestCodes.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
                }
            }

        }

        public async Task<TestCode> Get(long id)
        {
            return await _db.TestCodes.Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> Save(TestCodeModel testCode)
        {
            try
            {
                var _obj = await _db.TestCodes.Where(p => p.Code == testCode.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new TestCode() 
                    { 
                        Code = testCode.code, 
                        Name = testCode.name, 
                        TestTypeId = testCode.testtypeid,
                        CategoryId = testCode.categoryid,
                        NormalRangeM = testCode.normalrangem,
                        NormalRangeF = testCode.normalrangef,
                        NormalResult = testCode.normalresult,
                        LowerLimit = string.IsNullOrEmpty(testCode.lowerlimit) ? null : double.Parse(testCode.lowerlimit),
                        HigherLimit = string.IsNullOrEmpty(testCode.higherlimit) ? null : double.Parse(testCode.higherlimit),
                        LowerLimitF = string.IsNullOrEmpty(testCode.lowerlimitf) ? null : double.Parse(testCode.lowerlimitf),
                        HigherLimitF = string.IsNullOrEmpty(testCode.higherlimitf) ? null : double.Parse(testCode.higherlimitf),
                        Unit = testCode.unit,
                        IsTestHead = testCode.istesthead,
                        IsTestChild = testCode.istestchild,
                        PrintOrder = string.IsNullOrEmpty(testCode.printorder) ? null : int.Parse(testCode.printorder),
                        CodeBHYT = testCode.codebhyt,
                        NameBHYT = testCode.namebhyt,
                        Active = testCode.active 
                    };
                    await _db.TestCodes.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = testCode.name;
                    _obj.Active = testCode.active;
                    _obj.Name = testCode.name;
                    _obj.TestTypeId = testCode.testtypeid;
                    _obj.CategoryId = testCode.categoryid;
                    _obj.NormalRangeM = testCode.normalrangem;
                    _obj.NormalRangeF = testCode.normalrangef;
                    _obj.NormalResult = testCode.normalresult;
                    _obj.LowerLimit = string.IsNullOrEmpty(testCode.lowerlimit) ? null : double.Parse(testCode.lowerlimit);
                    _obj.HigherLimit = string.IsNullOrEmpty(testCode.higherlimit) ? null : double.Parse(testCode.higherlimit);
                    _obj.LowerLimitF = string.IsNullOrEmpty(testCode.lowerlimitf) ? null : double.Parse(testCode.lowerlimitf);
                    _obj.HigherLimitF = string.IsNullOrEmpty(testCode.higherlimitf) ? null : double.Parse(testCode.higherlimitf);
                    _obj.Unit = testCode.unit;
                    _obj.IsTestHead = testCode.istesthead;
                    _obj.IsTestChild = testCode.istestchild;
                    _obj.PrintOrder = string.IsNullOrEmpty(testCode.printorder) ? null : int.Parse(testCode.printorder);
                    _obj.CodeBHYT = testCode.codebhyt;
                    _obj.NameBHYT = testCode.namebhyt;
                    _obj.Active = testCode.active;
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
                var _obj = await _db.TestCodes.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.TestCodes.Remove(_obj);
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
