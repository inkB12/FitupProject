using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;


namespace FitupProject.BLL.Commons.VNPay
{
    public static class VnPayLibrary
    {
        public static string CreatePaymentUrl(
            VnPayOptions opt,
            string txnRef,
            decimal amountVnd,
            string orderInfo,
            string ipAddress,
            DateTimeOffset nowUtc)
        {
            // VNPAY yêu cầu thời gian GMT+7, format yyyyMMddHHmmss :contentReference[oaicite:3]{index=3}
            var nowGmt7 = ToGmt7(nowUtc);
            var createDate = nowGmt7.ToString("yyyyMMddHHmmss");
            var expireDate = nowGmt7.AddMinutes(opt.ExpireMinutes).ToString("yyyyMMddHHmmss");

            var vnpAmount = ((long)decimal.Round(amountVnd * 100, 0)).ToString(); // nhân 100 :contentReference[oaicite:4]{index=4}

            var data = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = opt.Version,
                ["vnp_Command"] = opt.Command,
                ["vnp_TmnCode"] = opt.TmnCode,
                ["vnp_Amount"] = vnpAmount,
                ["vnp_CurrCode"] = opt.CurrCode,
                ["vnp_TxnRef"] = txnRef,
                ["vnp_OrderInfo"] = SanitizeOrderInfo(orderInfo),
                ["vnp_OrderType"] = opt.OrderType,
                ["vnp_Locale"] = opt.Locale,
                ["vnp_ReturnUrl"] = opt.ReturnUrl,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_CreateDate"] = createDate,
                ["vnp_ExpireDate"] = expireDate
            };

            // (optional) bạn có thể add vnp_IpnUrl nếu hệ thống VNPAY/merchant config dùng (thường cấu hình trên portal)
            // data["vnp_IpnUrl"] = opt.IpnUrl;

            var hashData = BuildQueryString(data, encode: true);     // dùng urlencode giống demo docs :contentReference[oaicite:5]{index=5}
            var secureHash = HmacSHA512(opt.HashSecret, hashData);

            var query = BuildQueryString(data, encode: true) + "&vnp_SecureHash=" + WebUtility.UrlEncode(secureHash);
            return opt.BaseUrl + "?" + query;
        }

        public static bool ValidateSignature(IQueryCollection query, string hashSecret)
        {
            var data = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var (k, v) in query)
            {
                if (k.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)
                    && k != "vnp_SecureHash"
                    && k != "vnp_SecureHashType")
                {
                    data[k] = v.ToString();
                }
            }

            var receivedHash = query["vnp_SecureHash"].ToString();
            if (string.IsNullOrWhiteSpace(receivedHash)) return false;

            var signData = BuildQueryString(data, encode: true);
            var calc = HmacSHA512(hashSecret, signData);
            return string.Equals(calc, receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildQueryString(SortedDictionary<string, string> data, bool encode)
        {
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                if (sb.Length > 0) sb.Append('&');

                if (encode)
                    sb.Append(WebUtility.UrlEncode(kv.Key)).Append('=').Append(WebUtility.UrlEncode(kv.Value));
                else
                    sb.Append(kv.Key).Append('=').Append(kv.Value);
            }
            return sb.ToString();
        }

        private static string HmacSHA512(string key, string input)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
        }

        // VNPAY orderInfo: tiếng Việt không dấu + không ký tự đặc biệt :contentReference[oaicite:6]{index=6}
        private static string SanitizeOrderInfo(string s)
        {
            s = RemoveDiacritics(s);
            var cleaned = new string(s.Where(ch => char.IsLetterOrDigit(ch) || ch is ' ' or '-' or '_' or ':' or '.').ToArray());
            return cleaned.Length > 250 ? cleaned[..250] : cleaned;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
        }

        private static DateTime ToGmt7(DateTimeOffset utc)
        {
            // chạy Win/Linux đều ổn
            TimeZoneInfo tz;
            try { tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
            catch { tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }

            return TimeZoneInfo.ConvertTime(utc.UtcDateTime, tz);
        }
    }
}
