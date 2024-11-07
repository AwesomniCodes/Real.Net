// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="EntityDictionary.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using Awesomni.Codes.Real.Net.Utility;
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

    public abstract class EntityDictionary : Entity, IEntityDictionary<object, IEntity>, IReadOnlyDictionary<object, IEntity>, IReadOnlyCollection<KeyValuePair<object, IEntity>>
    {
        public abstract IEntity this[object key] { get; set; }
        public abstract ICollection<object> Keys { get; }
        public abstract ICollection<IEntity> Values { get; }
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract void Add(object key, IEntity value);
        public abstract void Add(KeyValuePair<object, IEntity> item);
        public abstract void Clear();
        public abstract bool Contains(KeyValuePair<object, IEntity> item);
        public abstract bool ContainsKey(object key);
        public abstract void Copy(object sourceKey, object destinationKey);
        public abstract void CopyTo(KeyValuePair<object, IEntity>[] array, int arrayIndex);
        public abstract IEntity? Get(object key);
        public abstract void Move(object sourceKey, object destinationKey);
        public abstract bool Remove(object key);
        public abstract bool Remove(KeyValuePair<object, IEntity> item);
        public abstract bool TryGetValue(object key, out IEntity value);
        public abstract IEnumerator<KeyValuePair<object, IEntity>> GetEnumerator();

        IEnumerable<object> IReadOnlyDictionary<object, IEntity>.Keys => Keys;
        IEnumerable<IEntity> IReadOnlyDictionary<object, IEntity>.Values => Values;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class EntityDictionary<TKey, TEntity> : EntityDictionary, IEntityDictionary<TKey, TEntity>, IReadOnlyDictionary<TKey, TEntity>, IReadOnlyCollection<KeyValuePair<TKey, TEntity>> where TEntity : class, IEntity
    {
        public static IEntityDictionary<TKey, TEntity> Create() => new EntityDictionary<TKey, TEntity>();
        
        private IEntityDictionary<TKey, TEntity> @this => this;
        protected readonly BehaviorSubject<SourceCache<(TKey Key, TEntity Entity), TKey>> _item;

        protected EntityDictionary()
        {
            _item = new BehaviorSubject<SourceCache<(TKey Key, TEntity Entity), TKey>>(new SourceCache<(TKey Key, TEntity Entity), TKey>(o => o.Key));

            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<IChangeDictionary<TKey, TEntity>>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<IChangeSubject<object?>>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Completion)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    _item.Value.Remove(key);
                });
            });
        }

        protected override IObserver<IEnumerable<IChange>> CreateObserverForChangesSubject()
            => Observer.Create<IEnumerable<IChange>>(changes =>
            {
                changes.ForEach(change =>
                {
                    if (change is IChangeSubject<TEntity>)
                    {
                        //TODO: The whole dictionary gets replaced
                        //item.OnNext(change);
                    }
                    else if (change is IChangeDictionary<TKey, TEntity> childChange)
                    {
                        childChange.Changes.ForEach(innerChange =>
                        {
                            if (innerChange is IChangeSubject<object?> innerValueChange && innerValueChange.ChangeType == ChangeType.Definition)
                            {
                                var changeType = innerChange.GetType().GetTypesIfImplemented(typeof(IChange<>)).Last().GetGenericArguments().Single()!;

                                Add(childChange.Key, (TEntity)Entity.Create(changeType, innerValueChange.Value));
                            }
                            else
                            {
                                Get(childChange.Key).NullThrow().Changes.OnNext(innerChange.Yield());
                            }
                        });
                    }
                });
            });

        protected override IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeSubject<IEntityDictionary<TKey, TEntity>>.Create(ChangeType.Definition).Yield())
               .Concat<IEnumerable<IChange<IEntity>>>(
                    _item.Switch()
                    .MergeMany(kE =>
                        kE.Entity.Changes
                        .Select(changes => ChangeDictionary<TKey, TEntity>.Create(kE.Key, changes.Cast<IChange<TEntity>>()).Yield())));

        public override void Add(object key, IEntity value) => Add((TKey)key, (TEntity)value);
        public override void Add(KeyValuePair<object, IEntity> item) => Add(item.Key, item.Value);
        public void Add(TKey key, TEntity value)
        {
            if (ContainsKey(key)) throw new ArgumentException($"The key {key} is already in the dictionary");
            _item.Value.AddOrUpdate((key, value));
        }
        public void Add(KeyValuePair<TKey, TEntity> item) => Add(item.Key, item.Value);
        public override void Clear() => _item.Value.Clear();
        public override bool Contains(KeyValuePair<object, IEntity> item)
            => _item.Value.Items.Any(kE => EqualityComparer<TKey>.Default.Equals((TKey)kE.Key, (TKey)item.Key) && kE.Entity == item.Value);
        public bool Contains(KeyValuePair<TKey, TEntity> item)
            => _item.Value.Items.Any(kE => EqualityComparer<TKey>.Default.Equals(kE.Key, item.Key) && kE.Entity == item.Value);
        public override bool ContainsKey(object key) => _item.Value.Keys.Contains((TKey)key);
        public bool ContainsKey(TKey key) => _item.Value.Keys.Contains(key);
        public override void Copy(object sourceKey, object destinationKey) => Copy((TKey)sourceKey, (TKey)destinationKey);
        public void Copy(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();
        public override void CopyTo(KeyValuePair<object, IEntity>[] array, int arrayIndex)
             => _item.Value.Items.Select((kE, index) => (KvP: new KeyValuePair<object, IEntity>(kE.Key!, kE.Entity), Index: index)).ForEach(item => array[arrayIndex + item.Index] = item.KvP);
        public void CopyTo(KeyValuePair<TKey, TEntity>[] array, int arrayIndex)
             => _item.Value.Items.Select((kE, index) => (KvP: new KeyValuePair<TKey,TEntity>(kE.Key, kE.Entity), Index: index)).ForEach(item => array[arrayIndex + item.Index] = item.KvP);
        public override IEntity? Get(object key) => Get((TKey)key);
        public TEntity? Get(TKey key)
            => _item.Value.Lookup(key).ValueOrDefault().Entity;
        public override IEnumerator<KeyValuePair<object, IEntity>> GetEnumerator() => _item.Value.Items.Select(kE => KeyValuePair.Create<object, IEntity>(kE.Key!, kE.Entity)).GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TEntity>> IEnumerable<KeyValuePair<TKey, TEntity>>.GetEnumerator() => _item.Value.Items.Select(kE => KeyValuePair.Create(kE.Key, kE.Entity)).GetEnumerator();
        public override void Move(object sourceKey, object destinationKey) => Move((TKey)sourceKey, (TKey)destinationKey);
        public void Move(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();
        public override bool Remove(object key) => Remove((TKey)key);
        public override bool Remove(KeyValuePair<object, IEntity> item) => Remove((TKey)item.Key);
        public bool Remove(KeyValuePair<TKey, TEntity> item) => Remove(item.Key);
        public bool Remove(TKey key)
        {
            var retVal = ContainsKey(key);
            if (retVal) _item.Value.RemoveKey(key);
            return retVal;
        }
        public override bool TryGetValue(object key, out IEntity value) => TryGetValue((TKey) key, out value);
        public bool TryGetValue(TKey key, out TEntity value)
        {
            var kvp = _item.Value.KeyValues.FirstOrOptional(kvp => EqualityComparer<TKey>.Default.Equals(kvp.Key, key));
            value = kvp.ValueOrDefault().Value.Entity;
            return kvp.HasValue;
        }
        public override ICollection<object> Keys => _item.Value.Keys.Cast<object>().AsList();
        ICollection<TKey> IDictionary<TKey, TEntity>.Keys => _item.Value.Keys.AsList();
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TEntity>.Keys => _item.Value.Keys;
        public override ICollection<IEntity> Values => _item.Value.Items.Select(kE => (IEntity)kE.Entity).AsList();
        ICollection<TEntity> IDictionary<TKey, TEntity>.Values => _item.Value.Items.Select(kE => kE.Entity).AsList();
        IEnumerable<TEntity> IReadOnlyDictionary<TKey, TEntity>.Values => _item.Value.Items.Select(kE => kE.Entity);
        public override int Count => _item.Value.Count;
        public override bool IsReadOnly => false;

        TEntity IReadOnlyDictionary<TKey, TEntity>.this[TKey key] { get => this[key]; }
        public TEntity this[TKey key]
        {
            get => Get(key) ?? throw new ArgumentOutOfRangeException("No value under key available");
            set => Add(key, value);
        }
        public override IEntity this[object key] { get => this[(TKey)key]; set => this[(TKey)key] = (TEntity)value; }
    }
}