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
        private readonly BehaviorSubject<SourceCache<(TKey Key, TDataObject DataObject), TKey>> item;
        private readonly ISubject<IEnumerable<IChange<IDataObject>>> _outSubject;
        private IDataDictionary<TKey, TDataObject> This => this;
        internal DataDictionary()
        {
            item = new BehaviorSubject<SourceCache<(TKey Key, TDataObject DataObject), TKey>>(new SourceCache<(TKey Key, TDataObject DataObject), TKey>(o => o.Key));

            _outSubject = new Subject<IEnumerable<IChange<IDataObject>>>();
            var childChangesObservable = item.Switch().MergeMany(dO => dO.DataObject.Changes.Select(changes => FlowRx.Create.Change.Dictionary<TKey, TDataObject>(dO.Key, changes.Cast<IChange<TDataObject>>()).Yield()));
            var outObservable = Observable.Return(FlowRx.Create.Change.Item<IDataDictionary<TKey, TDataObject>>(ChangeType.Create).Yield()).Concat(_outSubject.Merge(childChangesObservable));
            Changes = Subject.Create<IEnumerable<IChange>>(Observer.Create<IEnumerable<IChange>>(OnChangesIn), outObservable);


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

        public override ISubject<IEnumerable<IChange>> Changes { get; }
        public IEnumerator<TDataObject> GetEnumerator() => item.Value.Items.Select(dO => dO.DataObject).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public QDataObject Create<QDataObject>(TKey key, Func<QDataObject> creator) where QDataObject : TDataObject
        {
            var data = creator();
            Connect(key, data);
            return data;
        }

        public QDataObject Get<QDataObject>(TKey key) where QDataObject : class, TDataObject
            => (QDataObject) item.Value.Lookup(key).ValueOrDefault().DataObject;

        public void Connect(TKey key, TDataObject dataObject)
        {
            item.Value.AddOrUpdate((key, dataObject));
        }


        public void Disconnect(TKey key)
        {
            var dOItem = item.Value.Lookup(key).ValueOrDefault();
            item.Value.Remove(key);
        }

        private void OnChangesIn(IEnumerable<IChange> changes)
        {
            changes.ForEach(change =>
            {
                if (change is IChangeItem<TDataObject>)
                {
                    //TODO: The whole dictionary gets replaced
                    //item.OnNext(change);
                }
                else if(change is IChangeDictionary<TKey, TDataObject> childChange)
                {
                    childChange.Changes.ForEach(innerChange =>
                    {
                        if (innerChange is IChangeItem innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                        {
                            var changeItemType = innerChange.GetType().GetGenericArguments().Single();

                            if (changeItemType.IsGenericType && changeItemType.GetGenericTypeDefinition() == typeof(IDataDictionary<,>))
                            {
                                var dictionaryTypes = changeItemType.GetGenericArguments();
                                This.GetOrCreate(childChange.Key, () => (TDataObject) FlowRx.Create.Data.Dictionary(dictionaryTypes[0], dictionaryTypes[1]));
                            }
                            else
                            {
                                //Get type of value change here and provide it when value is null, instead
                                if (innerValueChange.Value == null)
                                {
                                    var innerValueChangeType = innerValueChange.GetType().GetGenericArguments().Single();
                                    This.GetOrCreate(childChange.Key, () => (TDataObject) FlowRx.Create.Data.Item(innerValueChangeType));
                                }
                                else
                                {
                                    This.GetOrCreate(childChange.Key, () => (TDataObject) FlowRx.Create.Data.Item(innerValueChange.Value));
                                }
                            }
                        }
                        else
                        {
                            Get<TDataObject>(childChange.Key).NullThrow().Changes.OnNext(innerChange.Yield());
                        }

                    });
                }
            });
        }

        public void Copy(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();

        public void Move(TKey sourceKey, TKey destinationKey) => throw new NotImplementedException();


        public IDataObject Create(object key, Func<IDataObject> creator) => Create((TKey)key, creator);
        public IDataObject? Get(object key) => Get((TKey)key);
        public void Connect(object key, IDataObject dataObject) => Connect((TKey) key, (TDataObject)dataObject);
        public void Disconnect(object key) => Disconnect((TKey)key);
        public void Copy(object sourceKey, object destinationKey) => Copy((TKey)sourceKey, (TKey)destinationKey);
        public void Move(object sourceKey, object destinationKey) => Move((TKey)sourceKey, (TKey)destinationKey);

    }
}