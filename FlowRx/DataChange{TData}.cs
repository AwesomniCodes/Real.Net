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

    public interface IChangeDictionary : IChange<IDataDictionary>
    {
        object Key { get; }
        IEnumerable<IChange> Changes { get; }
    }

    public interface IChangeDictionary<TKey, TDataObject> : IChangeDictionary, IChange<IDataDictionary<TKey, TDataObject>> where TDataObject : class, IDataObject
    {
        new TKey Key { get; }
        new IEnumerable<IChange<TDataObject>> Changes { get; }
    }

    public interface IChangeList : IChange<IDataList>
    {
        int Key { get; }
        IEnumerable<IChange> Changes { get; }
    }

    public interface IChangeList<TDataObject> : IChangeList, IChange<IDataList<TDataObject>> where TDataObject : class, IDataObject
    {
        new IEnumerable<IChange<TDataObject>> Changes { get; }
    }

    public interface IChangeDirectory : IChangeDictionary<string, IDataObject> { }

    public interface IChangeItem : IChange<IDataItem>
    {
        ChangeType ChangeType { get; }
        object? Value { get; }
    }

    public interface IChangeItem<TData> : IChange<IDataItem<TData>>, IChangeItem
    {
        new TData Value { get; }
    }

    public class ChangeDirectory : ChangeDictionary<string, IDataObject>, IChangeDirectory
    {
        internal ChangeDirectory(string key, IEnumerable<IChange<IDataObject>> changes) : base(key, changes) { }
    }

    public class ChangeDictionary<TKey, TDataObject> : IChangeDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        public TKey Key { get; }

        internal ChangeDictionary(TKey key, IEnumerable<IChange<TDataObject>> changes)
        {
            Key = key;
            Changes = changes;
        }

        public IEnumerable<IChange<TDataObject>> Changes { get; private set; }

        object IChangeDictionary.Key => Key!;

        IEnumerable<IChange> IChangeDictionary.Changes => Changes;
    }


    public abstract class ChangeItem : IChangeItem
    {
        protected ChangeItem(ChangeType changeType, object? value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object? Value { get; }
    }

    public class ChangeItem<TData> : ChangeItem, IChangeItem<TData>
    {
        internal ChangeItem(ChangeType changeType, TData value = default) : base(changeType, value) { }

        public new TData Value => base.Value is TData tValue ? tValue : default!;
    }


    public class ChangeList<TDataObject> : IChangeList<TDataObject> where TDataObject : class, IDataObject
    {
        public int Key { get; }

        internal ChangeList(int key, IEnumerable<IChange<TDataObject>> changes)
        {
            Key = key;
            Changes = changes;
        }

        public IEnumerable<IChange<TDataObject>> Changes { get; private set; }

        IEnumerable<IChange> IChangeList.Changes => Changes;
    }

}