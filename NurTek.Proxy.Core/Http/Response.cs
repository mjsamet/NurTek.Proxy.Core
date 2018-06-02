using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NurTek.Proxy.Core.Http
{
    public class Response
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public NameValueCollection Headers { get; set; }

        public string Content { get; set; }

        public byte[] Bytes { get; set; }

        public bool IsStream { get; set; }

        public Response()
        {
            Headers = new NameValueCollection();
        }
    }
}
