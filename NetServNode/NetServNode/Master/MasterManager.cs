using NetServNodeEntity;
using NetServNodeEntity.Enums;
using NetServNodeEntity.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading.Tasks;

namespace NetServNode.Master
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Timers;

    using NetServeNodeEntity.Message;

    using NetServNode.HttpUtilities;

    using Timer = System.Timers.Timer;

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
            this._timer = new Timer(NODE_HEALTH_WAITING_PERIOD_IN_SEC * 1000);
            this._timer.Elapsed += _CheckNodesStatus;
            Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount1(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
            Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount2(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
            Task.Factory.StartNew(() => this._StartCheckHelathForRetryCount3(), CancellationTokens.MasterNodeHealthMessageProcessor.Token);
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
                    NumberOfRunningThread =
                                                  nodeInformationMessage.NumberOfRunningTask
                };
                StaticProperties.HostedNodes.AddOrUpdate(
                    nodeInformation.NodeName,
                    nodeInformation,
                    (key, oldValue) => nodeInformation);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void _CheckNodesStatus(object sender, ElapsedEventArgs e)
        {
            var timeDifference = DateTime.UtcNow.AddSeconds(-NODE_HEALTH_WAITING_PERIOD_IN_SEC);
            if (StaticProperties.HostedNodes.Count > 0)
            {
                var nodes =
                    StaticProperties.HostedNodes.Values.Where(node => node.LastCheckinTime < timeDifference).ToList();
                for (int nodeIndex = 0; nodeIndex < nodes.Count(); nodeIndex++)
                {
                    this._nodesWithRetry1.Add(nodes[nodeIndex]);
                }
            }
        }
        private async void _StartCheckHelathForRetryCount2()
        {
            try
            {

                NodeInfo node = this._nodesWithRetry2.Take();
                Task.Delay(TimeSpan.FromSeconds(2));
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
                            NumberOfRunningThread =
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
            catch (Exception)
            {

            }
        }
        private async void _StartCheckHelathForRetryCount3()
        {
            try
            {

                NodeInfo node = this._nodesWithRetry3.Take();
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
                            NumberOfRunningThread =
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
                        StaticProperties.NodesToBeDeclaredDead.Add(node);
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        private async void _StartCheckHelathForRetryCount1()
        {
            //  int retryCount = 1;
            NodeInfo node = this._nodesWithRetry1.Take();

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
                            NumberOfRunningThread =
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
        private void _DeclareNodeDead()
        {
            NodeInfo node = null;
            try
            {
                node =
                   StaticProperties.NodesToBeDeclaredDead.Take(
                       CancellationTokens.MasterNodeHealthMessageProcessor.Token);
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

                StaticProperties.HostedNodes.TryRemove(node.NodeName, out node);
            }
        }
    }
}
