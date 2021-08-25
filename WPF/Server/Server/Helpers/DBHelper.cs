using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.Database;
using Server.Models;

namespace Server.Helpers
{
    public static class DBHelper
    {
        public static void LoadAll(ObservableCollection<User> users, ObservableCollection<Group>groups)
        {
            users.Clear();
            groups.Clear();
            
            using (Context db = new Context())
            {
                var tempUsers = db.Users.Include(x => x.Group).ToList();
                var tempGroups = db.Groups.ToList();
                
                ObservableHelper<Group>.ObjectsToObs(tempGroups, groups);
                ObservableHelper<User>.ObjectsToObs(tempUsers,users);
            }
        }


        public static void SaveUsers(ObservableCollection<User> users)
        {
            using (Context db = new Context())
            {
                foreach (var user in db.Users)
                {
                    var candidate = users.FirstOrDefault(x => x.Id == user.Id);
                    if (candidate==null) db.Users.Remove(user);
                    else if(candidate.Group!= user.Group) user.Group = candidate.Group;
                                
                }

                db.SaveChanges();
            }
        }
        
        public static void SaveGroupsDeleted(ObservableCollection<Group> groups)
        {
            using (Context db = new Context())
            {
                foreach (var group in db.Groups)
                {
                    var candidate = groups.FirstOrDefault(x => x.Id == group.Id);
                    if (candidate == null)
                    {
                        using (Context db2 = new Context())
                        {
                            var tempUsers = db2.Users.Include(x=>x.Group).Where(x => x.Group.Id == group.Id);
                            foreach (var item in tempUsers)
                            {
                                item.Group = null;
                            }
                                        
                            db2.SaveChanges();
                        }
                                    
                        db.Groups.Remove(group);
                    }

                }  
                db.SaveChanges();
            }
        }

    }
    
    
}