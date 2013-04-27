using Abot.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class ManualNonBlockingThreadManagerTest : ThreadManagerTest
    {
        protected override IThreadManager GetInstance(int maxThreads)
        {
            return new ManualNonBlockingThreadManager(maxThreads);
        }
    }
}
