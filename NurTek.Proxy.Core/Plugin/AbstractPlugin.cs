using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NurTek.Proxy.Core.Models;

namespace NurTek.Proxy.Core.Plugin
{
    public abstract class AbstractPlugin
    {
        protected AbstractPlugin(IProxyEvents proxyEvents)
        {
            
        }
    }
}
