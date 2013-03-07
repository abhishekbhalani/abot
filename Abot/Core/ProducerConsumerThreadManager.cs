using log4net;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Abot.Core
{
    public class ProducerConsumerThreadManager : IThreadManager
    {
        static ILog _logger = LogManager.GetLogger(typeof(ProducerConsumerThreadManager).FullName);

        CancellationTokenSource[] _cancellationTokens;
        BlockingCollection<ConsumerAction> _actionsToExecute = new BlockingCollection<ConsumerAction>();
        ConcurrentStack<ConsumerAction> _inProcessActionsToExecute = new ConcurrentStack<ConsumerAction>();

        public ProducerConsumerThreadManager(int maxThreads)
        {
            if ((maxThreads > 100) || (maxThreads < 1))
                throw new ArgumentException("MaxThreads must be from 1 to 100");
            
            _cancellationTokens = new CancellationTokenSource[maxThreads];

            for (int i = 0; i < maxThreads; i++)
            {
                _cancellationTokens[i] = new CancellationTokenSource();
                Task.Factory.StartNew(() => RunConsumer(i), _cancellationTokens[i].Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Max number of threads to use
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return _cancellationTokens.Length;
            }
        }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// </summary>
        public void DoWork(Action action)
        {
            DoWork(action, 0);
        }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread with a timeout
        /// </summary>
        public void DoWork(Action action, int timeoutInMilliSecs)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _actionsToExecute.Add(new ConsumerAction { Action = action, TimeoutInMillisecs = timeoutInMilliSecs });
        }

        /// <summary>
        /// Whether there are running threads
        /// </summary>
        public bool HasRunningThreads()
        {
            return _inProcessActionsToExecute.Count > 0;
        }

        public void AbortAll()
        {
            foreach (CancellationTokenSource cancellationTokenSource in _cancellationTokens)
                cancellationTokenSource.Cancel();
        }

        private void RunConsumer(int i)
        {
            foreach (ConsumerAction consumerAction in _actionsToExecute.GetConsumingEnumerable())
            {
                ReportAsInProgress(consumerAction);
                if (consumerAction.TimeoutInMillisecs > 0)
                {
                    ActionTimer timer = new ActionTimer(consumerAction.TimeoutInMillisecs, i);
                    timer.Elapsed += (sender, e) =>
                    {
                        ActionTimer elapsedTimer = sender as ActionTimer;
                        if (elapsedTimer != null)
                        {
                            elapsedTimer.Stop();
                            _cancellationTokens[elapsedTimer.TaskIndex].Cancel();
                        }
                    };
                    timer.Start();
                    consumerAction.Action.Invoke();
                    timer.Stop();
                }
                else
                {
                    consumerAction.Action.Invoke();
                }

                ReportAsProgressComplete(consumerAction);
            }
        }

        private void ReportAsInProgress(ConsumerAction consumerAction)
        {
            _inProcessActionsToExecute.Push(consumerAction);

        }

        private void ReportAsProgressComplete(ConsumerAction consumerAction)
        {
            ConsumerAction ddd;
            _inProcessActionsToExecute.TryPop(out ddd);
        }
    }

    internal class ConsumerAction
    {
        public Action Action { get; set; }
        public int TimeoutInMillisecs { get; set; }
        //public Task Task { get; set; }
    }

    internal class ActionTimer : System.Timers.Timer
    {
        public int  TaskIndex { get; set; }

        public ActionTimer(double timeoutInMillisecs, int taskIndex)
            :base(timeoutInMillisecs)
        {
            TaskIndex = taskIndex;
        }
    }
}