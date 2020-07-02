// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IEntityDictionary.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IEntityDictionary<TKey, TEntity> : IEntity, IEnumerable, IEnumerable<TEntity>, ICollection<KeyValuePair<TKey, TEntity>>, IEnumerable<KeyValuePair<TKey, TEntity>>, IDictionary<TKey, TEntity>,
        IReadOnlyCollection<KeyValuePair<TKey, TEntity>>, IReadOnlyDictionary<TKey, TEntity> where TEntity : class, IEntity
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