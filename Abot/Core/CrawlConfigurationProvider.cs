using Abot.Poco;

namespace Abot.Core
{
    public interface ICrawlConfigurationProvider
    {
        CrawlConfiguration GetConfiguration();
    }

    public class CrawlConfigurationProvider : ICrawlConfigurationProvider
    {
        public CrawlConfiguration GetConfiguration()
        {
            return new CrawlConfiguration();
        }
    }
}
