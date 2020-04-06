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
        protected readonly BehaviorSubject<SourceList<TDataObject>> _item;

        internal DataList()
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
                                .OfType<IChangeItem>()
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
                            if (innerChange is IChangeItem innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                            {
                                var changeType = innerChange.GetType().GetTypeIfImplemented(typeof(IChange<>))?.GetGenericArguments().Single();
                                if (changeType != null)
                                {
                                    var dataObject = (TDataObject)FlowRx.Create.Data.Object(changeType, innerValueChange.Value);
                                    Connect(childChange.Key, dataObject);
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
            => Observable.Return(FlowRx.Create.Change.Item<IDataList<TDataObject>>(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IDataObject>>>(
                    _item.Switch()
                    .Transform((dO, index) => (DataObject: dO, Index: index))
                    .MergeMany(dOWithIndex => 
                        dOWithIndex.DataObject.Changes
                        .Select(changes => FlowRx.Create.Change.List(dOWithIndex.Index, changes.Cast<IChange<TDataObject>>()).Yield())));

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        public QDataObject Create<QDataObject>(int key, Func<QDataObject> creator) where QDataObject : TDataObject
        {
            var data = creator();
            Connect(key, data);
            return data;
        }

        public QDataObject? Get<QDataObject>(int key) where QDataObject : class, TDataObject
            => (QDataObject) _item.Value.Items.ElementAt(key);

        public void Connect(int key, TDataObject dataObject) => _item.Value.Insert(key, dataObject);

        public void Disconnect(int key) => _item.Value.RemoveAt(key);

        #region common list implementations
        public TDataObject this[int key]
        {
            get => Get<TDataObject>(key) ?? throw new ArgumentOutOfRangeException($"No value under Key \"{key}\" available");
            set => _item.Value.ReplaceAt(key, value);
        }
        IDataObject IDataList.this[int key] { get => this[key]; set => this[key] = (TDataObject)value; }
        public int Count => _item.Value.Count;
        public bool IsReadOnly => false;
        public int IndexOf(TDataObject item) => _item.Value.Items.IndexOf(item);
        public void Insert(int index, TDataObject item) => _item.Value.Insert(index, item);
        public void RemoveAt(int index) => _item.Value.RemoveAt(index);
        public void Add(TDataObject item) => _item.Value.Add(item);
        public void Clear() => _item.Value.Clear();
        public bool Contains(TDataObject item) => _item.Value.Items.Contains(item);
        public void CopyTo(TDataObject[] array, int arrayIndex) => _item.Value.Items.Select((dO, index) => (dO, index)).ForEach(item => array[arrayIndex + item.index] = item.dO);
        public bool Remove(TDataObject item) => _item.Value.Remove(item);
        public IEnumerator<TDataObject> GetEnumerator() => _item.Value.Items.Select(dO => dO).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region ungeneric interface
        IDataObject IDataList.Create(int key, Func<IDataObject> creator)
            => Create(
                key,
                () => creator() is TDataObject tData ? tData : throw new ArgumentException("Type of created object does not fit to list type"));
        IDataObject? IDataList.Get(int key) => Get<TDataObject>(key);
        void IDataList.Connect(int key, IDataObject dataObject) => Connect(key, (TDataObject)dataObject);
        #endregion
    }
}