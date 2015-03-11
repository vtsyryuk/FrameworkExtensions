using System.Runtime.InteropServices;
using System.Security;

namespace System
{
    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string input)
        {
            if (input == null)
            {
                return null;
            }

            var secure = new SecureString();
            foreach (var c in input)
            {
                secure.AppendChar(c);
            }

            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(this SecureString input)
        {
            if (input == null)
            {
                return null;
            }

            var ptr = Marshal.SecureStringToBSTR(input);
            try
            {
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
        }
    }
}