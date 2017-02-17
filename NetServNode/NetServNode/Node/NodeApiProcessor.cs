using NetServNodeEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.Node
{
    public class NodeApiProcessor
    {
        public void RegisterNewMaster(NodeInfo nodeInfo)
        {
            try
            {
                StaticProperties.NodeConfig.MasterNodeAddress = nodeInfo.NodeAddress;
                NodeManager.SendNodeInfoToMasterTimer.Start();
            }
            catch (Exception)
            {

                throw;
            }

        }
        public NodeInfo GetNodeInfoForMasterSelection()
        {
            try
            {
                var nodeInfo = new NodeInfo() { NodeAddress = StaticProperties.NodeConfig.NodeAddress, CpuUsage = NodeManager.CpuCounter.NextValue(), NumberOfActorsRunning = StaticProperties.RunningActors.Count, NodeId = StaticProperties.NodeConfig.NodeId };
                return nodeInfo;
            }
            catch(Exception ex)
            {
                return null;
            }
           
        }

        public void RegisterNode(NodeInfo nodeInfo)
        {
            try
            {
                StaticProperties.HostedNodes.AddOrUpdate(nodeInfo.NodeId, nodeInfo, (key, value) => nodeInfo);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
