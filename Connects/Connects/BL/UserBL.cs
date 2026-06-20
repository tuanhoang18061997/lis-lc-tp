using Connects.Models;
using System;
using System.Linq;

namespace Connects.BL
{
    public class UserBL
    {
        private static LABContext db;

        public static User Get_UserByUserID(long? userID)
        {
            db = new LABContext();
            return db.Users.Where(p => p.Id == userID).FirstOrDefault();
        }

        public static User Get_UserByUserCode(string userCode)
        {
            db = new LABContext();
            return db.Users.Where(p => p.Code == userCode).FirstOrDefault();
        }
    }
}
