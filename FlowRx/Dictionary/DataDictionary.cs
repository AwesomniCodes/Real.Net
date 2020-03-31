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
        protected readonly BehaviorSubject<SourceCache<(TKey Key, TDataObject DataObject), TKey>> item;

        internal DataDictionary()
        {
            item = new BehaviorSubject<SourceCache<(TKey Key, TDataObject DataObject), TKey>>(new SourceCache<(TKey Key, TDataObject DataObject), TKey>(o => o.Key));

            Changes = CreateChangesSubject();

            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<IChangeDictionary<TKey, TDataObject>>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<IChangeItem>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Complete)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    item.Value.Remove(key);
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
                            if (innerChange is IChangeItem innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                            {
                                var changeType = innerChange.GetType().GetTypeIfImplemented(typeof(IChange<>))?.GetGenericArguments().Single()!;

                                Create(childChange.Key, () => (TDataObject)FlowRx.Create.Data.Object(changeType, innerValueChange.Value));
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
            => Observable.Return(FlowRx.Create.Change.Item<IDataDictionary<TKey, TDataObject>>(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IDataObject>>>(
                    item.Switch()
                    .MergeMany(dO =>
                        dO.DataObject.Changes
                        .Select(changes => FlowRx.Create.Change.Dictionary(dO.Key, changes.Cast<IChange<TDataObject>>()).Yield())));

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        public IEnumerator<TDataObject> GetEnumerator() => item.Value.Items.Select(dO => dO.DataObject).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public QDataObject Create<QDataObject>(TKey key, Func<QDataObject> creator) where QDataObject : TDataObject
        {
            var data = creator();
            Connect(key, data);
            return data;
        }
        
        public QDataObject? Get<QDataObject>(TKey key) where QDataObject : class, TDataObject
            => item.Value.Lookup(key).ValueOrDefault().DataObject as QDataObject;

        public void Connect(TKey key, TDataObject dataObject) => item.Value.AddOrUpdate((key, dataObject));

        public void Disconnect(TKey key) => item.Value.Remove(key);

        public void Copy(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();

        public void Move(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();

        public TDataObject this[TKey key]
        {
            get => Get<TDataObject>(key) ?? throw new ArgumentOutOfRangeException("No value under key available");
            set => Connect(key, value);
        }

        #region ungeneric interface
        IDataObject IDataDictionary.Create(object key, Func<IDataObject> creator)
            => Create(
                (TKey)key,
                () => creator() is TDataObject tData ? tData : throw new ArgumentException("Type of created object does not fit to dictionary type"));
        IDataObject? IDataDictionary.Get(object key) => item.Value.Lookup((TKey)key).ValueOrDefault().DataObject;
        void IDataDictionary.Connect(object key, IDataObject dataObject) => Connect((TKey)key, (TDataObject)dataObject);
        void IDataDictionary.Disconnect(object key) => Disconnect((TKey)key);
        void IDataDictionary.Copy(object sourceKey, object destinationKey) => Copy((TKey)sourceKey, (TKey)destinationKey);
        void IDataDictionary.Move(object sourceKey, object destinationKey) => Move((TKey)sourceKey, (TKey)destinationKey);
        IDataObject IDataDictionary.this[object index] { get => this[(TKey) index]; set => this[(TKey)index] = (TDataObject) value; }
        #endregion
    }
}