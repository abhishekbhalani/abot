using System;
using System.Net;

namespace Abot
{
    public class CrawledPage : PageToCrawl
    {
        public CrawledPage(Uri uri)
            : base(uri)
        {
            Content = "";
        }

        /// <summary>
        /// The raw content of the request
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Web request sent to the server
        /// </summary>
        public HttpWebRequest HttpWebRequest { get; set; }

        /// <summary>
        /// Web response from the server. NOTE: The Close() method has been called before setting this property.
        /// </summary>
        public HttpWebResponse HttpWebResponse { get; set; }

        /// <summary>
        /// The web exception that occurred during the crawl
        /// </summary>
        public WebException WebException { get; set; }

        ///// <summary>
        ///// Html parser that is loaded with this page's content.
        ///// </summary>
        //public IHtmlParser HtmlParser { get; set; }

        ///// <summary>
        ///// Http requester used to make the http request for this page.
        ///// </summary>
        //public IHttpRequester HttpRequester { get; set; }
    }
}
