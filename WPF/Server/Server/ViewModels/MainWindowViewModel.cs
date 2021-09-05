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
        public Group SelectedGroup { get; set; }


        private string _prevFilter="";
        public string SearchFilter { get; set; }

        public User SelectedUser { get; set;}

        private Window _window;

        public MainWindowViewModel(Window window)
        {
            SearchFilter = "";
            
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
                        DBHelper.SaveUsers(Users);
                        DBHelper.SaveGroupsDeleted(Groups);

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
                        
                        if (DBHelper.IsChanged(Users, _prevFilter))
                        {
                            var result=MessageBox.Show("У вас остались не сохранённые изменения, хотите сохранить?",
                                "Предупреждение", MessageBoxButton.YesNo);
                            if (result==MessageBoxResult.Yes)
                            {
                                DBHelper.SaveUsers(Users);
                                DBHelper.SaveGroupsDeleted(Groups);
                            }
                        }
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
                        if (DBHelper.IsChanged(Users, _prevFilter))
                        {
                            var result=MessageBox.Show("У вас остались не сохранённые изменения, хотите сохранить?",
                                "Предупреждение", MessageBoxButton.YesNo);
                            if (result==MessageBoxResult.Yes)
                            {
                                DBHelper.SaveUsers(Users);
                                DBHelper.SaveGroupsDeleted(Groups);
                            }
                        }
                        
                        DBHelper.LoadAll(Users, Groups, SearchFilter);
                        _prevFilter = SearchFilter;
                    }
                );
            }
        }
        
        
        
    }
}