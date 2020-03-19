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
    public interface IChange { }
    public interface IChange<out TDataObject> : IChange where TDataObject : class, IDataObject { }

    public interface IChangeDictionary<TKey, TDataObject> : IChange<IDataDictionary<TKey, TDataObject>> where TDataObject : class, IDataObject
    {
        TKey Key { get; }
        IEnumerable<IChange<TDataObject>> Changes { get; }
    }

    public interface IChangeItem : IChange<IDataItem>
    {
        ChangeType ChangeType { get; }
        object? Value { get; }
    }

    public interface IChangeItem<TData> : IChange<IDataItem<TData>>, IChangeItem
    {
        new TData Value { get; }
    }
    
    public class DictionaryChange<TKey, TDataObject> : IChangeDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        public TKey Key { get; }

        internal DictionaryChange(TKey key, IEnumerable<IChange<TDataObject>> changes)
        {
            Key = key;
            Changes = changes;
        }

        public IEnumerable<IChange<TDataObject>> Changes { get; private set; }

    }


    public abstract class DataItemChange : IChangeItem
    {
        protected DataItemChange(ChangeType changeType, object? value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object? Value { get; }
    }



    public class DataItemChange<TData> : DataItemChange, IChangeItem<TData>
    {
        internal DataItemChange(ChangeType changeType, TData value = default) : base(changeType, value) { }

        public new TData Value => base.Value is TData tValue ? tValue : default;
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
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IEnumerable<IChange> changes, IEnumerable<object>? curKeyChain = null)
        {
            return changes.SelectMany(change => change.Flattened(curKeyChain ?? Enumerable.Empty<object>()));
        }
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IChange change, IEnumerable<object>? curKeyChain = null)
        {
            var keyChain = (curKeyChain ?? Enumerable.Empty<object>()).ToList();

            if (change is IChangeItem valueChange)
            {
                yield return (keyChain, valueChange.ChangeType, valueChange.Value);
            }

            if (change is IChangeDictionary<object, IDataObject> childChange)
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