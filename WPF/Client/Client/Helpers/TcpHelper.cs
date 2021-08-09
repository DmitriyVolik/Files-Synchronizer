namespace Client.Helpers
{
    public class TcpHelper
    {
        public static System.Net.Sockets.TcpClient GetClient()
        {
            return new System.Net.Sockets.TcpClient("46.160.119.181", 5432);
        }
    }
}