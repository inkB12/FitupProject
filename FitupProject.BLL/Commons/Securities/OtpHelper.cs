using System.Security.Cryptography;
using System.Text;

namespace FitupProject.BLL.Commons.Securities
{
    public static class OtpHelper
    {
        public static string Generate6()
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return value.ToString("D6");
        }

        public static string HashOtp(string otp, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(otp)));
        }

        public static bool FixedEqualsBase64(string a, string b)
        {
            try
            {
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(a),
                    Convert.FromBase64String(b));
            }
            catch { return false; }
        }
    }
}
