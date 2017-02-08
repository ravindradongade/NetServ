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
        public IPAddress IpAddress
        {
            get
            {
                IPAddress ipAddress = null;
                IPAddress.TryParse(Address, out ipAddress);
                return ipAddress;
            }
        }
    }
}
