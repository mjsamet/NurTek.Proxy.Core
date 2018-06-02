using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using NurTek.Proxy.Core.Http;
using NurTek.Proxy.Core.Models;

namespace NurTek.Proxy.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var q = Request.QueryString["q"];
            if (string.IsNullOrEmpty(q))
                return View();
            Uri uri;
            bool validUrl = Uri.TryCreate(q, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!validUrl)
                return View();
            var decodedURL = HttpUtility.UrlDecode(q);

            Request.Headers.Remove("Host");
            var headers = new NameValueCollection();
            foreach (string requestHeader in Request.Headers)
            {
                headers.Add(requestHeader.ToLowerInvariant(), Request.Headers[requestHeader]);
            }

            var form = new NameValueCollection();
            foreach (string o in Request.Form)
            {
                form.Add(o, Request.Form[o]);
            }
            var files = new List<PostedFile>();
            foreach (string r in Request.Files)
            {
                var file = new PostedFile();
                var requestFile = Request.Files[r];
                file.Name = r;
                file.ContentType = requestFile.ContentType;
                file.FileName = requestFile.FileName;
                using (var ms = new MemoryStream())
                {
                    requestFile.InputStream.CopyTo(ms);
                    file.Contents = ms.ToArray();
                }
                files.Add(file);
            }
            var request = new Request(new HttpMethod(Request.HttpMethod), decodedURL, headers, form)
            {
                Files = files
            };

            var _proxy = new Core.Proxy { AppURL = $"{Request.Url.Scheme}://{Request.Url.Authority}" };
            var response = _proxy.Forward(request);

            Response.StatusCode = response.HttpStatusCode.GetHashCode();
            foreach (string responseHeader in response.Headers)
            {
                Response.Headers.Set(responseHeader, response.Headers[responseHeader]);
            }

            Response.ContentType = response.Headers["content-type"];
            if (response.IsStream)
                Response.OutputStream.Write(response.Bytes, 0, response.Bytes.Length);
            else
                Response.Output.Write(response.Content);
            Response.OutputStream.Flush();


            return null;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}