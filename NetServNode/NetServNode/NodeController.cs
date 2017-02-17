using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode
{
    using NetServeNodeEntity.Message;
    using NetServEntity;
    using NetServNodeEntity;
    using Node;
    using System.Web.Http;

    public class NodeController : ApiController
    {
        private NodeApiProcessor _nodeApiProcessor;
        public NodeController()
        {
            _nodeApiProcessor = new NodeApiProcessor();
        }
        [HttpPost]
        public async Task<bool> MasterDead([FromBody] NodeDeclaredDeadMessage message)
        {
            StaticProperties.MasterDeadMessageBlockingCollection.Add(message);
            return await Task.FromResult(true);

        }
        [HttpPost]
        public void RegisterMasterNode([FromBody]NodeInfo master)
        {
            _nodeApiProcessor.RegisterNewMaster(master);
        }

        [HttpGet]
        public async Task<NodeInfo> GetInfoForMasterSelection()
        {
            var nodeInfo = _nodeApiProcessor.GetNodeInfoForMasterSelection();
            return await Task.FromResult(nodeInfo);
        }

        [HttpPost]
        public  void IamNode([FromBody] NodeInfo nodeInfo)
        {
            _nodeApiProcessor.RegisterNode(nodeInfo);
        }

        [HttpPost]
        public async Task<string> SendTaskMessage([FromBody] TaskMessage taskMessage)
        {
            StaticProperties.TaskMessageContainer.Add(taskMessage);
            return await Task.FromResult("OK");
        }
       
    }
}
