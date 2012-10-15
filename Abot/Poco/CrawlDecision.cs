
namespace Abot.Poco
{
    public class CrawlDecision
    {
        public CrawlDecision()
        {
            Reason = "";
        }

        public bool Should { get; set; }

        public string Reason { get; set; }
    }
}
