using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NurTek.Proxy.Core.Helpers
{
    public static class Helper
    {
        public static string MD5Sifrele(string metin)
        {
            // MD5CryptoServiceProvider nesnenin yeni bir instance'sını oluşturalım.
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            //Girilen veriyi bir byte dizisine dönüştürelim ve hash hesaplamasını yapalım.
            byte[] btr = Encoding.UTF8.GetBytes(metin);
            btr = md5.ComputeHash(btr);

            //byte'ları biriktirmek için yeni bir StringBuilder ve string oluşturalım.
            StringBuilder sb = new StringBuilder();


            //hash yapılmış her bir byte'ı dizi içinden alalım ve her birini hexadecimal string olarak formatlayalım.
            foreach (byte ba in btr)
            {
                sb.Append(ba.ToString("x2").ToLower());
            }

            //hexadecimal(onaltılık) stringi geri döndürelim.
            return sb.ToString();
        }

        public static string CleanContentType(string contentType)
        {
            return string.IsNullOrWhiteSpace(contentType) ? "" : Regex.Replace(contentType, ";.*", "").Trim();
        }

        public static string ProxifyUrl(string url, string baseUrl, string appUrl)
        {
            var encodedUrl = url;

            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = AddHttp(baseUrl);
                encodedUrl = Rel2Abs(encodedUrl, baseUrl);
            }

            return $"{appUrl}?q={Uri.EscapeDataString(encodedUrl)}";
        }

        public static NameValueCollection ParseQueryString(string s)
        {
            NameValueCollection nvc = new NameValueCollection();
            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }
            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    nvc.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }
            return nvc;
        }

        private static string AddHttp(string url)
        {
            if (!Regex.IsMatch(url, @"^https?://", RegexOptions.IgnoreCase))
                url = "http://" + url;
            return url;
        }

        private static string Rel2Abs(string rel, string abs)
        {
            if (rel.StartsWith("//"))
                return "http:" + rel;
            var uri = new Uri(rel, UriKind.RelativeOrAbsolute);
            /* return if  already absolute URL */
            if (uri.IsAbsoluteUri)
                return rel;

            /* queries and  anchors */
            if (rel[0] == '#' || rel[0] == '?')
                return abs.EndsWith("/") ? $"{abs}{rel}" : $"{abs}/{rel}";
            var baseUri = new Uri(abs);
            /* remove  non-directory element from path */
            var path = Regex.Replace(baseUri.AbsolutePath, "/[^/]*$", "");
            path += "/";
            /* destroy path if  relative url points to root */
            if (rel[0] == '/') path = "";
            abs = $"{baseUri.Host.Substring(0, baseUri.Host.Length)}{path}{rel}";
            abs = Regex.Replace(abs, @"(/\.?/)", "/");
            abs = Regex.Replace(abs, @"/(?!\.\.)[^/]+/\.\./", "/");

            return $"{baseUri.Scheme}://{abs}";
        }
    }
}
