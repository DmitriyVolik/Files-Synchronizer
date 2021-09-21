using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Microsoft.EntityFrameworkCore;
using Server.Database;
using Server.Models;

namespace Server.Helpers
{
    public static class DBHelper
    {
        
       
        public static void LoadAll(ObservableCollection<User> users, ObservableCollection<Group>groups, List<User> oldUsers, string filter="")
        {
            users.Clear();
            groups.Clear();
            oldUsers.Clear();
            
            using (Context db = new Context())
            {
                List<User> tempUsers;

                if (filter!="")
                {
                    tempUsers = db.Users.Where(x=>x.Login.ToLower().Substring(0,filter.Length)==filter.ToLower()).Include(x => x.Group).ToList();
                }
                else
                {
                    tempUsers = db.Users.Include(x => x.Group).ToList();
                }
                var tempGroups = db.Groups.ToList();

                foreach (var user in tempUsers)
                {
                    oldUsers.Add((User)user.Clone());
                } 
                
                ObservableHelper<Group>.ListToObs(tempGroups, groups);
                ObservableHelper<User>.ListToObs(tempUsers,users);
            }
        }


        public static void SaveUsers(ObservableCollection<User> users)
        {
            using (Context db = new Context())
            {
                
                foreach (var user in db.Users)
                {
                    var candidate = users.FirstOrDefault(x => x.Id == user.Id);
                    if (candidate == null)
                    {
                        db.Users.Remove(user);
                    }
                    else
                    {
                        user.Group = candidate.Group;
                        db.ChangeTracker.DetectChanges();
                    }
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


        public static bool IsChanged(ObservableCollection<User> users, List<User> oldUsers, string filter="")
        {

            List<User> tempOldUsers;
            
            if (filter!="")
            {
                tempOldUsers = oldUsers.Where(x=>x.Login.ToLower().Substring(0,filter.Length)==filter.ToLower()).ToList();
            }
            else
            {
                tempOldUsers = oldUsers;
            }
            
            foreach (var user in tempOldUsers)
            {
                var candidate = users.FirstOrDefault(x => x.Id == user.Id);
                if (candidate == null) 
                {
                    return true;
                }
                
                if (user.Group==null && candidate.Group==null)
                {
                    continue;
                }
                try
                {
                    if (candidate.Group.Id!=user.Group.Id)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    return true;
                }
            }
            return false;


        }
        
        

    }
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
    
}