using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Client.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using Server.Database;
using Server.Helpers;
using Server.Models;

namespace Server.ViewModels
{
    public class GroupWindowViewModel:BaseViewModel
    {

        private Window _window;
        
        private MainWindowViewModel _mainWindow;

        private bool _isEdit;

        private string _name ="";
        
        private string _folderPath="";
        
        public string Name
        {
            get { return _name;}
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public string FolderPath
        {
            get { return _folderPath;}
            set
            {
                _folderPath = value;
                OnPropertyChanged("FolderPath");
            }
        }

        public GroupWindowViewModel(Window window, MainWindowViewModel mainWindow, bool isEdit=false/*ObservableCollection<Group> groups, Group selectedGroup=null*/)
        {
            _mainWindow = mainWindow;
            if (isEdit)
            {
                Name = _mainWindow.SelectedGroup.Name;
                FolderPath = _mainWindow.SelectedGroup.FolderPath;
            }
            
            _isEdit = isEdit;
            _window = window;
            _window.Closing += new CancelEventHandler(Window_Closing);
        }

        
        public RelayCommand SaveBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        _window.Owner.IsEnabled = true;
                        if (Name.Length<4)
                        {
                            MessageBox.Show("Минимальное количество символов в названии 4", "Ошибка");
                            return;
                        }
                        if (_isEdit==false)
                        {
                            var newGroup = new Group() {Name = Name, FolderPath = FolderPath};
                            _mainWindow.Groups.Add(newGroup);
                            using (Context db = new Context())
                            {
                                db.Groups.Add(newGroup);
                                db.SaveChanges();
                            }
                            _window.Close();
                        }
                        else
                        {

                            using (Context db = new Context())
                            {
                                var candidate=db.Groups.FirstOrDefault(x => x.Id == 
                                    _mainWindow.SelectedGroup.Id);

                                candidate.Name = Name;
                                candidate.FolderPath = FolderPath;
                                db.SaveChanges();

                            }
                            
                            DBHelper.LoadAll(_mainWindow.Users, _mainWindow.Groups);
                        }
                        _window.Close();
                    }
                );
            }
        }

        public RelayCommand SelectFolder
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                       
                        var dlg = new CommonOpenFileDialog();
                        dlg.IsFolderPicker = true;
                        
                        if (dlg.ShowDialog() == CommonFileDialogResult.Ok) 
                        {
                            FolderPath = dlg.FileName;
                        }
                    }
                );
            }
        }
        
        void Window_Closing(object sender, CancelEventArgs e)
        {
            _window.Owner.IsEnabled = true;
            
        }
        
    }
}