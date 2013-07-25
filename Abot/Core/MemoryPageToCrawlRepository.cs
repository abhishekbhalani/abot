using Abot.Poco;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot.Core
{
    public class MemoryPageToCrawlRepository : IPagesToCrawlRepository
    {
        ConcurrentQueue<PageToCrawl> _pagesToCrawl = new ConcurrentQueue<PageToCrawl>();
        public void Add(PageToCrawl page)
        {
            _pagesToCrawl.Enqueue(page);
        }

        public Poco.PageToCrawl GetNext()
        {
            PageToCrawl nextItem = null;

            if (_pagesToCrawl.Count > 0)//issue 14: have to check this again since it may have changed since calling this method
                _pagesToCrawl.TryDequeue(out nextItem);

            return nextItem;
        }

        public void Clear()
        {
            _pagesToCrawl = new ConcurrentQueue<PageToCrawl>();
        }

        public int Count()
        {
            return _pagesToCrawl.Count;
        }
    }
}
