using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Threading;
using System.Threading.Tasks;
using Server.Database;
using Server.Files;
using Server.Helpers;
using Server.Models;
using Server.Workers;

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

    private int targetBytesCount = 0;
    private int sentBytesCount = 0;

    public void SetGoal(int targetBytesCount)
    {
        this.targetBytesCount = targetBytesCount;
        this.sentBytesCount = 0;
    }

    public int SetNextResultNGetRest(int nextSentBytesCount)
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

    public static List<FileM> Files = new List<FileM>();
    
    public AsynchronousSocketListener()
    {
    }

    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.Byte[]")]
    public static void RefreshFiles()
    {
        while (true)
        {
            var temp = new List<FileM>();
            try
            {
                ScanFiles.ProcessDirectory(MainDirectory, temp);
                
            }
            catch (System.IO.IOException)
            {
                
            }
            Files = temp;
            Thread.Sleep(5000);
        }
    }

    public static async void AsyncRefreshFiles()
    {
        await Task.Run(RefreshFiles);
    }

    public static string MainDirectory= ConfigurationManager.AppSettings.Get("MainDirectory");
    public static void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
        // IPAddress ipAddress = IPAddress.Parse("192.168.0.109");
        IPAddress ipAddress = IPAddress.Parse("192.168.0.109");
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
            // ignored
            return;
        }
        
        // !!! проверить: в накопительном стринг билдере - столько байтов, сколько
        // было сообщено сервером в ответ на разведывательный запрос;
        // - если меньше - запустить очередное получение:
        // 1. если накопитель пообще пуст - со смещением 0;
        // 2. если в накопителе уже Х байт, то со смещением Х;
        // - если равно - выйти из цикла запуска получений

        if (bytesRead > 0) {  
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.Unicode.GetString(  
                state.buffer, 0, bytesRead));  
  
            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString(); 
            
            Console.WriteLine(content);
            
            if (content=="GET:FILES:LIST")
            {
                Console.WriteLine(JsonWorker<List<FileM>>.ObjToJson(Files));
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine(JsonWorker<List<FileM>>.ObjToJson(Files).Length);
                Console.WriteLine("-----------------------------------------------------");
                Send(handler, JsonWorker<List<FileM>>.ObjToJson(Files));
                
            } 
            else if (content.Contains("GET:FILE:"))
            {
                string path = content.Replace("GET:FILE:", "");
                Console.WriteLine(MainDirectory+path);
                
                if (File.Exists(MainDirectory+path))
                {
                    try
                    {
                        // !!! Отправлять так только если количество отправляемых байт - меньше
                        // размера буфера приемника, иначе - циклически вызывать асинхронную отправку,
                        // пока результаты ее прекращения не дадут в сумме отправляемый объем байт
                        String filePath = MainDirectory + path;
                        StateObject fileSendState = new StateObject();
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
                                MainDirectory + path
                            }
                        );
                        /* handler.BeginSendFile(
                            MainDirectory + path,
                            SendFileCallback,
                            new object[]
                            {
                                handler,
                                state,
                                MainDirectory + path
                            }
                        );  */
                        Console.WriteLine("file send!!");
                    }
                    catch (System.IO.IOException)
                    {
                        
                    }
                }
                else
                {
                    Send(handler, "0");
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
                        db.Users.Add(new User()
                            {Login = tempUser.Login, Password = PasswordHash.CreateHash(tempUser.Password)});

                        db.SaveChanges();
                        
                        Send(handler, "ADD:SUCCESS");
                    }
                }


            }
            else {  
                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
                    new AsyncCallback(ReadCallback), state);  
            }  
            
            /*if (content.IndexOf("<EOF>") > -1) {  
                // All the data has been read from the
                // client. Display it on the console.  
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",  
                    content.Length, content );  
                // Echo the data back to the client.  
                Send(handler, content);  
            } */
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
    
    // .BeginSend(bytes, 0, bytes.Length, SocketFlags.None, endSendCallback, clientSocket);
    
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
            
            int total = jsonSendState.SetNextResultNGetRest(bytesSent);
            
            if (total > 0)
            {
                // !!! Заменить BeginSendFile на BeginSend, чтобы можно было передавать байты
                // файла по частям
                // handler.BeginSendFile(MainDirectory+path,new AsyncCallback(SendFileCallback), handler);
                handler.BeginSend(jsonByteArray, total, jsonByteArray.Length - total,
                    SocketFlags.None, new AsyncCallback(SendCallback), handler);
                return;
            }
            
            
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);  
  
            handler.Shutdown(SocketShutdown.Both);  
            handler.Close();  
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());  
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
            String filePath = (String)((object[])ar.AsyncState)[3];
            // Complete sending the data to the remote device.  
            // handler.EndSendFile(ar);
            int nextSentBytesCount = handler.EndSend(ar);

            int total = fileSendState.SetNextResultNGetRest(nextSentBytesCount);
            
            if (total > 0)
            {
                // !!! Заменить BeginSendFile на BeginSend, чтобы можно было передавать байты
                // файла по частям
                // handler.BeginSendFile(MainDirectory+path,new AsyncCallback(SendFileCallback), handler);
                handler.BeginSend(fileByteArray, total, fileByteArray.Length - total,
                    SocketFlags.None, new AsyncCallback(SendFileCallback), handler);
                return;
            }
            
            Console.WriteLine("Sent File done");  
  
            handler.Shutdown(SocketShutdown.Both);  
            handler.Close();  
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());  
        }  
    }

    public static int Main(String[] args)
    {
        /*Console.WriteLine(ConfigurationManager.ConnectionStrings["Database"].ConnectionString);*/


        StartListening();


        return 0;
    }
}