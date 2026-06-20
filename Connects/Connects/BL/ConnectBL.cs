using System;
using System.Collections.Generic;
using System.Linq;
using Connects.Models;

namespace Connects.BL
{
    public class ConnectBL
    {
        private static LABContext db;

        public static Connect Get_Connect(int portID, string computerID)
        {
            db = new LABContext();
            return db.Connects.Where(p => p.PortId == portID && p.ComputerId == computerID).FirstOrDefault();
        }

        public static Connect Get_ConnectByDeviceId(long deviceID)
        {
            db = new LABContext();
            return db.Connects.Where(p => p.DeviceId == deviceID).FirstOrDefault();
        }

        public static List<Connect> Get_ListConnect(string computerID, bool status)
        {
            db = new LABContext();
            var lstConnect = db.Connects.Where(p => p.Status == status && p.ComputerId == computerID).ToList();

            return lstConnect != null ? lstConnect : new List<Connect>(); 
        }

        public static List<Connect> Get_ListConnectAll(string computerID)
        {
            db = new LABContext(); 
             var lstConnect = db.Connects.Where(p => p.ComputerId == computerID && p.PortId < 10).OrderBy(p => p.ComputerId).ThenBy(p => p.PortId).ToList();
            return lstConnect != null ? lstConnect : new List<Connect>();
        }

        public static void Update(string computerID, int portID, string com, double baudRate, double dataBit, string parity, double stopBit, bool rts, string description)
        {
            using (var db = new LABContext())
            {
                var connect = db.Connects.Where(p => p.ComputerId == computerID && p.PortId == portID).FirstOrDefault();
                if (connect != null)
                {
                    connect.Com = com;
                    connect.BaudRate = baudRate;
                    connect.DataBit = dataBit;
                    connect.Parity = parity;
                    connect.StopBit = stopBit;
                    connect.Rts = rts;
                    connect.Description = description;
                    db.SaveChanges();
                }
            }
        }

        public static void Update(string computerID, int portID, string com, double baudRate, string description)
        {
            using (var db = new LABContext())
            {
                var connect = db.Connects.Where(p => p.ComputerId == computerID && p.PortId == portID).FirstOrDefault();
                if (connect != null)
                {
                    connect.Com = com;
                    connect.BaudRate = baudRate;
                    connect.Description = description;
                    db.SaveChanges();
                }
            }
        }

        public static void Update_DeviceId(long deviceID)
        {
            using (var db = new LABContext())
            {
                var lstConnect = db.Connects.Where(p => p.DeviceId == deviceID).ToList();
                if (lstConnect != null)
                {
                    foreach (var connect in lstConnect)
                    {
                        connect.DeviceId = null;
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void Update_DeviceIdAndStatus(string computerID, int portID, long deviceID, bool status)
        {
            using (var db = new LABContext())
            {
                var connect = db.Connects.Where(p => p.ComputerId == computerID && p.PortId == portID).FirstOrDefault();
                if (connect != null)
                {
                    connect.DeviceId = deviceID;
                    connect.Status = status;
                    db.SaveChanges();
                }
            }
        }

        public static void Update_Status(string computerID, int portID, bool status)
        {
            using (var db = new LABContext())
            {
                var connect = db.Connects.Where(p => p.ComputerId == computerID && p.PortId == portID).FirstOrDefault();
                if (connect != null)
                {
                    connect.Status = status;
                    db.SaveChanges();
                }
            }
        }

        public static void Add(string computerID, int portID, string com, double baudRate, double dataBit, string parity, double stopBit, bool rts, string description)
        {
            using (var db = new LABContext())
            {
                Connect connect = new Connect();
                connect.ComputerId = computerID;
                connect.PortId = portID;
                connect.Com = com;
                connect.BaudRate = baudRate;
                connect.DataBit = dataBit;
                connect.Parity = parity;
                connect.StopBit = stopBit;
                connect.Rts = rts;
                connect.Description = description;
                db.Connects.Add(connect);
                db.SaveChanges();
            }
        }

        public static void Delete(string computerID, int portID)
        {
            using (var db = new LABContext())
            {
                var connect = db.Connects.Where(p => p.ComputerId == computerID && p.PortId == portID).FirstOrDefault();
                if (connect != null)
                {
                    db.Connects.Remove(connect);
                    db.SaveChanges();
                }
            }
        }

        public static bool DeleteConect(int portID, string computerID)
        {
            using (var db = new LABContext())
            {
                var connect = db.Connects.Where(p => p.PortId == portID && p.ComputerId == computerID).FirstOrDefault();
                if (connect != null)
                {
                    try
                    {
                        db.Connects.Remove(connect);
                        db.SaveChanges();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        public static Connect Get_ConnectByDevicePKAndComputer(long deviceID, string computer)
        {
            db = new LABContext();
            return db.Connects.Where(p => p.DeviceId == deviceID && p.ComputerId == computer).FirstOrDefault();
        }
    }
}
