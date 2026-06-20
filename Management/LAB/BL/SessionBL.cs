using Management.Models;
using Microsoft.EntityFrameworkCore;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class SessionBL
    {
        private readonly LABContext _db;
        public SessionBL(LABContext db)
        {
            _db = db;
        }

        public async Task<Session> GetSession(string computerId)
        {
            var _session = await _db.Sessions.Where(p => p.ComputerId == computerId && p.IsManage == true).FirstOrDefaultAsync();
            return _session;
        }
    }
}
