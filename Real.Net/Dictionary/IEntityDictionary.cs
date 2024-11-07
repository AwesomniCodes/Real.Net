// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IEntityDictionary.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using Awesomni.Codes.Real.Net.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IEntityDictionary<TKey, TEntity> : IEntity, IDictionary<TKey, TEntity> where TEntity : class, IEntity
    {
        TEntity GetOrAdd(TKey key, Func<TEntity> creator)
        {
            TEntity CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get(key) ?? CreateAndAdd();
        }

        TEntity? Get(TKey key);

        void Copy(TKey sourceKey, TKey destinationKey);

        void Move(TKey sourceKey, TKey destinationKey);
    }
}