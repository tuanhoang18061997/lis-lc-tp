using Connects.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Connects.BL
{
    public class MapBL
    {
        private static LABContext db;

        public static List<Map> Get_ListMapByDeviceID(long? deviceID)
        {
            db = new LABContext();
            return db.Maps.Where(p => p.DeviceId == deviceID && p.TestcodeIn != "Null" && p.TestcodeIn2 != "Null").ToList();
        }
 
        public static Map Get_MapByTestCodeIn(long deviceID, string testCodeIn)
        {
            db = new LABContext();
            return db.Maps.Where(p => p.DeviceId == deviceID && (p.TestcodeIn.Trim() == testCodeIn.Trim() || p.TestcodeIn2.Trim() == testCodeIn.Trim())).FirstOrDefault();
        }
    }
}
