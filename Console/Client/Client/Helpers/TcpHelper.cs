using System.Net.Sockets;

namespace Client.Helpers
{
    public class TcpHelper
    {
        public static TcpClient GetClient()
        {
            return new TcpClient("46.160.119.181", 5432);
        }
    }
}