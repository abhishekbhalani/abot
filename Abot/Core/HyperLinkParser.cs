using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Generic;

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

            HtmlNodeCollection anchorTags = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            HtmlNodeCollection areaTags = htmlDoc.DocumentNode.SelectNodes("//area[@href]");

            IList<Uri> hyperlinks = new List<Uri>();
            AddToHyperLinks(anchorTags, ref hyperlinks, pageUri);
            AddToHyperLinks(areaTags, ref hyperlinks, pageUri);

            return hyperlinks;
        }


        private void AddToHyperLinks(HtmlNodeCollection nodes, ref IList<Uri> hyperlinks, Uri page)
        {
            if (nodes == null)
                return;
            
            string hrefValue = "";
            foreach (HtmlNode node in nodes)
            {
                hrefValue = node.Attributes["href"].Value;

                try
                {
                    Uri newUri = new Uri(page, hrefValue.Split('#')[0]);
                    if ( IsSchemeAllowed(newUri) && (!hyperlinks.Contains(newUri)) )
                        hyperlinks.Add(newUri);
                }
                catch(Exception e)
                {
                    _logger.DebugFormat("Error parsing the link [{0}] on page [{1}]", hrefValue, page.AbsoluteUri, e);
                }
            }
        }

        private bool IsSchemeAllowed(Uri uri)
        {
            if ((uri.Scheme == "http") || (uri.Scheme == "https"))
                return true;

            return false;
        }
    }
}