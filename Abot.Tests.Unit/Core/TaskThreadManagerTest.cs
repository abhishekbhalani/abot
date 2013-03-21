using Abot.Core;
using NUnit.Framework;
using System.Net;
using System.Threading;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class TaskThreadManagerTest
    {
        private IThreadManager _unitUnderTest;

        private int _maxConcurrentTasks;

        [SetUp]
        public void SetUp()
        {
            ServicePointManager.DefaultConnectionLimit = 40;
            _maxConcurrentTasks = ServicePointManager.DefaultConnectionLimit * 2;
            _unitUnderTest = new TaskThreadManager(_maxConcurrentTasks);
        }

        [TearDown]
        public void TearDown()
        {
            if (_unitUnderTest != null)
            {
                _unitUnderTest.Dispose();
            }
        }

        [Test]
        public void HasRunningThreads()
        {
            Assert.IsFalse(_unitUnderTest.HasRunningThreads());

            _unitUnderTest.DoWork(() => Thread.Sleep(300));
            Thread.Sleep(20);

            Assert.IsTrue(_unitUnderTest.HasRunningThreads());

            Thread.Sleep(400);

            Assert.IsFalse(_unitUnderTest.HasRunningThreads());
        }

        [Test]
        public void DoWork_WorkItemsEqualToThreads_WorkIsCompletedAsync()
        {
            int count = 0;

            for (int i = 0; i < _maxConcurrentTasks; i++)
            {
                _unitUnderTest.DoWork(() =>
                {
                    Thread.Sleep(5);
                    Interlocked.Increment(ref count);
                });
            }

            Assert.IsTrue(count < _maxConcurrentTasks);
            Thread.Sleep(_maxConcurrentTasks * 6);

            Assert.AreEqual(_maxConcurrentTasks, count);
        }

        [Test]
        public void DoWork_MoreWorkThanTasks_WorkIsCompletedAsync()
        {
            int count = 0;
            for (int i = 0; i < 2 * _maxConcurrentTasks; i++)
            {
                _unitUnderTest.DoWork(() =>
                {
                    Thread.Sleep(5);
                    Interlocked.Increment(ref count);
                });
            }

            Thread.Sleep(2 * _maxConcurrentTasks * 6);
            Assert.AreEqual(2 * _maxConcurrentTasks, count);
        }

        [Test]
        public void AbortAll_WorkNeverCompleted()
        {
            int count = 0;

            _unitUnderTest.DoWork(() => { System.Threading.Thread.Sleep(1000); count++; });
            _unitUnderTest.DoWork(() => { System.Threading.Thread.Sleep(1000); count++; });
            _unitUnderTest.DoWork(() => { System.Threading.Thread.Sleep(1000); count++; });
            _unitUnderTest.DoWork(() => { System.Threading.Thread.Sleep(1000); count++; });
            _unitUnderTest.DoWork(() => { System.Threading.Thread.Sleep(1000); count++; });

            _unitUnderTest.AbortAll();

            System.Threading.Thread.Sleep(250);
            Assert.AreEqual(0, count);
        }
    }
}
