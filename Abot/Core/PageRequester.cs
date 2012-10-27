using Abot.Poco;
using log4net;
using System;
using System.IO;
using System.Net;

namespace Abot.Core
{
    public interface IPageRequester
    {
        /// <summary>
        /// Make an http web request to the url and download its content
        /// </summary>
        CrawledPage MakeRequest(Uri uri);

        /// <summary>
        /// Make an http web request to the url and download its content based on the param func decision
        /// </summary>
        CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent);
    }

    public class PageRequester : IPageRequester
    {
        ILog _logger = LogManager.GetLogger(typeof(PageRequester).FullName);

        string _userAgentString;

        public PageRequester(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent) || userAgent.Trim().Length == 0)
                throw new ArgumentNullException("userAgent");

            _userAgentString = userAgent;
        }

        public virtual CrawledPage MakeRequest(Uri uri)
        {
            return MakeRequest(uri, (x) => new CrawlDecision { Should = true });
        }

        public virtual CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            CrawledPage crawledPage = new CrawledPage(uri);

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = 7;
                request.UserAgent = _userAgentString;
                request.Accept = "*/*";

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                crawledPage.WebException = e;

                if (e.Response != null)
                    response = (HttpWebResponse)e.Response;

                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri, e);
            }
            catch (Exception e)
            {
                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri, e);
            }
            finally
            {
                crawledPage.HttpWebRequest = request;

                if (response != null)
                {
                    crawledPage.HttpWebResponse = response;
                    CrawlDecision shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
                    if (shouldDownloadContentDecision.Should)
                    {
                        string rawHtml = GetRawHtml(response, uri);
                        if (!string.IsNullOrEmpty(rawHtml) && !(rawHtml.Trim().Length == 0))
                            crawledPage.RawContent = rawHtml;
                    }
                    else
                    {
                        _logger.DebugFormat("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldDownloadContentDecision.Reason);
                    }
                    response.Close();
                }
            }

            return crawledPage;
        }

        protected virtual string GetRawHtml(HttpWebResponse response, Uri requestUri)
        {
            string rawHtml = "";
            try
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    rawHtml = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                _logger.WarnFormat("Error occurred while downloading content of url {0}", requestUri.AbsoluteUri, e);
            }

            return rawHtml;
        }
    }
}