using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Tests;
using Awesomni.Codes.FlowRx.Utility;
using ImpromptuInterface;
using System;
using System.Dynamic;
using System.Reactive.Linq;
using Xunit;

namespace Awesomni.Codes.FlowRx.Dynamic.Tests
{
    public class DynamicTest
    {

        public static dynamic GetDynamicCommonDirectory()
        {
            var root = EntityDirectory<string>.Create().AsDynamic();
            root.TestDirectory = EntityDirectory<string>.Create();
            root.TestDirectory.TestString = EntitySubject<string>.Create("TestString");
            root.TestDirectory.TestInt = EntitySubject<int>.Create(23);
            root.TestDirectory.TestDouble = EntitySubject<double>.Create(23.0);
            root.TestDirectory.TestBool = EntitySubject<bool>.Create(true);
            root.TestDirectory.TestList = EntityList<IEntitySubject<int>>.Create();
            root.TestDirectory.TestList.Add(EntitySubject<int>.Create(1));
            root.TestDirectory.TestList.Add(EntitySubject<int>.Create(10));
            root.TestDirectory.TestList.Add(EntitySubject<int>.Create(15));
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
            IEntityDirectory<string> dynamicDir = GetDynamicCommonDirectory();
            Assert.Equal(
                EntityTest.GetCommonDirectoryHardcodedDebugString(),
                dynamicDir.Changes.Snapshot().ToDebugStringList());
        }

        [Fact]
        public void Dynamic_Implementation_BaseValueInterface_With_Read_Write_Has_Hardcoded_Snapshot()
        {
            var dynamicEntity = EntityDynamic<ICommonDirectoryBaseValues>.Create();
            var commonDirectory = dynamicEntity.Value;
            commonDirectory.TestDirectory.TestBool = !commonDirectory.TestDirectory.TestBool;
            commonDirectory.TestDirectory.TestDouble += 23;
            commonDirectory.TestDirectory.TestInt += 23;
            commonDirectory.TestDirectory.TestString = $"{commonDirectory.TestDirectory.TestString}TestString";
            commonDirectory.TestDirectory.TestList.Add(1);
            commonDirectory.TestDirectory.TestList.Add(10);
            commonDirectory.TestDirectory.TestList.Add(15);
            Assert.Equal(
                EntityTest.GetCommonDirectoryHardcodedDebugString(),
                dynamicEntity.Changes.Snapshot().ToDebugStringList());
        }

        [Fact]
        public void Dynamic_Implementation_SubjectValueInterface_With_Read_Write_Has_Hardcoded_Snapshot()
        {
            var dynamicEntity = EntityDynamic<ICommonDirectorySubjectValues>.Create();
            var commonDirectory = dynamicEntity.Value;
            var testDirectory = commonDirectory.TestDirectory.FirstAsync().Wait();
            testDirectory.TestBool.OnNext(!testDirectory.TestBool.FirstAsync().Wait());
            testDirectory.TestDouble.OnNext(testDirectory.TestDouble.FirstAsync().Wait() + 23);
            testDirectory.TestInt.OnNext(testDirectory.TestInt.FirstAsync().Wait() + 23);
            testDirectory.TestString.OnNext($"{testDirectory.TestString.FirstAsync().Wait()}TestString");
            var testList = testDirectory.TestList.FirstAsync().Wait();
            testList.Add(1);
            testList.Add(10);
            testList.Add(15);

            Assert.Equal(
                EntityTest.GetCommonDirectoryHardcodedDebugString(),
                dynamicEntity.Changes.Snapshot().ToDebugStringList());
        }

        [Fact]
        public void Dynamic_Implementation_EntityValueInterface_With_Read_Write_Has_Hardcoded_Snapshot()
        {
            var dynamicEntity = EntityDynamic<ICommonDirectoryEntityValues>.Create();
            var commonDirectory = dynamicEntity.Value;
            commonDirectory.TestDirectory.Value.TestBool.OnNext(!commonDirectory.TestDirectory.Value.TestBool.Value);
            commonDirectory.TestDirectory.Value.TestDouble.OnNext(commonDirectory.TestDirectory.Value.TestDouble.Value + 23);
            commonDirectory.TestDirectory.Value.TestInt.OnNext(commonDirectory.TestDirectory.Value.TestInt.Value + 23);
            commonDirectory.TestDirectory.Value.TestString.OnNext($"{commonDirectory.TestDirectory.Value.TestString.Value}TestString");
            commonDirectory.TestDirectory.Value.TestList.Add(EntitySubject<int>.Create(1));
            commonDirectory.TestDirectory.Value.TestList.Add(EntitySubject<int>.Create(10));
            commonDirectory.TestDirectory.Value.TestList.Add(EntitySubject<int>.Create(15));
            Assert.Equal(
                EntityTest.GetCommonDirectoryHardcodedDebugString(),
                dynamicEntity.Changes.Snapshot().ToDebugStringList());
        }

        [Fact]
        public void Dynamic_Composed_Common_Modified_Directory_Has_Same_Snapshot_As_Undynamic_Composition()
        {
            IEntityDirectory<string> dynamicDir = GetDynamicCommonDirectoryWithCommonModifications();
            Assert.Equal(
                EntityTest.GetCommonDirectoryWithCommonModification().Changes.Snapshot().ToDebugStringList(),
                dynamicDir.Changes.Snapshot().ToDebugStringList());
        }
    }
}
