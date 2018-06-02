using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NurTek.Proxy.Core.Helpers;
using NurTek.Proxy.Core.Http;

namespace NurTek.Proxy.Core.Plugin
{
    public class StreamPlugin : AbstractPlugin
    {
        private static readonly string[] _outputBufferTypes = new[] { "text/html", "text/plain", "text/css", "text/javascript", "application/x-javascript", "application/javascript", "application/json" };
        private const int MaxContentLen = 5000000;

        public StreamPlugin(IProxyEvents proxyEvents) : base(proxyEvents)
        {
            proxyEvents.onCompleted += ProxyEvents_onCompleted;
        }

        private void ProxyEvents_onCompleted(ProxyEventArgs evt)
        {
            var response = evt.ProxyParams.Contents["response"] as Response;
            var contentType = Helper.CleanContentType(response.Headers.Get("content-type") ?? "");
            var contentLen = response.Headers.Get("content-length");
            var cLen = 0;
            int.TryParse(contentLen, out cLen);
            evt.Handled = !string.IsNullOrWhiteSpace(contentType) && (Array.IndexOf(_outputBufferTypes, contentType) == -1 || cLen > MaxContentLen);
        }
    }
}
