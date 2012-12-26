using System;

namespace Abot.Core
{
    public interface IProcessMemoryLimiter
    {
        int GetMemoryBeingUsedInMb();
        bool IsOverLimit();
    }

    public class ProcessMemoryLimiter : IProcessMemoryLimiter
    {
        int _limitInMb = 0;

        public ProcessMemoryLimiter(int limitInMb)
        {
            if (limitInMb < 0)
                throw new ArgumentException("limitInMb must be positive");

            _limitInMb = limitInMb;
        }

        public bool IsOverLimit()
        {
            if(_limitInMb < 1)
                return false;

            return _limitInMb > GetMemoryBeingUsedInMb();
        }

        public virtual int GetMemoryBeingUsedInMb()
        {
            throw new NotImplementedException();
        }
    }
}
