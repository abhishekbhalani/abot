using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot
{
    public interface ICrawler
    {
        void Crawl(Uri uri);
    }

    public abstract class CrawlerBase
    {
        public CrawlerBase()
        {
            //set all default components
        }
        //TODO Where do rules go?
        //TODO Where do events go?
        public abstract void BeforeCall();
        public abstract void AfterCall();

        public abstract void BeforeDownloadPageContent();
        public abstract void AfterDownloadPageContent();

        public abstract bool ShouldMakeCall();//PageToCrawl
        public abstract bool ShouldDownloadPageContent();//RequestedPage
    }
}
