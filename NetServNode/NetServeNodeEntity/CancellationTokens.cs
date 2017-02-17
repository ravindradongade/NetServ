using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetServNodeEntity
{
    public static class CancellationTokens
    {
        static CancellationTokens()
        {
            MasterNodeListenerCancellationToken = new CancellationTokenSource();
            MasterNodeHealthMessageProcessor = new CancellationTokenSource();
            HttpOperation = new CancellationTokenSource();
            MessageProccessingCancellationToken = new CancellationTokenSource();
            MasterSelectionToken = new CancellationTokenSource();
        }
        public static CancellationTokenSource MasterNodeListenerCancellationToken { get; set; }
        public static CancellationTokenSource MasterNodeHealthMessageProcessor { get; set; }

        public static CancellationTokenSource HttpOperation { get; set; }

        public static CancellationTokenSource MessageProccessingCancellationToken { get; set; }

        public static CancellationTokenSource MasterSelectionToken { get; set; }

    }
}
