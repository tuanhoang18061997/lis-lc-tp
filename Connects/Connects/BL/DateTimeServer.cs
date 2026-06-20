using Connects.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace Connects.BL
{
    public class DateTimeServer
    {
        public static DateTime Get_DateServerByEntity()
        {
            try
            {
                using (var db = new LABContext())
                {
                    //return ((IObjectContextAdapter)db).ObjectContext.CreateQuery<DateTime>("CurrentDateTime() ").AsEnumerable().FirstOrDefault();
                    return db.Users.Select(q => DateTime.Now).FirstOrDefault();
                };            
            }
            catch(Exception ex)
            {
                return DateTime.Now;
            }
        }
    }
}
