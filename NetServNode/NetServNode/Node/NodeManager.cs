using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.Node
{
    using System.Diagnostics;
    using System.Timers;

    using NetServeNodeEntity.Message;

    using NetServEntity;

    using NetServNode.HttpUtilities;

    using NetServNodeEntity;
    using NetServNodeEntity.Message;


    internal class NodeManager
    {
        private HttpWrapper _httpWrapper;
        private int SEND_NODE_UPDATE_INTERVAL = 5;
        public static Timer SendNodeInfoToMasterTimer;

        private static bool IS_SENDNODEHEALTH_IN_PROGRESS;
        private static object _lockableObject = new object();
        public static PerformanceCounter CpuCounter;
        private NodeTaskManager _nodeTaskManager;

        private const string SEND_HEALTHINFO_ENDPOINT = "Master/IamNode";

        private const string MASTE_DEAD_BROADCAST_ENDPOINT = "Node/MasterDead";

        private const string GET_INFO_FOR_MASTER_SELECTION = "Node/GetInfoForMasterSelection";

        private const string REGISTER_NEW_MASTER = "Node/RegisterNewMaster";

        private const string MASER_PING = "Master/MasterPing";

        public NodeManager()
        {
            _httpWrapper = new HttpWrapper();
        }
        public void StartNodeManager()
        {
            _Intialize();
            Task.Factory.StartNew(() => _WhoWillDoMasterSelection(), CancellationTokens.MasterSelectionToken.Token);

        }
        private void _WhoWillDoMasterSelection()
        {
            foreach (var message in StaticProperties.MasterDeadMessageBlockingCollection.GetConsumingEnumerable())
            {
                try
                {

                    if (StaticProperties.MasterSelectionProcessStarted) return;
                    bool amIMasterSelectionManager = StaticProperties.NextMasterSelectionManager.NodeId == StaticProperties.NodeConfig.NodeId;

                    if (amIMasterSelectionManager)
                    {
                        int majority = (StaticProperties.HostedNodes.Count / 10) * 7;
                        bool doIHaveMajorityToDeclareMasterAsDead = false;
                        int messagesCount = 0;
                        lock (StaticProperties.MasterDeadMessages)
                        {
                            messagesCount = StaticProperties.MasterDeadMessages.Count;
                            doIHaveMajorityToDeclareMasterAsDead = messagesCount >= majority;
                        }
                        if (doIHaveMajorityToDeclareMasterAsDead)
                        {
                            StaticProperties.MasterSelectionProcessStarted = true;
                            Task.Factory.StartNew(() => _StartMasterSelection());
                            return;
                        }
                    }

                    lock (StaticProperties.MasterDeadMessages)
                    {
                        if (!StaticProperties.MasterDeadMessages.Any(n => n.DeclaredBy == message.DeclaredBy))
                        {
                            StaticProperties.MasterDeadMessages.Add(message);
                        }
                    }

                }
                catch (Exception)
                {

                    throw;
                }

            }

        }

        public async Task<bool> IsMasterReachable()
        {
            var result = await _httpWrapper.DoHttpGet<string>(StaticProperties.NodeConfig.MasterNodeUri+"/"+MASER_PING);
            return result == "OK";
        }

        private async void _StartMasterSelection()
        {
            try
            {
                var myCpuUsage = CpuCounter.NextValue();
                List<NodeInfo> nodeInfos = new List<NodeInfo>();
                foreach (var hostedNode in StaticProperties.HostedNodes)
                {
                    var nodeInfo = await this._httpWrapper.DoHttpGet<NodeInfo>(
                          hostedNode.Value.NodeAddress + "/" + GET_INFO_FOR_MASTER_SELECTION);
                    if (nodeInfo != null)
                    {
                        nodeInfos.Add(nodeInfo);
                    }
                }
                var nodeCanBeMaster = nodeInfos.OrderByDescending(n => n.CpuUsage).OrderByDescending(nd => nd.NumberOfActorsRunning).FirstOrDefault();
                NodeInfo winner = null;
                if (nodeCanBeMaster != null)
                {
                    int myRunningActorsCount = 0;
                    lock (StaticProperties.RunningActors)
                    {
                        myRunningActorsCount = StaticProperties.RunningActors.Count;
                    }
                    if (nodeCanBeMaster.CpuUsage <= myCpuUsage && nodeCanBeMaster.NumberOfActorsRunning < myRunningActorsCount)
                    {
                        winner = nodeCanBeMaster;
                    }
                }
                if (winner == null)
                {
                    winner = new NodeInfo() { NodeId = StaticProperties.NodeConfig.NodeId, NodeAddress = StaticProperties.NodeConfig.NodeAddress, NodeName = StaticProperties.NodeConfig.NodeName };
                }
                _BroadCastMasterSelected(winner);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void _BroadCastMasterSelected(NodeInfo winner)
        {
            foreach (var hostedNode in StaticProperties.HostedNodes)
            {
                this._httpWrapper.DoHttpPostWithNoReturn<NodeInfo>(
                       hostedNode.Value.NodeAddress + "/" + GET_INFO_FOR_MASTER_SELECTION, winner);

            }
            StaticProperties.NodeConfig.IsMaster = true;
            StaticProperties.MasterSelectionProcessStarted = false;
            IS_SENDNODEHEALTH_IN_PROGRESS = false;

        }
        private void _Intialize()
        {
            _SendHealthInfoToMaster();
            SendNodeInfoToMasterTimer = new Timer( this.SEND_NODE_UPDATE_INTERVAL*1000);
            SendNodeInfoToMasterTimer.Elapsed += _SendHealthInfo_Elapsed;
            SendNodeInfoToMasterTimer.Start();
            CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //this._httpWrapper = new HttpWrapper();
            _nodeTaskManager = new NodeTaskManager();
        }
        private void _SendHealthInfo_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._SendHealthInfoToMaster();
        }
        private async void _SendHealthInfoToMaster()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return;

            if (IS_SENDNODEHEALTH_IN_PROGRESS)
            {
                return;
            }
            IS_SENDNODEHEALTH_IN_PROGRESS = true;
            int retryCount = 1;
            RETRY:
            var nodeInformationMessage = new NodeInformationMessage();
            nodeInformationMessage.NodeAddress = StaticProperties.NodeConfig.NodeAddress;
            nodeInformationMessage.CpuUsage = CpuCounter.NextValue();
            lock (StaticProperties.RunningActors)
            {
                nodeInformationMessage.NumberOfRunningTask = StaticProperties.RunningActors.Count;
            }
            // nodeInformationMessage.RamUsage=Process.GetCurrentProcess().
            nodeInformationMessage.NodeId = StaticProperties.NodeConfig.NodeId;
            nodeInformationMessage.NodeName = StaticProperties.NodeConfig.NodeName;
            nodeInformationMessage.NodeAddress = "http://"+StaticProperties.NodeConfig.NodeAddress + ":" + StaticProperties.NodeConfig.NodePort;
            nodeInformationMessage.Actros = StaticProperties.NodeConfig.Actors.Select(a => new NetServeNodeEntity.Actors.ActorModel() { ActorName = a }).ToList();
            var result = await this._httpWrapper.DoHttpPost<string, NodeInformationMessage>(
                  StaticProperties.NodeConfig.MasterNodeUri + "/" + SEND_HEALTHINFO_ENDPOINT,
                  nodeInformationMessage);
            if (result == "OK")
            {
                IS_SENDNODEHEALTH_IN_PROGRESS = false;
                return;
            }
            else
            {
                if (retryCount < 3) goto RETRY;
                else
                {
                    SendNodeInfoToMasterTimer.Stop();
                    this._BroadCastMasterDead();
                }
            }
            IS_SENDNODEHEALTH_IN_PROGRESS = false;
        }
        private void _BroadCastMasterDead()
        {
            foreach (var hostedNode in StaticProperties.HostedNodes)
            {
                this._httpWrapper.DoHttpPostWithNoReturn(
                    hostedNode.Value.NodeAddress + "/" + MASTE_DEAD_BROADCAST_ENDPOINT,
                    StaticProperties.NodeConfig.NodeName);
            }
        }
    }
}
