using System.Linq;
using Server.Database;
using Server.Models;

namespace Server.Helpers
{
    public class DbHelper
    {
        public static Group GetGroupBySession(string sessionToken)
        {

            Group group;
            
            using (var db=new Context())
            {
                var session = db.Sessions.FirstOrDefault(x => x.Token == sessionToken);

                if (session==null)
                {
                    return null;
                }

                db.Entry(session).Reference("User").Load();
                
                db.Entry(session.User).Reference("Group").Load();

                if (session.User.Group==null)
                {
                    return null;
                }
                group = session.User.Group;

            }

            return group;
        }
    }
}