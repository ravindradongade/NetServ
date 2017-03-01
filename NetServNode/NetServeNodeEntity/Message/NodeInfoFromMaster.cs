using NetServNodeEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServeNodeEntity.Message
{
   public  class NodeInfoFromMaster
    {
        public NodeInfo  Node { get; set; }
        public NodeInfo MasterSelector { get; set; }
    }
}
