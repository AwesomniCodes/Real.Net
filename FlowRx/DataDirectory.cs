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
        private readonly BehaviorSubject<SourceCache<IDataObject, string>> item;
        private readonly ISubject<IEnumerable<SomeChange>> _outSubject;

        public DataDirectory(object key) : base(key)
        {
            item = new BehaviorSubject<SourceCache<IDataObject, string>>(new SourceCache<IDataObject, string>(o => o.Key.ToString()));

            _outSubject = new Subject<IEnumerable<SomeChange>>();
            var childChangesObservable = item.Switch().MergeMany(dO => dO.Changes.Select(changes => ChildChange.Create(Key,changes).Yield()));
            var outObservable = Observable.Return(ValueChange<IDataDirectory>.Create(ChangeType.Created, Key).Yield()).Concat(_outSubject.Merge(childChangesObservable));
            Changes = Subject.Create<IEnumerable<SomeChange>>(Observer.Create<IEnumerable<SomeChange>>(OnChangesIn), outObservable);
        }

        public override ISubject<IEnumerable<SomeChange>> Changes { get; }
        public IEnumerator<IDataObject> GetEnumerator() { return item.Value.Items.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public IDataItem<TData> GetOrCreate<TData>(string key, TData value = default(TData))
        {
            IDataItem<TData> data = item.Value.Lookup(key).ValueOrDefault() as IDataItem<TData>;

            if (data == null)
            {
                data = new DataItem<TData>(key, value);
                item.Value.AddOrUpdate(data);
            }

            return data;
        }

        public IDataDirectory GetOrCreateDirectory(string key)
        {
            IDataDirectory data = item.Value.Lookup(key).ValueOrDefault() as IDataDirectory;

            if (data == null)
            {
                data = new DataDirectory(key);
                item.Value.AddOrUpdate((IDataObject)data);
            }

            return data;
        }

        public IDataItem<TData> Get<TData>(string key) => (IDataItem<TData>) Get(key);

        public IDataObject Get(string key)
        {
            return item.Value.Lookup(key?.ToString()).Value;
        }

        public void Delete(string key) {  }

        private void OnChangesIn(IEnumerable<SomeChange> changes)
        {
            changes.ForEach(change =>
            {
                if (!EqualityComparer<object>.Default.Equals(Key, change.Key))
                {
                    throw new InvalidOperationException();
                }

                if (change is ValueChange<IDataDirectory>)
                {
                    //TODO: The whole directory gets replaced
                    //item.OnNext(change);
                }
                else if(change is ChildChange childChange)
                {
                    childChange.Changes.ForEach(innerChange =>
                    {
                        if (innerChange is ValueChange innerValueChange && innerValueChange.ChangeType == ChangeType.Created)
                        {
                            if (innerChange is ValueChange<IDataDirectory> innerDirectoryValueChange)
                            {
                                GetOrCreateDirectory(innerChange.Key.ToString());
                            }
                            else
                            {
                                MethodInfo method = GetType().GetMethod("GetOrCreate");
                                MethodInfo generic = method.MakeGenericMethod(innerValueChange.Value.GetType());
                                generic.Invoke(this, new object[] { innerValueChange.Key, innerValueChange.Value });
                            }
                        }
                        else
                        {
                            Get(innerChange.Key.ToString()).Changes.OnNext(innerChange.Yield());
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