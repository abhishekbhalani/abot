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
            //TODO use this to creat a custom cnfig section.... http://msdn.microsoft.com/en-us/library/2tw134k3(v=vs.85).aspx
            return new CrawlConfiguration();
        }
    }
}
