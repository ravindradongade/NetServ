using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServeNodeEntity.Message
{
   public  class NodeDeclaredDeadMessage
    {
       public string NodeId { get; set; }

       public bool IsDead { get; set; }

       public string DeclaredBy { get; set; }

       public bool AmIMaster { get; set; }
    }
}
