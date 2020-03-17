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
    public class DataObjectTest
    {
        public static IDataDirectory<IDataObject> GetCommonDirectory()
        {
            var root = DataDirectory<IDataObject>.Creation();
            var subFolder = root.Create("TestDirectory", DataDirectory<IDataObject>.Creation);
            var testString = subFolder.Create("TestString", DataItem<string>.Creation("TestString"));
            var testInt = subFolder.Create("TestInt", DataItem<int>.Creation(23));
            var testDouble = subFolder.Create("TestDouble", DataItem<double>.Creation(23.0));
            var testBool = subFolder.Create("TestBool", DataItem<bool>.Creation(true));
            return root;
        }


        public static IDataDirectory<IDataObject> GetMirroredDirectory(IDataDirectory<IDataObject> directory)
        {
            var mirror = DataDirectory<IDataObject>.Creation();

            directory.Changes.Subscribe(mirror.Changes);

            return mirror;
        }


        public static IDataDirectory<IDataObject> GetCommonDirectoryWithCommonModification()
        {
            var root = GetCommonDirectory();
            var subFolder = root.Get<IDataDirectory<IDataObject>>("TestDirectory").NullThrow();
            var testString = subFolder.Get<IDataItem<string>>("TestString").NullThrow();
            var testInt = subFolder.Get<IDataItem<int>>("TestInt").NullThrow();
            var testDouble = subFolder.Get<IDataItem<double>>("TestDouble").NullThrow();
            var testBool = subFolder.Get<IDataItem<bool>>("TestBool").NullThrow();

            testInt.OnNext(20);
            testInt.OnCompleted();
            testDouble.OnNext(1.5);
            testInt.OnNext(24);
            testString.OnNext("NewTestString");
            testBool.OnNext(false);
            return root;
        }

        public static IEnumerable<object[]> GetCommonTestDirectoriesObjects()
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
        [MemberData(nameof(GetCommonTestDirectoriesObjects))]
        public void Mirroring_A_Directory_Is_Working_As_Expected(IDataDirectory<IDataObject> root)
        {
            var mirror = GetMirroredDirectory(root);
            var mirrorSnapshot = mirror.Changes
                .Snapshot()
                .SelectMany(changes => changes)
                .Flattened()
                .Select(fC => fC.ToDebugString())
                .ToList();

            var rootSnapshot = root.Changes
                .Snapshot()
                .SelectMany(changes => changes)
                .Flattened()
                .Select(fC => fC.ToDebugString())
                .ToList();

            Assert.Equal(mirrorSnapshot, rootSnapshot);
        }

        [Theory]
        [MemberData(nameof(GetCommonTestDirectoriesObjects))]
        public void When_Subscribing_Multiple_Times_The_Same_Definition_Is_Returned(IDataDirectory<IDataObject> root)
        {
            var snapshot1 = root.Changes
                .Snapshot()
                .SelectMany(changes => changes)
                .Flattened()
                .Select(fC => fC.ToDebugString())
                .ToList();

            var snapshot2 = root.Changes
                .Snapshot()
                .SelectMany(changes => changes)
                .Flattened()
                .Select(fC => fC.ToDebugString())
                .ToList();

            Assert.Equal(snapshot1, snapshot2);
        }


        [Fact]
        public void When_Subscribing_To_A_Data_Object_Specific_Definition_Is_Returned()
        {
            var commonDirectory = GetCommonDirectoryWithCommonModification();

            var snapshot1 = commonDirectory.Changes
                .Snapshot()
                .SelectMany(changes => changes)
                .Flattened()
                .Select(fC => fC.ToDebugString())
                .ToList();

            var snapshot2 = commonDirectory.Changes
                .Snapshot()
                .SelectMany(changes => changes)
                .Flattened()
                .Select(fC => fC.ToDebugString())
                .ToList();

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
