using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.Node
{
    using System.Diagnostics;
    using System.Timers;

    using NetServEntity;

    using NetServNode.HttpUtilities;

    using NetServNodeEntity;
    using NetServNodeEntity.Message;


    internal class NodeManager
    {
        private HttpWrapper _httpWrapper;
        private int SEND_NODE_UPDATE_INTERVAL = 5;
        private Timer _timer;

        private static bool IS_SENDNODEHEALTH_IN_PROGRESS;
        private static object _lockableObject=new object();
        private PerformanceCounter _cpuCounter;

        private const string SEND_HEALTHINFO_ENDPOINT = "IamNode";

        private const string MASTE_DEAD_BROADCAST_ENDPOINT = "MasterDead";

        public NodeManager()
        {
            this._Intialize();
        }

        public void ProcessTaskMessage(TaskMessage taskMessage)
        {
            
        }
        private void _Intialize()
        {
            this._timer=new Timer(this.SEND_NODE_UPDATE_INTERVAL);
            this._timer.Elapsed += _SendHealthInfo_Elapsed;
            this._timer.Start();
            this._cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            this._httpWrapper = new HttpWrapper();
        }

        private void _SendHealthInfo_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._SendHealthInfoToMaster();
        }

        private async void _SendHealthInfoToMaster()
        {
            if (IS_SENDNODEHEALTH_IN_PROGRESS)
            {
               return;
            }
            int retryCount = 1;
            RETRY:
            var nodeInformationMessage = new NodeInformationMessage();
            nodeInformationMessage.NodeAddress = StaticProperties.NodeConfig.NodeAddress;
            nodeInformationMessage.CpuUsage = this._cpuCounter.NextValue();
            nodeInformationMessage.NumberOfRunningTask = StaticProperties.NumberOfRunningTasks;
            // nodeInformationMessage.RamUsage=Process.GetCurrentProcess().
            nodeInformationMessage.NodeId = StaticProperties.NodeConfig.NodeId;
            nodeInformationMessage.NodeName = StaticProperties.NodeConfig.NodeName;
            nodeInformationMessage.NodeAddress = StaticProperties.NodeConfig.NodeAddress;
            nodeInformationMessage.Actros = StaticProperties.NodeConfig.Actors;
            var result = await this._httpWrapper.DoHttpPost<string, NodeInformationMessage>(
                  StaticProperties.NodeConfig.NodeAddress + "/" + SEND_HEALTHINFO_ENDPOINT,
                  nodeInformationMessage);
            if (result != "OK")
            {
                IS_SENDNODEHEALTH_IN_PROGRESS = false;
                return;
            }
            else
            {
                if (retryCount < 3) goto RETRY;
                else
                {
                    this._timer.Stop();
                    this._BroadCastMasterDead();
                }
            }
        }

        private void _BroadCastMasterDead()
        {
            foreach (var hostedNode in StaticProperties.HostedNodes)
            {
                this._httpWrapper.DoHttpPostWithNoReturn(
                    hostedNode + "/" + MASTE_DEAD_BROADCAST_ENDPOINT,
                    StaticProperties.NodeConfig.NodeName);
            }
        }


    }
}
