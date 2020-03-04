using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Utility;
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
            var root = new DataDirectory("TestRoot");
            var subFolder = root.GetOrCreateDirectory("TestDirectory");
            var testString = subFolder.GetOrCreate("TestString", "TestString");
            var testInt = subFolder.GetOrCreate("TestInt", 23);
            var testDouble = subFolder.GetOrCreate("TestDouble", 23.0);
            var testBool = subFolder.GetOrCreate("TestBool", true);
            testInt.OnNext(20);
            testDouble.OnNext(1.5);
            testInt.OnNext(24);
            testString.OnNext("NewTestString");
            testBool.OnNext(false);

            var snapshot = root.Changes.Snapshot();
            Assert.IsTrue(true);

        }
    }
}
