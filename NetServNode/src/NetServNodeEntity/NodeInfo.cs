using NetServNodeEntity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public class NodeInfo
    {
        public string NodeName { get; set; }
        public string NodeId { get; set; }
        public NodeTypes NodeType { get; set; }
        public string NodeAddress { get; set; }
        public int NodePort { get; set; }
        public int NumberOfRunningThread { get; set; }
        public int CpuUsage { get; set; }

        public DateTime LastCheckinTime { get; set; }
        public int TotalNodeConnectRetryCount { get; set; }
    }
}
