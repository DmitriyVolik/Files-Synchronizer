using System.Linq;
using Server.Database;

namespace Server.Helpers
{
    public class DbHelper
    {
        public static int GetGroupIdBySession(string sessionToken)
        {

            int id;
            
            using (var db=new Context())
            {
                var session = db.Sessions.FirstOrDefault(x => x.Token == sessionToken);

                if (session==null)
                {
                    return -1;
                }

                db.Entry(session).Reference("User").Load();
                
                db.Entry(session.User).Reference("Group").Load();

                if (session.User.Group==null)
                {
                    return -1;
                }
                id = session.User.Group.Id;

            }

            return id;
        }
    }
}