using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Client.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Server.Database;
using Server.Helpers;
using Server.Models;
using Server.Views;

namespace Server.ViewModels
{
    public class MainWindowViewModel:BaseViewModel
    {
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<Group> Groups { get; set;}= new ObservableCollection<Group>();

        private Window _window;

        public MainWindowViewModel(Window window)
        {
            _window = window;

            DBHelper.LoadAll(Users, Groups);
        }

        public RelayCommand SaveBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        using (Context db = new Context())
                        {
                            foreach (var user in db.Users)
                            {
                                var candidate = Users.FirstOrDefault(x => x.Id == user.Id);
                                if (candidate==null)
                                {
                                    db.Users.Remove(user);
                                }
                                else if(candidate.Group!= user.Group && candidate.Group.Name!="")
                                {
                                    user.Group = candidate.Group;
                                }
                            }

                            db.SaveChanges();
                        }

                        MessageBox.Show("Изменения сохранены!");
                    }
                );
            }
        }
        
        
        
        public RelayCommand ResetBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        Users.Clear();
                        Groups.Clear();

                        DBHelper.LoadAll(Users, Groups);
                    }
                );
            }
        }
        
        public RelayCommand AddGroupBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        _window.IsEnabled = false;
                        Window addWindow = new GroupWindow();
                        addWindow.Owner = _window;
                        addWindow.Show();
                        
                    }
                );
            }
        }
        
        
    }
}