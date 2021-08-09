using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Packets
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