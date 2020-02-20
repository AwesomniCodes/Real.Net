// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.DataSystem
{
    using DynamicData;
    using DynamicData.Kernel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Reflection;

    public class DataDirectory : DataObject, IDataDirectory // , IEnumerable<DataObject>
    {
        private readonly BehaviorSubject<SourceCache<IDataObject, string>> item;
        private readonly ISubject<DataChange> _dataChangeObservable;

        private bool _isSyncUpdate;

        public DataDirectory(object key) : base(key)
        {
            item = new BehaviorSubject<SourceCache<IDataObject, string>>(new SourceCache<IDataObject, string>(o => o.Key.ToString()));

            _dataChangeObservable = new Subject<DataChange>();
            Link = Subject.Create<DataChange>(Observer.Create<DataChange>(OnDataLinkNext), _dataChangeObservable);
        }

        public override ISubject<DataChange> Link { get; }
        public override IDataObject Clone() { throw new NotImplementedException(); }

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

                data.Link.Subscribe(change =>
                {
                    if (_isSyncUpdate)
                    {
                        change = change.CreateWithSameType(change.ChangeType | DataChangeType.Sync, change.KeyChain, change.Value);
                    }
                    _dataChangeObservable.OnNext(change.ForwardUp(Key));
                    if (change.ChangeType.HasFlag(DataChangeType.Remove))
                    {
                        item.Value.Remove(change.KeyChain[0]?.ToString());
                    }
                });
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
                _dataChangeObservable.OnNext(new DataChange<IDataDirectory>(DataChangeType.Created, key).ForwardUp(Key));

                data.Link.Subscribe(change =>
                {
                    if (_isSyncUpdate)
                    {
                        change = change.CreateWithSameType(change.ChangeType | DataChangeType.Sync, change.KeyChain, change.Value);
                    }
                    _dataChangeObservable.OnNext(change.ForwardUp(Key));
                    if (change.ChangeType.HasFlag(DataChangeType.Remove))
                    {
                        item.Value.Remove(change.KeyChain[0]?.ToString());
                    }
                });
            }

            return data;
        }

        public IDataItem<TData> Get<TData>(string key) => (IDataItem<TData>) Get(key);

        public IDataObject Get(string key)
        {
            return item.Value.Lookup(key?.ToString()).Value;
        }

        public void Delete(string key) {  }

        private void OnDataLinkNext(DataChange change)
        {
            if (!EqualityComparer<object>.Default.Equals(Key, change.KeyChain[0]))
            {
                throw new InvalidOperationException();
            }

            if (change.ChangeType.HasFlag(DataChangeType.Sync))
            {
                _isSyncUpdate = true;
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
                        childDataObject = (IDataObject) generic.Invoke(this,
                            new object[] {change.KeyChain[0], change.Value});
                    }
                }
                else
                {
                    childDataObject = Get(change.KeyChain[0]?.ToString());
                }

                childDataObject?.Link.OnNext(change);
            }
            _isSyncUpdate = false;
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