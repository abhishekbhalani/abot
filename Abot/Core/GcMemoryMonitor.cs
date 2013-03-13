using log4net;
using System;

namespace Abot.Core
{
    public interface IMemoryMonitor
    {
        int GetCurrentUsageInMb();
    }

    public class GcMemoryMonitor : IMemoryMonitor
    {
        static ILog _logger = LogManager.GetLogger(typeof(GcMemoryMonitor).FullName);

        public virtual int GetCurrentUsageInMb()
        {
            int currentUsageInMb = Convert.ToInt32(GC.GetTotalMemory(false) / 1024);

            _logger.DebugFormat("GC reporting [{0}]mb currently thought to be allocated", currentUsageInMb);

            return currentUsageInMb;       
        }
    }
}
