﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="ChangeList.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using System.Collections.Generic;

    public interface IChangeList<TEntity> : IChange<IEntityList<TEntity>> where TEntity : class, IEntity
    {
        int Key { get; }
        IEnumerable<IChange<TEntity>> Changes { get; }
    }

    public abstract class ChangeList : IChangeList<IEntity>
    {
        public abstract int Key { get; }
        public abstract IEnumerable<IChange<IEntity>> Changes { get; }
    }

    public class ChangeList<TEntity> : ChangeList, IChangeList<TEntity> where TEntity : class, IEntity
    {
        private readonly IEnumerable<IChange<TEntity>> _changes;

        public static IChangeList<TEntity> Create(int key, IEnumerable<IChange<TEntity>> changes)
            => new ChangeList<TEntity>(key, changes);
        protected ChangeList(int key, IEnumerable<IChange<TEntity>> changes)
        {
            Key = key;
            _changes = changes;
        }
        public override int Key { get; }

        IEnumerable<IChange<TEntity>> IChangeList<TEntity>.Changes => _changes;
        public override IEnumerable<IChange<IEntity>> Changes => _changes;
    }
}