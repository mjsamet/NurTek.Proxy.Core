using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurTek.Proxy.Core.Models
{
    public class PostedFile
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] Contents { get; set; }
    }
}
