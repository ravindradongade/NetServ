using NetServeNodeEntity.Actors;
using NetServNodeEntity.Enums;
using System.Collections.Generic;

namespace NetServNodeEntity.Message
{
    public class NodeInformationMessage
    {
        public string NodeName { get; set; }
        public string NodeId { get; set; }
        public NodeTypes NodeType { get; set; }
        public string NodeAddress { get; set; }
        public int NodePort { get; set; }
        public int NumberOfRunningTask { get; set; }
        public float CpuUsage { get; set; }

        public int RamUsage { get; set; }

        public List<ActorModel> Actros { get; set; }
        
    }
}
