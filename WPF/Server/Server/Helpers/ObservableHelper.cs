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
        public static ObservableCollection<T> ObjectsToObs(List<T> objects)
        {
            var res = new ObservableCollection<T>();
            
            foreach (var obj in objects) 
            {
                res.Add(obj);
            }
            return res;
        }

        public static ObservableCollection<User> UsersToObs(List<User> users)
        {
            var res = new ObservableCollection<User>();
            
            foreach (var user in users)
            {
                if (user.Group==null)
                {
                    user.Group = new Group() {Name = ""};
                }
                res.Add(user);
            }
            
            return res;
            
        }
    }
}