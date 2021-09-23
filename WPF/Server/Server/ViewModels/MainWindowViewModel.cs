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

        public List<User> OldUsers; 
        public ObservableCollection<Group> Groups { get; set;}= new ObservableCollection<Group>();
        public Group SelectedGroup { get; set; }


        private string _prevFilter="";
        public string SearchFilter { get; set; }

        public User SelectedUser { get; set;}

        private Window _window;

        public MainWindowViewModel(Window window)
        {
            SearchFilter = "";
            
            _window = window;

            OldUsers = new List<User>();
            
            _window.Closing += new CancelEventHandler(Window_Closing);

            DBHelper.LoadAll(Users, Groups, OldUsers);
        }

        public RelayCommand SaveBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        
                        /*foreach (var item in Groups)
                        {
                            MessageBox.Show(item.Name);
                        }*/
                        DBHelper.SaveGroupsDeleted(Groups, Users);
                        
                        
                        /*foreach (var item in Users)
                        {
                            if (item.Group!=null)
                            {
                                MessageBox.Show(item.Group.Name);
                            }
                            
                        }*/
                        
                        DBHelper.SaveUsers(Users);

                        OldUsers.Clear();

                        foreach (var user in Users)
                        {
                            OldUsers.Add((User)user.Clone());
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
                        
                        if (DBHelper.IsChanged(Users, OldUsers,  _prevFilter))
                        {
                            var result=MessageBox.Show("У вас остались не сохранённые изменения, хотите сохранить?",
                                "Предупреждение", MessageBoxButton.YesNo);
                            if (result==MessageBoxResult.Yes)
                            {
                                DBHelper.SaveGroupsDeleted(Groups, Users);
                                DBHelper.SaveUsers(Users);
                                
                            }
                        }
                        SearchFilter = "";
                        OnPropertyChanged("SearchFilter");
                        _prevFilter = "";
                        DBHelper.LoadAll(Users, Groups, OldUsers);
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
                        Window addWindow = new GroupWindow(this);
                        addWindow.Owner = _window;
                        addWindow.Show();
                        
                    }
                );
            }
        }

        public RelayCommand EditBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        if (SelectedGroup==null)
                        {
                            MessageBox.Show("Не выбрана группа!");
                            return;
                        }
                        Window addWindow = new GroupWindow(this, true);
                        _window.IsEnabled=false;
                        addWindow.Owner = _window;
                        addWindow.Show();
                    }
                );
            }
        }
        
        
        public RelayCommand SearchBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        if (SearchFilter=="")
                        {
                            return;
                        }
                        if (DBHelper.IsChanged(Users,OldUsers ,_prevFilter))
                        {
                            var result=MessageBox.Show("У вас остались не сохранённые изменения, хотите сохранить?",
                                "Предупреждение", MessageBoxButton.YesNo);
                            if (result==MessageBoxResult.Yes)
                            {
                                DBHelper.SaveGroupsDeleted(Groups, Users);
                                DBHelper.SaveUsers(Users);
                                
                            }
                        }
                        
                        DBHelper.LoadAll(Users, Groups, OldUsers,SearchFilter);
                        _prevFilter = SearchFilter;
                    }
                );
            }
        }
        
        
        void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DBHelper.IsChanged(Users, OldUsers,  _prevFilter))
            {
                var result=MessageBox.Show("У вас остались не сохранённые изменения, хотите сохранить?",
                    "Предупреждение", MessageBoxButton.YesNo);
                if (result==MessageBoxResult.Yes)
                {
                    DBHelper.SaveGroupsDeleted(Groups, Users);
                    DBHelper.SaveUsers(Users);
                }
            }

        }
        
        
        
    }
}