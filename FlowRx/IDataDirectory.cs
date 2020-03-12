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
        TDataObject Create<TDataObject>(string key, Func<TDataObject> creator) where TDataObject : IDataObject;

        TDataObject GetOrCreate<TDataObject>(string key, Func<TDataObject> creator) where TDataObject : class, IDataObject
            => Get<TDataObject>(key) ?? Create(key, creator);

        TDataObject? Get<TDataObject>(string key) where TDataObject : class, IDataObject;


        void Connect(string key, IDataObject dataObject);

        void Disconnect(string key);

        void Copy(string sourceKey, string destinationKey);

        void Move(string sourceKey, string destinationKey);
    }
}