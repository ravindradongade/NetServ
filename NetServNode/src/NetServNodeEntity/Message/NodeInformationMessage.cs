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
        public int NumberOfRunningThread { get; set; }
        public int CpuUsage { get; set; }
    }
}
