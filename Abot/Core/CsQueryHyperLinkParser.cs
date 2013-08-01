using Abot.Poco;
using CsQuery;
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
          //  CsQuery.Config.DomIndexProvider = DomIndexProviders.Simple;
        }

        public CSQueryHyperlinkParser(bool isRespectMetaRobotsNoFollowEnabled, bool isRespectAnchorRelNoFollowEnabled)
            :this(isRespectMetaRobotsNoFollowEnabled, isRespectAnchorRelNoFollowEnabled, null)
        {
          //  CsQuery.Config.DomIndexProvider = DomIndexProviders.Simple;
        }

        public CSQueryHyperlinkParser(bool isRespectMetaRobotsNoFollowEnabled, bool isRespectAnchorRelNoFollowEnabled, Func<string, string> cleanURLFunc)
        {
            _isRespectMetaRobotsNoFollowEnabled = isRespectMetaRobotsNoFollowEnabled;
            _isRespectAnchorRelNoFollowEnabled = isRespectAnchorRelNoFollowEnabled;
            _cleanURLFunc = cleanURLFunc;
            CsQuery.Config.DomIndexProvider = DomIndexProviders.Simple;
        }
        
        protected override string ParserType
        {
            get { return "CsQuery"; }
        }

        protected override string[] GetHrefValues(CrawledPage crawledPage)
        {
            if (_isRespectMetaRobotsNoFollowEnabled)
            {
                var robotsMeta = crawledPage.CsQueryDocument["meta[name=robots]"].Attr("content");
                if (robotsMeta != null && robotsMeta.ToLower() == "nofollow")
                {
                    return null;
                }
            }
            //List<string> hrefs = new List<string>();
            //string tString ="";
            //crawledPage.CsQueryDocument["a, area"].Each((i,obj)=>
            //    {
            //        tString=_cleanURLFunc != null ? _cleanURLFunc(obj.GetAttribute("href")) : obj.GetAttribute("href");
            //        if(!string.IsNullOrWhiteSpace(tString))
            //        {
            //            hrefs.Add(tString);
            //        }
            //    });
            string[] hrefValues = crawledPage.CsQueryDocument.Select("a, area")
            .Elements
            .Select(y => _cleanURLFunc != null ? _cleanURLFunc(y.GetAttribute("href")) : y.GetAttribute("href"))
            .Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();

            return hrefValues;
        }

        protected override string GetBaseHrefValue(CrawledPage crawledPage)
        {
            string baseTagValue = crawledPage.CsQueryDocument.Select("base").Attr("href") ?? "";
            return baseTagValue.Trim();
        }
    }
}