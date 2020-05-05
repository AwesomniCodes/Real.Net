// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDictionary.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using DynamicData;
    using DynamicData.Kernel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;

    public abstract class DataDictionary : DataObject, IDataDictionary<object, IDataObject>
    {
        public abstract IDataObject this[object key] { get; set; }
        public abstract ICollection<object> Keys { get; }
        public abstract ICollection<IDataObject> Values { get; }
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract void Add(object key, IDataObject value);
        public abstract void Add(KeyValuePair<object, IDataObject> item);
        public abstract void Clear();
        public abstract bool Contains(KeyValuePair<object, IDataObject> item);
        public abstract bool ContainsKey(object key);
        public abstract void Copy(object sourceKey, object destinationKey);
        public abstract void CopyTo(KeyValuePair<object, IDataObject>[] array, int arrayIndex);
        public abstract IDataObject? Get(object key);
        public abstract void Move(object sourceKey, object destinationKey);
        public abstract bool Remove(object key);
        public abstract bool Remove(KeyValuePair<object, IDataObject> item);
        public abstract bool TryGetValue(object key, out IDataObject value);
        public abstract IEnumerator<KeyValuePair<object, IDataObject>> GetEnumerator();

        IEnumerable<object> IReadOnlyDictionary<object, IDataObject>.Keys => Keys;
        IEnumerable<IDataObject> IReadOnlyDictionary<object, IDataObject>.Values => Values;
        IEnumerator<IDataObject> IEnumerable<IDataObject>.GetEnumerator() => ((IEnumerable<KeyValuePair<object, IDataObject>>)this).Select(kvp => kvp.Value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class DataDictionary<TKey, TDataObject> : DataDictionary, IDataDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        private IDataDictionary<TKey, TDataObject> @this => this;
        protected readonly BehaviorSubject<SourceCache<(TKey Key, TDataObject DataObject), TKey>> _item;

        public static IDataDictionary<TKey, TDataObject> Create() => new DataDictionary<TKey, TDataObject>();

        protected DataDictionary()
        {
            _item = new BehaviorSubject<SourceCache<(TKey Key, TDataObject DataObject), TKey>>(new SourceCache<(TKey Key, TDataObject DataObject), TKey>(o => o.Key));

            Changes = CreateChangesSubject();

            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<IChangeDictionary<TKey, TDataObject>>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<IChangeItem<object?>>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Complete)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    _item.Value.Remove(key);
                });
            });
        }

        protected virtual ISubject<IEnumerable<IChange>> CreateChangesSubject()
            => Subject.Create<IEnumerable<IChange>>(
                    CreateObserverForChangesSubject(),
                    CreateObservableForChangesSubject());

        protected virtual IObserver<IEnumerable<IChange>> CreateObserverForChangesSubject()
            => Observer.Create<IEnumerable<IChange>>(changes =>
            {
                changes.ForEach(change =>
                {
                    if (change is IChangeItem<TDataObject>)
                    {
                        //TODO: The whole dictionary gets replaced
                        //item.OnNext(change);
                    }
                    else if (change is IChangeDictionary<TKey, TDataObject> childChange)
                    {
                        childChange.Changes.ForEach(innerChange =>
                        {
                            if (innerChange is IChangeItem<object?> innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                            {
                                var changeType = innerChange.GetType().GetTypesIfImplemented(typeof(IChange<>)).Last().GetGenericArguments().Single()!;

                                Add(childChange.Key, (TDataObject)DataObject.Create(changeType, innerValueChange.Value));
                            }
                            else
                            {
                                Get(childChange.Key).NullThrow().Changes.OnNext(innerChange.Yield());
                            }
                        });
                    }
                });
            });

        protected virtual IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeItem<IDataDictionary<TKey, TDataObject>>.Create(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IDataObject>>>(
                    _item.Switch()
                    .MergeMany(dO =>
                        dO.DataObject.Changes
                        .Select(changes => ChangeDictionary<TKey, TDataObject>.Create(dO.Key, changes.Cast<IChange<TDataObject>>()).Yield())));

        public override void Add(object key, IDataObject value) => Add((TKey)key, (TDataObject)value);
        public override void Add(KeyValuePair<object, IDataObject> item) => Add(item.Key, item.Value);
        public void Add(TKey key, TDataObject value)
        {
            if (ContainsKey(key)) throw new ArgumentException($"The key {key} is already in the dictionary");
            _item.Value.AddOrUpdate((key, value));
        }
        public void Add(KeyValuePair<TKey, TDataObject> item) => Add(item.Key, item.Value);
        public override void Clear() => _item.Value.Clear();
        public override bool Contains(KeyValuePair<object, IDataObject> item)
            => _item.Value.Items.Any(dO => EqualityComparer<TKey>.Default.Equals((TKey)dO.Key, (TKey)item.Key) && dO.DataObject == item.Value);
        public bool Contains(KeyValuePair<TKey, TDataObject> item)
            => _item.Value.Items.Any(dO => EqualityComparer<TKey>.Default.Equals(dO.Key, item.Key) && dO.DataObject == item.Value);
        public override bool ContainsKey(object key) => _item.Value.Keys.Contains((TKey)key);
        public bool ContainsKey(TKey key) => _item.Value.Keys.Contains(key);
        public override void Copy(object sourceKey, object destinationKey) => Copy((TKey)sourceKey, (TKey)destinationKey);
        public void Copy(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();
        public override void CopyTo(KeyValuePair<object, IDataObject>[] array, int arrayIndex)
             => _item.Value.Items.Select((dO, index) => (KvP: new KeyValuePair<object, IDataObject>(dO.Key!, dO.DataObject), Index: index)).ForEach(item => array[arrayIndex + item.Index] = item.KvP);
        public void CopyTo(KeyValuePair<TKey, TDataObject>[] array, int arrayIndex)
             => _item.Value.Items.Select((dO, index) => (KvP: new KeyValuePair<TKey,TDataObject>(dO.Key, dO.DataObject), Index: index)).ForEach(item => array[arrayIndex + item.Index] = item.KvP);
        public override IDataObject? Get(object key) => Get((TKey)key);
        public TDataObject? Get(TKey key)
            => _item.Value.Lookup(key).ValueOrDefault().DataObject;
        public override IEnumerator<KeyValuePair<object, IDataObject>> GetEnumerator() => _item.Value.Items.Select(dO => KeyValuePair.Create<object, IDataObject>(dO.Key!, dO.DataObject)).GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TDataObject>> IEnumerable<KeyValuePair<TKey, TDataObject>>.GetEnumerator() => _item.Value.Items.Select(dO => KeyValuePair.Create(dO.Key, dO.DataObject)).GetEnumerator();
        IEnumerator<TDataObject> IEnumerable<TDataObject>.GetEnumerator() => _item.Value.Items.Select(dO => dO.DataObject).GetEnumerator();
        public override void Move(object sourceKey, object destinationKey) => Move((TKey)sourceKey, (TKey)destinationKey);
        public void Move(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();
        public override bool Remove(object key) => Remove((TKey)key);
        public override bool Remove(KeyValuePair<object, IDataObject> item) => Remove((TKey)item.Key);
        public bool Remove(KeyValuePair<TKey, TDataObject> item) => Remove(item.Key);
        public bool Remove(TKey key)
        {
            var retVal = ContainsKey(key);
            if (retVal) _item.Value.RemoveKey(key);
            return retVal;
        }
        public override bool TryGetValue(object key, out IDataObject value) => TryGetValue((TKey) key, out value);
        public bool TryGetValue(TKey key, out TDataObject value)
        {
            var kvp = _item.Value.KeyValues.FirstOrOptional(kvp => EqualityComparer<TKey>.Default.Equals(kvp.Key, key));
            value = kvp.ValueOrDefault().Value.DataObject;
            return kvp.HasValue;
        }
        public override ISubject<IEnumerable<IChange>> Changes { get; }
        public override ICollection<object> Keys => _item.Value.Keys.Cast<object>().AsList();
        ICollection<TKey> IDictionary<TKey, TDataObject>.Keys => _item.Value.Keys.AsList();
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TDataObject>.Keys => _item.Value.Keys;
        public override ICollection<IDataObject> Values => _item.Value.Items.Select(dO => (IDataObject)dO.DataObject).AsList();
        ICollection<TDataObject> IDictionary<TKey, TDataObject>.Values => _item.Value.Items.Select(dO => dO.DataObject).AsList();
        IEnumerable<TDataObject> IReadOnlyDictionary<TKey, TDataObject>.Values => _item.Value.Items.Select(dO => dO.DataObject);
        public override int Count => _item.Value.Count;
        public override bool IsReadOnly => false;
        TDataObject IReadOnlyDictionary<TKey, TDataObject>.this[TKey key] => throw new NotImplementedException();
        public TDataObject this[TKey key]
        {
            get => Get(key) ?? throw new ArgumentOutOfRangeException("No value under key available");
            set => Add(key, value);
        }
        public override IDataObject this[object key] { get => this[(TKey)key]; set => this[(TKey)key] = (TDataObject)value; }
    }
}