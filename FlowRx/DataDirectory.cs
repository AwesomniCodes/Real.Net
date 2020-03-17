// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDirectory.cs" project="FlowRx" solution="FlowRx" />
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

    public class DataDirectory<TDataObject> : DataObject, IDataDirectory<TDataObject> where TDataObject : class, IDataObject
    {
        private readonly BehaviorSubject<SourceCache<(string Key, TDataObject DataObject), string>> item;
        private readonly ISubject<IEnumerable<IChange<IDataObject>>> _outSubject;
        private IDataDirectory<TDataObject> This => this;
        public static Func<IDataDirectory<TDataObject>> Creation => () => new DataDirectory<TDataObject>();
        private DataDirectory()
        {
            item = new BehaviorSubject<SourceCache<(string Key, TDataObject DataObject), string>>(new SourceCache<(string Key, TDataObject DataObject), string>(o => o.Key));

            _outSubject = new Subject<IEnumerable<IChange<IDataObject>>>();
            var childChangesObservable = item.Switch().MergeMany(dO => dO.DataObject.Changes.Select(changes => DirectoryChange<TDataObject>.Creation(dO.Key, changes.Cast<IChange<TDataObject>>())().Yield()));
            var outObservable = Observable.Return(DataItemChange<IDataDirectory<TDataObject>>.Creation(ChangeType.Create)().Yield()).Concat(_outSubject.Merge(childChangesObservable));
            Changes = Subject.Create<IEnumerable<IChange<IDataObject>>>(Observer.Create<IEnumerable<IChange<IDataObject>>>(OnChangesIn), outObservable);


            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<IDirectoryChange<TDataObject>>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<IDataItemChange>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Complete)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    item.Value.Remove(key.ToString());
                });
            });
        }

        public override ISubject<IEnumerable<IChange<IDataObject>>> Changes { get; }
        public IEnumerator<IDataObject> GetEnumerator() { return item.Value.Items.Select(dO => dO.DataObject).GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public QDataObject Create<QDataObject>(string key, Func<QDataObject> creator) where QDataObject : TDataObject
        {
            var data = creator();
            Connect(key, data);
            return data;
        }

        public QDataObject Get<QDataObject>(string key) where QDataObject : class, TDataObject
            => (QDataObject) item.Value.Lookup(key).ValueOrDefault().DataObject;

        public void Connect(string key, TDataObject dataObject)
        {
            item.Value.AddOrUpdate((key, dataObject));
        }

        public void Disconnect(string key)
        {
            var dOItem = item.Value.Lookup(key).ValueOrDefault();
            item.Value.Remove(key);
        }

        private void OnChangesIn(IEnumerable<IChange<IDataObject>> changes)
        {
            changes.ForEach(change =>
            {
                if (change is IDataItemChange<TDataObject>)
                {
                    //TODO: The whole directory gets replaced
                    //item.OnNext(change);
                }
                else if(change is IDirectoryChange<IDataObject> childChange)
                {
                    childChange.Changes.ForEach(innerChange =>
                    {
                        if (innerChange is IDataItemChange innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                        {
                            if (innerChange is IDataItemChange<IDataDirectory<TDataObject>> innerDirectoryValueChange)
                            {
                                    This.GetOrCreate(childChange.Key.ToString(), () => (TDataObject) DataDirectory<IDataObject>.Creation());
                            }
                            else
                            {
                                //Get type of value change here and provide it when value is null, instead
                                if (innerValueChange.Value == null)
                                {
                                    var innerValueChangeType = innerValueChange.GetType().GetGenericArguments().Single();
                                    This.GetOrCreate(childChange.Key.ToString(), () => (TDataObject) DataItem.Creation(innerValueChangeType)());
                                }
                                else
                                {
                                    This.GetOrCreate(childChange.Key.ToString(), () => (TDataObject) DataItem.Creation(innerValueChange.Value)());
                                }
                            }
                        }
                        else
                        {
                            Get<TDataObject>(childChange.Key.ToString()).NullThrow().Changes.OnNext(innerChange.Yield());
                        }

                    });
                }
            });
        }

        public void Copy(string sourceKey, string destinationKey)
        {
            throw new NotImplementedException();
        }

        public void Move(string sourceKey, string destinationKey)
        {
            throw new NotImplementedException();
        }

    }
}