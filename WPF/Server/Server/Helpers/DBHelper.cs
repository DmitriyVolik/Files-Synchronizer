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
            using (Context db = new Context())
            {
                var tempUsers = db.Users.Include(x => x.Group).ToList();
                var tempGroups = db.Groups.ToList();
                
                ObservableHelper<Group>.ObjectsToObs(tempGroups, groups);
                ObservableHelper<User>.UsersToObs(tempUsers,users);
            }
        }
        


    }
    
    
}