using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Abot.Core
{
    public interface IHyperLinkParser
    {
        /// <summary>
        /// Parses html to extract hyperlinks, converts each into an absolute url
        /// </summary>
        IEnumerable<Uri> GetLinks(Uri pageUri, string pageHtml);

        /// <summary>
        /// Parses html to extract hyperlinks, converts each into an absolute url
        /// </summary>
        IEnumerable<Uri> GetLinks(CrawledPage crawledPage);
    }

    public abstract class HyperLinkParser : IHyperLinkParser
    {
        ILog _logger = LogManager.GetLogger(typeof(HyperLinkParser));

        /// <summary>
        /// Parses html to extract hyperlinks, converts each into an absolute url
        /// </summary>
        public virtual IEnumerable<Uri> GetLinks(Uri pageUri, string pageHtml)
        {
            if (pageUri == null)
                throw new ArgumentNullException("pageUri");

            if (pageHtml == null)
                throw new ArgumentNullException("pageHtml");

            return GetLinks(GetCrawledWebPage(pageUri, pageHtml));
        }

        /// <summary>
        /// Parses html to extract hyperlinks, converts each into an absolute url
        /// </summary>
        public virtual IEnumerable<Uri> GetLinks(CrawledPage crawledPage)
        {
            CheckParams(crawledPage);

            Stopwatch timer = Stopwatch.StartNew();

            List<Uri> uris = GetUris(crawledPage, GetHrefValues(crawledPage));
            
            timer.Stop();
            _logger.DebugFormat("{0} parsed links from [{1}] in [{2}] milliseconds", ParserType, crawledPage.Uri, timer.ElapsedMilliseconds);

            return uris;
        }

        #region Abstract

        protected abstract string ParserType { get; }

        protected abstract CrawledPage GetCrawledWebPage(Uri pageUri, string pageHtml);

        protected abstract IEnumerable<string> GetHrefValues(CrawledPage crawledPage);

        protected abstract string GetBaseHrefValue(CrawledPage crawledPage);

        #endregion

        protected virtual void CheckParams(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                throw new ArgumentNullException("crawledPage");
        }

        protected virtual List<Uri> GetUris(CrawledPage crawledPage, IEnumerable<string> hrefValues)
        {
            List<Uri> uris = new List<Uri>();
            if (hrefValues == null || hrefValues.Count() < 1)
                return uris;

            //If html base tag exists use it instead of page uri for relative links
            Uri uriToUse = crawledPage.Uri;
            string baseHref = GetBaseHrefValue(crawledPage);
            if (!string.IsNullOrEmpty(baseHref))
            {
                try
                {
                    uriToUse = new Uri(baseHref);
                }
                catch { }
            }

            string href = "";
            foreach (string hrefValue in hrefValues)
            {
                try
                {
                    href = hrefValue.Split('#')[0];
                    Uri newUri = new Uri(uriToUse, href);

                    if (!uris.Contains(newUri))
                        uris.Add(newUri);
                }
                catch (Exception e)
                {
                    _logger.DebugFormat("Could not parse link [{0}] on page [{1}]", hrefValue, crawledPage.Uri, e);
                }
            }

            return uris;
        }
    }
}