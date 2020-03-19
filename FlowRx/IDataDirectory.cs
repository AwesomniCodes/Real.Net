// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataDictionary.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IDataDictionary<TKey, TDataObject> : IDataObject, IEnumerable<IDataObject> where TDataObject : class, IDataObject
    {
        QDataObject Create<QDataObject>(TKey key, Func<QDataObject> creator) where QDataObject : TDataObject;

        QDataObject GetOrCreate<QDataObject>(TKey key, Func<QDataObject> creator) where QDataObject : class, TDataObject
            => Get<QDataObject>(key) ?? Create(key, creator);

        QDataObject? Get<QDataObject>(TKey key) where QDataObject : class, TDataObject;


        void Connect(TKey key, TDataObject dataObject);

        void Disconnect(TKey key);

        void Copy(TKey sourceKey, TKey destinationKey);

        void Move(TKey sourceKey, TKey destinationKey);
    }
}