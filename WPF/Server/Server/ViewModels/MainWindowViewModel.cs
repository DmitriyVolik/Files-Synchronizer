using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Client.ViewModels;
using Microsoft.EntityFrameworkCore;
using Server.Database;
using Server.Helpers;
using Server.Models;

namespace Server.ViewModels
{
    public class MainWindowViewModel:BaseViewModel
    {
        public ObservableCollection<User> Users { get; set;}
        public ObservableCollection<Group> Groups { get; set;}
        
        public MainWindowViewModel()
        {
            using (Context db = new Context())
            {
                var tempUsers = db.Users.Include(x=>x.Group).ToList();
                var tempGroups = db.Groups.ToList();
                
                Users=ObservableHelper<User>.ObjectsToObs(tempUsers);
                
                Groups=ObservableHelper<Group>.ObjectsToObs(tempGroups);
            }

        }
        
        public RelayCommand TestBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        MessageBox.Show(Users[0].Group.Name);
                    }
                );
            }
        }
    }
}