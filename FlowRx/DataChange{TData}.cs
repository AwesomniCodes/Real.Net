// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataChange{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    [Flags]
    public enum ChangeType : int
    {
        Create = 1,
        Complete = 2,
        Connect = 4,
        Disconnect = 8,
        Modify = 16,
        Error = 32,
    }
    public interface IChange<out TDataObject> where TDataObject : class, IDataObject { }
    public abstract class Change<TDataObject> : IChange<TDataObject> where TDataObject : class, IDataObject { }

    public interface IDirectoryChange<TDataObject> : IChange<IDataDirectory<TDataObject>> where TDataObject : class, IDataObject
    {
        object Key { get; }
        IEnumerable<IChange<TDataObject>> Changes { get; }

    }

    public class DirectoryChange<TDataObject> : Change<IDataDirectory<TDataObject>>, IDirectoryChange<TDataObject> where TDataObject : class, IDataObject
    {
        public object Key { get; }

        private DirectoryChange(object key, IEnumerable<IChange<TDataObject>> changes)
        {
            Key = key;
            Changes = changes;
        }

        public IEnumerable<IChange<TDataObject>> Changes { get; private set; }

        public static Func<IDirectoryChange<TDataObject>> Creation(object key, IEnumerable<IChange<TDataObject>> changes) => () => new DirectoryChange<TDataObject>(key, changes);
    }

    public interface IDataItemChange : IChange<IDataItem>
    {
        ChangeType ChangeType { get; }
        object? Value { get; }
    }

    public abstract class DataItemChange : Change<IDataItem>, IDataItemChange
    {
        protected DataItemChange(ChangeType changeType, object? value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object? Value { get; }

        public static Func<IDataItemChange> Creation(ChangeType changeType, object value)
        {
            MethodInfo method = typeof(DataItemChange<>).MakeGenericType(value.GetType()).GetMethod(nameof(DataItemChange.Creation), BindingFlags.Static | BindingFlags.Public);
            return (Func<IDataItemChange>)method.Invoke(null, new object[] { changeType, value });
        }

        public static Func<IDataItemChange> Creation(ChangeType changeType, Type type)
        {
            MethodInfo method = typeof(DataItemChange<>).MakeGenericType(type).GetMethod(nameof(DataItemChange.Creation), BindingFlags.Static | BindingFlags.Public);
            return (Func<IDataItemChange>)method.Invoke(null, new object?[] { changeType, type.GetDefault() });
        }
    }


    public interface IDataItemChange<TData> : IChange<IDataItem<TData>>, IDataItemChange
    {
        new TData Value { get; }
    }

    public class DataItemChange<TData> : DataItemChange, IDataItemChange<TData>
    {
        internal DataItemChange(ChangeType changeType, TData value = default) : base(changeType, value) { }

        public new TData Value => base.Value is TData tValue ? tValue : default;

        public static Func<IDataItemChange<TData>> Creation(ChangeType changeType, TData value = default)
            => () => new DataItemChange<TData>(changeType, value);
    }

    public static class ChangeExtensions
    {

        public static string ToDebugString(this (List<object> KeyChain, ChangeType changeType, object? Value) flatChange)
        {
            var sb = new StringBuilder();
            sb.Append('.');
            foreach (var key in flatChange.KeyChain)
            {
                sb.Append($"/{key.ToString()}");
            }
            
            return $"{sb.ToString()} - {flatChange.changeType}: {flatChange.Value}";
        }
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IEnumerable<IChange<IDataObject>> changes, IEnumerable<object>? curKeyChain = null)
        {
            return changes.SelectMany(change => change.Flattened(curKeyChain ?? Enumerable.Empty<object>()));
        }
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IChange<IDataObject> change, IEnumerable<object>? curKeyChain = null)
        {
            var keyChain = (curKeyChain ?? Enumerable.Empty<object>()).ToList();

            if (change is IDataItemChange valueChange)
            {
                yield return (keyChain, valueChange.ChangeType, valueChange.Value);
            }

            if (change is IDirectoryChange<IDataObject> childChange)
            {
                keyChain = keyChain.Concat(childChange.Key.Yield()).ToList();

                foreach (var flattenedChildItem in childChange.Changes.Flattened(keyChain))
                {
                    yield return flattenedChildItem;
                }
            }
        }

    }
}