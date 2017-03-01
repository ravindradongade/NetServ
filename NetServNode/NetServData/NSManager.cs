using NetServHttpWrapper;
using NetServNodeEntity;
using System;
using System.Collections.Generic;

namespace NetServData
{
    public class NSManager : IDataManager
    {
        private const string MASTER_DEAD = "/ns/MasterDead";
        private const string REGISTER_MASTER = "/ns/RegisterMaster";
        private const string GET_LIVE_MASTER = "/ns/GetLiveMaster";
        private readonly HttpWrapper _httpWrapper;
        public NSManager()
        {
            _httpWrapper = new HttpWrapper();
        }
        public async void DeclareMasterDead(MasterDetails node)
        {
          await  _httpWrapper.DoHttpPost<MasterDetails>(string.Format("{0}{1}", StaticProperties.NodeConfig.NamingService.Uri,MASTER_DEAD), node);
        }

        public void DeclareNodeDead(NodeDetails node)
        {
            throw new NotImplementedException();
        }

        public List<NodeDetails> GetAllNodes()
        {
            throw new NotImplementedException();
        }

        public MasterDetails GetLiveMasterDetails()
        {
            var masterDeatil = _httpWrapper.DoHttpGet<MasterDetails>(StaticProperties.NodeConfig.NamingService.Uri + GET_LIVE_MASTER).Result;
            return masterDeatil;
        }

        public List<NodeDetails> GetLiveNodes()
        {
            throw new NotImplementedException();
        }

        public NodeDetails GetNode(string nodeId)
        {
            throw new NotImplementedException();
        }

        public async void InsertMaster(MasterDetails master)
        {
           await _httpWrapper.DoHttpPost<MasterDetails>(StaticProperties.NodeConfig.NamingService.Uri+REGISTER_MASTER, master);
        }

        public void InsertNode(NodeDetails node)
        {
            throw new NotImplementedException();
        }
    }
}
