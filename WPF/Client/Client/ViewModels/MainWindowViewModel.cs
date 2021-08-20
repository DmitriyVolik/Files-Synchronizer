using System.IO;
using System.Threading;
using System.Windows;
using Chat.Packets;
using Client.Helpers;
using Client.Models;
using Client.Packets;
using Client.Workers;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Client.ViewModels
{
    public class MainWindowViewModel:BaseViewModel
    {

        private Window _window;

        private UserData UserData;
        
        public MainWindowViewModel(UserData userData, Window window)
        {
            _window = window;
            UserData = userData;
            if (UserData.FolderPath==null)
            {
                UserData.FolderPath = "Не выбрана";
            }
        }
        
        public string FolderPath
        {
            get { return UserData.FolderPath; }
            set
            {
                UserData.FolderPath = value;
                OnPropertyChanged("FolderPath");
            }
        }
        
        public RelayCommand SignOutBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        
                        using (var client = TcpHelper.GetClient())
                        {
                            using (var stream = client.GetStream())
                            {
                                PacketSender.SendJsonString(stream,"SIGN:OUT:"+Encryptor.EncodeDecrypt(UserData.SessionToken));
                            }
                        }

                        UserData.SessionToken = null;
                        File.WriteAllText(@"UserData.json", JsonWorker<UserData>.ObjToJson(UserData));
                        
                        var nextWindow = new AuthenticationWindow();
                        nextWindow.Show();
                        _window.Close();
                        

                    }
                );
            }
        }
        
        
        public RelayCommand SelectFolderBtn
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
                            File.WriteAllText(@"UserData.json", JsonWorker<UserData>.ObjToJson(UserData));
                        }

                    }
                );
            }
        }
    }
}