using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurTek.Proxy.Core
{
    public interface IProxyEvents
    {
        event BeforeRequestDelegate onBeforeRequest;

        //event HeadersReceivedDelegate onHeadersReceived;

        event CompletedDelegate onCompleted;

        string BaseURL { get; set; }

        string AppURL { get; set; }
    }
}
