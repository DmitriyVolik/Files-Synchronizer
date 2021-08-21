using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using Server.Models;

namespace Server.Helpers
{
    public class ObservableHelper<T>
    {
        public static ObservableCollection<T> ObjectsToObs(List<T> users)
        {
            var res = new ObservableCollection<T>();
            
            foreach (var user in users)
            {
                res.Add(user);
            }

            return res;
        }
    }
}