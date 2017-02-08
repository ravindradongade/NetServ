using NetServNodeEntity.Enums;

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

        public string[] Actros { get; set; }
        
    }
}
