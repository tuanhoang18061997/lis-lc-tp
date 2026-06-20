using Management.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class DigitalSignBL
    {
        private readonly LABContext _db;
        public DigitalSignBL(LABContext db)
        {
            _db = db;
        }

        //public async Task<List<TestCode>> GetList(bool all = false)
        //{
        //    if (all)
        //    {
        //        return await _db.TestCodes.ToListAsync();
        //    }
        //    else
        //    {
        //        return await _db.TestCodes.Where(p => p.Active == true).ToListAsync();
        //    }
        //}

        //public async Task<List<TestCode>> GetListByValue(string value, bool all = false)
        //{
        //    if (all)
        //    {
        //        if (string.IsNullOrEmpty(value))
        //        {
        //            return await _db.TestCodes.ToListAsync();
        //        }
        //        else
        //        {
        //            return await _db.TestCodes.Where(p => p.Name.Contains(value)).ToListAsync();
        //        }
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(value))
        //        {
        //            return await _db.TestCodes.Where(p => p.Active == true).ToListAsync();
        //        }
        //        else
        //        {
        //            return await _db.TestCodes.Where(p => p.Active == true && p.Name.Contains(value)).ToListAsync();
        //        }
        //    }

        //}

        //public async Task<TestCode> Get(long id)
        //{
        //    return await _db.TestCodes.Where(p => p.Id == id).FirstOrDefaultAsync();
        //}

        public async Task<bool> Save(string ReferenceType, string? ReferenceKeyResult, long? SignId, string SignUserId, string TaxCode, string TargetText, string RequestUrl, string ResponseData, int? Status, long? CreatorId, DateTime CreatedAt)
        {
            try
            {
                var _digitalSign = new DigitalSign();
                _digitalSign.ReferenceType = ReferenceType;
                _digitalSign.ReferenceKeyResult = ReferenceKeyResult;
                _digitalSign.SignId = SignId;
                _digitalSign.SignUserId = SignUserId;
                _digitalSign.TaxCode = TaxCode;
                _digitalSign.TargetText = TargetText;
                _digitalSign.RequestUrl = RequestUrl ?? null;
                _digitalSign.ResponseData = ResponseData;
                _digitalSign.Status = Status;
                _digitalSign.CreatorId = CreatorId;
                _digitalSign.CreatedAt = CreatedAt;
                _digitalSign.Deleted = false;
                await _db.DigitalSigns.AddAsync(_digitalSign);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSignStatus_CKS_CDHA(long resultCDHAId, string keyResult, long userInsertIdOrUpdateId, DateTime dateTime)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.KeyResultForHis == keyResult).FirstOrDefaultAsync();
                if (_resultCDHA == null) return false;
                _resultCDHA.UserUpdateId = userInsertIdOrUpdateId;
                _resultCDHA.UpdateTime = dateTime;
                _resultCDHA.SignStatus = 0;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSignStatus_CKS_XN(long resultXNId, string keyResult, long userInsertIdOrUpdateId, DateTime dateTime)
        {
            try
            {
                var _resultXN = await _db.ResultXNs.Where(p => p.Active == true && p.Id == resultXNId && p.KeyResultForHis == keyResult).FirstOrDefaultAsync();
                if (_resultXN == null) return false;
                _resultXN.UserUpdateId = userInsertIdOrUpdateId;
                _resultXN.UpdateTime = dateTime;
                _resultXN.SignStatus = 0;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(long? id, string referenceKeyResult)
        {
            try
            {
                var _digitalSign = await _db.DigitalSigns.Where(p => p.Deleted == false && p.SignId == id && p.ReferenceKeyResult == referenceKeyResult).FirstOrDefaultAsync();
                if (_digitalSign == null) return false;
                _digitalSign.Deleted = true;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
