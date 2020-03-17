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

    public interface IDataDirectory<TDataObject> : IDataObject, IEnumerable<IDataObject> where TDataObject : class, IDataObject
    {
        QDataObject Create<QDataObject>(string key, Func<QDataObject> creator) where QDataObject : TDataObject;

        QDataObject GetOrCreate<QDataObject>(string key, Func<QDataObject> creator) where QDataObject : class, TDataObject
            => Get<QDataObject>(key) ?? Create(key, creator);

        QDataObject? Get<QDataObject>(string key) where QDataObject : class, TDataObject;


        void Connect(string key, TDataObject dataObject);

        void Disconnect(string key);

        void Copy(string sourceKey, string destinationKey);

        void Move(string sourceKey, string destinationKey);
    }
}