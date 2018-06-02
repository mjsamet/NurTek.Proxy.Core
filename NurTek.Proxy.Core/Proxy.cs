using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NurTek.Proxy.Core.Http;
using NurTek.Proxy.Core.Models;
using NurTek.Proxy.Core.Plugin;

namespace NurTek.Proxy.Core
{
    public class Proxy : IProxyEvents
    {
        public event BeforeRequestDelegate onBeforeRequest;

        //public event HeadersReceivedDelegate onHeadersReceived;

        public event CompletedDelegate onCompleted;
        public string BaseURL { get; set; }
        public string AppURL { get; set; }
        private HeaderRewritePlugin _headerRewritePlugin;
        private StreamPlugin _streamPlugin;
        private ProxifyPlugin _proxifyPlugin;

        public Proxy()
        {
            _headerRewritePlugin = new HeaderRewritePlugin(this);
            _streamPlugin = new StreamPlugin(this);
            _proxifyPlugin = new ProxifyPlugin(this);
        }

        public Response Forward(Request request)
        {
            var response = new Response();
            HttpClient httpClient = new HttpClient();
            // var restClient = new RestClient($"{request.URL.Scheme}://{request.URL.Host}");
            BaseURL = request.URL.ToString();
            RaiseBeforeRequest(new ProxyParams { Contents = new Dictionary<string, object> { { "request", request }, { "response", response } } });
            var restRequest = request.CreateRestRequest();

            var httpResponseMessage = httpClient.SendAsync(restRequest).Result;
            response.Content = httpResponseMessage.Content.ReadAsStringAsync().Result;
            response.Bytes = httpResponseMessage.Content.ReadAsByteArrayAsync().Result;

            foreach (var resHeader in httpResponseMessage.Headers)
            {
                response.Headers.Set(resHeader.Key.ToLowerInvariant(), string.Join(", ", resHeader.Value));
            }
            foreach (var resHeader in httpResponseMessage.Content.Headers)
            {
                response.Headers.Set(resHeader.Key.ToLowerInvariant(), string.Join(", ", resHeader.Value));
            }

            response.HttpStatusCode = httpResponseMessage.StatusCode;

            //if it is stream, streamplugin will handle before all plugins and we will understand this response is stream
            response.IsStream = RaiseCompleted(new ProxyParams { Contents = new Dictionary<string, object> { { "request", request }, { "response", response } } });
            
            return response;
        }

        private bool RaiseBeforeRequest(ProxyParams prm)
        {
            if (onBeforeRequest == null)
                return false;
            foreach (BeforeRequestDelegate d in onBeforeRequest.GetInvocationList())
            {
                var eventArgs = new ProxyEventArgs { ProxyParams = prm, Handled = false };
                d(eventArgs);
                if (eventArgs.Handled)
                    return true;
            }

            return false;
        }

        private bool RaiseCompleted(ProxyParams prm)
        {
            if (onCompleted == null)
                return false;
            foreach (CompletedDelegate d in onCompleted.GetInvocationList())
            {
                var eventArgs = new ProxyEventArgs { ProxyParams = prm, Handled = false };
                d(eventArgs);
                if (eventArgs.Handled)
                    return true;
            }
            return false;
        }
    }
}
