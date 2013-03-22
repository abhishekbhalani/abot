using Abot.Poco;
using System;

namespace Abot.Core
{
    public interface IPageFetcher
    {
        /// <summary>
        /// Make an http web request to the url and download its content
        /// </summary>
        CrawledPage FetchPage(Uri uri);

        /// <summary>
        /// Make an http web request to the url and download its content based on the param func decision
        /// </summary>
        CrawledPage FetchPage(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent);
    }
}
