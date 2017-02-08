using NetServNodeEntity;
using System.Linq;

namespace NetServNode
{
    using System;

    using Microsoft.Owin.Hosting;

    using NetServNode.Master;
    using NetServNode.Owin;

    public class NetServManager

    {
        private const string NODE = "Node";
        private const int DefaultNodePort = 3300;

        private const int DefaultMaxThreads = 50;

        private const int DefaultCpuUsage = 75;
        private readonly MasterManager _masterManager;



        public NetServManager()
        {
            this._masterManager=new MasterManager();
        }
        private void _ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(StaticProperties.NodeConfig.NodeName))
            {
                StaticProperties.NodeConfig.NodeName = _GenerateNodeName();
            }
            if (StaticProperties.NodeConfig.NodePort == 0)
            {
                StaticProperties.NodeConfig.NodePort = DefaultNodePort;
            }
            if (StaticProperties.NodeConfig.MaxThreads == 0)
            {
                StaticProperties.NodeConfig.MaxThreads = DefaultMaxThreads;
            }
            if (StaticProperties.NodeConfig.MaxCpuUsage == 0)
            {
                StaticProperties.NodeConfig.MaxCpuUsage = DefaultCpuUsage;
            }

            if (StaticProperties.NodeConfig.IsMaster && _IsThisDuplicateMaster())
            {
                throw new System.Exception("Master Node already hosted");
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

        private void _StartNodeManager()
        {
            
        }
        public void StartNode()
        {
            try
            {
                this._ValidateConfiguration();
                WebApp.Start<Startup>(url: "http://localhost:" + StaticProperties.NodeConfig.NodePort);
                StaticProperties.NodeConfig.NodeId = Guid.NewGuid().ToString();
                if (StaticProperties.NodeConfig.IsMaster)
                {
                    this._masterManager.StartMasterManager();
                }
                else
                {
                    this._StartNodeManager();
                }
               
            }
            catch (Exception)
            {

                throw;
            }


        }

    }
}
