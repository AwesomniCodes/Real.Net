// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeFactory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;
    using System.Reflection;

    public class ChangeFactory : IChangeFactory
    {
        //public IChangeItem Item(ChangeType changeType, object value)
        //    => (IChangeItem)GetType()
        //    .GetMethod(nameof(Item), 1, new Type[] { typeof(ChangeType), Type.MakeGenericMethodParameter(1) })
        //    .MakeGenericMethod(value.GetType())
        //    .Invoke(this, new object[] { changeType, value });

        //public IChangeItem Item(ChangeType changeType, Type type)
        //    => (IChangeItem)GetType()
        //    .GetMethod(nameof(Item), 1, new Type[] { typeof(ChangeType), Type.MakeGenericMethodParameter(1) })
        //    .MakeGenericMethod(type)
        //    .Invoke(this, new object?[] { changeType, type.GetDefault() });

        public IChangeItem<TData> Item<TData>(ChangeType changeType, TData value = default)
            => new ChangeItem<TData>(changeType, value);

        public IChangeDictionary<TKey, TDataObject> Dictionary<TKey, TDataObject>(TKey key, IEnumerable<IChange<TDataObject>> changes) where TDataObject : class, IDataObject
            => (typeof(TKey) == typeof(string) && typeof(TDataObject) == typeof(IDataObject)) ?
            (IChangeDictionary<TKey, TDataObject>)Directory(key as string ?? string.Empty, changes) :
            new ChangeDictionary<TKey, TDataObject>(key, changes);

        public IChangeDirectory Directory(string key, IEnumerable<IChange<IDataObject>> changes)
            => new ChangeDirectory(key, changes);

        public IChangeList<TDataObject> List<TDataObject>(int index, IEnumerable<IChange<TDataObject>> changes) where TDataObject : class, IDataObject
            => new ChangeList<TDataObject>(index, changes);
    }
}