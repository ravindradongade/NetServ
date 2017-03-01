using Common.Logging;
using NetServNodeEntity;
using System;

namespace NetServNode.Node
{
    public class NodeApiProcessor
    {
        private readonly ILog log = LogManager.GetLogger(typeof(NodeApiProcessor));
        public void RegisterNewMaster(NodeInfo nodeInfo)
        {
            try
            {
                StaticProperties.NodeConfig.MasterNodeAddress = nodeInfo.NodeAddress;
                NodeManager.SendNodeInfoToMasterTimer.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
        public NodeInfo GetNodeInfoForMasterSelection()
        {
            try
            {
                var nodeInfo = new NodeInfo() { NodeAddress = StaticProperties.NodeConfig.NodeAddress + ":" + StaticProperties.NodeConfig.NodePort, CpuUsage = NodeManager.CpuCounter.NextValue(), NumberOfActorsRunning = StaticProperties.RunningActors.Count, NodeId = StaticProperties.NodeConfig.NodeId };
                return nodeInfo;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

        }

        public void RegisterNode(NodeInfo nodeInfo)
        {
            try
            {
                StaticProperties.HostedNodes.AddOrUpdate(nodeInfo.NodeId, nodeInfo, (key, value) => nodeInfo);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
