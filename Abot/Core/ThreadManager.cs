using log4net;
using System;
using System.Threading;

namespace Abot.Core
{
    public interface IThreadManager
    {
        /// <summary>
        /// Max number of threads to use
        /// </summary>
        int MaxThreads { get; }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// </summary>
        /// <param name="action">The action to perform</param>
        void DoWork(Action action);

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// </summary>
        /// <param name="action">The action to perform</param>
        /// <param name="timeoutInMilliSecs">The amount of time to allow the action to perform. If above this threshold will abort the thread.</param>
        void DoWork(Action action, int timeoutInMilliSecs);

        /// <summary>
        /// Whether there are running threads
        /// </summary>
        bool HasRunningThreads();

        /// <summary>
        /// Abort all running threads
        /// </summary>
        void AbortAll();
    }

    public class ThreadManager : IThreadManager
    {
        static ILog _logger = LogManager.GetLogger(typeof(ThreadManager).FullName);
        object _lock = new object();
        Thread[] _threads = new Thread[10];

        public ThreadManager(int maxThreads)
        {
            if ((maxThreads > 100) || (maxThreads < 1))
                throw new ArgumentException("MaxThreads must be from 1 to 100");
            else
                _threads = new Thread[maxThreads];
        }

        /// <summary>
        /// Max number of threads to use
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return _threads.Length;
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
        /// Will perform the action asynchrously on a seperate thread
        /// </summary>
        public void DoWork(Action action, int timeoutInMilliSecs)
        {
            lock (_lock)
            {
                int freeThreadIndex = GetFreeThreadIndex();
                while (freeThreadIndex < 0)
                {
                    _logger.Debug("Waiting for a free thread to do work, sleeping 100 millisec");
                    System.Threading.Thread.Sleep(100);
                    freeThreadIndex = GetFreeThreadIndex();
                }

                if (MaxThreads > 1)
                {
                    _threads[freeThreadIndex] = new Thread(new ThreadStart(action));
                    _logger.DebugFormat("Doing work on thread Index:[{0}] Id[{1}]", freeThreadIndex, _threads[freeThreadIndex].ManagedThreadId);
                    _threads[freeThreadIndex].Start();

                    if (timeoutInMilliSecs > 0)
                    {
                        //Have to create an instance of timer and dispose it so its declared
                        Timer timer = new Timer(o => DoNothing(), null, int.MaxValue, Timeout.Infinite);
                        timer.Dispose();

                        //Use the declared reference to timer and pass it into the TimeOutThread method so it can be properly disposed of after 1 use
                        timer = new Timer(o => TimeOutThread(_threads[freeThreadIndex], timeoutInMilliSecs, timer), null, timeoutInMilliSecs, Timeout.Infinite);
                    }
                }
                else
                {
                    action.Invoke();
                }
            }
        }

        public void AbortAll()
        {
            _logger.Debug("Aborting all threads");
            lock (_lock)
            {
                foreach (Thread thread in _threads)
                {
                    if (thread != null)
                        thread.Abort();
                }
            }
        }

        private object DoNothing()
        {
            return null;
        }

        /// <summary>
        /// Whether there are running threads
        /// </summary>
        public bool HasRunningThreads()
        {
            lock (_lock)
            {
                for(int i = 0; i < _threads.Length; i++)
                {
                    if (_threads[i] == null)
                    {
                        _logger.DebugFormat("Thread Null Index:[{0}]", i);
                    }
                    else if (_threads[i].IsAlive)
                    {
                        _logger.DebugFormat("Thread Is Running Index:[{0}] Id:[{1}] State:[{2}]", i, _threads[i].ManagedThreadId, _threads[i].ThreadState);
                        return true;
                    }
                    else
                    {
                        _logger.DebugFormat("Thread Not Running Index:[{0}] Id:[{1}] State:[{2}]", i, _threads[i].ManagedThreadId, _threads[i].ThreadState);
                    }
                }
            }

            _logger.DebugFormat("No Threads Running!!");
            return false;
        }

        private int GetFreeThreadIndex()
        {
            int freeThreadIndex = -1;
            int currentIndex = 0;
            lock (_lock)
            {
                foreach (Thread thread in _threads)
                {
                    if ((thread == null) || !thread.IsAlive)
                    {
                        freeThreadIndex = currentIndex;
                        break;
                    }

                    currentIndex++;
                }
            }
            return freeThreadIndex;;
        }

        private void TimeOutThread(Thread thread, int timeoutInMillisec, Timer timer)
        {
            timer.Dispose();
            if (thread == null || !thread.IsAlive)
                return;

            _logger.WarnFormat("Thread Id[{0}] being aborted after [{1}] millisecs timeout", thread.ManagedThreadId, timeoutInMillisec);
            thread.Abort();
        }
    }
}
