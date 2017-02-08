using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public static class NodeSockets
    {
        public static Socket NodeLister { get; set; }
        public static Socket NodeSender { get; set; }

        public static Socket MasterListener { get; set; }

        public static Socket MasterSender { get; set; }
    }
}
