using System;
using System.Net;

namespace Abot.Poco
{
    public class CrawledPage : PageToCrawl
    {
        public CrawledPage(Uri uri)
            : base(uri)
        {
            RawContent = "";
        }

        /// <summary>
        /// The raw content of the request
        /// </summary>
        public string RawContent { get; set; }

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

        public override string ToString()
        {
            if(HttpWebResponse == null)
                return Uri.AbsoluteUri;
            else
                return string.Format("{0}[{1}]", Uri.AbsoluteUri, (int)HttpWebResponse.StatusCode);
        }
    }
}
