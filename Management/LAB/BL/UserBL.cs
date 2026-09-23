using Management.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Diagnostics;
using Type = Management.Models.Type;

namespace Management.BL
{
    public class UserBL
    {
        private readonly LABContext _db;
        public UserBL(LABContext db)
        {
            _db = db;
        }

        public async Task CreateUserAdmin()
        {
            var _user = await _db.Users.Where(p => p.Code == AdminModel.Code).FirstOrDefaultAsync();
            if (_user == null)
            {
                _user = new User { Code = AdminModel.Code, Name = AdminModel.Name, Password = AdminModel.Password, Active = true };
                await _db.Users.AddAsync(_user);
                await _db.SaveChangesAsync();
            }

            var _lstFunction = await _db.Functions.ToListAsync();
            if (_lstFunction != null)
            {
                var _lstUserFunction = await _db.UserFunctions.Where(p => p.UserId == _user.Id).ToListAsync();
                foreach (var item in _lstFunction)
                {
                    var _userFunction = _lstUserFunction.Where(p => p.FunctionId == item.Id).FirstOrDefault();
                    if (_userFunction == null)
                    {
                        await _db.UserFunctions.AddAsync(new UserFunction { UserId = _user.Id, FunctionId = item.Id });
                    }
                }
                await _db.SaveChangesAsync();
            }
        }

        public async Task<User> GetUserByCodeAndPassword(User user)
        {
            var _user = await _db.Users.Where(p => p.Active == true && p.Code == user.Code && p.Password == user.Password).FirstOrDefaultAsync();
            if (_user != null)
            {
                return new User() { Id = _user.Id, Code = _user.Code, Name = _user.Name, Password = _user.Password };
            }
            return null;
        }

        public async Task<bool> ChangePassword(string code, string passwordOld, string passwordNew)
        {
            var _user = await _db.Users.Where(p => p.Active == true && p.Code == code && p.Password == passwordOld).FirstOrDefaultAsync();
            if (_user != null)
            {
                _user.Password = passwordNew;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<User>> GetListUser(string type)
        {
            var _lstUser = await (from user in _db.Users
                                  join userType in _db.UserTypes on user.Id equals userType.UserId
                                  where user.Active == true && userType.Type.Code == type && user.Code != AdminModel.Code
                                  orderby user.Name
                                  select user).ToListAsync();
            return _lstUser;
        }

        public async Task<List<User>> GetListUser(bool all = false)
        {
            if (all)
            {
                return await _db.Users.Where(p => p.Code != AdminModel.Code).ToListAsync();
            }
            else
            {
                return await _db.Users.Where(p => p.Active == true && p.Code != AdminModel.Code).ToListAsync();
            }
        }

        public async Task<User> GetUser(string code, bool all = false)
        {
            if (all)
            {
                return await _db.Users.Where(p => p.Code == code).FirstOrDefaultAsync();
            }
            else
            {
                return await _db.Users.Where(p => p.Active == true && p.Code == code).FirstOrDefaultAsync();
            }
        }

        public async Task<User> GetUserById(long? id, bool all = false)
        {
            if (all)
            {
                return await _db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();
            }
            else
            {
                return await _db.Users.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
            }
        }

        public async Task<User> GetUser(long id, bool all = false)
        {
            if (all)
            {
                return await _db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();
            }
            else
            {
                return await _db.Users.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
            }
        }

        public async Task<List<User>> GetListUserByValue(string value, bool all = false)
        {
            if (all)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Users.Where(p => p.Code != AdminModel.Code).ToListAsync();
                }
                else
                {
                    return await _db.Users.Where(p => p.Name.Contains(value) && p.Code != AdminModel.Code).ToListAsync();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    return await _db.Users.Where(p => p.Active == true && p.Code != AdminModel.Code).ToListAsync();
                }
                else
                {
                    return await _db.Users.Where(p => p.Active == true && p.Name.Contains(value) && p.Code != AdminModel.Code).ToListAsync();
                }
            }
        }

        public async Task<List<Type>> GetListType()
        {
            return await _db.Types.ToListAsync();
        }

        public async Task<List<Function>> GetListFunction()
        {
            return await _db.Functions.OrderBy(p => p.Sort).ToListAsync();
        }

        public async Task<bool> Save(UserModel user)
        {
            try
            {
                var _user = await _db.Users.Where(p => p.Code == user.code).FirstOrDefaultAsync();
                if (_user == null)
                {
                    _user = new User()
                    {
                        Code = user.code,
                        Name = user.name,
                        Password = user.pass,
                        Active = user.active,
                        MaBHYT = user.mabhyt,
                        Cks = user.cks,
                        Cccd = user.cccd,
                        HisEmployeeId = user.hisEmployeeId
                    };
                    await _db.Users.AddAsync(_user);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _user.Name = user.name;
                    _user.Password = user.pass;
                    _user.MaBHYT = user.mabhyt;
                    _user.Cks = user.cks;
                    _user.Cccd = user.cccd;
                    _user.HisEmployeeId = user.hisEmployeeId;
                    _user.Active = user.active;
                    await _db.SaveChangesAsync();
                }
                await SaveUserType(_user.Id, user.type);
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
                var _user = await _db.Users.Where(p => p.Active == true && p.Code == code).FirstOrDefaultAsync();
                if (_user != null)
                {
                    var _lstUserType = await _db.UserTypes.Where(p => p.UserId == _user.Id).ToListAsync();
                    if (_lstUserType != null && _lstUserType.Count > 0)
                    {
                        _db.UserTypes.RemoveRange(_lstUserType);
                    }

                    var _lstUserFunction = await _db.UserFunctions.Where(p => p.UserId == _user.Id).ToListAsync();
                    if (_lstUserFunction != null && _lstUserFunction.Count > 0)
                    {
                        _db.UserFunctions.RemoveRange(_lstUserFunction);
                    }

                    _db.Users.Remove(_user);
                    await _db.SaveChangesAsync();
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

        public async Task SaveUserType(long userId, string type)
        {
            try
            {
                var _lstUserType = await _db.UserTypes.Where(p => p.UserId == userId).ToListAsync();
                if (_lstUserType != null && _lstUserType.Count > 0)
                {
                    _db.UserTypes.RemoveRange(_lstUserType);
                }

                var lstTypeId = type.Split(';').ToList();
                if (lstTypeId != null)
                {
                    foreach (var typeId in lstTypeId)
                    {
                        if (!string.IsNullOrEmpty(typeId.Trim()))
                        {
                            var _userType = new UserType() { UserId = userId, TypeId = long.Parse(typeId) };
                            await _db.UserTypes.AddAsync(_userType);
                        }
                    }
                }
                await _db.SaveChangesAsync();
            }
            catch { }
        }

        public async Task<bool> Save_UserFunction(UserFunctionModel userFunction)
        {
            try
            {
                var _lstUserFunction = await _db.UserFunctions.Where(p => p.User.Code == userFunction.code).ToListAsync();
                if (_lstUserFunction != null && _lstUserFunction.Count > 0)
                {
                    _db.UserFunctions.RemoveRange(_lstUserFunction);
                }

                var _lstFunctionId = userFunction.functions.Split(';').ToList();
                var _user = await _db.Users.Where(p => p.Code == userFunction.code).FirstOrDefaultAsync();
                if (_lstFunctionId != null && _user != null)
                {
                    foreach (var functionId in _lstFunctionId)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(functionId.Trim()))
                            {
                                var _userFunction = new UserFunction() { UserId = _user.Id, FunctionId = long.Parse(functionId) };
                                await _db.UserFunctions.AddAsync(_userFunction);
                            }
                        }
                        catch { }
                    }
                }
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserFunction>> GetUserFunction(long userId)
        {
            return await _db.UserFunctions.Where(p => p.UserId == userId).OrderBy(p => p.Function.Sort).ToListAsync();
        }
    }
}