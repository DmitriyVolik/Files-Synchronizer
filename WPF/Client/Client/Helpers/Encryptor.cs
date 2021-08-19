using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Helpers
{
    public class Encryptor
    {
        static readonly UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

        public static string Encrypt(string data)
        {
            byte[] secret = ProtectedData.Protect(unicodeEncoding.GetBytes(data), null, DataProtectionScope.CurrentUser);
            string base64 = Convert.ToBase64String(secret);
            return base64;
        }
        
        public static string Decrypt(string data)
        {
            
            byte[] backagain = Convert.FromBase64String(data);
            byte[] clearbytes = ProtectedData.Unprotect(backagain, null, DataProtectionScope.CurrentUser);
            string roundtripped = unicodeEncoding.GetString(clearbytes);
            return roundtripped;

        }
        

        
    }
}