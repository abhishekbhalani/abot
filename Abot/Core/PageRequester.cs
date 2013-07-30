using Abot.Poco;
using log4net;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Abot.Core
{
    public interface IPageRequester
    {
        /// <summary>
        /// Make an http web request to the url and download its content
        /// </summary>
        void MakeRequest(Uri uri, Action<CrawledPage> pageLoadedAction);

        /// <summary>
        /// Make an http web request to the url and download its content based on the param func decision
        /// </summary>
        void MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent, Action<CrawledPage> pageLoadedAction);

        int RunningRequests();
        int IncompleteRequests();
    }

    public class PageRequester : IPageRequester
    {
        static ILog _logger = LogManager.GetLogger(typeof(PageRequester).FullName);

        protected CrawlConfiguration _config;
        protected string _userAgentString;

        int runningCount = 0;
        int incompleteRequests = 0;
        volatile int downloaded = 0;

        HttpClient client = null;
        DateTime startTime;
        public PageRequester(CrawlConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _userAgentString = config.UserAgentString.Replace("@ABOTASSEMBLYVERSION@", Assembly.GetAssembly(this.GetType()).GetName().Version.ToString());
            _config = config;
            //if (_config.HttpServicePointConnectionLimit > 0)
            //    ServicePointManager.DefaultConnectionLimit = 500; //_config.HttpServicePointConnectionLimit;
            ServicePointManager.DefaultConnectionLimit = 500;
            client = BuildRequestObject();

            startTime = DateTime.Now;
            var ts = new ThreadStart(() =>
            {
                while (1 == 1)
                {
                    Console.WriteLine("Crawling Report : " + Convert.ToDouble(downloaded) / (DateTime.Now - startTime).TotalSeconds);
                    Thread.Sleep(5000);
                }
            });
            Thread th = new Thread(ts);
            th.Start();

        }

        public virtual int RunningRequests()
        {
            return runningCount;
        }
        public virtual int IncompleteRequests()
        {
            return incompleteRequests;
        }
        /// <summary>
        /// Make an http web request to the url and download its content
        /// </summary>
        public virtual void MakeRequest(Uri uri, Action<CrawledPage> pageLoadedAction)
        {
            MakeRequest(uri, (x) => new CrawlDecision { Allow = true }, pageLoadedAction);
        }

        /// <summary>
        /// Make an http web request to the url and download its content based on the param func decision
        /// </summary>
        public virtual void MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent, Action<CrawledPage> pageLoadedAction)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            CrawledPage crawledPage = null;

            try
            {
                Interlocked.Increment(ref runningCount);
                Interlocked.Increment(ref incompleteRequests);
                client.GetAsync(uri).ContinueWith(request =>
                {
                    // Console.WriteLine("requested: " + called + " : " + requested);
                    try
                    {
                        crawledPage = new CrawledPage(uri);
                        crawledPage.HttpWebResponse = request.Result;
                        crawledPage.HttpWebRequest = request.Result.RequestMessage;
                        request.Result.EnsureSuccessStatusCode();
                        CrawlDecision shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
                        if (shouldDownloadContentDecision.Allow)
                        {
                            request.Result.Content.ReadAsStringAsync().ContinueWith(content =>
                            {

                                Interlocked.Decrement(ref runningCount);
                                Interlocked.Increment(ref downloaded);
                                if (content != null)
                                {
                                    crawledPage.RawContent = content.Result;
                                    crawledPage.PageSizeInBytes = Encoding.UTF8.GetBytes(crawledPage.RawContent).Length;
                                    pageLoadedAction(crawledPage);
                                }
                                Interlocked.Decrement(ref incompleteRequests);
                            });
                        }
                        else
                        {
                            Interlocked.Decrement(ref runningCount);
                            Interlocked.Decrement(ref incompleteRequests);
                            _logger.DebugFormat("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldDownloadContentDecision.Reason);
                            pageLoadedAction(crawledPage);
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        Interlocked.Decrement(ref runningCount);
                        Interlocked.Decrement(ref incompleteRequests);
                        crawledPage.WebException = e;


                        _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                        _logger.Debug(e);
                    }
                    catch (AggregateException e)
                    {
                        Interlocked.Decrement(ref runningCount);
                        Interlocked.Decrement(ref incompleteRequests);
                        _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                        _logger.Debug(e);
                    }
                    catch (Exception e)
                    {
                        Interlocked.Decrement(ref runningCount);
                        Interlocked.Decrement(ref incompleteRequests);
                        Console.WriteLine(e.Message + " : " + uri.AbsoluteUri);
                        _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                        _logger.Debug(e);
                    }

                });
            }
            catch (HttpRequestException e)
            {
                crawledPage.WebException = e;


                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " : " + uri.AbsoluteUri);
                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                _logger.Debug(e);
            }
        }

        protected virtual HttpClient BuildRequestObject()
        {
            var request = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = _config.IsHttpRequestAutoRedirectsEnabled,
                MaxAutomaticRedirections = _config.HttpRequestMaxAutoRedirects > 0 ? _config.HttpRequestMaxAutoRedirects : 50,
                AutomaticDecompression = _config.IsHttpRequestAutomaticDecompressionEnabled ? DecompressionMethods.GZip | DecompressionMethods.Deflate : DecompressionMethods.None
            });
            request.DefaultRequestHeaders.Add("User-Agent", _userAgentString);
            request.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            //  if (_config.HttpRequestTimeoutInSeconds > 0)
            //     request.Timeout = new TimeSpan(0, 0, _config.HttpRequestTimeoutInSeconds);
            //request.MaxResponseContentBufferSize = 256000;
            return request;
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
                _logger.WarnFormat("Error occurred while downloading content of url {0}", requestUri.AbsoluteUri);
                _logger.Warn(e);
            }

            return rawHtml;
        }
    }
}