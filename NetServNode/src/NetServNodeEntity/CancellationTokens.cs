using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public class CancellationTokens
    {
        public static CancellationTokenSource MasterNodeListenerCancellationToken { get; set; }
        public static CancellationTokenSource MasterNodeHealthMessageProcessor { get; set; }

    }
}
