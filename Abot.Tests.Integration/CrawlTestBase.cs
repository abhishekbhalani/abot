using log4net;
using System.Collections.Generic;
using System.Linq;

namespace Abot.Tests.Integration
{
    public abstract class CrawlTestBase
    {
        private static ILog _logger = LogManager.GetLogger(typeof(CrawlTestBase).FullName);
        protected abstract List<PageResult> GetExpectedCrawlResult();

        protected void PrintDescrepancies(List<PageResult> actualCrawlResult)
        {
            List<Discrepancy> allDescrepancies = GetDescrepancies(actualCrawlResult);
            if (allDescrepancies.Count < 1)
            {
                _logger.Info("No discrepancies between expected and actual results");
                return;
            }

            IEnumerable<Discrepancy> missingPages = allDescrepancies.Where(d => d.DiscrepencyType == DiscrepencyType.MissingPageFromResult);
            IEnumerable<Discrepancy> unexpectedPages = allDescrepancies.Where(d => d.DiscrepencyType == DiscrepencyType.UnexpectedPageInResult);
            IEnumerable<Discrepancy> unexpectedHttpStatusPages = allDescrepancies.Where(d => d.DiscrepencyType == DiscrepencyType.UnexpectedHttpStatus);

            foreach(Discrepancy descrepancy in missingPages)
                _logger.InfoFormat("Missing:[0][1]", descrepancy.Expected.Url, descrepancy.Expected.HttpStatusCode);
            foreach(Discrepancy descrepancy in unexpectedHttpStatusPages)
                _logger.InfoFormat("Unexpected Http Status: [0] Expected:[1] Actual:[2]", descrepancy.Expected.Url, descrepancy.Expected.HttpStatusCode, descrepancy.Actual.HttpStatusCode);
            foreach(Discrepancy descrepancy in unexpectedPages)
                _logger.InfoFormat("Unexpected Page:[0][1]", descrepancy.Actual.Url, descrepancy.Actual.HttpStatusCode);
        }

        private List<Discrepancy> GetDescrepancies(List<PageResult> actualCrawlResult)
        {
            List<Discrepancy> discrepancies = new List<Discrepancy>();
            List<PageResult> expectedCrawlResult = GetExpectedCrawlResult();

            foreach (PageResult actualPage in actualCrawlResult)
            {
                Discrepancy discrepancy = ReturnIfIsADiscrepency(expectedCrawlResult.FirstOrDefault(p => p.Url == actualPage.Url), actualPage);
                if (discrepancy != null)
                    discrepancies.Add(discrepancy);
            }

            if (expectedCrawlResult.Count != actualCrawlResult.Count)
            {
                foreach (PageResult expectedPage in expectedCrawlResult)
                {
                    PageResult missingPage = actualCrawlResult.FirstOrDefault(a => a.Url == expectedPage.Url);
                    if (missingPage != null)
                        discrepancies.Add(new Discrepancy { Actual = null, Expected = expectedPage, DiscrepencyType = DiscrepencyType.MissingPageFromResult });
                }
            }

            return discrepancies;
        }

        private Discrepancy ReturnIfIsADiscrepency(PageResult expectedPage, PageResult actualPage)
        {
            Discrepancy discrepancy = null;
            if (expectedPage == null)
            {
                discrepancy = new Discrepancy { Actual = actualPage, Expected = null, DiscrepencyType = DiscrepencyType.UnexpectedPageInResult };
            }
            else
            {
                if(expectedPage.HttpStatusCode != actualPage.HttpStatusCode)
                    discrepancy = new Discrepancy { Actual = actualPage, Expected = null, DiscrepencyType = DiscrepencyType.UnexpectedHttpStatus };
                
            }

            return discrepancy;
        }
    }

    internal class PageResult
    {
        public string Url { get; set; }

        public int HttpStatusCode { get; set; }

        public override bool Equals(object obj)
        {
            PageResult other = obj as PageResult;
            if(other == null)
                return false;

            if (this.Url == other.Url && this.HttpStatusCode == other.HttpStatusCode)
                return true;

            return false;
        }

        public override string ToString()
        {
            return Url + HttpStatusCode;
        }
    }

    internal class Discrepancy
    {
        public PageResult Expected { get; set; }

        public PageResult Actual { get; set; }

        public DiscrepencyType DiscrepencyType { get; set; }
    }

    internal enum DiscrepencyType
    {
        UnexpectedPageInResult,
        UnexpectedHttpStatus,
        MissingPageFromResult
    }
}
