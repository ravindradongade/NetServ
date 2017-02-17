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

    using NetServNode.HttpUtilities;

    using Timer = System.Timers.Timer;
    using TaskSchedulers;
    using NetServEntity;

    public class MasterManager
    {
        private const int NODE_HEALTH_WAITING_PERIOD_IN_SEC = 25;
        private static object _lockableObjectForTimer = new object();
        private Timer _timer;
        private bool NODE_HEALTH_PROCESS_STARTED;
        private const string QUERY_MESSAGE = "QUERY";
        private const int RETRY_COUNT = 3;


        private const string NODE_HEALTH_CHECK_ENDPOINT = "GetNodeHealthInfoForMaster";

        private const string NODE_DECLAREDDEAD_ENDPOINT = "DeclareDead";
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
            this._nodesWithRetry1 = new BlockingCollection<NodeInfo>();
            this._nodesWithRetry2 = new BlockingCollection<NodeInfo>();
            this._nodesWithRetry3 = new BlockingCollection<NodeInfo>();
        }
        
        public void StartMasterManager()
        {
            //Start check node status timer
            this._timer = new Timer(NODE_HEALTH_WAITING_PERIOD_IN_SEC * 1000);
            this._timer.Elapsed += _CheckNodesStatus;

            //start Check helath retry thread for all 3 retry
            Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount1(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
            Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount2(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
            Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount3(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
            //Start declare noded dead task
            Task.Factory.StartNew(() => this._DeclareNodeDead(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
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
                    Task.Factory.StartNew(async() =>
                    {
                        MasterTaskMessageManager masterTaskManager = new MasterTaskMessageManager();
                    var result=   await masterTaskManager.SendTaskToNode(taskMessage, nodeInformation.NodeAddress);
                        if(result)
                        {
                            lock (StaticProperties.TaskMessages)
                            {
                                StaticProperties.TaskMessages.Remove(taskMessage);
                            }
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, TaskSchedulersHolder.SchedulerToSendMissedTaskToNode);
                   
                }

            }
            catch (Exception)
            {


            }
        }

        /// <summary>
        /// This will trigger in every configured interval by timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _CheckNodesStatus(object sender, ElapsedEventArgs e)
        {
            //Sustract buffer sconds from utc.now
            var timeDifference = DateTime.UtcNow.AddSeconds(-NODE_HEALTH_WAITING_PERIOD_IN_SEC);
            if (StaticProperties.HostedNodes.Count > 0)
            {
                //If last checkin croses time difference
                var nodes =
                    StaticProperties.HostedNodes.Values.Where(node => node.LastCheckinTime < timeDifference).ToList();
                for (int nodeIndex = 0; nodeIndex < nodes.Count(); nodeIndex++)
                {
                    //Assume we need to check node status
                    this._nodesWithRetry1.Add(nodes[nodeIndex]);
                }
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
            catch (Exception)
            {

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
