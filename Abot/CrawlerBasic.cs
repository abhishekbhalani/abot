using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot
{
    public class CrawlerBasic : CrawlerBase
    {

        public override void BeforeCall()
        {
            throw new NotImplementedException();
        }

        public override void AfterCall()
        {
            throw new NotImplementedException();
        }

        public override void BeforeDownloadPageContent()
        {
            throw new NotImplementedException();
        }

        public override void AfterDownloadPageContent()
        {
            throw new NotImplementedException();
        }

        public override bool ShouldMakeCall()
        {
            //Add rules hook here
            throw new NotImplementedException();
        }

        public override bool ShouldDownloadPageContent()
        {
            //Add rules hook here
            throw new NotImplementedException();
        }
    }
}
