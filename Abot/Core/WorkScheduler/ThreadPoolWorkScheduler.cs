using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Abot.Core
{
    public class ThreadPoolWorkScheduler : IWorkScheduler
    {
        static ILog _logger = LogManager.GetLogger(typeof(ThreadPoolWorkScheduler));

        /// <summary>
        /// Maximum number of concurrently running tasks allowed.
        /// </summary>
        public int MaxConcurrentTasks
        {
            get;
            private set;
        }

        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        object _freeTaskLock = new object();
        int _freeTaskCount;

        /// <summary>
        /// Create a new work scheduler that will use Tasks to handle concurrency.
        /// </summary>
        /// <param name="maxConcurrentTasks">The maximum number of concurrently running tasks allowed</param>
        public ThreadPoolWorkScheduler(int maxConcurrentTasks)
        {
            if (maxConcurrentTasks <= 0)
                throw new ArgumentException("Max concurrent tasks must be greater than 0.", "maxConcurrentTasks");

            _freeTaskCount = maxConcurrentTasks;
            MaxConcurrentTasks = maxConcurrentTasks;
        }

        /// <summary>
        /// Wait for a task to become available and then perform the specified action.
        /// </summary>
        public void DoWork(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_cancellationTokenSource.IsCancellationRequested)
                throw new InvalidOperationException("Cannot call DoWork() after AbortAll() or Dispose() have been called.");


            //Spin until we can create a new task without exceeding the limit
            while (true)
            {
                if (_freeTaskCount > 0)
                {
                    lock (_freeTaskLock)
                    {
                        if (_freeTaskCount > 0)
                        {
                            _freeTaskCount--;
                            break;
                        }
                    }
                }

                //Yield so that we don't starve other threads
                Thread.Sleep(0);
            }

            _logger.DebugFormat("Starting up a task on thread id {0}.", Thread.CurrentThread.ManagedThreadId);

            Task workTask = new Task(action, _cancellationTokenSource.Token);
            workTask.ContinueWith(ReleaseTask);
            workTask.Start();
        }

        /// <summary>
        /// Whether there are any tasks currently executing
        /// </summary>
        public bool HasRunningJobs()
        {
            return _freeTaskCount < MaxConcurrentTasks;
        }

        /// <summary>
        /// Stop all running tasks.
        /// </summary>
        public void AbortAll()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private void ReleaseTask(Task t)
        {
            lock (_freeTaskLock)
            {
                _freeTaskCount++;
            }

            _logger.Debug("Task complete");
        }
    }
}
