using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.Master
{
    using System.Net.Http.Headers;

    using NetServEntity;
    using NetServNodeEntity;
    using Common.Logging;
    using NetServHttpWrapper;

    public class MasterTaskMessageManager
    {
        private readonly ILog log = LogManager.GetLogger(typeof(MasterManager));
        private const string SEND_MESSAGE_TASK_ENDPOINT = "Node/SendTaskMessage";
        private const int RETRY_COUNT = 3;

        private HttpWrapper _httpWrapper;
        public MasterTaskMessageManager()
        {
            this._httpWrapper = new HttpWrapper();
        }
        public async Task<bool> ProcessTaskMessage(TaskMessage message)
        {
            try
            {
                int count = 0;
                RETRY:
                var nodes = _GetNode(message);
                if (nodes == null)
                {
                    count++;
                    if (count < 3)
                    {
                        // Try 3 times
                        goto RETRY;
                    }
                    else
                    {
                        log.Debug("Could not find any node to process task. Adding message to Queue. Actor Id: " + message.ActorId);
                        lock (StaticProperties.TaskMessages)
                        {
                            StaticProperties.TaskMessages.Add(message);
                        }
                        return true;
                    }

                }
                bool success = false;
                //var nodeWithSameMessageId = nodes.FirstOrDefault(n => n.RegistedActors.Any(ac => ac.ActorName ==message.Actor));
                //if (nodeWithSameMessageId != null)
                //{
                //    success = await _SendTaskToNode(message, nodeWithSameMessageId.NodeAddress);
                //}
                //else
                //{
                foreach (var nodeInfo in nodes)
                {
                    log.Debug("Sending Task message to node");
                    var result = await this._httpWrapper.DoHttpPost<string, TaskMessage>(
                           nodeInfo.NodeAddress + "/" + SEND_MESSAGE_TASK_ENDPOINT,
                           message);
                    if (result != null)
                    {
                        log.Debug("Sent message to node. Node Id: " + nodeInfo.NodeName);
                        success = true;
                        break;
                    }

                }
                //}
                if (!success)
                {
                    log.Debug("Could send message to any node. Adding message to Queue. Actor Id: " + message.ActorId);
                    lock (StaticProperties.TaskMessages)
                    {
                        StaticProperties.TaskMessages.Add(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }


        }
        public async Task<bool> SendTaskToNode(TaskMessage taskMessage, string nodeAddress)
        {
            return await _SendTaskToNode(taskMessage, nodeAddress);
        }
        private async Task<bool> _SendTaskToNode(TaskMessage taskMessage, string nodeAddress)
        {
            var result = await this._httpWrapper.DoHttpPost<string, TaskMessage>(
                                nodeAddress + "/" + SEND_MESSAGE_TASK_ENDPOINT,
                                taskMessage);
            return result != null;
        }

        private IEnumerable<NodeInfo> _GetNode(TaskMessage taskMessage)
        {
            try
            {
                int count = 0;
                //Get actor. Try 3 times just to get latest actors
                RETRY:
                var nodes = StaticProperties.HostedNodes.Where(x => x.Value.RegistedActors.Any(an => an.ActorName == taskMessage.Actor));
                count++;
                if (count < 2) goto RETRY;
                else if (nodes.Count() != 0)
                {
                    //Sort nodes with least number of actors running and least cpu usage
                    var nodesWithDescendingThreads =
                        nodes.OrderByDescending(x => x.Value.NumberOfActorsRunning).Select(x => x.Value);
                    var finalList = nodesWithDescendingThreads.OrderByDescending(x => x.CpuUsage);
                    return finalList;

                }
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

        }
    }
}
