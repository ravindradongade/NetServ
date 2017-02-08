using NetServNodeEntity;
using System.Linq;

namespace NetServNode
{
    public class NodeManager

    {
        private const string NODE = "Node";
        private const int DEFAULT_NODE2NODE_PORT = 3300;
        private const int DEFAULT_CLIENT2NODE_PORT = 5300;

        public NodeManager()
        {

        }
        public void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(StaticProperties.NodeConfig.NodeName))
            {
                StaticProperties.NodeConfig.NodeName = _GenerateNodeName();
            }
            if (StaticProperties.NodeConfig.NodeToNodePort == 0)
            {
                StaticProperties.NodeConfig.NodeToNodePort = DEFAULT_NODE2NODE_PORT;
            }
            if (StaticProperties.NodeConfig.ClientToNodePort == 0)
            {
                StaticProperties.NodeConfig.NodeToNodePort = DEFAULT_CLIENT2NODE_PORT;
            }
            if (StaticProperties.NodeConfig.IsMaster && _IsThisDuplicateMaster())
            {
                throw new System.Exception("Master Node already hosted");
            }
        }
        private string _GenerateNodeName()
        {
            return NODE + "_" + System.Net.Dns.GetHostName();
        }
        private bool _IsThisDuplicateMaster()
        {
            return StaticProperties.HostedNodes.Any(node => node.NodeType == NetServNodeEntity.Enums.NodeTypes.Master);
        }
    }
}
