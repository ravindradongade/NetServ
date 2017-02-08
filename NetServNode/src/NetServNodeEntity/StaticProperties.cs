using NetServNodeEntity.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public static class StaticProperties
    {
        public static readonly ConcurrentDictionary<string,NodeInfo> HostedNodes;
        public static NodeConfiguration NodeConfig;
        public static BlockingCollection<string> NodeHealthInfoMessagesCollection;
        public static BlockingCollection<NodeInfo> NodesToBeDeclaredDead;
        static StaticProperties()
        {
            HostedNodes = new ConcurrentDictionary<string, NodeInfo>();
            NodeHealthInfoMessagesCollection = new BlockingCollection<string>();
            NodesToBeDeclaredDead = new BlockingCollection<NodeInfo>();
        }
    }
}
