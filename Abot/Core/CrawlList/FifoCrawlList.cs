using Abot.Poco;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Abot.Core
{
    public class FifoCrawlList : ICrawlList
    {
        static ILog _logger = LogManager.GetLogger(typeof(FifoCrawlList).FullName);
        ConcurrentQueue<PageToCrawl> _pagesToCrawl = new ConcurrentQueue<PageToCrawl>();
        //HashSet<string> _visitedUris = new HashSet<string>();
        //ReaderWriterLockSlim _visitedLock = new ReaderWriterLockSlim();
        ConcurrentDictionary<string, byte> _visitedUris = new ConcurrentDictionary<string, byte>();
         
        bool _allowUriRecrawling = false;

        public FifoCrawlList(bool allowUriRecrawling)
        {
            _allowUriRecrawling = allowUriRecrawling;
        }

        /// <summary>
        /// Count of remaining items that are currently scheduled
        /// </summary>
        public int Count
        {
            get
            {
                return _pagesToCrawl.Count;
            }
        }

        /// <summary>
        /// Schedules the param to be crawled in a FIFO fashion
        /// </summary>
        public void Add(PageToCrawl page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            if (_allowUriRecrawling)
            {
                _pagesToCrawl.Enqueue(page);
            }
            else
            {
                if (_visitedUris.TryAdd(page.Uri.AbsoluteUri, 0))
                {
                    _pagesToCrawl.Enqueue(page);
                }
            }
        }

        /// <summary>
        /// Gets the next page to crawl
        /// </summary>
        public PageToCrawl GetNext()
        {
            PageToCrawl nextItem = null;

            if(_pagesToCrawl.Count > 0)//issue 14: have to check this again since it may have changed since calling this method
                _pagesToCrawl.TryDequeue(out nextItem);

            return nextItem;
        }
    }
}
