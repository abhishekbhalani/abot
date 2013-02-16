using CsQuery;
using HtmlAgilityPack;
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
        /// The Html Agility Pack (http://htmlagilitypack.codeplex.com/) document that can be used to retrieve/modify html elements on the crawled page.
        /// </summary>
        public HtmlDocument HtmlDocument { get; set; }

        /// <summary>
        /// CsQuery (https://github.com/jamietre/CsQuery) document that can be used to retrieve/modify html elements on the crawled page.
        /// </summary>
        public CQ CsQueryDocument { get; set; }

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

        /// <summary>
        /// The actual byte size of the page's raw content. This property is due to the Content-length header being untrustable.
        /// </summary>
        public long PageSizeInBytes { get; set; }
    }
}
