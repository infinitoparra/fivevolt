using System.Security.Cryptography;
using System.Text;

namespace ZigBeeCoordinator.Lib.Infrastructure
{
    public static class SecurityHelpers
    {
        public static byte[] GetMD5Hash(string input)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            return bs;
        }

        public static string GetMD5HashHexString(string input)
        {
            byte[] bs = GetMD5Hash(input);
            StringBuilder s = new StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }
    }
}
