using Abot.Poco;
using System;
using System.Collections.Generic;

namespace Abot.Core
{
    public interface IHyperLinkParser
    {
        /// <summary>
        /// Parses html to extract hyperlinks, converts each into an absolute url
        /// </summary>
        IEnumerable<Uri> GetLinks(CrawledPage crawledPage);
    }
}
