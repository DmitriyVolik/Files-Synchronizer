using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Client.Helpers;
using Client.Models;
using Client.Packets;
using Client.Workers;
using Microsoft.Win32.TaskScheduler;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Client.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly Window _window;

        private readonly UserData UserData;

        public int TimeDelay { get; set; }

        public MainWindowViewModel(UserData userData, Window window)
        {
            _window = window;
            UserData = userData;
            TimeDelay = 1;
            if (UserData.FolderPath == null) UserData.FolderPath = "Не выбрана";
        }

        public string FolderPath
        {
            get => UserData.FolderPath.Remove(UserData.FolderPath.Length - 1);
            set
            {
                UserData.FolderPath = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SignOutBtn
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    using (var client = TcpHelper.GetClient())
                    {
                        using (var stream = client.GetStream())
                        {
                            PacketSender.SendJsonString(stream,
                                "SIGN:OUT:" + Encryptor.EncodeDecrypt(UserData.SessionToken));
                        }
                    }

                    UserData.SessionToken = null;
                    File.WriteAllText(@"Sync\UserData.json", JsonWorker<UserData>.ObjToJson(UserData));

                    var nextWindow = new AuthenticationWindow();
                    nextWindow.Show();
                    _window.Close();
                });
            }
        }

        public RelayCommand SelectFolderBtn
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var dlg = new CommonOpenFileDialog();
                    dlg.IsFolderPicker = true;

                    if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        FolderPath = dlg.FileName + "\\";
                        File.WriteAllText(@"Sync\UserData.json", JsonWorker<UserData>.ObjToJson(UserData));
                    }
                });
            }
        }

        public RelayCommand SyncFiles
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                        var process = new Process();
                        process.StartInfo.FileName = currentDir + @"\Sync\Client.exe";
                        process.Start();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Ошибка");
                    }
                });
            }
        }

        public RelayCommand CreateTask
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (TimeDelay < 1 || TimeDelay > 24)
                        MessageBox.Show("Задержка не может быть менее чем 1 и более чем 24 часа!", "Ошибка");

                    using (TaskService ts = new())
                    {
                        TaskDefinition td = ts.NewTask();
                        td.RegistrationInfo.Description = "Синхронизация файлов с сервером";

                        var dt = new DailyTrigger();
                        dt.StartBoundary = DateTime.Now;
                        dt.Repetition = new RepetitionPattern(TimeSpan.FromHours(TimeDelay), TimeSpan.FromDays(1));
                        td.Triggers.Add(dt);

                        var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                        td.Actions.Add(new ExecAction(currentDir + @"Sync\Client.exe"));

                        ts.RootFolder.RegisterTaskDefinition(@"FilesSynchronizer", td);
                    }

                    MessageBox.Show("Задача добавлена!", "Успех");
                });
            }
        }

        public RelayCommand DelTask
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    using (TaskService ts = new())
                    {
                        try
                        {
                            ts.RootFolder.DeleteTask("FilesSynchronizer");
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Вы еще не добавляли задачу!", "Ошибка");
                            return;
                        }

                        MessageBox.Show("Задача удалена!", "Успех");
                    }
                });
            }
        }
    }
}