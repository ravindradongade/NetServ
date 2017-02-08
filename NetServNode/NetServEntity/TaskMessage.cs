using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServEntity
{
    public class TaskMessage
    {
        public string TaskClassName { get; set; }

        public string TaskMethodName { get; set; }

        public object[] MethodParameters { get; set; }

        public object [] ConstructorParameters { get; set; }

        public string Actor { get; set; }

        public string ActorId { get; set; }
    }
}
