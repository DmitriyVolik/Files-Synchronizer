using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using Chat.Packets;
using Client.Helpers;
using Client.Models;
using Client.Packets;
using Client.Workers;
using TcpClient = System.Net.Sockets.TcpClient;

namespace Client.ViewModels
{
    public class AuthenticationWindowViewModel:BaseViewModel
    {
        
        
        public string CurrentLogin { get; set; }

        public readonly Window _window;

        public AuthenticationWindowViewModel(Window window)
        {
            CurrentLogin = "";
            _window = window;
            
            try
            {
                using (var client = TcpHelper.GetClient())
                {
                    using (var stream = client.GetStream())
                    {
                        
                    }
                    
                }

                /*var client = TcpHelper.GetClient();
                var stream = client.GetStream();*/
            }
            catch (Exception e)
            {
                MessageBox.Show("Сервер недоступен, попробуйте зайти позже", "Ошибка сервера");
                Application.Current.Shutdown();
            }
        }

        public RelayCommand RegistrationBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        var pwBox = obj as PasswordBox;

                        if (CurrentLogin.Length < 4 || pwBox.Password.Length < 4)
                        {
                            MessageBox.Show("Минимальное кол-во символов в логине и пароле 4", "Ошибка");
                            return;
                        }

                        var user = new User(){Login = CurrentLogin, Password = pwBox.Password};
                        
                        try
                        {
                            string answer;
                            using (var client = TcpHelper.GetClient())
                            {
                                using (var stream = client.GetStream())
                                {
                                    PacketSender.SendJsonString(stream, "ADD:USER:" + JsonWorker<User>.ObjToJson(user));

                                    answer = PacketRecipient.GetJsonData(stream);
                                }
                            }

                            if (answer.Contains("ADD:SUCCESS:"))
                            {
                                string token = answer.Replace("ADD:SUCCESS:", "");
                                
                                File.WriteAllText(@"UserData.json", JsonWorker<UserData>.ObjToJson(new UserData(){SessionToken = token}));
                                
                                var nextWindow = new MainWindow();
                                nextWindow.Show();
                                _window.Close();
                            }
                            else if(answer=="USER:EXISTS")
                            {
                                MessageBox.Show("Логин занят", "Ошибка");
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Сервер не доступен, повторите попытку позже", "Ошибка");
                        }
                        
                    }
                );
            }
        }
        
        
        public RelayCommand SignInBtn
        {
            get
            {
                return new RelayCommand(
                    obj =>
                    {
                        var pwBox = obj as PasswordBox;

                        var user = new User(){Login = CurrentLogin, Password = pwBox.Password};
                        
                        try
                        {
                            string answer;
                            using (var client = TcpHelper.GetClient())
                            {
                                using (var stream = client.GetStream())
                                {
                                    PacketSender.SendJsonString(stream, "SIGN:IN:" + JsonWorker<User>.ObjToJson(user));

                                    answer = PacketRecipient.GetJsonData(stream);
                                }
                            }

                            if (answer.Contains("SIGN:IN:SUCCESS:"))
                            {
                                string token = answer.Replace("SIGN:IN:SUCCESS:", "");
                                
                                
                                var nextWindow = new MainWindow();
                                nextWindow.Show();
                                _window.Close();

                            }
                            else if(answer=="SIGN:IN:INCORRECT")
                            {
                                MessageBox.Show("Неверный логин или пароль", "Ошибка");
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Сервер не доступен, повторите попытку позже", "Ошибка");
                        }

                    }
                );
            }
        }
        
        
    }
}