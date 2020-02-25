using Awesomni.Codes.FlowRx.DataSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace Awesomni.Codes.FlowRx.Tests
{
    [TestClass]
    public class DataObjectTest
    {
        [TestMethod]
        public void DummyTest()
        {

            var system1 = new DataDirectory("TestRoot");
            var system2 = new DataDirectory("TestRoot2");
            //system1.Link.AsLogOutput().Subscribe(logMessage => _log.Info($"system1:{logMessage}"));
            //system2.Link.AsLogOutput().Subscribe(logMessage => _log.Info($"system2:{logMessage}"));
            
            //var storeSync = new DataSync();

            //storeSync.SyncObjects.Add(system1.Root);
            //storeSync.SyncObjects.Add(system2.Root);



            var subFolder = system1.GetOrCreateDirectory("TestDirectory");

            var testString = subFolder.GetOrCreate("TestString", "TestString");
            var testInt = subFolder.GetOrCreate("TestInt", 23);
            var testDouble = subFolder.GetOrCreate("TestDouble", 23.0);
            var testBool = subFolder.GetOrCreate("TestBool", true);


            var subFolder2 = system2.GetOrCreateDirectory("TestDirectory");
            var testBool2 = subFolder2.GetOrCreate("TestBool", true);
            testInt.OnNext(20);
            testDouble.OnNext(1.5);
            testInt.OnNext(24);
            testString.OnNext("NewTestString");
            testBool.OnNext(false);
            testBool2.OnNext(true);

            Assert.IsTrue(true);
            var testList = system1.Changes.TakeUntil(Observable.Interval(TimeSpan.FromSeconds(1))).ToList().FirstAsync().Wait();
        }
    }
}
