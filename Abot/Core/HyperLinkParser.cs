using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using log4net;

namespace Abot.Core
{
    public interface IHyperLinkParser
    {
        /// <summary>
        /// Retrieves the pages links
        /// </summary>
        IList<Uri> GetHyperLinks(Uri pageUri, string pageHtml);
    }

    //public class HtmlParser : IHyperLinkParser
    //{
    //    ILog _logger = LogManager.GetLogger(typeof(HtmlParser));
    //    HtmlDocument _htmlDoc;
    //    Uri _pageUri;

    //    public HtmlParser(Uri pageUri, string html)
    //    {
    //        if (pageUri == null)
    //            throw new ArgumentNullException("uri");

    //        if (html == null)
    //            throw new ArgumentNullException("html");

    //        _htmlDoc = new HtmlDocument();
    //        _htmlDoc.LoadHtml(html);
    //        _pageUri = pageUri;
    //    }

    //    public IList<Uri> GetHyperLinks()
    //    {
    //        HtmlNodeCollection anchorTags;
    //        HtmlNodeCollection areaTags;

    //        anchorTags = _htmlDoc.DocumentNode.SelectNodes("//a[@href]");
    //        areaTags = _htmlDoc.DocumentNode.SelectNodes("//area[@href]");

    //        IList<Uri> hyperlinks = new List<Uri>();
    //        AddToHyperLinks(anchorTags, ref hyperlinks, _pageUri);
    //        AddToHyperLinks(areaTags, ref hyperlinks, _pageUri);

    //        return hyperlinks;
    //    }

    //    public string GetMetaRobotsTag()
    //    {
    //        return GetMetaTagValue("name", "robots");
    //    }

    //    private string GetMetaTagValue(string attributeName, string attributeValue)
    //    {
    //        string value = "";

    //        HtmlNode node = GetMetaTag(attributeName, attributeValue);
    //        if (node != null)
    //        {
    //            value = node.GetAttributeValue("content", "");
    //        }

    //        return value.Trim();
    //    }

    //    private HtmlNode GetMetaTag(string attributeName, string attributeValue)
    //    {
    //        return GetTag(string.Format("meta[translate(@{0}, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{1}']", attributeName, attributeValue));
    //    }

    //    private HtmlNode GetTag(string tagName)
    //    {
    //        HtmlNode node = _htmlDoc.DocumentNode.SelectSingleNode("//" + tagName);

    //        return node;
    //    }

    //    private void AddToHyperLinks(HtmlNodeCollection nodes, ref IList<Uri> hyperlinks, Uri page)
    //    {
    //        if (nodes == null)
    //            return;
            
    //        string hrefValue = "";
    //        foreach (HtmlNode node in nodes)
    //        {
    //            hrefValue = node.Attributes["href"].Value;

    //            try
    //            {
    //                Uri newUri = new Uri(page, hrefValue.Split('#')[0]);
    //                if ( IsSchemeAllowed(newUri) && (!hyperlinks.Contains(newUri)) )
    //                    hyperlinks.Add(newUri);
    //            }
    //            catch(Exception e)
    //            {
    //                _logger.DebugFormat("Error parsing the link [{0}] on page [{1}]", hrefValue, page.AbsoluteUri, e);
    //            }
    //        }
    //    }

    //    private bool IsSchemeAllowed(Uri uri)
    //    {
    //        if ((uri.Scheme == "http") || (uri.Scheme == "https"))
    //            return true;

    //        return false;
    //    }
    //}
}