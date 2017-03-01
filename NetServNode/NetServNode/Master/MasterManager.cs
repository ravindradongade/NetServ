using NetServNodeEntity;
using NetServNodeEntity.Enums;
using NetServNodeEntity.Message;
using System;
using System.Linq;

using System.Threading.Tasks;

namespace NetServNode.Master
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Timers;
    using NetServeNodeEntity.Message;
    using Timer = System.Timers.Timer;
    using TaskSchedulers;
    using NetServEntity;
    using Common.Logging;
    using NetServData;
    using NetServHttpWrapper;

    public class MasterManager
    {
        private readonly ILog log = LogManager.GetLogger(typeof(MasterManager));
        private const int NODE_HEALTH_WAITING_PERIOD_IN_SEC = 50;
        private static object _lockableObjectForTimer = new object();
        private Timer _timer;
        private bool NODE_HEALTH_PROCESS_STARTED;
        private const string QUERY_MESSAGE = "QUERY";
        private const int RETRY_COUNT = 3;
        private IDataManager _dataManager;
        private const string NODE_HEALTH_CHECK_ENDPOINT = "node/GetNodeHealthInfoForMaster";
        private const string NODE_DECLAREDDEAD_ENDPOINT = "node/DeclareDead";
        private const string BROADCAST_NODE_TO_ALL_NODES = "node/IamNode";
        private const string REGISTER_MASTER_TO_NS = "/ns/RegisterMaster";
        private readonly HttpWrapper _httpWrapper;

        private BlockingCollection<NodeInfo> _nodesWithRetry1;

        private BlockingCollection<NodeInfo> _nodesWithRetry2;

        private BlockingCollection<NodeInfo> _nodesWithRetry3;

        public MasterManager()
        {
            this._Intiallize();
        }
        private void _Intiallize()
        {
            log.Debug("Master Manager Intiallize");
            this._nodesWithRetry1 = new BlockingCollection<NodeInfo>();
            this._nodesWithRetry2 = new BlockingCollection<NodeInfo>();
            this._nodesWithRetry3 = new BlockingCollection<NodeInfo>();
        }

        public void StartMasterManager()
        {
            try
            {
                //Start check node status timer
                this._timer = new Timer(NODE_HEALTH_WAITING_PERIOD_IN_SEC * 1000);
                this._timer.Elapsed += _CheckNodesStatus;
                this._timer.Start();
                //start Check helath retry thread for all 3 retry
                Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount1(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
                Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount2(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
                Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount3(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
                //Start declare noded dead task
                Task.Factory.StartNew(() => this._DeclareNodeDead(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
                _dataManager = DataFactory.GetDataManager(StaticProperties.NodeConfig.StorageType, StaticProperties.NodeConfig.ConnectionString);
                _RegisterMasterToStorage();
            }
            catch (Exception ex)
            {

                log.Error(ex);
            }

        }

        private void _RegisterMasterToStorage()
        {
            var masterDetails = new MasterDetails() { Address = StaticProperties.NodeConfig.NodeAddress, Id = StaticProperties.NodeConfig.NodeId, Name = StaticProperties.NodeConfig.NodeName, Port = StaticProperties.NodeConfig.NodePort };
            log.Debug("Registering master to:" + StaticProperties.NodeConfig.StorageType);
            _dataManager.InsertMaster(masterDetails);
            log.Debug("Registraion done");

        }

        private MessageType _GetMessageType(string type)
        {
            MessageType messageType = (MessageType)int.Parse(type);
            return messageType;
        }
        private void _ProcessMessageFromNode(string message, MessageType messageType)
        {
            if (messageType == MessageType.HealthInfo)
            {
                StaticProperties.NodeHealthInfoMessagesCollection.Add(message);
            }
        }
        public void ProcessNodeHealthMessage(NodeInformationMessage nodeInformationMessage)
        {

            try
            {
                log.Debug("Got the node info from node :" + nodeInformationMessage.NodeName);
                var nodeInformation = new NodeInfo()
                {
                    CpuUsage = nodeInformationMessage.CpuUsage,
                    LastCheckinTime = DateTime.UtcNow,
                    NodeAddress = nodeInformationMessage.NodeAddress,
                    NodeId = nodeInformationMessage.NodeId,
                    NodeName = nodeInformationMessage.NodeName,
                    NodePort = nodeInformationMessage.NodePort,
                    NodeType = nodeInformationMessage.NodeType,
                    NumberOfActorsRunning =
                                                  nodeInformationMessage.NumberOfRunningTask,
                    RegistedActors = nodeInformationMessage.Actros
                };
                if (StaticProperties.NextMasterSelectionManager.NodeId != null)
                {
                    lock (StaticProperties.NextMasterSelectionManager)
                    {
                        if (StaticProperties.NextMasterSelectionManager.NodeId != null)
                        {
                            StaticProperties.NextMasterSelectionManager = nodeInformation;
                        }
                    }
                }
                StaticProperties.HostedNodes.AddOrUpdate(
                    nodeInformation.NodeName,
                    nodeInformation,
                    (key, oldValue) => nodeInformation);
                TaskMessage taskMessage = null;
                lock (StaticProperties.TaskMessages)
                {
                    StaticProperties.TaskMessages.FirstOrDefault(m => nodeInformation.RegistedActors.Any(a => a.ActorName == m.Actor));
                }
                if (taskMessage != null)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        MasterTaskMessageManager masterTaskManager = new MasterTaskMessageManager();
                        var result = await masterTaskManager.SendTaskToNode(taskMessage, nodeInformation.NodeAddress);
                        if (result)
                        {
                            lock (StaticProperties.TaskMessages)
                            {
                                StaticProperties.TaskMessages.Remove(taskMessage);
                            }
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, TaskSchedulersHolder.SchedulerToSendMissedTaskToNode);

                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void _BroadcastNodesToAllNodes(NodeInfo node)
        {
            try
            {
                log.Debug("Broadcasting node to all nodes");
                var nodeInfoFromMaster = new NodeInfoFromMaster() { MasterSelector = StaticProperties.NextMasterSelectionManager, Node = node };
                var nodes = StaticProperties.HostedNodes.Values;
                foreach (var nodeInfo in nodes)
                {
                    if (node.NodeId != nodeInfo.NodeId)
                        _httpWrapper.DoHttpPostWithNoReturn<NodeInfoFromMaster>(nodeInfo.NodeAddress, nodeInfoFromMaster);
                }
                nodes = null;
            }
            catch (Exception ex)
            {

                log.Error(ex);
            }

        }

        /// <summary>
        /// This will trigger in every configured interval by timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _CheckNodesStatus(object sender, ElapsedEventArgs e)
        {
            try
            {
                log.Debug("Checking Node status");
                //Sustract buffer sconds from utc.now
                var currentTime = DateTime.UtcNow;
                if (StaticProperties.HostedNodes.Count > 0)
                {
                    //If last checkin croses time difference
                    var nodes =
                        StaticProperties.HostedNodes.Values.Where(node => currentTime.Subtract(node.LastCheckinTime).Seconds > 60).ToList();
                    for (int nodeIndex = 0; nodeIndex < nodes.Count(); nodeIndex++)
                    {
                        //Assume we need to check node status
                        this._nodesWithRetry1.Add(nodes[nodeIndex]);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        /// <summary>
        /// Second retry count
        /// </summary>
        private async void _StartCheckHelathForRetryCount2()
        {
            try
            {
                foreach (var node in _nodesWithRetry2.GetConsumingEnumerable())
                {
                    if (node != null)
                    {
                        log.Debug("Checking node health -Retry2 :" + node.NodeName);
                        var nodeInformationMessage =
                              await
                              this._httpWrapper.DoHttpGet<NodeInformationMessage>(
                                  node.NodeAddress + "/" + NODE_HEALTH_CHECK_ENDPOINT);
                        if (nodeInformationMessage != null)
                        {
                            var nodeInformation = new NodeInfo()
                            {
                                CpuUsage = nodeInformationMessage.CpuUsage,
                                LastCheckinTime = DateTime.UtcNow,
                                NodeAddress = nodeInformationMessage.NodeAddress,
                                NodeId = nodeInformationMessage.NodeId,
                                NodeName = nodeInformationMessage.NodeName,
                                NodePort = nodeInformationMessage.NodePort,
                                NodeType = nodeInformationMessage.NodeType,
                                NumberOfActorsRunning =
                                                              nodeInformationMessage.NumberOfRunningTask,
                                DeadRetryCount = 0
                            };
                            StaticProperties.HostedNodes.AddOrUpdate(
                                nodeInformation.NodeName,
                                nodeInformation,
                                (key, oldValue) => nodeInformation);
                        }

                        else
                        {
                            this._nodesWithRetry3.Add(node);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Last retry
        /// </summary>
        private async void _StartCheckHelathForRetryCount3()
        {
            try
            {
                foreach (var node in this._nodesWithRetry3.GetConsumingEnumerable())
                {
                    //Last retry. wait for 2 more seconds
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    if (node != null)
                    {
                        log.Debug("Checking node health -Retry3 :" + node.NodeName);
                        var nodeInformationMessage =
                              await
                              this._httpWrapper.DoHttpGet<NodeInformationMessage>(
                                  node.NodeAddress + "/" + NODE_HEALTH_CHECK_ENDPOINT);
                        if (nodeInformationMessage != null)
                        {
                            var nodeInformation = new NodeInfo()
                            {
                                CpuUsage = nodeInformationMessage.CpuUsage,
                                LastCheckinTime = DateTime.UtcNow,
                                NodeAddress = nodeInformationMessage.NodeAddress,
                                NodeId = nodeInformationMessage.NodeId,
                                NodeName = nodeInformationMessage.NodeName,
                                NodePort = nodeInformationMessage.NodePort,
                                NodeType = nodeInformationMessage.NodeType,
                                NumberOfActorsRunning =
                                                              nodeInformationMessage.NumberOfRunningTask,
                                DeadRetryCount = 0
                            };
                            StaticProperties.HostedNodes.AddOrUpdate(
                                nodeInformation.NodeName,
                                nodeInformation,
                                (key, oldValue) => nodeInformation);
                        }

                        else
                        {
                            //Time to declare dead
                            StaticProperties.NodesToBeDeclaredDead.Add(node);
                        }
                    }
                }


            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// First retry
        /// </summary>
        private async void _StartCheckHelathForRetryCount1()
        {
            foreach (var node in this._nodesWithRetry1.GetConsumingEnumerable())
            {
                log.Debug("Checking node health -Retry1 :" + node.NodeName);
                if (node != null)
                {

                    try
                    {

                        var nodeInformationMessage =
                            await
                            this._httpWrapper.DoHttpGet<NodeInformationMessage>(
                                node.NodeAddress + "/" + NODE_HEALTH_CHECK_ENDPOINT);
                        if (nodeInformationMessage != null)
                        {
                            var nodeInformation = new NodeInfo()
                            {
                                CpuUsage = nodeInformationMessage.CpuUsage,
                                LastCheckinTime = DateTime.UtcNow,
                                NodeAddress = nodeInformationMessage.NodeAddress,
                                NodeId = nodeInformationMessage.NodeId,
                                NodeName = nodeInformationMessage.NodeName,
                                NodePort = nodeInformationMessage.NodePort,
                                NodeType = nodeInformationMessage.NodeType,
                                NumberOfActorsRunning =
                                                              nodeInformationMessage.NumberOfRunningTask,
                                DeadRetryCount = 0
                            };
                            StaticProperties.HostedNodes.AddOrUpdate(
                                nodeInformation.NodeName,
                                nodeInformation,
                                (key, oldValue) => nodeInformation);


                        }

                        else
                        {
                            this._nodesWithRetry2.Add(node);
                        }

                    }
                    catch (Exception)
                    {

                        throw;
                    }


                }
            }
        }

        private void _DeclareNodeDead()
        {

            foreach (var node in StaticProperties.NodesToBeDeclaredDead.GetConsumingEnumerable())
            {
                try
                {
                    log.Debug("Declaring node dead. Node name: " + node.NodeName);
                    //Broadcast dead declared message
                    this._httpWrapper.DoHttpPost(
                                    node.NodeAddress + "/" + NODE_DECLAREDDEAD_ENDPOINT,
                                    new NodeDeclaredDeadMessage()
                                    {
                                        AmIMaster = true,
                                        DeclaredBy = "",
                                        IsDead = true,
                                        NodeId = node.NodeId
                                    });
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    var temp = node;
                    StaticProperties.HostedNodes.TryRemove(node.NodeName, out temp);
                }

            }

        }
    }
}
