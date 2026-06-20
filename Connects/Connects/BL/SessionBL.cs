using Connects.Models;
using System;
using System.Linq;

namespace Connects.BL
{
    public class SessionBL
    {
        private static LABContext db;
        private static Session session = null;

        public static void InsertOrUpdate(long userId, string computerID)
        {
            using (var db = new LABContext())
            {
                var sessionOld = db.Sessions.Where(p => p.UserId == userId && p.ComputerId == computerID).FirstOrDefault();
                if (sessionOld == null)
                {
                    Session sessionNew = new Session();
                    sessionNew.UserId = userId;
                    sessionNew.ComputerId = computerID;
                    sessionNew.IsConnect = true;

                    db.Sessions.Add(sessionNew);
                    db.SaveChanges();
                }
                else
                {
                    sessionOld.IsConnect = true;
                    db.SaveChanges();
                }
            }
        }

        public static void UpdateStatus(string computerId, bool status)
        {
            using (var db = new LABContext())
            {
                var session = db.Sessions.Where(p => p.ComputerId == computerId && p.IsConnect == true).FirstOrDefault();
                if (session != null)
                {
                    session.IsConnect = status;
                    db.SaveChanges();
                }
            }
        }

        public static Session Get_SessionByComputerIDForConnect(string computerId)
        {         
            if (session == null)
            {
                db = new LABContext();
                return db.Sessions.Where(p => p.ComputerId == computerId && p.IsConnect == true).FirstOrDefault();
            }
            return session;
        }

        public static void Reset_Session()
        {
            session = null;
        }

        public static void ResetStatusAllUserOnCumputerID(string computerID)
        {
            using (var db = new LABContext())
            {
                var lstSession = db.Sessions.Where(p => p.ComputerId == computerID).ToList();
                if (lstSession != null)
                {
                    foreach (var item in lstSession)
                    {
                        item.IsConnect = false;
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}
