// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityList.cs" project="FlowRx" solution="FlowRx" />
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

    public abstract class EntityList : Entity, IEntityList<IEntity>, IReadOnlyCollection<IEntity>, IList
    {
        public abstract IEntity this[int index] { get; set; }
        object IList.this[int index] { get => this[index]; set => this[index] = (IEntity) value; }

        public abstract bool IsFixedSize { get; }
        public abstract bool IsReadOnly { get; }
        public abstract int Count { get; }
        public abstract bool IsSynchronized { get; }
        public abstract object SyncRoot { get; }
        public abstract int Add(object value);
        public abstract void Add(IEntity item);
        public abstract void Clear();
        public abstract bool Contains(object value);
        public abstract bool Contains(IEntity item);
        public abstract void CopyTo(Array array, int index);
        public abstract void CopyTo(IEntity[] array, int arrayIndex);
        public abstract IEnumerator<IEntity> GetEnumerator();
        public abstract int IndexOf(object value);
        public abstract int IndexOf(IEntity item);
        public abstract void Insert(int index, object value);
        public abstract void Insert(int index, IEntity item);
        public abstract void Remove(object value);
        public abstract bool Remove(IEntity item);
        public abstract void RemoveAt(int index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class EntityList<TEntity> : EntityList, IEntityList<TEntity>, IReadOnlyCollection<TEntity> where TEntity : class, IEntity
    {
        public static IEntityList<TEntity> Create() => new EntityList<TEntity>();
        
        private IEntityList<TEntity> @this => this;
        private object? _syncRoot;
        protected readonly BehaviorSubject<SourceList<TEntity>> _item;

        protected EntityList()
        {
            _item = new BehaviorSubject<SourceList<TEntity>>(new SourceList<TEntity>());

            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<IChangeList<TEntity>>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<IChangeSubject<object?>>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Complete)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    _item.Value.RemoveAt(key);
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
                        //TODO: The whole list gets replaced
                        //item.OnNext(change);
                    }
                    else if (change is IChangeList<TEntity> childChange)
                    {
                        childChange.Changes.ForEach(innerChange =>
                        {
                            if (innerChange is IChangeSubject<object?> innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                            {
                                var changeType = innerChange.GetType().GetTypesIfImplemented(typeof(IChange<>)).Last().GetGenericArguments().Single();
                                if (changeType != null)
                                {
                                    var entity = (TEntity)Entity.Create(changeType, innerValueChange.Value);
                                    Insert(childChange.Key, entity);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Received an invalid IChangeItem that is not implementing IChange<>");
                                }
                            }
                            else
                            {
                                @this[childChange.Key].Changes.OnNext(innerChange.Yield());
                            }
                        });
                    }
                });
            });

        protected override IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeSubject<IEntityList<TEntity>>.Create(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IEntity>>>(
                    _item.Switch()
                    .MergeMany(entity =>
                        entity.Changes
                        .Select(changes => ChangeList<TEntity>.Create(_item.Value.Items.IndexOf(entity), changes.Cast<IChange<TEntity>>()).Yield())));

        public override int Add(object value)
        {
            if (value is TEntity entity)
            {
                _item.Value.Add(entity);
                return _item.Value.Count - 1;
            }
            return -1;
        }
        public override void Add(IEntity item) => @this.Add((TEntity)item);
        public void Add(TEntity item) => _item.Value.Add(item);
        public override void Clear() => _item.Value.Clear();
        public override bool Contains(object value) => value is TEntity entity && Contains(entity);
        public override bool Contains(IEntity item) => item is TEntity entity && Contains(entity);
        public bool Contains(TEntity item) => _item.Value.Items.Contains(item);
        public override void CopyTo(Array array, int index)
        {
            if (!(array is TEntity[] tdArray)) throw new ArgumentException(nameof(array));
            CopyTo(tdArray, index);
        }
        public override void CopyTo(IEntity[] array, int arrayIndex) => _item.Value.Items.Select((kE, index) => (kE, index)).ForEach(item => array[arrayIndex + item.index] = item.kE);
        public void CopyTo(TEntity[] array, int arrayIndex) => _item.Value.Items.Select((kE, index) => (kE, index)).ForEach(item => array[arrayIndex + item.index] = item.kE);
        public override IEnumerator<IEntity> GetEnumerator() => @this.GetEnumerator();
        public override int IndexOf(object value) => value is TEntity entity ? IndexOf(entity) : -1;
        public override int IndexOf(IEntity item) => item is TEntity entity ? IndexOf(entity) : -1;
        public int IndexOf(TEntity item) => _item.Value.Items.IndexOf(item);
        public override void Insert(int index, object value) => Insert(index, (TEntity)value);
        public override void Insert(int index, IEntity item) => Insert(index, (TEntity)item);
        public void Insert(int index, TEntity item) => _item.Value.Insert(index, item);
        public override void Remove(object value) => @this.Remove((TEntity)value);
        public override bool Remove(IEntity item) => @this.Remove((TEntity)item);
        public bool Remove(TEntity item) => _item.Value.Remove(item);
        public override void RemoveAt(int index) => _item.Value.RemoveAt(index);
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _item.Value.Items.Select(kE => kE).GetEnumerator();
        public override bool IsFixedSize => false;
        public override bool IsReadOnly => false;
        public override int Count => _item.Value.Count;
        public override bool IsSynchronized => false;
        public override object SyncRoot => _syncRoot ?? (_syncRoot = new object());

        TEntity IList<TEntity>.this[int index]
        {
            get => _item.Value.Items.ElementAt(index);
            set => _item.Value.ReplaceAt(index, value);
        }
        public override IEntity this[int index] { get => @this[index]; set => @this[index] = (TEntity)value; }
    }
}