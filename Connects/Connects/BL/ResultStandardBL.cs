using Connects.Models;
using System;
using log4net;
using System.Linq;
using System.Collections.Generic;

namespace Connects.BL
{
    public class ResultStandardBL
    {
        private static readonly ILog log4net = LogManager.GetLogger(typeof(BL.ResultStandardBL).Name);

        public static void Insert(string seq, Map map, double? result, string posNeg, DateTime insertTime)
        {
            Insert(string.Empty, seq, map, result, posNeg, insertTime);
        }

        public static void Insert(string sid, string seq, Map map, double? result, string posNeg, DateTime insertTime)
        {
            using (var db = new LABContext())
            {
                try
                {
                    ResultStandard resultStandard = new ResultStandard();
                    resultStandard.Seq = seq;
                    resultStandard.TestCodeId = map.TestCodeId;
                    resultStandard.TestCodeIn = map.TestcodeIn;
                    resultStandard.Result = result;
                    resultStandard.PosNeg = posNeg;
                    resultStandard.Unit = map.TestCode != null ? map.TestCode.Unit : null;
                    resultStandard.Status = StatusForResultStandard.NotUpdate;
                    resultStandard.DeviceId = map.DeviceId;
                    resultStandard.InsertTime = insertTime; // Time hiện tại hệ thống
                    resultStandard.ComputerId = Environment.MachineName;
                    
                    resultStandard.ReturnTime = insertTime; // Time trên máy XN trả về với KQ
                    db.ResultStandards.Add(resultStandard);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    log4net.Error("Không thể cập nhật Seq " + seq + " và TestCode " + map.TestCode + " vào Result_Standard " + ex);
                }
            }
        }
    }
}
