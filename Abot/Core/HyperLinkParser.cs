using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Generic;

namespace Abot.Core
{
    public interface IHyperLinkParser
    {
        IEnumerable<Uri> GetLinks(Uri pageUri, string pageHtml);
    }

    public class HyperLinkParser : IHyperLinkParser
    {
        ILog _logger = LogManager.GetLogger(typeof(HyperLinkParser));

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

            List<Uri> hyperlinks = GetHyperLinks(aTags, pageUri);
            hyperlinks.AddRange(GetHyperLinks(areaTags, pageUri));

            return hyperlinks;
        }

        private List<Uri> GetHyperLinks(HtmlNodeCollection nodes, Uri page)
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
    }
}