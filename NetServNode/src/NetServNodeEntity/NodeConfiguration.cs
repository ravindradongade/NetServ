using NetServNodeEntity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public class NodeConfiguration
    {
        public string NodeName { get; set; }
        public string NodeAddress { get; set; }
        public int NodeToNodePort { get; set; }
        public int ClientToNodePort { get; set; }
        public bool IsMaster { get; set; }
        public StorageType StorageType { get; set; }
        public string ConnectionString { get; set; }
        public IPAddress NodeIpAddress
        {
            get
            {
                IPAddress ipAdress = null;
                IPAddress.TryParse(NodeAddress, out ipAdress);
                return ipAdress;
            }
        }
        public int MaxThreads { get; set; }
        public int MaxCpuUsage { get; set; }

        public bool NamingServiceEnable { get; set; }
        public NamingServiceConfiguration NamingService { get; set; }
    }
}
