using System.IO;
using System.Threading;
using System.Windows;
using Client.Models;

namespace Client.ViewModels
{
    public class MainWindowViewModel:BaseViewModel
    {
        private UserData _userData;

        public string Folder { get; set; }

        public MainWindowViewModel(UserData userData)
        {
            this._userData = userData;
        }
        public RelayCommand SignOutBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        
                        
                    }
                );
            }
        }
    }
}