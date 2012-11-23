using log4net;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Abot.Core
{
    public interface IDomainRateLimiter
    {
        void RateLimit(Uri uri);
    }

    public class DomainRateLimiter : IDomainRateLimiter
    {
        static ILog _logger = LogManager.GetLogger(typeof(DomainRateLimiter).FullName);
        ConcurrentDictionary<string, IRateLimiter> _rateLimiterLookup = new ConcurrentDictionary<string, IRateLimiter>();
        long _minMillisecondDelay;

        public DomainRateLimiter(long minMillisecondDelay)
        {
            if (minMillisecondDelay < 1)
                throw new ArgumentException("minMillisecondDelay delay must be at least 1");

            _minMillisecondDelay = minMillisecondDelay;
        }

        public void RateLimit(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            IRateLimiter rateLimiter;
            _rateLimiterLookup.TryGetValue(uri.Authority, out rateLimiter);

            if (rateLimiter == null)
            {
                rateLimiter = new RateLimiter(1, TimeSpan.FromMilliseconds(_minMillisecondDelay));
                _rateLimiterLookup.TryAdd(uri.Authority, rateLimiter);
            }

            Stopwatch timer = Stopwatch.StartNew();
            rateLimiter.WaitToProceed();
            timer.Stop();

            if(timer.ElapsedMilliseconds > 10)
                _logger.DebugFormat("Rate limited [{0}] [{1}] milliseconds", uri.AbsolutePath, timer.ElapsedMilliseconds);
        }
    }
}
