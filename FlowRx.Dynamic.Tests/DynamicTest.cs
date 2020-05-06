using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Tests;
using Awesomni.Codes.FlowRx.Utility;
using System;
using Xunit;

namespace FlowRx.Dynamic.Tests
{
    public class DynamicTest
    {

        public static dynamic GetDynamicCommonDirectory()
        {
            var root = DataDirectory<string>.Create().AsDynamic();
            root.TestDirectory = DataDirectory<string>.Create();
            root.TestDirectory.TestString = DataItem<string>.Create("TestString");
            root.TestDirectory.TestInt = DataItem<int>.Create(23);
            root.TestDirectory.TestDouble = DataItem<double>.Create(23.0);
            root.TestDirectory.TestBool = DataItem<bool>.Create(true);
            return root;
        }

        public static dynamic GetDynamicCommonDirectoryWithCommonModifications()
        {
            var root = GetDynamicCommonDirectory();
            root.TestDirectory.TestInt.OnNext(20);
            root.TestDirectory.TestInt.OnCompleted();
            root.TestDirectory.TestDouble.OnNext(1.5);
            root.TestDirectory.TestString.OnNext("NewTestString");
            root.TestDirectory.TestBool.OnNext(false);

            return root;
        }

        [Fact]
        public void Dynamic_Composed_Common_Directory_Has_Same_Snapshot_As_Undynamic_Composition()
        {
            var commonDir = DataObjectTest.GetCommonDirectory();
            IDataObject dynamicDir = GetDynamicCommonDirectory();

            var snapshot1 = commonDir.Changes.Snapshot().ToDebugStringList();
            var snapshot2 = dynamicDir.Changes.Snapshot().ToDebugStringList();
            Assert.Equal(snapshot1, snapshot2);
        }


        [Fact]
        public void Dynamic_Composed_Common_Modified_Directory_Has_Same_Snapshot_As_Undynamic_Composition()
        {
            var commonDir = DataObjectTest.GetCommonDirectoryWithCommonModification();
            IDataObject dynamicDir = GetDynamicCommonDirectoryWithCommonModifications() ;

            var snapshot1 = commonDir.Changes.Snapshot().ToDebugStringList();
            var snapshot2 = dynamicDir.Changes.Snapshot().ToDebugStringList();
            Assert.Equal(snapshot1, snapshot2);
        }
    }
}
