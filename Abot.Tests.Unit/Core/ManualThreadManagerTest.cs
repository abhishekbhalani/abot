using Abot.Core;
using NUnit.Framework;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class ManualThreadManagerTest : ThreadManagerTest
    {
        protected override IThreadManager GetInstance(int maxThreads)
        {
            return new ManualThreadManager(maxThreads);
        }
    }
}
