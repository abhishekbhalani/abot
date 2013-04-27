using log4net;
using System;
using System.Threading;

namespace Abot.Core
{
    /// <summary>
    /// Handles the multithreading implementation details
    /// </summary>
    public interface IThreadManager : IDisposable
    {
        /// <summary>
        /// Max number of threads to use.
        /// </summary>
        int MaxThreads { get; }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// </summary>
        /// <param name="action">The action to perform</param>
        void DoWork(Action action);

        /// <summary>
        /// Whether there are running threads
        /// </summary>
        bool HasRunningThreads();

        /// <summary>
        /// Abort all running threads
        /// </summary>
        void AbortAll();
    }

    public class ManualThreadManager : IThreadManager
    {
        static ILog _logger = LogManager.GetLogger(typeof(ManualThreadManager).FullName);
        bool _abortAllCalled = false;
        int _numberOfRunningThreads = 0;
        int _maxThreads = 0;
        ManualResetEvent _resetEvent = new ManualResetEvent(true);
        Object _locker = new Object();

        public ManualThreadManager(int maxThreads)
        {
            if ((maxThreads > 100) || (maxThreads < 1))
                throw new ArgumentException("MaxThreads must be from 1 to 100");

            _maxThreads = maxThreads;
        }

        /// <summary>
        /// Max number of threads to use
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return _maxThreads;
            }
        }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// </summary>
        public void DoWork(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_abortAllCalled)
                throw new InvalidOperationException("Cannot call DoWork() after AbortAll() or Dispose() have been called.");

            if (MaxThreads > 1)
            {
                _resetEvent.WaitOne();
                lock (_locker)
                {
                    _numberOfRunningThreads++;
                    if (_numberOfRunningThreads >= MaxThreads)
                    {
                        _logger.DebugFormat("Starting another thread, increasing running threads to [{0}].", _numberOfRunningThreads);
                        _resetEvent.Reset();
                    }

                    _logger.DebugFormat("Starting another thread, increasing running threads to [{0}].", _numberOfRunningThreads);
                    new Thread(() => RunActionOnAThread(action)).Start();
                }
            }
            else
            {
                try
                {
                    action.Invoke();
                    _logger.Debug("Action completed successfully.");
                }
                catch (Exception e)
                {
                    _logger.Error("Error occurred while running action.");
                    _logger.Error(e);
                }
            }
        }

        public void AbortAll()
        {
            //Do nothing
            _abortAllCalled = true;
        }

        public void Dispose()
        {
            AbortAll();
        }

        public bool HasRunningThreads()
        {
            return _numberOfRunningThreads > 0;
        }

        private void RunActionOnAThread(Action action)
        {
            try
            {
                action.Invoke();
                _logger.Debug("Action completed successfully.");
            }
            catch (Exception e)
            {
                _logger.Error("Error occurred while running action.");
                _logger.Error(e);
            }
            finally
            {
                lock (_locker)
                {
                    _numberOfRunningThreads--;
                    _logger.DebugFormat("[{0}] threads are running.", _numberOfRunningThreads);
                    if (_numberOfRunningThreads < MaxThreads)
                        _resetEvent.Set();
                }
            }
        }
    }
}