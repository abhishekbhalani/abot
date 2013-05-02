using System;
using System.Threading;
using System.Threading.Tasks;

namespace Abot.Core
{
    /// <summary>
    /// A ThreadManager implementation that will use tpl Tasks to handle concurrency.
    /// </summary>
    public class TaskThreadManager : ThreadManager
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public TaskThreadManager(int maxConcurrentTasks)
            :base(maxConcurrentTasks)
        {
        }

        public override void AbortAll()
        {
            base.AbortAll();
            _cancellationTokenSource.Cancel();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
        }

        protected override void RunActionOnDedicatedThread(Action action)
        {
            Task.Factory.StartNew(() => RunAction(action), _cancellationTokenSource.Token);
        }
    }
}
