using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Server.Database;
using Server.Files;
using Server.Helpers;
using Server.Models;
using Server.Workers;

namespace Server
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;

        private long targetBytesCount = 0;
        private long sentBytesCount = 0;
    
        public long GetRest()
        {
            return this.targetBytesCount - this.sentBytesCount;
        }

        public void SetGoal(long targetBytesCount)
        {
            this.targetBytesCount = targetBytesCount;
            this.sentBytesCount = 0;
        }

        public long SetNextResultNGetRest(long nextSentBytesCount)
        {
            this.sentBytesCount += nextSentBytesCount;
            return this.targetBytesCount - this.sentBytesCount;
        }

        public void ResetGoal()
        {
            this.targetBytesCount = 0;
            this.sentBytesCount = 0;
        }
    }  
  
    public class AsynchronousSocketListener
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static List<FilesGroup>FilesGroups  = new List<FilesGroup>();
    
        public const int maxSize=2000000000;

        [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.Byte[]")]
        public static void RefreshFiles()
        {
            while (true)
            {
                using (Context db = new Context())
                {

                    foreach (var filesGroup in db.Groups)
                    {
                        var temp = new List<FileM>();
                        try
                        {
                            if (filesGroup.FolderPath!="")
                            {
                                Console.WriteLine(filesGroup.FolderPath);
                                ScanFiles.Start(filesGroup.FolderPath, temp);
                            }

                            var candidate = FilesGroups.FirstOrDefault(x => x.GroupId == filesGroup.Id);

                            if (candidate==null)
                            {
                                FilesGroups.Add(new FilesGroup(){GroupId = filesGroup.Id, files = temp});
                            }
                            else
                            {
                                candidate.files = temp;
                            }
                        }
                        catch (System.IO.IOException)
                        {
                
                        }
                        
                    }
                    Thread.Sleep(60000);
                }
                
                
            }
        }

        public static async void AsyncRefreshFiles()
        {
            await Task.Run(RefreshFiles);
        }
        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            var builder= new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();

            IPAddress ipAddress = IPAddress.Parse(config["LocalIp"]);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1024);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,  
                SocketType.Stream, ProtocolType.Tcp );

            int count = 0;
        
            AsyncRefreshFiles();

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try {  
                listener.Bind(localEndPoint);  
                listener.Listen(100);  
  
                while (true) {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();  
  
                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");  
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),  
                        listener );  
  
                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne(); 
                
                }  
  
            } catch (Exception e) {  
                Console.WriteLine(e.ToString());
            }  
  
            Console.WriteLine("\nPress ENTER to continue...");  
            Console.Read();  
    
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("Accept");
            // Signal the main thread to continue.  
            allDone.Set();  
  
            // Get the socket that handles the client request.  
            Socket listener = (Socket) ar.AsyncState;  
            Socket handler = listener.EndAccept(ar);  
  
            // Create the state object.  
            StateObject state = new StateObject();  
            state.workSocket = handler;
            handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,  
                ReadCallback, state);  
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;  
  
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject) ar.AsyncState;  
            Socket handler = state.workSocket;  
  
            // Read data from the client socket.

            int bytesRead;
        

            try
            {
                bytesRead = handler.EndReceive(ar);  
            }
            catch (Exception e)
            {
                return;
            }

            if (bytesRead > 0) {  
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.Unicode.GetString(  
                    state.buffer, 0, bytesRead));  
  
                // Check for end-of-file tag. If it is not there, read
                // more data.  
                try
                {
                    content = state.sb.ToString();
                    if (content.Contains("GET:FILES:LIST:"))
                    {

                        string token = content.Replace("GET:FILES:LIST:", "");
                        
                        Console.WriteLine(token);

                        Group group = DbHelper.GetGroupBySession(token);

                        if (group!= null)
                        {
                            var filesGroup = FilesGroups.FirstOrDefault(x => x.GroupId == group.Id).files;

                            if (filesGroup!=null)
                            {
                                Console.WriteLine(JsonWorker<List<FileM>>.ObjToJson(filesGroup));
                                Console.WriteLine("-----------------------------------------------------");
                                Console.WriteLine(JsonWorker<List<FileM>>.ObjToJson(filesGroup).Length);
                                Console.WriteLine("-----------------------------------------------------");
                                Send(handler, JsonWorker<List<FileM>>.ObjToJson(filesGroup));
                            }
                            
                        }
                        
                    } 
                    else if (content.Contains("GET:FILE:"))
                    {
                        string temp = content.Replace("GET:FILE:", "");
                        
                        string path=temp.Substring(0, temp.IndexOf(':'));

                        string token = temp.Replace(path+":", "");
                        
                        Console.WriteLine(path);
                        Console.WriteLine(token);

                        var groupDb = DbHelper.GetGroupBySession(token);

                        var group = FilesGroups.FirstOrDefault(x => x.GroupId == groupDb.Id);
                        
                        if (group!=null)
                        {
                            var file = group.files.FirstOrDefault(x => x.Path == path);
                            Console.WriteLine(path);

                            if (file != null && File.Exists(groupDb.FolderPath+file.Path))
                            {
                                try
                                {
                                    StateObject fileSendState = new StateObject();

                                    if (file.Size<=maxSize)
                                    {
                                        String filePath = groupDb.FolderPath + file.Path;
                                        byte[] fileByteArray = File.ReadAllBytes(filePath);
                                        state.SetGoal(fileByteArray.Length);
                                        handler.BeginSend(
                                            fileByteArray,
                                            0,
                                            fileByteArray.Length,
                                            SocketFlags.None,
                                            SendFileCallback,
                                            new object[]
                                            {
                                                handler,
                                                fileByteArray,
                                                fileSendState,
                                                file.Size,
                                                maxSize
                                    
                                            }
                                        );
                            
                                    }
                                    Console.WriteLine("file send!!");
                                }
                                catch (Exception e)
                                {
                                    // ignored
                                }
                            }
                            else
                            {
                                Send(handler, "0");
                            }

                        }
                    }
                    else if (content.Contains("ADD:USER:"))
                    {
                        string data = content.Replace("ADD:USER:", "");

                        ClientUser tempUser = JsonWorker<ClientUser>.JsonToObj(data);

                        using (Context db = new Context())
                        {
                            if (db.Users.FirstOrDefault(x => x.Login ==tempUser.Login)!=null)
                            {
                                Send(handler, "USER:EXISTS");
                            }
                            else
                            {
                                var user = new User()
                                    {Login = tempUser.Login, Password = PasswordHash.CreateHash(tempUser.Password)};
                        
                                db.Users.Add(user);

                        
                        
                                var bytes = new byte[16];
                                using (var rng = new RNGCryptoServiceProvider())
                                {
                                    rng.GetBytes(bytes);
                                }
                        
                                string token = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                                Console.WriteLine(token);

                                db.Sessions.Add(new Session() {Token = token, User = user});
                        
                                Send(handler, "ADD:SUCCESS:"+ token);
                                db.SaveChanges();
                            }
                        }


                    }
                    else if (content.Contains("SIGN:IN:"))
                    {
                        string data = content.Replace("SIGN:IN:", "");

                        ClientUser tempUser = JsonWorker<ClientUser>.JsonToObj(data);
                
                        using (Context db = new Context())
                        {
                            var candidate = db.Users.FirstOrDefault(x => x.Login == tempUser.Login);
                    
                            if (candidate!= null && PasswordHash.ValidatePassword(tempUser.Password, candidate.Password))
                            {
                                var bytes = new byte[16];
                                using (var rng = new RNGCryptoServiceProvider())
                                {
                                    rng.GetBytes(bytes);
                                }
                        
                                string token = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                        
                                db.Sessions.Add(new Session() {Token = token, User = candidate});

                                db.SaveChanges();
                        
                                Send(handler, "SIGN:IN:SUCCESS:"+token);
                            }
                            else
                            {
                                Send(handler, "SIGN:IN:INCORRECT");
                            }
                        }

                    }
                    else if (content.Contains("SIGN:OUT:"))
                    {
                        string data = content.Replace("SIGN:OUT:", "");

                        using (Context db = new Context())
                        {
                            var candidate = db.Sessions.FirstOrDefault(x => x.Token == data);
                            if (candidate!=null)
                            {
                                db.Sessions.Remove(candidate);
                                db.SaveChanges();
                            }
                    
                        }
                    }
                    else if (content.Contains("CHECK:SESSION:"))
                    {
                        string token = content.Replace("CHECK:SESSION:", "");

                        using (var db=new Context())
                        {
                            Console.WriteLine("Check");
                            if (db.Sessions.FirstOrDefault(x => x.Token == token) != null)
                            {
                                Send(handler, "SESSION:CORRECT");
                            }
                            else
                            {
                                Send(handler, "SESSION:INCORRECT");
                            }
                            
                        }
                    }

                    else {  
                        // Not all data received. Get more.  
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
                            new AsyncCallback(ReadCallback), state);  
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }  
        }

        private static void Send(Socket handler, String data)
        {
            StateObject fileSendState = new StateObject();
        
        

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.Unicode.GetBytes(data);  
  
            fileSendState.SetGoal(byteData.Length);
            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,  
                new AsyncCallback(SendCallback), new object[]
                {
                    handler,
                    byteData,
                    fileSendState
                });  
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket) ((object[])ar.AsyncState)[0];
                byte[] jsonByteArray = (byte[])((object[])ar.AsyncState)[1];
                StateObject jsonSendState = (StateObject)((object[])ar.AsyncState)[2];
            
                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
            
                long total = jsonSendState.SetNextResultNGetRest(bytesSent);
            
                if (total > 0)
                {
                    handler.BeginSend(jsonByteArray, (int)total, jsonByteArray.Length - (int)total,
                        SocketFlags.None, new AsyncCallback(SendCallback), handler);
                    return;
                }
            
            
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);  
  
                handler.Shutdown(SocketShutdown.Both);  
                handler.Close();  
  
            }
            catch (Exception e)
            {
                //ignored
            }  
        }
    
        private static void SendFileCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket) ((object[])ar.AsyncState)[0];
                byte[] fileByteArray = (byte[])((object[])ar.AsyncState)[1];
                StateObject fileSendState = (StateObject)((object[])ar.AsyncState)[2];

                long fileSize = (long)((object[]) ar.AsyncState)[3];
                int maxSize=(int)((object[]) ar.AsyncState)[4];
            
                // Complete sending the data to the remote device.  
                // handler.EndSendFile(ar);
                int nextSentBytesCount = handler.EndSend(ar);

                long total = fileSendState.SetNextResultNGetRest(nextSentBytesCount);
            
                if (total > 0)
                {
                    // !!! Заменить BeginSendFile на BeginSend, чтобы можно было передавать байты
                    // файла по частям
                    // handler.BeginSendFile(MainDirectory+path,new AsyncCallback(SendFileCallback), handler);
                    handler.BeginSend(fileByteArray, (int)total, fileByteArray.Length - (int)total,
                        SocketFlags.None, new AsyncCallback(SendFileCallback), handler);
                    return;
                }
            
                Console.WriteLine("Sent File done");
                if (fileSize<maxSize)
                {
                    handler.Shutdown(SocketShutdown.Both);  
                    handler.Close(); 

                }
            
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());  
            }  
        }
        
        public static void Main(String[] args)
        {
            StartListening();

        }
    }
}