using Abot.Poco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abot.Core
{
    /// <summary>
    /// Parser that uses CsQuery https://github.com/jamietre/CsQuery to parse page links
    /// </summary>
    public class CSQueryHyperlinkParser : HyperLinkParser
    {
        protected override string ParserType
        {
            get { return "CsQuery"; }
        }

        protected override CrawledPage GetCrawledWebPage(Uri pageUri, string pageHtml)
        {
            return new CrawledPage(pageUri) { RawContent = pageHtml };
        }

        protected override IEnumerable<string> GetHrefValues(CrawledPage crawledPage)
        {
            IEnumerable<string> hrefValues = crawledPage.CsQueryDocument.Select("a, area")
            .Elements
            .Where(a => !string.IsNullOrWhiteSpace(a.GetAttribute("href")))
            .Select(y => y.GetAttribute("href"));

            return hrefValues;
        }

        protected override string GetBaseHrefValue(CrawledPage crawledPage)
        {
            string baseTagValue = crawledPage.CsQueryDocument.Select("base").Attr("href") ?? "";
            return baseTagValue.Trim();
        }
    }
}