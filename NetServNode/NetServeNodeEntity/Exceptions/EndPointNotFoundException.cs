using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServeNodeEntity.Exceptions
{
    public class EndPointNotFoundException : Exception
    {
        public override string Message
        {
            get
            {
                return "Endpoint not found!!";
            }
        }
    }
}
