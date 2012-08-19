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
        /// Make an http web request to the url
        /// </summary>
        CrawledPage MakeRequest(Uri uri);
    }

    public class PageRequester : IPageRequester
    {
        ILog _logger = LogManager.GetLogger(typeof(PageRequester).FullName);

        string _userAgentString;

        public PageRequester(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                throw new ArgumentNullException("userAgent");

            _userAgentString = userAgent;
        }

        public virtual CrawledPage MakeRequest(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            if(!IsValidScheme(uri))
                throw new ArgumentException("Invalid uri scheme, must be http or https");

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
                    if (IsContentDownloadable(response))
                    {
                        string rawHtml = GetRawHtml(response, uri);
                        if (!string.IsNullOrWhiteSpace(rawHtml))
                            crawledPage.RawContent = rawHtml;
                    }
                    else
                    {
                        _logger.DebugFormat("Did not download page content for [{0}]", uri.AbsoluteUri);
                    }
                    crawledPage.HttpWebResponse = response;
                    response.Close();
                }
            }

            return crawledPage;
        }

        protected virtual string GetRawHtml(HttpWebResponse response, Uri requestUri)
        {
            if (!IsContentDownloadable(response))
                return "";

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

        protected virtual bool IsValidScheme(Uri uri)
        {
            return uri.Scheme.StartsWith("http");
        }

        protected virtual bool IsContentDownloadable(HttpWebResponse response)
        {
            return response.ContentType.ToLower().Contains("text/html");
        }
    }
}