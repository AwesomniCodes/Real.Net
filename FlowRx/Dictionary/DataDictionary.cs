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

    public class DataDictionary<TKey, TDataObject> : DataObject, IDataDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
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
                                Get<TDataObject>(childChange.Key).NullThrow().Changes.OnNext(innerChange.Yield());
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

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        public QDataObject? Get<QDataObject>(TKey key) where QDataObject : class, TDataObject
            => _item.Value.Lookup(key).ValueOrDefault().DataObject as QDataObject;

        public void Copy(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();

        public void Move(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();

        #region ungeneric interface
        IDataObject? IDataDictionary.Get(object key) => _item.Value.Lookup((TKey)key).ValueOrDefault().DataObject;
        void IDataDictionary.Add(object key, IDataObject dataObject) => Add((TKey)key, (TDataObject)dataObject);
        bool IDataDictionary.Remove(object key) => Remove((TKey)key);
        void IDataDictionary.Copy(object sourceKey, object destinationKey) => Copy((TKey)sourceKey, (TKey)destinationKey);
        void IDataDictionary.Move(object sourceKey, object destinationKey) => Move((TKey)sourceKey, (TKey)destinationKey);
        #endregion

        #region Common Dictionary implementations

        public ICollection<TKey> Keys => _item.Value.Keys.AsList();

        public ICollection<TDataObject> Values => _item.Value.Items.Select(dO => dO.DataObject).AsList();

        public int Count => _item.Value.Count;

        public bool IsReadOnly => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TDataObject>.Keys => Keys;

        IEnumerable<TDataObject> IReadOnlyDictionary<TKey, TDataObject>.Values => Values;

        public IEnumerator<TDataObject> GetEnumerator() => _item.Value.Items.Select(dO => dO.DataObject).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TKey key, TDataObject value)
        {
            if (ContainsKey(key)) throw new ArgumentException($"The key {key} is already in the dictionary");
            _item.Value.AddOrUpdate((key, value));
        }

        public void Add(KeyValuePair<TKey, TDataObject> item) => Add(item.Key, item.Value);

        public bool ContainsKey(TKey key) => _item.Value.Keys.Contains(key);

        public bool Contains(KeyValuePair<TKey, TDataObject> item)
            => _item.Value.Items.Any(dO => EqualityComparer<TKey>.Default.Equals(dO.Key, item.Key) && dO.DataObject == item.Value);

        public bool Remove(TKey key)
        {
            var retVal = ContainsKey(key);
            if (retVal) _item.Value.RemoveKey(key);
            return retVal;
        }

        public bool TryGetValue(TKey key, out TDataObject value)
        {
            var kvp = _item.Value.KeyValues.FirstOrOptional(kvp => EqualityComparer<TKey>.Default.Equals(kvp.Key, key));
            value = kvp.ValueOrDefault().Value.DataObject;
            return kvp.HasValue;
        }

        public void Clear() => _item.Value.Clear();


        public void CopyTo(KeyValuePair<TKey, TDataObject>[] array, int arrayIndex)
             => _item.Value.Items.Select((dO, index) => (KvP: new KeyValuePair<TKey,TDataObject>(dO.Key, dO.DataObject), Index: index)).ForEach(item => array[arrayIndex + item.Index] = item.KvP);

        public bool Remove(KeyValuePair<TKey, TDataObject> item)
            => Remove(item.Key);

        IEnumerator<KeyValuePair<TKey, TDataObject>> IEnumerable<KeyValuePair<TKey, TDataObject>>.GetEnumerator()
            => _item.Value.Items.Select(dO => new KeyValuePair<TKey, TDataObject>(dO.Key, dO.DataObject)).GetEnumerator();

        public TDataObject this[TKey key]
        {
            get => Get<TDataObject>(key) ?? throw new ArgumentOutOfRangeException("No value under key available");
            set => Add(key, value);
        }

        IDataObject IDataDictionary.this[object index] { get => this[(TKey) index]; set => this[(TKey)index] = (TDataObject) value; }
        #endregion
    }
}