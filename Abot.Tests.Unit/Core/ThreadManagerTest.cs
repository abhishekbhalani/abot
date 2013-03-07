using Abot.Core;
using NUnit.Framework;
using System;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public abstract class ThreadManagerTest
    {
        IThreadManager _unitUnderTest;

        protected abstract IThreadManager GetInstance(int maxThreads);

        [SetUp]
        public void SetUp()
        {
            _unitUnderTest = GetInstance(10);
        }

        [Test]
        public void Constructor_CreatesDefaultInstance()
        {
            Assert.IsNotNull(_unitUnderTest);
            Assert.AreEqual(10, _unitUnderTest.MaxThreads);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_OverMax()
        {
            _unitUnderTest = GetInstance(101);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_BelowMinimum()
        {
            _unitUnderTest = GetInstance(0);
        }

        [Test]
        public void HasRunningThreads()
        {
            //No threads should be running
            Assert.IsFalse(_unitUnderTest.HasRunningThreads());

            //Add word to be run on a thread
            _unitUnderTest.DoWork(() => System.Threading.Thread.Sleep(300));
            System.Threading.Thread.Sleep(20);

            //Should have 1 running thread
            Assert.IsTrue(_unitUnderTest.HasRunningThreads());

            //Wait for the 1 running thread to finish
            System.Threading.Thread.Sleep(400);

            //Should have 0 threads running since the thread should have completed by now
            Assert.IsFalse(_unitUnderTest.HasRunningThreads());
        }

        [Test]
        public void DoWork_WorkCompleted()
        {
            int count = 0;

            _unitUnderTest.DoWork(() => count++);
            _unitUnderTest.DoWork(() => count++);
            _unitUnderTest.DoWork(() => count++);
            _unitUnderTest.DoWork(() => count++);
            _unitUnderTest.DoWork(() => count++);

            System.Threading.Thread.Sleep(20);

            Assert.AreEqual(5, count);
        }

        [Test]
        public void DoWork_NoThreadsAvailable_WaitForAvailableThreadThenDoesWork()
        {
            _unitUnderTest = GetInstance(1);

            //Add long running job that will take up the only available thread
            _unitUnderTest.DoWork(() => System.Threading.Thread.Sleep(20));

            //This work should still get done, but after the first stops blocking
            int count = 0;
            _unitUnderTest.DoWork(() => count++);
            System.Threading.Thread.Sleep(25);

            Assert.AreEqual(1, count);
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
