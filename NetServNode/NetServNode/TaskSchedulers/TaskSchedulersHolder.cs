using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.TaskSchedulers
{
    public class TaskSchedulersHolder
    {
        public static LimitedConcurrencyLevelTaskScheduler SchedulerToSendMissedTaskToNode { get; set; }
    }
}
