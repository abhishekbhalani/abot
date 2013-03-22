using System;

namespace Abot.Core
{
    public interface IWorkScheduler : IDisposable
    {
        /// <summary>
        /// Schedule some work to be completed
        /// </summary>
        void DoWork(Action action);

        /// <summary>
        /// Whether there are running jobs
        /// </summary>
        bool HasRunningJobs();

        /// <summary>
        /// Abort all running tasks
        /// </summary>
        void AbortAll();
    }
}
