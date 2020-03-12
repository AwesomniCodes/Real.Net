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

    public class DataDirectory : DataObject, IDataDirectory
    {
        private readonly BehaviorSubject<SourceCache<(string Key, IDataObject DataObject), string>> item;
        private readonly ISubject<IEnumerable<SomeChange>> _outSubject;

        public static IDataDirectory Create() => new DataDirectory();
        private DataDirectory()
        {
            item = new BehaviorSubject<SourceCache<(string Key, IDataObject DataObject), string>>(new SourceCache<(string Key, IDataObject DataObject), string>(o => o.Key.ToString()));

            _outSubject = new Subject<IEnumerable<SomeChange>>();
            var childChangesObservable = item.Switch().MergeMany(dO => dO.DataObject.Changes.Select(changes => ChildChange.Create(dO.Key,changes).Yield()));
            var outObservable = Observable.Return(ValueChange<IDataDirectory>.Create(ChangeType.Create).Yield()).Concat(_outSubject.Merge(childChangesObservable));
            Changes = Subject.Create<IEnumerable<SomeChange>>(Observer.Create<IEnumerable<SomeChange>>(OnChangesIn), outObservable);


            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<ChildChange>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<ValueChange>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Complete)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    item.Value.Remove(key.ToString());
                });
            });
        }

        public override ISubject<IEnumerable<SomeChange>> Changes { get; }
        public IEnumerator<IDataObject> GetEnumerator() { return item.Value.Items.Select(dO => dO.DataObject).GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public IDataItem Create(string key, object value)
        {
            var data = DataItem.Create(value);
            Connect(key, data);
            return data;
        }

        public IDataItem Create(string key, Type type) => Create(key, type.GetDefault());

        public IDataItem<TData> Create<TData>(string key, TData value = default)
        {
            var data = DataItem<TData>.Create(value);
            Connect(key, data);
            return data;
        }

        public IDataDirectory CreateDirectory(string key)
        {
            var directory = DataDirectory.Create();
            Connect(key, directory);
            return directory;
        }

        //public IDataItem<TData> GetOrCreate<TData>(string key, TData value = default) => Get<TData>
        //{
        //    IDataItem<TData> data = item.Value.Lookup(key).ValueOrDefault().DataObject as IDataItem<TData>;

        //    if (data == null)
        //    {
        //        data = DataItem<TData>.Create();
        //        Connect(key, data);
        //    }

        //    return data;
        //}

        //public IDataDirectory GetOrCreateDirectory(string key)
        //{
        //    IDataDirectory data = item.Value.Lookup(key).ValueOrDefault().DataObject as IDataDirectory;

        //    if (data == null)
        //    {
        //        data = DataDirectory.Create();
        //        Connect(key, data);
        //    }

        //    return data;
        //}

        public IDataObject? Get(string key) => item.Value.Lookup(key).ValueOrDefault().DataObject;

        public void Connect(string key, IDataObject dataObject)
        {
            item.Value.AddOrUpdate((key, dataObject));
        }

        public void Disconnect(string key)
        {
            var dOItem = item.Value.Lookup(key).ValueOrDefault();
            item.Value.Remove(key);
        }

        private void OnChangesIn(IEnumerable<SomeChange> changes)
        {
            changes.ForEach(change =>
            {
                if (change is ValueChange<IDataDirectory>)
                {
                    //TODO: The whole directory gets replaced
                    //item.OnNext(change);
                }
                else if(change is ChildChange childChange)
                {
                    childChange.Changes.ForEach(innerChange =>
                    {
                        if (innerChange is ValueChange innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                        {
                            if (innerChange is ValueChange<IDataDirectory> innerDirectoryValueChange)
                            {
                                ((IDataDirectory)this).GetOrCreateDirectory(childChange.Key.ToString());
                            }
                            else
                            {
                                //Get type of value change here and provide it when value is null, instead
                                if(innerValueChange.Value == null)
                                {
                                    var innerValueChangeType = innerValueChange.GetType().GetGenericArguments().Single();
                                    ((IDataDirectory)this).GetOrCreate(childChange.Key.ToString(), innerValueChangeType);
                                }
                                else
                                {
                                    ((IDataDirectory)this).GetOrCreate(childChange.Key.ToString(), innerValueChange.Value);
                                }
                            }
                        }
                        else
                        {
                            Get(childChange.Key.ToString()).NullThrow().Changes.OnNext(innerChange.Yield());
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