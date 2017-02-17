using NetServNodeEntity;
using System.Linq;

namespace NetServNode
{
    using System;

    using Microsoft.Owin.Hosting;

    using NetServNode.Master;
    using NetServNode.Owin;
    using Node;

    public class NetServManager

    {
        private const string NODE = "Node";
        private const int DefaultNodePort = 3300;

        private const int DefaultMaxThreads = 50;

        private const int DefaultCpuUsage = 75;
        private readonly MasterManager _masterManager;
        private readonly NodeManager _nodeManager;


        public NetServManager()
        {
            this._masterManager = new MasterManager();
            this._nodeManager = new NodeManager();
        }
        private void _ValidateConfiguration(NodeConfiguration nodeConfiguration)
        {
            if (string.IsNullOrEmpty(nodeConfiguration.NodeName))
            {
                nodeConfiguration.NodeName = _GenerateNodeName();
            }
            if (nodeConfiguration.NodePort == 0)
            {
                nodeConfiguration.NodePort = DefaultNodePort;
            }
            if (nodeConfiguration.MaxThreads == 0)
            {
                nodeConfiguration.MaxThreads = DefaultMaxThreads;
            }
            if (nodeConfiguration.MaxCpuUsage == 0)
            {
                nodeConfiguration.MaxCpuUsage = DefaultCpuUsage;
            }

            if (nodeConfiguration.IsMaster && _IsThisDuplicateMaster())
            {
                throw new System.ArgumentNullException("Master Node already hosted");
            }
            if (!nodeConfiguration.IsMaster)
            {
                if (nodeConfiguration.NodePort == 0)
                    throw new System.ArgumentNullException("Master Node Port is empty!!!");
                if (string.IsNullOrEmpty(nodeConfiguration.MasterNodeAddress))
                    throw new System.ArgumentNullException("Master Node Address is empty!!!");

            }
        }
        private string _GenerateNodeName()
        {
            return NODE + "_" + System.Net.Dns.GetHostName();
        }
        private bool _IsThisDuplicateMaster()
        {
            return StaticProperties.HostedNodes.Any(node => node.Value.NodeType == NetServNodeEntity.Enums.NodeTypes.Master);
        }
        public void StartNode(NodeConfiguration nodeConfiguration)
        {
            try
            {
                this._ValidateConfiguration(nodeConfiguration);

                StaticProperties.NodeConfig = nodeConfiguration;
                if (!StaticProperties.NodeConfig.IsMaster)
                {
                    if (!_nodeManager.IsMasterReachable().Result)
                    {
                        throw new OperationCanceledException("Master is not reachable");
                    }
                }
                WebApp.Start<Startup>(url: "http://+:" + StaticProperties.NodeConfig.NodePort);
                if (StaticProperties.NodeConfig.IsMaster)
                {
                    this._masterManager.StartMasterManager();
                }
                else
                {
                    _nodeManager.StartNodeManager();
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public void StopNode()
        {

        }

    }
}
