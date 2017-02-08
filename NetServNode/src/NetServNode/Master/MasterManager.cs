using NetServNodeEntity;
using NetServNodeEntity.Enums;
using NetServNodeEntity.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetServNode.Master
{

    public class MasterManager
    {
        private const int NODE_HEALTH_WAITING_PERIOD_IN_SEC = 15;
        private static object _lockableObjectForTimer = new object();
        private Timer _timer;
        private bool NODE_HEALTH_PROCESS_STARTED;
        private const string QUERY_MESSAGE = "QUERY";
        private const int RETRY_COUNT = 3;
        public void Intiallize()
        {

        }
        public void Process()
        {

        }
        private async void _StartMessageRecieverFromNodes()
        {
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;
            TcpListener tcpListener = new TcpListener(StaticProperties.NodeConfig.NodeIpAddress, StaticProperties.NodeConfig.NodeToNodePort);
            tcpListener.Start();
            while (true && !CancellationTokens.MasterNodeListenerCancellationToken.IsCancellationRequested)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();

                var networkStream = tcpClient.GetStream();
                // int i;
                // Loop to receive all the data sent by the client.

                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, networkStream.Read(bytes, 0, bytes.Length));
                var messageType = _GetMessageType(data);
                byte[] msg = System.Text.Encoding.ASCII.GetBytes("OK");

                // Send back a response.
                networkStream.Write(msg, 0, msg.Length);
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, networkStream.Read(bytes, 0, bytes.Length));
                _ProcessMessageFromNode(data, messageType);

                tcpClient.Dispose();

            }

        }
        private MessageType _GetMessageType(string type)
        {
            MessageType messageType = (MessageType)int.Parse(type);
            return messageType;
        }
        private void _ProcessMessageFromNode(string message, MessageType messageType)
        {
            if (messageType == MessageType.HealthInfo)
            {
                StaticProperties.NodeHealthInfoMessagesCollection.Add(message);
            }
        }
        private void _ProcessNodeHealthMessage()
        {
            while (true && !CancellationTokens.MasterNodeHealthMessageProcessor.IsCancellationRequested)
            {
                try
                {
                    var message = StaticProperties.NodeHealthInfoMessagesCollection.Take();
                    try
                    {
                        var nodeInformationMessage = JsonConvert.DeserializeObject<NodeInformationMessage>(message);
                        var nodeInformation = new NodeInfo()
                        {
                            CpuUsage = nodeInformationMessage.CpuUsage,
                            LastCheckinTime = DateTime.UtcNow,
                            NodeAddress = nodeInformationMessage.NodeAddress,
                            NodeId = nodeInformationMessage.NodeId,
                            NodeName = nodeInformationMessage.NodeName,
                            NodePort = nodeInformationMessage.NodePort,
                            NodeType = nodeInformationMessage.NodeType,
                            NumberOfRunningThread = nodeInformationMessage.NumberOfRunningThread
                        };
                        StaticProperties.HostedNodes.AddOrUpdate(nodeInformation.NodeName, nodeInformation, (key, oldValue) => nodeInformation);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }

        private async void _CheckNodeHealth()
        {
            lock (_lockableObjectForTimer)
            {
                if (NODE_HEALTH_PROCESS_STARTED)
                {
                    return;
                }
                NODE_HEALTH_PROCESS_STARTED = true;
            }

            var timeDifference = DateTime.UtcNow.AddSeconds(-NODE_HEALTH_WAITING_PERIOD_IN_SEC);
            var nodes = StaticProperties.HostedNodes.Values.Where(node => node.LastCheckinTime < timeDifference);
            foreach (var node in nodes)
            {
                if(node.TotalNodeConnectRetryCount>RETRY_COUNT)
                {
                    StaticProperties.NodesToBeDeclaredDead.Add(node);
                }
                
                using (TcpClient tcpClient = new TcpClient())
                {
                    
                    await tcpClient.ConnectAsync(node.NodeAddress, node.NodePort);
                    if (tcpClient.Connected)
                    {
                        tcpClient.SendTimeout = 5;
                        tcpClient.ReceiveTimeout = 5;
                        var queryMessage = Encoding.ASCII.GetBytes(QUERY_MESSAGE);
                        NetworkStream stream = tcpClient.GetStream();
                        await stream.WriteAsync(queryMessage, 0, queryMessage.Length);
                        var response = new byte[1024];
                        await stream.ReadAsync(response, 0, response.Length);
                        string responseData = Encoding.ASCII.GetString(response);

                        _ProcessMessageFromNode(responseData, MessageType.HealthInfo);

                    }
                    else
                    {
                        node.TotalNodeConnectRetryCount++;
                    }
                    
                }

            }

        }

        private async void  _DeclareNodeDead()
        {
            
        }
    }
}
