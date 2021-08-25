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
        public static void ObjectsToObs(List<T> objects, ObservableCollection<T> obs)
        {

            foreach (var obj in objects)
            {
                obs.Add(obj);
            }

        }
    }
}