using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using Server.Models;

namespace Server.Helpers
{
    public class ObservableHelper<T>
    {
        public static void ObjectsToObs(List<T> users, ObservableCollection<T> obs)
        {

            foreach (var user in users)
            {
                obs.Add(user);
            }

        }
        
        public static void UsersToObs(List<User> users, ObservableCollection<User> usersOb)
        {
            foreach (var user in users)
            {
                if (user.Group==null)
                {
                    user.Group = new Group() {Name = ""};
                }
                
                usersOb.Add(user);
            }

        }
    }
}