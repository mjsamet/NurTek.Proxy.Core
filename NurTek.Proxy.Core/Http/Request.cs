using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using NurTek.Proxy.Core.Helpers;
using NurTek.Proxy.Core.Models;

namespace NurTek.Proxy.Core.Http
{
    public class Request
    {
        public HttpMethod Method { get; set; }

        private Uri _url;

        public Uri URL
        {
            get
            {
                if (Get == null || Get.Count == 0)
                    return _url;
                var url = _url.ToString();
                var quest = Helper.ParseQueryString(_url.Query);
                if (string.IsNullOrWhiteSpace(_url.Query))
                    url += "?";
                url = Get.Cast<string>().Aggregate(url, (current, o) => quest.Get(o) == null ? current + $"{o}={Get[o]}&" : current);
                return new Uri(url);
            }
            set { _url = value; }
        }
        private string _protocol_version = "1.1";
        private NameValueCollection _params;
        public NameValueCollection Headers { get; }
        public NameValueCollection Post { get; set; }
        public NameValueCollection Get { get; private set; }

        public List<PostedFile> Files { private get; set; }

        public Request(HttpMethod method, string url, NameValueCollection headers, NameValueCollection body)
        {
            _params = new NameValueCollection();
            Headers = headers;

            Post = new NameValueCollection();
            Get = new NameValueCollection();
            Files = new List<PostedFile>();

            Method = method;
            SetURL(url);
            Post = body;

        }

        public void SetURL(string url)
        {
            var regex = new Regex("#.*");
            url = regex.Replace(url, "");

            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            var query = uri.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                regex = new Regex(@"\?.*");
                url = regex.Replace(url, "");
                Get = Helper.ParseQueryString(query);
            }

            _url = new Uri(url, UriKind.RelativeOrAbsolute);
            Headers.Add("host", uri.Host);
        }

        public void SetBody(string body, string contentType = null)
        {
            Post.Clear();
            Files.Clear();

            Post = Helper.ParseQueryString(body);
        }

        public void AddBody(string name, string value)
        {
            Post.Set(name, value);
        }

        public HttpRequestMessage GetRequestMessage()
        {
            var httpCont = new HttpRequestMessage(HttpMethod.Get, URL);
            foreach (string header in Headers)
            {
                if (!header.Equals("content-length"))
                    httpCont.Headers.Add(header.ToUpperInvariant(), Headers[header]);
            }
            return httpCont;
        }

        public HttpRequestMessage CreateRestRequest()
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(Method, URL);
            foreach (string header in Headers)
            {
                //HttpRequestMessage's content will determine automatic type and length
                if (!header.Equals("content-type") && !header.Equals("content-length"))
                    httpRequestMessage.Headers.Add(header, Headers[header]);
            }
            if (Files?.Any() ?? false)
            {
                var httpContent = new MultipartFormDataContent();
                foreach (var postedFile in Files)
                {
                    httpContent.Add(new StreamContent(new MemoryStream(postedFile.Contents)), postedFile.Name, postedFile.FileName);
                }
                foreach (string s in Post)
                {
                    httpContent.Add(new StringContent(Post[s]), s);
                }

                httpRequestMessage.Content = httpContent;
            }
            else if (Post?.Count > 0)
            {
                var list = (from string o in Post select new KeyValuePair<string, string>(o, Post[o])).ToList();
                var httpContent = new FormUrlEncodedContent(list);
                httpRequestMessage.Content = httpContent;
            }
            return httpRequestMessage;
        }
    }
}
