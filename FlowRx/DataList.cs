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
        protected readonly BehaviorSubject<SourceList<TDataObject>> item;

        internal DataList()
        {
            item = new BehaviorSubject<SourceList<TDataObject>>(new SourceList<TDataObject>());

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
                    item.Value.RemoveAt(key);
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
                    item.Switch()
                    .Transform((dO, index) => (DataObject: dO, Index: index))
                    .MergeMany(dOWithIndex => 
                        dOWithIndex.DataObject.Changes
                        .Select(changes => FlowRx.Create.Change.List<TDataObject>(dOWithIndex.Index, changes.Cast<IChange<TDataObject>>()).Yield())));

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        public IEnumerator<TDataObject> GetEnumerator() => item.Value.Items.Select(dO => dO).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public QDataObject Create<QDataObject>(int key, Func<QDataObject> creator) where QDataObject : TDataObject
        {
            var data = creator();
            Connect(key, data);
            return data;
        }

        public QDataObject Get<QDataObject>(int key) where QDataObject : class, TDataObject
            => (QDataObject) item.Value.Items.ElementAt(key);

        public void Connect(int key, TDataObject dataObject)
        {
            item.Value.Insert(key, dataObject);
        }

        public void Disconnect(int key)
        {
            item.Value.RemoveAt(key);
        }

        public void Copy(int sourceKey, int destinationKey) => throw new NotImplementedException();

        public void Move(int sourceKey, int destinationKey) => throw new NotImplementedException();

        public IDataObject Create(int key, Func<IDataObject> creator)
        {
            throw new NotImplementedException();
        }

        public IDataObject? Get(int key)
        {
            throw new NotImplementedException();
        }

        public void Connect(int key, IDataObject dataObject)
        {
            throw new NotImplementedException();
        }
    }
}