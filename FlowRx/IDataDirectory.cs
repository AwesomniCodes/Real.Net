// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IDataDirectory : IDataObject, IEnumerable<IDataObject>
    {
        IDataItem Create(string key, object value);
        IDataItem Create(string key, Type type);
        IDataItem<TData> Create<TData>(string key, TData value = default);

        IDataDirectory CreateDirectory(string key);

        IDataItem GetOrCreate(string key, object value) => (IDataItem?) Get(key) ?? Create(key, value);
        IDataItem GetOrCreate(string key, Type type) => (IDataItem?) Get(key) ?? Create(key, type);
        IDataItem<TData> GetOrCreate<TData>(string key, TData value = default) => Get<TData>(key) ?? Create(key, value);
        IDataDirectory GetOrCreateDirectory(string key) => GetDirectory(key) ?? CreateDirectory(key);

        IDataObject? Get(string key);
        IDataItem<TData>? Get<TData>(string key) => (IDataItem<TData>?)Get(key);
        IDataDirectory? GetDirectory(string key) => (IDataDirectory?) Get(key);



        void Connect(string key, IDataObject dataObject);

        void Disconnect(string key);

        void Copy(string sourceKey, string destinationKey);

        void Move(string sourceKey, string destinationKey);
    }
}