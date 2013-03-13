using log4net;
using System;
using System.Runtime;

namespace Abot.Core
{
    public interface IMemoryManager : IMemoryMonitor, IDisposable
    {
        bool IsCurrentUsageAbove(int sizeInMb);
        bool IsSpaceAvailable(int sizeInMb);
    }

    public class MemoryManager : IMemoryManager
    {
        static ILog _logger = LogManager.GetLogger(typeof(MemoryManager).FullName);
        IMemoryMonitor _memoryMonitor;

        public MemoryManager(IMemoryMonitor memoryMonitor)
        {
            if (memoryMonitor == null)
                throw new ArgumentNullException("memoryMonitor");

            _memoryMonitor = memoryMonitor;
        }

        public virtual bool IsCurrentUsageAbove(int sizeInMb)
        {
            return GetCurrentUsageInMb() > sizeInMb;
        }

        public virtual bool IsSpaceAvailable(int sizeInMb)
        {
            if (sizeInMb < 1)
                return true;

            bool isAvailable = true;

            MemoryFailPoint _memoryFailPoint = null;
            try
            {
                _memoryFailPoint = new MemoryFailPoint(sizeInMb);
            }
            catch (InsufficientMemoryException e)
            {
                isAvailable = false;
            }
            finally
            {
                if (_memoryFailPoint != null)
                    _memoryFailPoint.Dispose();
            }

            return isAvailable;
        }

        public virtual int GetCurrentUsageInMb()
        {
            return _memoryMonitor.GetCurrentUsageInMb();
        }

        public void Dispose()
        {
            _memoryMonitor.Dispose();
        }
    }
}
