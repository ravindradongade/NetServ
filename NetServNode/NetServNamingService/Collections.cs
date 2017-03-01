using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNamingService
{
    public static class Collections
    {
        public static List<MasterDetails> Masters;
        public static object MastersLock;
        public static bool MasterElectionStarted;
        static Collections()
        {
            Masters = new List<MasterDetails>();
            MastersLock = new object();
        }
    }
}
