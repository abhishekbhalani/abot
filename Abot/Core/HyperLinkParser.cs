using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Generic;

namespace Abot.Core
{
    public interface IHyperLinkParser
    {
        /// <summary>
        /// Parses html to extract hyperlinks, converts each into an absolute url
        /// </summary>
        IEnumerable<Uri> GetLinks(Uri pageUri, string pageHtml);
    }

    public class HyperLinkParser : IHyperLinkParser
    {
        ILog _logger = LogManager.GetLogger(typeof(HyperLinkParser));

        /// <summary>
        /// Parses html to extract anchor and area tag href values
        /// </summary>
        public IEnumerable<Uri> GetLinks(Uri pageUri, string pageHtml)
        {
            if (pageUri == null)
                throw new ArgumentNullException("pageUri");

            if (pageHtml == null)
                throw new ArgumentNullException("pageHtml");

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            HtmlNodeCollection aTags = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            HtmlNodeCollection areaTags = htmlDoc.DocumentNode.SelectNodes("//area[@href]");

            Uri uriToUse = pageUri;

            //If html base tag exists use it instead of page uri for relative links
            string baseHref = GetBaseTagHref(htmlDoc);
            if (!string.IsNullOrEmpty(baseHref))
            {
                try
                {
                    uriToUse = new Uri(baseHref);
                }
                catch { }
            }

            List<Uri> hyperlinks = GetLinks(aTags, uriToUse);
            hyperlinks.AddRange(GetLinks(areaTags, uriToUse));

            return hyperlinks;
        }

        private List<Uri> GetLinks(HtmlNodeCollection nodes, Uri page)
        {
            List<Uri> uris = new List<Uri>();

            if (nodes == null)
                return uris;

            string hrefValue = "";
            foreach (HtmlNode node in nodes)
            {
                hrefValue = node.Attributes["href"].Value;

                try
                {
                    Uri newUri = new Uri(page, hrefValue.Split('#')[0]);
                    if (ShouldIncludedLink(newUri) && (!uris.Contains(newUri)))
                        uris.Add(newUri);
                }
                catch (Exception e)
                {
                    _logger.DebugFormat("Could not parse link [{0}] on page [{1}]", hrefValue, page.AbsoluteUri, e);
                }
            }

            return uris;
        }

        protected virtual bool ShouldIncludedLink(Uri uri)
        {
            return ((uri.Scheme == "http") || (uri.Scheme == "https"));
        }

        private string GetBaseTagHref(HtmlDocument doc)
        {
            string hrefValue = "";
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//base");

            //Must use node.InnerHtml instead of node.InnerText since "aaa<br />bbb" will be returned as "aaabbb"
            if (node != null)
                hrefValue = node.GetAttributeValue("href", "").Trim();

            return hrefValue;
        }
    }
}