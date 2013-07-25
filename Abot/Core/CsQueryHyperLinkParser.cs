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
        bool _isRespectMetaRobotsNoFollowEnabled;
        bool _isRespectAnchorRelNoFollowEnabled;
        
        public CSQueryHyperlinkParser()
        {
        }

        public CSQueryHyperlinkParser(bool isRespectMetaRobotsNoFollowEnabled, bool isRespectAnchorRelNoFollowEnabled)
            :this(isRespectMetaRobotsNoFollowEnabled, isRespectAnchorRelNoFollowEnabled, null)
        {
        }

        public CSQueryHyperlinkParser(bool isRespectMetaRobotsNoFollowEnabled, bool isRespectAnchorRelNoFollowEnabled, Func<string, string> cleanURLFunc)
        {
            _isRespectMetaRobotsNoFollowEnabled = isRespectMetaRobotsNoFollowEnabled;
            _isRespectAnchorRelNoFollowEnabled = isRespectAnchorRelNoFollowEnabled;
            _cleanURLFunc = cleanURLFunc;
        }
        
        protected override string ParserType
        {
            get { return "CsQuery"; }
        }

        protected override IEnumerable<string> GetHrefValues(CrawledPage crawledPage)
        {
            if (_isRespectMetaRobotsNoFollowEnabled)
            {
                var robotsMeta = crawledPage.CsQueryDocument["meta[name=robots]"].Attr("content");
                if (robotsMeta != null && robotsMeta.ToLower() == "nofollow")
                {
                    return null;
                }
            }
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