using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServNode.Node
{
    using NetServEntity;
    using NetServNode.HttpUtilities;
    using NetServNodeEntity;
    using TaskSchedulers;

    public class NodeTaskManager
    {
        private HttpWrapper _httpWrapper;
        private readonly LimitedConcurrencyLevelTaskScheduler _taskSecheduler;
        public NodeTaskManager()
        {
            this._httpWrapper = new HttpWrapper();
            _taskSecheduler = new LimitedConcurrencyLevelTaskScheduler(StaticProperties.NodeConfig.MaxThreads);
        }

        public void StartListeningToTaskMessages()
        {
            try
            {
                foreach (var taskMessage in StaticProperties.TaskMessageContainer.GetConsumingEnumerable())
                {
                    Task.Factory.StartNew(() => ExecuteTask(taskMessage), CancellationTokens.MessageProccessingCancellationToken.Token, TaskCreationOptions.LongRunning, _taskSecheduler);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// Gets the actor instance
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        private object _GetActorInstance(string actorName)
        {
            try
            {

                object instance = null;
                bool got = StaticProperties.ActrorsDictionary.TryGetValue(actorName, out instance);
                if (!got)
                {
                    var type = Type.GetType(actorName);
                    if (type != null)
                    {
                        instance = Activator.CreateInstance(type);
                        if (instance != null)
                        {
                            StaticProperties.ActrorsDictionary.TryAdd(actorName, instance);
                        }
                        return instance;
                    }
                }
                return instance;
            }
            catch (Exception)
            {

                return null;
            }
        }

        //private void EnQueueTask(TaskMessage taskMessage)
        //{
        //    lock (StaticProperties.LocableObjectForTaskQueue)
        //    {
        //        try
        //        {
        //            StaticProperties.TaskMessagesToBeProcessed.Add(taskMessage);
        //        }
        //        catch
        //        {

        //        }
        //    }
        //}
        //private TaskMessage DequeueTaskWithMessageId()
        //{
        //    lock (StaticProperties.LocableObjectForTaskQueue)
        //    {
        //        try
        //        {
        //            //Check if message actor id and running actor id are same, then when till running actor completes its task
        //            int totalMessages = StaticProperties.TaskMessagesToBeProcessed.Count;
        //            for (int index = 0; index < totalMessages; index++)
        //            {
        //                var message = StaticProperties.TaskMessagesToBeProcessed[index];
        //                if (!StaticProperties.RunningActors.Any(rc => rc.ActorId == message.ActorId))
        //                {
        //                    StaticProperties.TaskMessagesToBeProcessed.Remove(message);
        //                    return message;
        //                }
        //            }
        //            return null;
        //        }
        //        catch
        //        {
        //            return null;
        //        }
        //    }
        //}

       

        private void ExecuteTask(TaskMessage message)
        {
            try
            {
                lock (StaticProperties.RunningActors)
                {
                    StaticProperties.RunningActors.Add(message);
                }
                var instance = _GetActorInstance(message.TaskClassName);
                var classType = instance.GetType();
                classType.GetMethod(message.TaskMethodName).Invoke(instance, message.MethodParameters);
            }
            catch (Exception)
            {

                
            }
            finally
            {
                lock (StaticProperties.RunningActors)
                {
                    StaticProperties.RunningActors.Remove(message);
                }
            }
           
        }
    }
}
