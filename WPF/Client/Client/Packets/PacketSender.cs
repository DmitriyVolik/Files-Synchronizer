using System.Net.Sockets;
using System.Text;

namespace Client.Packets
{
    class PacketSender
    {
        //  Отправка строки
        public static void SendJsonString(NetworkStream Stream, string dataString)
        {
            byte[] data = new byte[1024];
            data = Encoding.Unicode.GetBytes(dataString);
            Stream.Write(data, 0, data.Length);
        }
    }
}