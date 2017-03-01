using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public class NamingServiceConfiguration
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string Uri
        {
            get
            {
                return "http://" + Address + ":" + Port;
            }
        }
    }
}
