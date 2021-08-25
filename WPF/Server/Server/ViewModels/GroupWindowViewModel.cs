using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Client.ViewModels;
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
                            

                            /*candidate.Name = Name;
                            candidate.FolderPath = FolderPath;
                            
                            _mainWindow.Groups.Add(new Group(){Name = "wdasd", FolderPath = "wdasd"});*/
                            


                            /*OnPropertyChanged("Groups");*/

                            using (Context db = new Context())
                            {
                                var candidate=db.Groups.FirstOrDefault(x => x.Id == 
                                    _mainWindow.SelectedGroup.Id);

                                candidate.Name = Name;
                                candidate.FolderPath = FolderPath;
                                db.SaveChanges();

                            }
                            
                            DBHelper.LoadAll(_mainWindow.Users, _mainWindow.Groups);

                            
                            
                            

                            /*var users = _mainWindow.Users.Where(x => x.Group!=null);*/

                            /*foreach (var user in users.Where(x=>x.Group.Id==newGroup.Id))
                            {
                                user.Group =_mainWindow.Groups.FirstOrDefault(x=> x.Id!=newGroup.Id);
                            }*/
                            
                            /*_mainWindow.Groups.Remove(candidate);
                            
                            _mainWindow.Groups.Add(newGroup);*/
                            
                            //ПОСЛЕ ИЗМЕНЕНИЯ ГРУППЫ СОХРАНЯТЬ В ДБ И СКАЧИВАТЬ ПОВТОРНО ДЛЯ КОРРЕКТНОГО ОТОБРАЖЕНИЯ!!!
                            
                        }
                        _window.Close();
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