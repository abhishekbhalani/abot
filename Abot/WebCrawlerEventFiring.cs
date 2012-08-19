using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot
{
    public interface IWebCrawlerEventFiring : IWebCrawler
    {
        
    }

    class WebCrawlerEventFiring : IWebCrawlerEventFiring
    {
        ///// <summary>
        ///// Asynchronous event that is fired before a page is crawled.
        ///// </summary>
        //event EventHandler<PageCrawlStartingArgs> PageCrawlStarting;

        ///// <summary>
        ///// Asynchronous event that is fired when an individual page has been crawled.
        ///// </summary>
        //event EventHandler<PageCrawlCompletedArgs> PageCrawlCompleted;
    }
}
