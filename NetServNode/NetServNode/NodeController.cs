using System.Threading.Tasks;

namespace NetServNode
{
    using NetServeNodeEntity.Message;
    using NetServEntity;
    using NetServNodeEntity;
    using Node;
    using System.Web.Http;

    [RoutePrefix("route")]
    public class NodeController : ApiController
    {
        private NodeApiProcessor _nodeApiProcessor;
        public NodeController()
        {
            _nodeApiProcessor = new NodeApiProcessor();
        }
        [HttpPost]
        [Route("MasterDead")]
        public async Task<bool> MasterDead([FromBody] NodeDeclaredDeadMessage message)
        {
            StaticProperties.MasterDeadMessageBlockingCollection.Add(message);
            return await Task.FromResult(true);

        }
        [HttpPost]
        [Route("RegisterMasterNode")]
        public void RegisterMasterNode([FromBody]NodeInfo master)
        {
            _nodeApiProcessor.RegisterNewMaster(master);
        }

        [HttpGet]
        [Route("GetInfoForMasterSelection")]
        public async Task<NodeInfo> GetInfoForMasterSelection()
        {
            var nodeInfo = _nodeApiProcessor.GetNodeInfoForMasterSelection();
            return await Task.FromResult(nodeInfo);
        }

        [HttpPost]
        [Route("IamNode")]
        public  void IamNode([FromBody] NodeInfo nodeInfo)
        {
            _nodeApiProcessor.RegisterNode(nodeInfo);
        }

        [HttpPost]
        [Route("SendTaskMessage")]
        public async Task<string> SendTaskMessage([FromBody] TaskMessage taskMessage)
        {
            StaticProperties.TaskMessageContainer.Add(taskMessage);
            return await Task.FromResult("OK");
        }
       
    }
}
