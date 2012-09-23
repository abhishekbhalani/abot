using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abot.Core
{
    public interface IHyperLinkParser
    {
        IEnumerable<Uri> GetHyperLinks(Uri pageUri, string pageHtml);
    }

    public class HyperLinkParser : IHyperLinkParser
    {
        ILog _logger = LogManager.GetLogger(typeof(HyperLinkParser));

        public IEnumerable<Uri> GetHyperLinks(Uri pageUri, string pageHtml)
        {
            if (pageUri == null)
                throw new ArgumentNullException("pageUri");

            if (pageHtml == null)
                throw new ArgumentNullException("pageHtml");

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            IEnumerable<HtmlNode> anchorTags = htmlDoc.DocumentNode.SelectNodes("//a[@href]").Concat(htmlDoc.DocumentNode.SelectNodes("//area[@href]"));

            List<Uri> hyperlinks = new List<Uri>();

            if (anchorTags == null)
                return hyperlinks;

            string hrefValue = "";
            foreach (HtmlNode node in anchorTags)
            {
                hrefValue = node.Attributes["href"].Value;
                try
                {
                    Uri newUri = new Uri(pageUri, hrefValue.Split('#')[0]);
                    if (IsUriValid(newUri) && (!hyperlinks.Contains(newUri)))
                        hyperlinks.Add(newUri);
                }
                catch (Exception e)
                {
                    _logger.DebugFormat("Error parsing the link [{0}] on page [{1}]", hrefValue, pageUri.AbsoluteUri, e);
                }
            }

            return hyperlinks;
        }

        protected virtual bool IsUriValid(Uri uri)
        {
            return ((uri.Scheme == "http") || (uri.Scheme == "https"));
        }
    }
}