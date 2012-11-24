using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;

namespace Abot.Core
{
    public interface IScheduler
    {
        /// <summary>
        /// Count of remaining items that are currently scheduled
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Schedules the param to be crawled
        /// </summary>
        void Add(PageToCrawl page);

        /// <summary>
        /// Gets the next page to crawl
        /// </summary>
        PageToCrawl GetNext();
    }

    public class FifoScheduler : IScheduler
    {
        static ILog _logger = LogManager.GetLogger(typeof(FifoScheduler).FullName);
        Queue<PageToCrawl> _pagesToCrawl = new Queue<PageToCrawl>();
        Object locker = new Object();

        /// <summary>
        /// Count of remaining items that are currently scheduled
        /// </summary>
        public int Count
        {
            get
            {
                lock (locker)
                {
                    return _pagesToCrawl.Count;
                }
            }
        }

        /// <summary>
        /// Schedules the param to be crawled in a FIFO fashion
        /// </summary>
        public void Add(PageToCrawl page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            _logger.DebugFormat("Scheduling for crawl [{0}]", page.Uri.AbsoluteUri);

            lock (locker)
            {
                _pagesToCrawl.Enqueue(page);
            }
        }

        /// <summary>
        /// Gets the next page to crawl
        /// </summary>
        public PageToCrawl GetNext()
        {
            PageToCrawl nextItem = null;
            lock (locker)
            {
                if(_pagesToCrawl.Count > 0)//issue 14: have to check this again since it may have changed since calling this method
                    nextItem = _pagesToCrawl.Dequeue();
            }

            return nextItem;
        }
    }
}
