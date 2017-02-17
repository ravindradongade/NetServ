using NetServNodeEntity.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    using System.Threading;

    using NetServeNodeEntity.Message;

    using NetServEntity;

    public static class StaticProperties
    {
        public static readonly ConcurrentDictionary<string, NodeInfo> HostedNodes;
        public static NodeConfiguration NodeConfig;
        public static BlockingCollection<string> NodeHealthInfoMessagesCollection;
        public static BlockingCollection<NodeInfo> NodesToBeDeclaredDead;
        public static List<TaskMessage> TaskMessages;
       
        #region NodeManager
        private static Timer NodeInfoSendToMasterTimer;
        #endregion

        #region Task Processing
        public static List<TaskMessage> RunningActors;
       // public static List<TaskMessage> TaskMessagesToBeProcessed;
        public static BlockingCollection<TaskMessage> TaskMessageContainer;
        public static ConcurrentDictionary<string, object> ActrorsDictionary;

        #endregion
        #region Master Selection

        public static List<NodeDeclaredDeadMessage> MasterDeadMessages;
        public static NodeInfo NextMasterSelectionManager;

        public static bool MasterSelectionProcessStarted;
        public static BlockingCollection<NodeDeclaredDeadMessage> MasterDeadMessageBlockingCollection;
        #endregion


        static StaticProperties()
        {
            HostedNodes = new ConcurrentDictionary<string, NodeInfo>();
            NodeHealthInfoMessagesCollection = new BlockingCollection<string>();
            NodesToBeDeclaredDead = new BlockingCollection<NodeInfo>();
            ActrorsDictionary = new ConcurrentDictionary<string, object>();
           

            RunningActors = new List<TaskMessage>();
            //TaskMessagesToBeProcessed = new List<TaskMessage>();
            TaskMessageContainer = new BlockingCollection<TaskMessage>();
            ActrorsDictionary = new ConcurrentDictionary<string, object>();

            MasterDeadMessages = new List<NodeDeclaredDeadMessage>();
            MasterDeadMessageBlockingCollection = new BlockingCollection<NodeDeclaredDeadMessage>();

        }
    }
}
