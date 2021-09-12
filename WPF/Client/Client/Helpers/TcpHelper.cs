using Microsoft.Extensions.Configuration;

namespace Client.Helpers
{
    public class TcpHelper
    {
        public static System.Net.Sockets.TcpClient GetClient()
        {
            var builder= new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();
            return new System.Net.Sockets.TcpClient(config["ServerIp"], 5432);
        }
    }
}