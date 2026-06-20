using System;
using System.Collections.Generic;
using System.Linq;
using Connects.Models;

namespace Connects.BL
{
    public class DeviceBL
    {
        private static LABContext db;

        public static Device Get_Device(long id)
        {
            db = new LABContext();
            return db.Devices.Where(p => p.Id == id).FirstOrDefault();
        }

        public static Device Get_Device(string code)
        {
            db = new LABContext();
            return db.Devices.Where(p => p.Code == code).FirstOrDefault();
        }

        public static List<Device> Get_ListDevice()
        {
            db = new LABContext();
            return db.Devices.Where(p => p.Active == true).OrderBy(p => p.Name).ToList();
        }

        public static void Update(long id, string code, string name, string protocol, string computerID, bool active)
        {
            using (var db = new LABContext())
            {
                var device = db.Devices.Where(p => p.Id == id).FirstOrDefault();
                if (device != null)
                {
                    device.Code = code;
                    device.Name = name;
                    device.Protocol = protocol;
                    device.Active = active;
                    db.SaveChanges();
                }
            }
        }

        public static void Add(string code, string name, string protocol, string computerID, bool active)
        {
            using (var db = new LABContext())
            {
                Device device = new Device();
                device.Code = code;
                device.Name = name;
                device.Protocol = protocol;
                device.Active = active;

                db.Devices.Add(device);
                db.SaveChanges();
            }
        }

        public static void Delete(long deviceID)
        {
            using (var db = new LABContext())
            {
                var device = db.Devices.Where(p => p.Id == deviceID).FirstOrDefault();
                if (device != null)
                {
                    db.Devices.Remove(device);
                    db.SaveChanges();
                }
            }
        }

        public static bool CheckExist(string code)
        {
            using (var db = new LABContext())
            {
                var device = db.Devices.Where(p => p.Code == code).FirstOrDefault();
                if (device != null) return true;
                return false;
            }
        }

        public static void UpdateProcessQC(long deviceID, bool processQC)
        {
            using (var db = new LABContext())
            {
                var device = db.Devices.Where(p => p.Id == deviceID).FirstOrDefault();
                if (device != null)
                {
                    device.ProcessQc = processQC;
                    db.SaveChanges();
                }
            }
        }
    }
}
