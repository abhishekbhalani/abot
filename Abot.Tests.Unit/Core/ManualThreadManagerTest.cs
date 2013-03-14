using Abot.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class ManualThreadManagerTest : ThreadManagerTest
    {
        //int _manualCount = 0;
        //int _prodConCount = 0;
        //object locker = new object();

        protected override IThreadManager GetInstance(int maxThreads)
        {
            return new ManualThreadManager(maxThreads);
        }

        //[Test]
        //public void ManyInstance_LightWork()
        //{
        //    int instanceCount = 100;
        //    int threadCount = 10;
        //    int actionCount = 50;
        //    int sleepTime = 5;//light work

        //    List<Action> manualActions = GetManualActions(actionCount, sleepTime);
        //    List<Action> prodConActions = GetProdConActions(actionCount, sleepTime);

        //    Stopwatch timer1 = Stopwatch.StartNew();
        //    List<IThreadManager> manual = GetManualThreadManagerInstances(instanceCount, threadCount);
        //    DoWork(manual, manualActions);
        //    timer1.Stop();
        //    //Keeps about 20 threads open, and low cpu in the resource manager, likely due to only starting a thread when there is work to be done
        //    //Takes about 25 seconds to complete work

        //    Stopwatch timer2 = Stopwatch.StartNew();
        //    List<IThreadManager> prodCon = GetProducerConsumerInstances(instanceCount, threadCount);
        //    DoWork(prodCon, prodConActions);
        //    timer2.Stop();
        //    //Keeps about 500 threads open and high cpu in the resource manager, likely due to starting a thread even if there is no work to do
        //    //Takes about 30 seconds to complete work

        //    Assert.AreEqual(actionCount * instanceCount, _manualCount);
        //    Assert.AreEqual(actionCount * instanceCount, _prodConCount);

        //    float expectedDiffPercentage = 35.0f;
        //    VerifyIsAtLeastXPercentFaster(timer1.ElapsedMilliseconds, timer2.ElapsedMilliseconds, expectedDiffPercentage);
        //}

        //[Test]
        //public void ManyInstance_HeavyWork()
        //{

        //}

        //[Test]
        //public void SingleInstance_LightWork()
        //{

        //}

        //[Test]
        //public void SingleInstance_HeavyWork()
        //{

        //}

        //private List<Action> GetManualActions(int number, int sleepTime)
        //{
        //    List<Action> actions = new List<Action>();
        //    for (int i = 0; i < number; i++)
        //        actions.Add(() =>
        //        {
        //            System.Threading.Thread.Sleep(sleepTime);
        //            lock (locker)
        //            {
        //                _manualCount++;
        //            }
        //        });

        //    return actions;
        //}

        //private List<Action> GetProdConActions(int number, int sleepTime)
        //{
        //    List<Action> actions = new List<Action>();
        //    for (int i = 0; i < number; i++)
        //        actions.Add(() =>
        //        {
        //            System.Threading.Thread.Sleep(sleepTime);
        //            lock (locker)
        //            {
        //                _prodConCount++;
        //            }
        //        });

        //    return actions;
        //}

        //private List<IThreadManager> GetManualThreadManagerInstances(int number, int maxThreads)
        //{
        //    List<IThreadManager> instances = new List<IThreadManager>();
        //    for (int i = 0; i < number; i++)
        //        instances.Add(GetManualThreadManagerInstance(maxThreads));

        //    return instances;
        //}

        //private List<IThreadManager> GetProducerConsumerInstances(int number, int maxThreads)
        //{
        //    List<IThreadManager> instances = new List<IThreadManager>();
        //    for (int i = 0; i < number; i++)
        //        instances.Add(GetProducerConsumerInstance(maxThreads));

        //    return instances;
        //}

        //private ManualThreadManager GetManualThreadManagerInstance(int maxThreads)
        //{
        //    return new ManualThreadManager(maxThreads);
        //}

        //private ProducerConsumerThreadManager GetProducerConsumerInstance(int maxThreads)
        //{
        //    return new ProducerConsumerThreadManager(maxThreads);
        //}

        //private void DoWork(List<IThreadManager> mgrs, List<Action> actions)
        //{
        //    foreach (IThreadManager mgr in mgrs)
        //    {
        //        foreach (Action action in actions)
        //            mgr.DoWork(action);

        //        while (mgr.HasRunningThreads())
        //        {
        //            System.Threading.Thread.Sleep(1000);
        //        }
        //        mgr.Dispose();
        //    }
        //}

        //private void VerifyIsAtLeastXPercentFaster(long expectedFasterMilliSecs, long expectedSlowerMilliSecs, float expectedDiffPercentage)
        //{
        //    Assert.IsTrue(expectedFasterMilliSecs < expectedSlowerMilliSecs);

        //    long diff = expectedSlowerMilliSecs - expectedFasterMilliSecs;
        //    double diffPercentage = ((double)diff / (double)expectedSlowerMilliSecs) * 100;
        //    Assert.IsTrue(diffPercentage >= expectedDiffPercentage);
        //}
    }
}
