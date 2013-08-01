using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;

namespace Abot.Core
{
    /// <summary>
    /// Handles parsing hyperlinks out of the raw html
    /// </summary>
    public interface IHyperLinkParser
    {
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
        public virtual IEnumerable<Uri> GetLinks(CrawledPage crawledPage)
        {
            CheckParams(crawledPage);

            //Stopwatch timer = Stopwatch.StartNew();
            var uris = GetUris(crawledPage, GetHrefValues(crawledPage));

            // timer.Stop();
            //            _logger.DebugFormat("{0} parsed links from [{1}] in [{2}] milliseconds", ParserType, crawledPage.Uri, timer.ElapsedMilliseconds);

            return uris;
        }

        #region Abstract

        protected abstract string ParserType { get; }

        protected abstract string[] GetHrefValues(CrawledPage crawledPage);

        protected abstract string GetBaseHrefValue(CrawledPage crawledPage);

        #endregion

        protected virtual void CheckParams(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                throw new ArgumentNullException("crawledPage");
        }
        protected virtual IEnumerable<Uri> GetUris(CrawledPage crawledPage, string[] hrefValues)
        {
            List<Uri> uris = new List<Uri>();
            if (hrefValues == null || hrefValues.Count() < 1)
                return uris;

            //Use the uri of the page that actually responded to the request instead of crawledPage.Uri (Issue 82).
            //Using HttpWebRequest.Address instead of HttpWebResonse.ResponseUri since this is the best practice and mentioned on http://msdn.microsoft.com/en-us/library/system.net.httpwebresponse.responseuri.aspx

            Uri uriToUse = crawledPage.HttpWebRequest.RequestUri ?? crawledPage.Uri;

            //If html base tag exists use it instead of page uri for relative links
            string baseHref = GetBaseHrefValue(crawledPage);
            if (!string.IsNullOrEmpty(baseHref))
            {
                try
                {
                    uriToUse = new Uri(baseHref);
                }
                catch { }
            }

            for (int x = 0; x < hrefValues.Count(); x++)
            {
                try
                {
                    if (hrefValues[x].Contains('#'))
                    {
                        hrefValues[x] = hrefValues[x].Substring(0, hrefValues[x].IndexOf('#'));
                    }
                    Uri newUri = new Uri(uriToUse, hrefValues[x]);

                   // if (!uris.Contains(newUri))
                        uris.Add(newUri);
                }
                catch (Exception e)
                {
                    _logger.DebugFormat("Could not parse link [{0}] on page [{1}]", hrefValues[x], crawledPage.Uri);
                    _logger.Debug(e);
                }
            }

            return uris.Distinct();
        }
    }
}