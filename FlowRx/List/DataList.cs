// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataList.cs" project="FlowRx" solution="FlowRx" />
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

    public class DataList<TDataObject> : DataObject, IDataList<TDataObject> where TDataObject : class, IDataObject
    {
        private object? _syncRoot;
        protected readonly BehaviorSubject<SourceList<TDataObject>> _item;

        public static IDataList<TDataObject> Create() => new DataList<TDataObject>();

        protected DataList()
        {
            _item = new BehaviorSubject<SourceList<TDataObject>>(new SourceList<TDataObject>());

            Changes = CreateChangesSubject();

            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<IChangeList<TDataObject>>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<IChangeItem<object?>>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Complete)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    _item.Value.RemoveAt(key);
                });
            });
        }

        private ISubject<IEnumerable<IChange>> CreateChangesSubject()
            => Subject.Create<IEnumerable<IChange>>(
                    CreateObserverForChangesSubject(),
                    CreateObservableForChangesSubject());

        private IObserver<IEnumerable<IChange>> CreateObserverForChangesSubject()
            => Observer.Create<IEnumerable<IChange>>(changes =>
            {
                changes.ForEach(change =>
                {
                    if (change is IChangeItem<TDataObject>)
                    {
                        //TODO: The whole list gets replaced
                        //item.OnNext(change);
                    }
                    else if (change is IChangeList<TDataObject> childChange)
                    {
                        childChange.Changes.ForEach(innerChange =>
                        {
                            if (innerChange is IChangeItem<object?> innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                            {
                                var changeType = innerChange.GetType().GetTypesIfImplemented(typeof(IChange<>)).Last().GetGenericArguments().Single();
                                if (changeType != null)
                                {
                                    var dataObject = (TDataObject)DataObject.Create(changeType, innerValueChange.Value);
                                    Add(childChange.Key, dataObject);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Received an invalid IChangeItem that is not implementing IChange<>");
                                }
                            }
                            else
                            {
                                Get<TDataObject>(childChange.Key).NullThrow().Changes.OnNext(innerChange.Yield());
                            }
                        });
                    }
                });
            });

        private IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeItem<IDataList<TDataObject>>.Create(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IDataObject>>>(
                    _item.Switch()
                    .Transform((dO, index) => (DataObject: dO, Index: index))
                    .MergeMany(dOWithIndex => 
                        dOWithIndex.DataObject.Changes
                        .Select(changes => ChangeList<TDataObject>.Create(dOWithIndex.Index, changes.Cast<IChange<TDataObject>>()).Yield())));

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        #region Common List implementations
        public TDataObject this[int key]
        {
            get => Get<TDataObject>(key) ?? throw new ArgumentOutOfRangeException($"No value under Key \"{key}\" available");
            set => _item.Value.ReplaceAt(key, value);
        }
        IDataObject IDataList.this[int key] { get => this[key]; set => this[key] = (TDataObject)value; }
        object IList.this[int key] { get => this[key]; set => this[key] = (TDataObject)value; }
        public int Count => _item.Value.Count;
        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot => _syncRoot ?? (_syncRoot = new object());
        public int IndexOf(TDataObject item) => _item.Value.Items.IndexOf(item);
        public int IndexOf(object value)
            => value is TDataObject dataObjectValue ? IndexOf(dataObjectValue) : -1;
        public void Insert(int index, TDataObject item) => _item.Value.Insert(index, item);
        public void Insert(int index, object value)
        {
            if (value is TDataObject dataObjectValue)
            {
                Insert(index, dataObjectValue);
            }
        }
        public void RemoveAt(int index) => _item.Value.RemoveAt(index);
        public void Add(TDataObject item) => _item.Value.Add(item);
        public int Add(object value)
        {
            if (value is TDataObject dataObjectValue)
            {
                _item.Value.Add(dataObjectValue);
                return _item.Value.Count - 1;
            }
            return -1;
        }
        public void Clear() => _item.Value.Clear();
        public bool Contains(TDataObject item) => _item.Value.Items.Contains(item);
        public bool Contains(object value)
            => value is TDataObject dataObjectValue ? Contains(dataObjectValue) : false;
        public void CopyTo(TDataObject[] array, int arrayIndex) => _item.Value.Items.Select((dO, index) => (dO, index)).ForEach(item => array[arrayIndex + item.index] = item.dO);
        public void CopyTo(Array array, int index)
        {
            if (!(array is TDataObject[] tdArray)) throw new ArgumentException("array");
            CopyTo(tdArray, index);
        }
        public bool Remove(TDataObject item) => _item.Value.Remove(item);
        public bool Remove(object value) => value is TDataObject dataObjectValue ? Remove(dataObjectValue) : false;
        void IList.Remove(object value) => Remove(value);
        public IEnumerator<TDataObject> GetEnumerator() => _item.Value.Items.Select(dO => dO).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region interface
        public QDataObject? Get<QDataObject>(int key) where QDataObject : class, TDataObject
            => (QDataObject)_item.Value.Items.ElementAt(key);

        public void Add(int key, TDataObject dataObject) => _item.Value.Insert(key, dataObject);

        public void Remove(int key) => _item.Value.RemoveAt(key);
        IDataObject? IDataList.Get(int key) => Get<TDataObject>(key);
        void IDataList.Add(int key, IDataObject dataObject) => Add(key, (TDataObject)dataObject);
        #endregion
    }
}