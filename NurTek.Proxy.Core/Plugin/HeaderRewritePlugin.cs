using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NurTek.Proxy.Core.Http;

namespace NurTek.Proxy.Core.Plugin
{
    public class HeaderRewritePlugin : AbstractPlugin
    {
        public HeaderRewritePlugin(IProxyEvents proxyEvents) : base(proxyEvents)
        {
            proxyEvents.onBeforeRequest += ProxyEventsOnBeforeRequest;
            proxyEvents.onCompleted += ProxyEvents_onCompleted;
        }

        private void ProxyEvents_onCompleted(ProxyEventArgs evt)
        {
            var response = evt.ProxyParams.Contents["response"] as Response;
            if (response == null) return;

            // we need content-encoding (in case server refuses to serve it in plain text)
            // content-length: final size of content sent to user may change via plugins, so it makes no sense to send old content-length
            var forwardHeaders = new List<string> { "content-type", "zzzcontent-length", "accept-ranges", "content-range", "content-disposition", "location", "set-cookie" };
            NameValueCollection col = new NameValueCollection();
            foreach (string responseHeader in response.Headers)
            {
                col.Add(responseHeader.ToLowerInvariant(), response.Headers[responseHeader]);
            }
            foreach (string forwardHeader in response.Headers)
            {
                if (forwardHeaders.IndexOf(forwardHeader) == -1)
                    col.Remove(forwardHeader);
            }

            response.Headers = col;
            response.Headers.Set("cache-control", "no-cache, no-store, must-revalidate");
            response.Headers.Set("pragma", "no-cache");
            response.Headers.Set("expires", "0");
        }

        private void ProxyEventsOnBeforeRequest(ProxyEventArgs evt)
        {
            var request = evt.ProxyParams.Contents["request"] as Request;
            if (request == null) return;
            request.Headers.Remove("connection");
            request.Headers.Set("accept-encoding", "identity"); // // tell target website that we only accept plain text without any transformations
            request.Headers.Remove("referer"); // mask proxy referer
        }
    }
}
