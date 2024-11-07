// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="ChangeDictionary.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using System;
    using System.Collections.Generic;

    public interface IChangeDictionary<TKey, TEntity> : IChange<IEntityDictionary<TKey, TEntity>> where TEntity : class, IEntity
    {
        TKey Key { get; }
        IEnumerable<IChange<TEntity>> Changes { get; }
    }

    public abstract class ChangeDictionary : IChangeDictionary<object, IEntity>
    {
        public abstract object Key { get; }
        public abstract IEnumerable<IChange<IEntity>> Changes { get; }
    }

    public class ChangeDictionary<TKey, TEntity> : ChangeDictionary, IChangeDictionary<TKey, TEntity> where TEntity : class, IEntity
    {
        private readonly TKey _key;
        private readonly IEnumerable<IChange<TEntity>> _changes;

        public static IChangeDictionary<TKey, TEntity> Create(TKey key, IEnumerable<IChange<TEntity>> changes)
             => (typeof(TKey) == typeof(string) && typeof(TEntity) == typeof(IEntity)) ?
            (IChangeDictionary<TKey, TEntity>) ChangeDirectory<TKey>.Create(key, changes) :
            new ChangeDictionary<TKey, TEntity>(key, changes);

        protected ChangeDictionary(TKey key, IEnumerable<IChange<TEntity>> changes)
        {
            _key = key ?? throw new ArgumentNullException();
            _changes = changes;
        }

        TKey IChangeDictionary<TKey, TEntity>.Key => _key;
        public override object Key => _key!;
        IEnumerable<IChange<TEntity>> IChangeDictionary<TKey, TEntity>.Changes => _changes;
        public override IEnumerable<IChange<IEntity>> Changes => _changes;
    }
}