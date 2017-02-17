using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.MasterController
{
    using System.Web.Http;

    using NetServEntity;

    using NetServNode.Master;

    using NetServNodeEntity;
    using NetServNodeEntity.Message;

    public class MasterController : ApiController
    {
        private MasterManager _masterManager;

        private MasterTaskMessageManager _masterTaskManager;
        public MasterController()
        {
            this._masterManager = new MasterManager();
            this._masterTaskManager = new MasterTaskMessageManager();
        }
        [HttpPost]
        public Task<string> IamNode([FromBody] NodeInformationMessage nodeInformationMessage)
        {
            this._masterManager.ProcessNodeHealthMessage(nodeInformationMessage);
            return Task.FromResult("OK");
        }

        [HttpPost]
        public void PassTaskMessageAsync([FromBody] TaskMessage taskMessage)
        {
            this._masterTaskManager.ProcessTaskMessage(taskMessage);
        }
        [HttpGet]
        public string MasterPing()
        {
            return "OK";
        }
    }
}
