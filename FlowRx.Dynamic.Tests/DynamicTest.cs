using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Tests;
using Awesomni.Codes.FlowRx.Utility;
using System;
using Xunit;

namespace FlowRx.Dynamic.Tests
{
    public class DynamicTest
    {

        public static IDataDirectory<string> GetDynamicDirectory()
        {
            var rootUndynamic = DataDirectory<string>.Create();
            var root = rootUndynamic.AsDynamic();
            root.TestDirectory = DataDirectory<string>.Create();
            root.TestDirectory.TestString = DataItem<string>.Create("TestString");
            root.TestDirectory.TestInt = DataItem<int>.Create(23);
            root.TestDirectory.TestDouble = DataItem<double>.Create(23.0);
            root.TestDirectory.TestBool = DataItem<bool>.Create(true);
            root.TestDirectory.TestBool.OnNext(false);
            return rootUndynamic;

        }

        [Fact]
        public void Dynamic_Composed_Directory_Has_Same_Snapshot_As_Undynamic_Composition()
        {
            var commonDir = DataObjectTest.GetCommonDirectory();
            var dynamicDir = GetDynamicDirectory();

            var snapshot1 = commonDir.Changes.Snapshot().ToDebugStringList();
            var snapshot2 = dynamicDir.Changes.Snapshot().ToDebugStringList();
        }
    }
}
