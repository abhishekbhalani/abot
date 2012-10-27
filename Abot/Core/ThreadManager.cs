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
        void DoWork(Action action);

        /// <summary>
        /// Whether there are running threads
        /// </summary>
        bool HasRunningThreads();
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
            lock (_lock)
            {
                int freeThreadIndex = GetFreeThreadIndex();
                while (freeThreadIndex < 0)
                {
                    _logger.Debug("Waiting for a free thread to do work, sleeping 1 sec");
                    System.Threading.Thread.Sleep(1000);
                    freeThreadIndex = GetFreeThreadIndex();
                }

                _logger.DebugFormat("Free thread [{0}] available", freeThreadIndex);

                if (MaxThreads > 1)
                {
                    _threads[freeThreadIndex] = new Thread(new ThreadStart(action));
                    _threads[freeThreadIndex].Start();
                }
                else
                {
                    action.Invoke();
                }
            }
        }

        /// <summary>
        /// Whether there are running threads
        /// </summary>
        public bool HasRunningThreads()
        {
            lock (_lock)
            {
                int threadIndex = 0;
                foreach (Thread thread in _threads)
                {
                    if (thread != null)
                    {
                        if ((thread.ThreadState == ThreadState.Running) || (thread.ThreadState == ThreadState.WaitSleepJoin))
                            return true;
                    }
                    threadIndex++;
                }
            }

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
                    if ((thread == null) || thread.ThreadState != ThreadState.Running)
                    {
                        freeThreadIndex = currentIndex;
                        break;
                    }

                    currentIndex++;
                }
            }
            return freeThreadIndex;;
        }
    }
}
