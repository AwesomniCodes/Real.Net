// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataDictionary.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IDataDictionary : IDataObject, IEnumerable/*, ICollection, IDictionary*/
    {
        IDataObject Create(object key, Func<IDataObject> creator);

        IDataObject GetOrCreate(object key, Func<IDataObject> creator)
            => Get(key) ?? Create(key, creator);

        IDataObject? Get(object key);


        void Connect(object key, IDataObject dataObject);

        void Disconnect(object key);

        void Copy(object sourceKey, object destinationKey);

        void Move(object sourceKey, object destinationKey);

        IDataObject this[object index] { get; set; }
    }

    public interface IDataDictionary<TKey, TDataObject> : IDataDictionary, IEnumerable<TDataObject>, ICollection<KeyValuePair<TKey, TDataObject>>, IEnumerable<KeyValuePair<TKey, TDataObject>>, IEnumerable, IDictionary<TKey, TDataObject>,
        IReadOnlyCollection<KeyValuePair<TKey, TDataObject>>, IReadOnlyDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        QDataObject Create<QDataObject>(TKey key, Func<QDataObject> creator) where QDataObject : TDataObject;

        QDataObject GetOrCreate<QDataObject>(TKey key, Func<QDataObject> creator) where QDataObject : class, TDataObject
            => Get<QDataObject>(key) ?? Create(key, creator);

        QDataObject? Get<QDataObject>(TKey key) where QDataObject : class, TDataObject;


        void Connect(TKey key, TDataObject dataObject);

        void Disconnect(TKey key);

        void Copy(TKey sourceKey, TKey destinationKey);

        void Move(TKey sourceKey, TKey destinationKey);

        //TDataObject this[TKey index] { get; set; }
    }
}