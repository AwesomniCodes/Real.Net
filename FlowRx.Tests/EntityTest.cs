using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Awesomni.Codes.FlowRx.Tests
{
    public class EntityTest
    {
        public static IEntityDirectory<string> GetCommonDirectory()
        {
            var root = EntityDirectory<string>.Create();
            var subFolder = root.GetOrAdd("TestDirectory", EntityDirectory<string>.Create);
            var testString = subFolder.GetOrAdd("TestString", () => EntityValue<string>.Create("TestString"));
            var testInt = subFolder.GetOrAdd("TestInt", () => EntityValue<int>.Create(23));
            var testDouble = subFolder.GetOrAdd("TestDouble", () => EntityValue<double>.Create(23.0));
            var testBool = subFolder.GetOrAdd("TestBool", () => EntityValue<bool>.Create(true));
            var testList = subFolder.GetOrAdd("TestList", () => EntityList<IEntityValue<int>>.Create());
            testList.Add(EntityValue<int>.Create(1));
            testList.Add(EntityValue<int>.Create(10));
            testList.Add(EntityValue<int>.Create(15));
            return root;
        }

        public static IList<string> GetCommonDirectoryHardcodedDebugString()
            => new List<string>
                {
                    " - Create: ",
                    "/TestDirectory - Create: ",
                    "/TestDirectory/TestString - Create: TestString",
                    "/TestDirectory/TestInt - Create: 23",
                    "/TestDirectory/TestDouble - Create: 23",
                    "/TestDirectory/TestBool - Create: True",
                    "/TestDirectory/TestList - Create: ",
                    "/TestDirectory/TestList/0 - Create: 1",
                    "/TestDirectory/TestList/1 - Create: 10",
                    "/TestDirectory/TestList/2 - Create: 15"
                };


        public static IEntityDirectory<string> GetMirroredDirectory(IEntityDirectory<string> directory)
        {
            var mirror = EntityDirectory<string>.Create();

            directory.Changes.Subscribe(mirror.Changes);

            return mirror;
        }


        public static IEntityDirectory<string> GetCommonDirectoryWithCommonModification()
        {
            var root = GetCommonDirectory();
            var subFolder = root.Get<IEntityDirectory<string>>("TestDirectory").NullThrow();
            var testString = subFolder.Get<IEntityValue<string>>("TestString").NullThrow();
            var testInt = subFolder.Get<IEntityValue<int>>("TestInt").NullThrow();
            var testDouble = subFolder.Get<IEntityValue<double>>("TestDouble").NullThrow();
            var testBool = subFolder.Get<IEntityValue<bool>>("TestBool").NullThrow();

            testInt.OnNext(20);
            testInt.OnCompleted();
            testDouble.OnNext(1.5);
            testInt.OnNext(24);
            testString.OnNext("NewTestString");
            testBool.OnNext(false);
            return root;
        }

        public static IEnumerable<object[]> GetCommonTestDirectories()
        {
            yield return new object[] { GetCommonDirectory() };
            yield return new object[] { GetCommonDirectoryWithCommonModification() };
            yield return new object[] { GetMirroredDirectory(GetCommonDirectory()) };
        }

        public static IEnumerable<object[]> GetCommonTestDirectoriesWithFlattenedDefinitions()
        {
            yield return new object[] { GetCommonDirectory(),  };
            yield return new object[] { GetCommonDirectoryWithCommonModification() };
        }

        [Theory]
        [MemberData(nameof(GetCommonTestDirectories))]
        public void Mirroring_A_Directory_Is_Working_As_Expected(IEntityDirectory<string> root)
        {
            var mirror = GetMirroredDirectory(root);
            var mirrorSnapshot = mirror.Changes.Snapshot().ToDebugStringList();
            var rootSnapshot = root.Changes.Snapshot().ToDebugStringList();

            Assert.Equal(mirrorSnapshot, rootSnapshot);
        }

        [Theory]
        [MemberData(nameof(GetCommonTestDirectories))]
        public void When_Subscribing_Multiple_Times_The_Same_Definition_Is_Returned(IEntityDirectory<string> root)
        {
            var snapshot1 = root.Changes.Snapshot().ToDebugStringList();
            var snapshot2 = root.Changes.Snapshot().ToDebugStringList();

            Assert.Equal(snapshot1, snapshot2);
        }

        [Fact]
        public void Common_Directory_Snapshot_Equals_Hardcoded()
        {
            var commonDirectory = GetCommonDirectory();

            var snapshot1 = commonDirectory.Changes.Snapshot().ToDebugStringList();
            var snapshot2 = GetCommonDirectoryHardcodedDebugString();

            Assert.Equal(snapshot1, snapshot2);
            
        }

        [Fact]
        public void When_Subscribing_To_An_Entity_Specific_Definition_Is_Returned()
        {
            var commonDirectory = GetCommonDirectoryWithCommonModification();

            var snapshot1 = commonDirectory.Changes.Snapshot().ToDebugStringList();
            var snapshot2 = commonDirectory.Changes.Snapshot().ToDebugStringList();

            Assert.Equal(snapshot1, snapshot2);
        }

        [Fact(Skip = "ExperimentalTest")]
        public async Task SyncBufferTest()
        {
            var i1 = new BehaviorSubject<int>(1);
            var i2 = new BehaviorSubject<int>(4);
            var sum = i1.CombineLatest(i2, (i1Value, i2Value) => i1Value + i2Value);
            var listAsync = sum.SynchronousBuffer().Select(buf => buf.Last()).ToList().RunAsync(new CancellationToken());

            Action syncChange1 = () =>
            {
                i1.OnNext(2);
                i2.OnNext(5);
                i1.OnNext(7);
            };

            Action syncChange2 = () =>
            {
                i1.OnNext(1);
                i2.OnNext(1);
            };

            Action syncChange3 = () =>
            {
                i1.OnNext(3);
                i1.OnCompleted();
                i2.OnCompleted();
            };

            
             var changeTask = Task.Run(syncChange1)
                .ContinueWith(t => syncChange2())
                .ContinueWith(t => syncChange3());

            var list = await listAsync;

            await changeTask;

            Assert.Equal(new List<int> { 5, 12, 2, 4 }, list.ToList());
        }

    }
}
