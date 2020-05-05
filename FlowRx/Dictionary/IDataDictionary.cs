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

    public interface IDataDictionary<TKey, TDataObject> : IDataObject, IEnumerable, IEnumerable<TDataObject>, ICollection<KeyValuePair<TKey, TDataObject>>, IEnumerable<KeyValuePair<TKey, TDataObject>>, IDictionary<TKey, TDataObject>,
        IReadOnlyCollection<KeyValuePair<TKey, TDataObject>>, IReadOnlyDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        TDataObject GetOrAdd(TKey key, Func<TDataObject> creator)
        {
            TDataObject CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get(key) ?? CreateAndAdd();
        }

        TDataObject? Get(TKey key);

        void Copy(TKey sourceKey, TKey destinationKey);

        void Move(TKey sourceKey, TKey destinationKey);
    }
}