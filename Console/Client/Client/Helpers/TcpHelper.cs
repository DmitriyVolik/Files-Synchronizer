using System;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace Client.Helpers
{
    public class TcpHelper
    {
        public static TcpClient GetClient()
        {
            var builder= new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();
            
            
            return new TcpClient(config["ServerIp"], 5432);
        }
    }
}