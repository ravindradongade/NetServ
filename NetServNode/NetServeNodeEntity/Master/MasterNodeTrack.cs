using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetServNodeEntity.Master
{
    public class MasterNodeTrack
    {
        public DateTime LastCheckIn { get; set; }
        public string NodeId { get; set; }

        public int NumberOfRunningThread { get; set; }

        public int CpuUsage { get; set; }
    }
}
