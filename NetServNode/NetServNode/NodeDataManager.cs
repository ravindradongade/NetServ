using NetServData;
using NetServNodeEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode
{
    internal class NodeDataManager
    {
        private readonly IDataManager _dataManager;
        internal NodeDataManager()
        {
            _dataManager = DataFactory.GetDataManager(StaticProperties.NodeConfig.StorageType, StaticProperties.NodeConfig.ConnectionString);
        }

    }
}
