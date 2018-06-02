using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NurTek.Proxy.Core.Models;

namespace NurTek.Proxy.Core
{
    public class ProxyEventArgs
    {
        public ProxyParams ProxyParams { get; set; }
        public bool Handled { get; set; }
    }
    public delegate void BeforeRequestDelegate(ProxyEventArgs evt);

    public delegate void HeadersReceivedDelegate(ProxyEventArgs evt);

    public delegate void CompletedDelegate(ProxyEventArgs evt);    
}
