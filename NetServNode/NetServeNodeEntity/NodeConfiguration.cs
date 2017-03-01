using NetServNodeEntity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public class NodeConfiguration
    {
        public NodeConfiguration(string nodeName, string nodeAddress, int nodePort, bool isMaster,
            StorageType storageType, string connectionString, int maxThreads,int maxCpuUsage, bool nameServiceEnabled,
            NamingServiceConfiguration namingService, string[] actors)
        {
            NodeName = nodeName;
            NodeId = Guid.NewGuid().ToString();
            NodeAddress = nodeAddress;
            NodePort = nodePort;
            IsMaster = isMaster;
            StorageType = storageType;
            ConnectionString = connectionString;
            MaxThreads = maxThreads;
            MaxCpuUsage = maxCpuUsage;
            NamingServiceEnable = nameServiceEnabled;
            NamingService = namingService;
            Actors = actors;
            

        }
        public string NodeId { get; private set; }
        public string NodeName { get;  set; }
        public string NodeAddress { get;  set; }
        public int NodePort { get;  set; }
          public bool IsMaster { get; set; }
        public StorageType StorageType { get;  set; }
        public string ConnectionString { get;  set; }
        public int MaxThreads { get;  set; }
        public int MaxCpuUsage { get;  set; }
        public bool NamingServiceEnable { get; set; }
        public NamingServiceConfiguration NamingService { get; set; }
        public string[] Actors { get; set; }
        public string MasterNodeAddress { get; set; }
        public int MasterPort { get; set; }
        public string MasterNodeUri
        {
            get
            {
                return "http://"+ MasterNodeAddress + ":" + MasterPort;
            }
        }
    }
}
