// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.DataSystem
{
    using Awesomni.Codes.FlowRx.Utility.Extensions;
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

    public class DataDirectory : DataObject, IDataDirectory // , IEnumerable<DataObject>
    {
        private readonly BehaviorSubject<SourceCache<IDataObject, string>> item;
        private readonly ISubject<IEnumerable<DataChange>> _outSubject;

        public DataDirectory(object key) : base(key)
        {
            item = new BehaviorSubject<SourceCache<IDataObject, string>>(new SourceCache<IDataObject, string>(o => o.Key.ToString()));

            _outSubject = new Subject<IEnumerable<DataChange>>();
            var childChangesObservable = item.Switch().MergeMany(dO => dO.Changes.Select(changes => changes.Select(change => change.ForwardUp(Key))));
            var outObservable = Observable.Return(DataChange<IDataDirectory>.Create(DataChangeType.Created, Key).Yield()).Concat(_outSubject.Merge(childChangesObservable));
            Changes = Subject.Create<IEnumerable<DataChange>>(Observer.Create<IEnumerable<DataChange>>(OnChangeIn), outObservable);
        }

        public override ISubject<IEnumerable<DataChange>> Changes { get; }
        public IEnumerator<IDataObject> GetEnumerator() { return item.Value.Items.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public IDataItem<TData> GetOrCreate<TData>(string key, TData value = default(TData))
        {
            IDataItem<TData> data;
            if (item.Value.Lookup(key).ValueOrDefault() is IDataItem<TData> dataItem)
            {
                data = dataItem;
            }
            else
            {
                data = new DataItem<TData>(key, value);
                item.Value.AddOrUpdate(data);
            }
            return data;
        }

        public IDataDirectory GetOrCreateDirectory(string key)
        {
            IDataDirectory data;
            if (item.Value.Lookup(key).ValueOrDefault() is IDataDirectory dataDirectory)
            {
                data = dataDirectory;
            }
            else
            {
                data = new DataDirectory(key);
                item.Value.AddOrUpdate((IDataObject)data);
                _outSubject.OnNext(DataChange<IDataDirectory>.Create(DataChangeType.Created, key).ForwardUp(Key).Yield());
            }

            return data;
        }

        public IDataItem<TData> Get<TData>(string key) => (IDataItem<TData>) Get(key);

        public IDataObject Get(string key)
        {
            return item.Value.Lookup(key?.ToString()).Value;
        }

        public void Delete(string key) {  }

        private void OnChangeIn(IEnumerable<DataChange> changes)
        {
            changes.ForEach(change =>
            {
                if (!EqualityComparer<object>.Default.Equals(Key, change.KeyChain[0]))
                {
                    throw new InvalidOperationException();
                }

                if (change.KeyChain.Count == 1)
                {
                    //TODO: The whole directory gets replaced
                    //item.OnNext(change);
                }
                else
                {
                    change = change.ForwardDown(Key);

                    IDataObject childDataObject;

                    if (change.KeyChain.Count == 1 && change.ChangeType.HasFlag(DataChangeType.Created))
                    {
                        if (change.GetType().GenericTypeArguments?.FirstOrDefault() == typeof(IDataDirectory))
                        {
                            childDataObject = GetOrCreateDirectory(change.KeyChain[0]?.ToString());
                        }
                        else
                        {
                            MethodInfo method = GetType().GetMethod("GetOrCreate");
                            MethodInfo generic = method.MakeGenericMethod(change.Value.GetType());
                            childDataObject = (IDataObject)generic.Invoke(this,
                                new object[] { change.KeyChain[0], change.Value });
                        }
                    }
                    else
                    {
                        childDataObject = Get(change.KeyChain[0]?.ToString());
                    }

                    childDataObject?.Changes.OnNext(change.Yield());
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