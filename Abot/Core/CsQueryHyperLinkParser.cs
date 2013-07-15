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
        Func<string, string> _cleanURLFunc;
        public CSQueryHyperlinkParser()
        {
        }
        public CSQueryHyperlinkParser(Func<string, string> cleanURLFunc)
        {
            _cleanURLFunc = cleanURLFunc;
        }
        protected override string ParserType
        {
            get { return "CsQuery"; }
        }

        protected override IEnumerable<string> GetHrefValues(CrawledPage crawledPage)
        {
            IEnumerable<string> hrefValues = crawledPage.CsQueryDocument.Select("a, area")
            .Elements
            .Select(y => _cleanURLFunc != null ? _cleanURLFunc(y.GetAttribute("href")) : y.GetAttribute("href"))
            .Where(a => !string.IsNullOrWhiteSpace(a));

            return hrefValues;
        }

        protected override string GetBaseHrefValue(CrawledPage crawledPage)
        {
            string baseTagValue = crawledPage.CsQueryDocument.Select("base").Attr("href") ?? "";
            return baseTagValue.Trim();
        }
    }
}