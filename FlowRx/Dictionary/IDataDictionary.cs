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
        IDataObject GetOrAdd(object key, Func<IDataObject> creator)
        {
            IDataObject CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get(key) ?? CreateAndAdd();
        }

        IDataObject? Get(object key);


        void Add(object key, IDataObject dataObject);

        bool Remove(object key);

        void Copy(object sourceKey, object destinationKey);

        void Move(object sourceKey, object destinationKey);

        IDataObject this[object index] { get; set; }
    }

    public interface IDataDictionary<TKey, TDataObject> : IDataDictionary, IEnumerable<TDataObject>, ICollection<KeyValuePair<TKey, TDataObject>>, IEnumerable<KeyValuePair<TKey, TDataObject>>, IEnumerable, IDictionary<TKey, TDataObject>,
        IReadOnlyCollection<KeyValuePair<TKey, TDataObject>>, IReadOnlyDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        QDataObject GetOrAdd<QDataObject>(TKey key, Func<QDataObject> creator) where QDataObject : class, TDataObject
        {
            QDataObject CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get<QDataObject>(key) ?? CreateAndAdd();
        } 

        QDataObject? Get<QDataObject>(TKey key) where QDataObject : class, TDataObject;

        void Copy(TKey sourceKey, TKey destinationKey);

        void Move(TKey sourceKey, TKey destinationKey);

        //TDataObject this[TKey index] { get; set; }
    }
}