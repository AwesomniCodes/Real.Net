// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.DataSystem
{
    using DynamicData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Reflection;

    public class DataDirectory : DataObject, IDataDirectory // , IEnumerable<DataObject>
    {
        private readonly BehaviorSubject<SourceCache<DataObject, string>> item;
        private readonly ISubject<DataUpdateInfo> _dataUpdateInfoObservable;

        private bool _isSyncUpdate;

        public DataDirectory(object key) : base(key)
        {
            item = new BehaviorSubject<SourceCache<DataObject, string>>(new SourceCache<DataObject, string>(o => o.Key.ToString()));

            _dataUpdateInfoObservable = new Subject<DataUpdateInfo>();
            Link = Subject.Create<DataUpdateInfo>(Observer.Create<DataUpdateInfo>(OnDataLinkNext), _dataUpdateInfoObservable);
        }

        public override ISubject<DataUpdateInfo> Link { get; }
        public override DataObject Clone() { throw new NotImplementedException(); }

        public IEnumerator<DataObject> GetEnumerator() { return item.Value.Items.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public DataItem<TData> GetOrCreate<TData>(string key, TData value = default(TData))
        {
            DataItem<TData> data;
            if (item.Value.Lookup(key).Value is DataItem<TData> dataItem)
            {
                data = dataItem;
            }
            else
            {
                data = new DataItem<TData>(key, value);
                item.Value.AddOrUpdate(data);

                data.Link.Subscribe(updateInfo =>
                {
                    if (_isSyncUpdate)
                    {
                        updateInfo = updateInfo.CreateWithSameType(updateInfo.UpdateType | DataUpdateType.Sync, updateInfo.KeyChain, updateInfo.Value);
                    }
                    _dataUpdateInfoObservable.OnNext(updateInfo.ForwardUp(Key));
                    if (updateInfo.UpdateType.HasFlag(DataUpdateType.Remove))
                    {
                        item.Value.Remove(updateInfo.KeyChain[0]?.ToString());
                    }
                });
            }

            return data;
        }

        public DataDirectory GetOrCreateDirectory(string key)
        {
            DataDirectory data;
            if (item.Value.Lookup(key).Value is DataDirectory dataDirectory)
            {
                data = dataDirectory;
            }
            else
            {
                data = new DataDirectory(key);
                item.Value.AddOrUpdate((DataObject)data);
                _dataUpdateInfoObservable.OnNext(new DataUpdateInfo<DataDirectory>(DataUpdateType.Created, key).ForwardUp(Key));

                data.Link.Subscribe(updateInfo =>
                {
                    if (_isSyncUpdate)
                    {
                        updateInfo = updateInfo.CreateWithSameType(updateInfo.UpdateType | DataUpdateType.Sync, updateInfo.KeyChain, updateInfo.Value);
                    }
                    _dataUpdateInfoObservable.OnNext(updateInfo.ForwardUp(Key));
                    if (updateInfo.UpdateType.HasFlag(DataUpdateType.Remove))
                    {
                        item.Value.Remove(updateInfo.KeyChain[0]?.ToString());
                    }
                });
            }

            return data;
        }

        public DataItem<TData> Get<TData>(string key) => (DataItem<TData>) Get(key);

        public DataObject Get(string key)
        {
            return item.Value.Lookup(key?.ToString()).Value;
        }

        public void Delete(string key) {  }

        private void OnDataLinkNext(DataUpdateInfo updateInfo)
        {
            if (!EqualityComparer<object>.Default.Equals(Key, updateInfo.KeyChain[0]))
            {
                throw new InvalidOperationException();
            }

            if (updateInfo.UpdateType.HasFlag(DataUpdateType.Sync))
            {
                _isSyncUpdate = true;
            }

            if (updateInfo.KeyChain.Count == 1)
            {
                //TODO: The whole directory gets replaced
                //item.OnNext(updateInfo);
            }
            else
            {
                updateInfo = updateInfo.ForwardDown(Key);

                DataObject childDataObject;

                if (updateInfo.KeyChain.Count == 1 && updateInfo.UpdateType.HasFlag(DataUpdateType.Created))
                {
                    if (updateInfo.GetType().GenericTypeArguments?.FirstOrDefault() == typeof(DataDirectory))
                    {
                        childDataObject = GetOrCreateDirectory(updateInfo.KeyChain[0]?.ToString());
                    }
                    else
                    {
                        MethodInfo method = GetType().GetMethod("GetOrCreate");
                        MethodInfo generic = method.MakeGenericMethod(updateInfo.Value.GetType());
                        childDataObject = (DataObject) generic.Invoke(this,
                            new object[] {updateInfo.KeyChain[0], updateInfo.Value});
                    }
                }
                else
                {
                    childDataObject = Get(updateInfo.KeyChain[0]?.ToString());
                }

                childDataObject?.Link.OnNext(updateInfo);
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