using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace Client.Helpers
{
    public class Encryptor
    {
        private static ushort secretKey;

        static Encryptor()
        {
            var builder= new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();
            secretKey = Convert.ToUInt16(config["SecretKey"]);
        }
        
        public static string EncodeDecrypt(string str)
        {
            var ch = str.ToArray(); 
            string newStr = "";      
            foreach (var c in ch)  
                newStr += TopSecret(c);  
            return newStr;
        }
 
        public static char TopSecret(char character)
        {
            character = (char)(character ^ secretKey);
            return character;
        }
        

        
    }
}