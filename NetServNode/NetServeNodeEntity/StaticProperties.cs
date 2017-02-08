using NetServNodeEntity.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    using System.Threading;

    using NetServEntity;

    public static class StaticProperties
    {
        public static readonly ConcurrentDictionary<string, NodeInfo> HostedNodes;
        public static NodeConfiguration NodeConfig;
        public static BlockingCollection<string> NodeHealthInfoMessagesCollection;
        public static BlockingCollection<NodeInfo> NodesToBeDeclaredDead;
        public static ConcurrentQueue<TaskMessage> TaskMessages;
        public static object LocableObjectForTaskQueue;

        #region Task Processing
        public static List<TaskMessage> RunningActors;
        public static List<TaskMessage> TaskMessagesToBeProcessed;
        public static BlockingCollection<int> TaskPool;
        public static BlockingCollection<TaskMessage> TaskMessageContainer;
        public static ConcurrentDictionary<string, object> ActrorsDictionary;

        #endregion



        static StaticProperties()
        {
            HostedNodes = new ConcurrentDictionary<string, NodeInfo>();
            NodeHealthInfoMessagesCollection = new BlockingCollection<string>();
            NodesToBeDeclaredDead = new BlockingCollection<NodeInfo>();
            ActrorsDictionary = new ConcurrentDictionary<string, object>();
        }
    }
}
