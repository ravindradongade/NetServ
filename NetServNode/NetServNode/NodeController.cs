using System.Threading.Tasks;

namespace NetServNode
{
    using Common.Logging;
    using NetServeNodeEntity.Message;
    using NetServEntity;
    using NetServNodeEntity;
    using Node;
    using System.Web.Http;

    [RoutePrefix("route")]
    public class NodeController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(NodeController));
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
            log.Debug("MasterDead Controller");
            return await Task.FromResult(true);

        }
        [HttpPost]
        [Route("RegisterMasterNode")]
        public void RegisterMasterNode([FromBody]NodeInfo master)
        {
            _nodeApiProcessor.RegisterNewMaster(master);
            log.Debug("RegisterMasterNode Controller");
        }

        [HttpGet]
        [Route("GetInfoForMasterSelection")]
        public async Task<NodeInfo> GetInfoForMasterSelection()
        {
            var nodeInfo = _nodeApiProcessor.GetNodeInfoForMasterSelection();
            log.Debug("GetInfoForMasterSelection Controller");
            return await Task.FromResult(nodeInfo);
        }

        [HttpPost]
        [Route("IamNode")]
        public  void IamNode([FromBody] NodeInfoFromMaster nodeInfo)
        {
            lock(StaticProperties.NextMasterSelectionManager)
            {
                StaticProperties.NextMasterSelectionManager = nodeInfo.MasterSelector;
            }
            _nodeApiProcessor.RegisterNode(nodeInfo.Node);
            log.Debug("IamNode Controller");
        }

        [HttpPost]
        [Route("SendTaskMessage")]
        public async Task<string> SendTaskMessage([FromBody] TaskMessage taskMessage)
        {
            StaticProperties.TaskMessageContainer.Add(taskMessage);
            log.Debug("SendTaskMessage Controller");
            return await Task.FromResult("OK");
        }
        [HttpPost]
        [Route("RegisterNextMasterSelector")]
        public void RegisterNextMasterSelector([FromBody] NodeInfo nodeInfo)
        {
            StaticProperties.NextMasterSelectionManager = nodeInfo;
            log.Debug("RegisterNextMasterSelector Controller");
        }
       
    }
}
