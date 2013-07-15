using Abot.Poco;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Abot.Core
{
    /// <summary>
    /// Handles managing the priority of what pages need to be crawled
    /// </summary>
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
        /// Schedules the param to be crawled
        /// </summary>
        void Add(IEnumerable<PageToCrawl> pages);

        /// <summary>
        /// Gets the next page to crawl
        /// </summary>
        PageToCrawl GetNext();

        /// <summary>
        /// Clear all currently scheduled pages
        /// </summary>
        void Clear();

    }

    public class FifoScheduler : IScheduler
    {
        static ILog _logger = LogManager.GetLogger(typeof(FifoScheduler).FullName);
        ICrawledUrlRepository _urlRepository;
        IPagesToCrawlRepository _pagesToCrawlRepository;
        bool _allowUriRecrawling = false;
        
        
        public FifoScheduler(bool allowUriRecrawling = false, ICrawledUrlRepository urlRepository = null, IPagesToCrawlRepository pagesRespository = null)
        {

            _allowUriRecrawling = allowUriRecrawling;
            if (allowUriRecrawling == false)
            {
                _urlRepository = urlRepository ?? new MemoryUrlRepository();
            }
            _pagesToCrawlRepository = pagesRespository ?? new MemoryPageToCrawlRepository();
        }

        /// <summary>
        /// Count of remaining items that are currently scheduled
        /// </summary>
        public int Count
        {
            get
            {
                return _pagesToCrawlRepository.Count();
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
                //_logger.DebugFormat("Scheduling for crawl [{0}]", page.Uri.AbsoluteUri);
                _pagesToCrawlRepository.Add(page);
            }
            else
            {
                if (_urlRepository.AddIfNew(page.Uri))
                {
                    //_logger.DebugFormat("Scheduling for crawl [{0}]", page.Uri.AbsoluteUri);
                    _pagesToCrawlRepository.Add(page);
                }
            }
        }

        public void Add(IEnumerable<PageToCrawl> pages)
        {
            if (pages == null)
                throw new ArgumentNullException("pages");

            foreach (PageToCrawl page in pages)
                Add(page);
        }

        /// <summary>
        /// Gets the next page to crawl
        /// </summary>
        public PageToCrawl GetNext()
        {
            PageToCrawl nextItem = _pagesToCrawlRepository.GetNext();

            return nextItem;
        }

        /// <summary>
        /// Clear all currently scheduled pages
        /// </summary>
        public void Clear()
        {
            _pagesToCrawlRepository.Clear();
        }
    }
}
