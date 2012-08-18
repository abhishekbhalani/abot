using Abot.Core;
using System;

namespace Abot
{
    public class CrawlerBasic : CrawlerBase
    {
        //public CrawlerBasic()
        //    :this(new ThreadManager(), new FifoScheduler(), new HtmlParser(), new HttpRequester())
        //{
        //}

        public CrawlerBasic(IThreadManager threadManager, IScheduler scheduler, IHyperLinkParser hyperLinkParser, IHttpRequester httpRequester)
           : base(threadManager, scheduler, hyperLinkParser, httpRequester)
        {

        }

        protected override void BeforeCall()
        {
            throw new NotImplementedException();
        }

        protected override bool ShouldMakeCall(PageToCrawl pageToCrawl)
        {
            throw new NotImplementedException();
        }

        protected override void AfterCall()
        {
            throw new NotImplementedException();
        }

        protected override void BeforeDownloadPageContent()
        {
            throw new NotImplementedException();
        }

        protected override bool ShouldDownloadPageContent(CrawledPage crawledPage)
        {
            throw new NotImplementedException();
        }

        protected override void AfterDownloadPageContent()
        {
            throw new NotImplementedException();
        }
    }
}
