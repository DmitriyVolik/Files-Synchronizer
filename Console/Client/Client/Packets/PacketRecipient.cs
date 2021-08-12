using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Packets
{
    class PacketRecipient
    {
        public static string GetJsonData(NetworkStream stream, long bufferSize)
        {
            byte[] data = new byte[bufferSize];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
           
                bytes = stream.Read(data, 0, data.Length);
                
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (stream.DataAvailable);
            return builder.ToString();
        }
    }
}