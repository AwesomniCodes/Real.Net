using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Tests;
using Awesomni.Codes.FlowRx.Utility;
using FlowRx.Tests;
using ImpromptuInterface;
using System;
using System.Dynamic;
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
            IDataDirectory<string> dynamicDir = GetDynamicCommonDirectory();
            Assert.Equal(
                DataObjectTest.GetCommonDirectoryHardcodedDebugString(),
                dynamicDir.Changes.Snapshot().ToDebugStringList());
        }

        [Fact]
        public void Dynamic_Implemenation_Common_Directory_Has_Same_Snapshot_As_Undynamic_Composition()
        {

            dynamic expando = GetDynamicCommonDirectory();

            ICommonDirectoryBaseValues myInterface = Impromptu.ActLike<ICommonDirectoryBaseValues>(expando);
            myInterface.TestDirectory.TestBool = true;
            Assert.Equal(
                DataObjectTest.GetCommonDirectoryHardcodedDebugString(),
                DataDynamicObject<ICommonDirectoryBaseValues>.Create().Changes.Snapshot().ToDebugStringList());
        }

        [Fact]
        public void Dynamic_Composed_Common_Modified_Directory_Has_Same_Snapshot_As_Undynamic_Composition()
        {
            IDataDirectory<string> dynamicDir = GetDynamicCommonDirectoryWithCommonModifications();
            Assert.Equal(
                DataObjectTest.GetCommonDirectoryWithCommonModification().Changes.Snapshot().ToDebugStringList(),
                dynamicDir.Changes.Snapshot().ToDebugStringList());
        }
    }
}
