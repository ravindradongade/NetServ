using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetServNode.Node
{
    using System.Diagnostics;
    using System.Timers;
    using NetServNodeEntity;
    using NetServNodeEntity.Message;
    using NetServData;
    using NetServHttpWrapper;

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
        private IDataManager _dataManager;

        public NodeManager()
        {
            _httpWrapper = new HttpWrapper();
            _dataManager = DataFactory.GetDataManager(StaticProperties.NodeConfig.StorageType, StaticProperties.NodeConfig.ConnectionString);
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

        public  Tuple<string,int> GetMasterFromStorage()
        {
          var master=   _dataManager.GetLiveMasterDetails();
            if(master!=null)
            {
                var tuple = new Tuple<string, int>(master.Address, master.Port);
                return tuple;
            }
            else
            {
                return null;
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
                var hostedNodes = StaticProperties.HostedNodes.Values;
                foreach (var hostedNode in hostedNodes)
                {
                    var nodeInfo = await this._httpWrapper.DoHttpGet<NodeInfo>(
                          hostedNode.NodeAddress + "/" + GET_INFO_FOR_MASTER_SELECTION);
                    if (nodeInfo != null)
                    {
                        nodeInfos.Add(nodeInfo);
                    }
                }
                hostedNodes = null;
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
            var hostedNodes = StaticProperties.HostedNodes.Values;
            foreach (var hostedNode in hostedNodes)
            {
                this._httpWrapper.DoHttpPostWithNoReturn<NodeInfo>(
                       hostedNode.NodeAddress + "/" + GET_INFO_FOR_MASTER_SELECTION, winner);

            }
            StaticProperties.NodeConfig.IsMaster = true;
            StaticProperties.MasterSelectionProcessStarted = false;
            IS_SENDNODEHEALTH_IN_PROGRESS = false;
            hostedNodes = null;
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
            var hostedNodes = StaticProperties.HostedNodes.Values;
            foreach (var hostedNode in hostedNodes)
            {
                this._httpWrapper.DoHttpPostWithNoReturn(
                    hostedNode.NodeAddress + "/" + MASTE_DEAD_BROADCAST_ENDPOINT,
                    StaticProperties.NodeConfig.NodeName);
            }
        }
    }
}
