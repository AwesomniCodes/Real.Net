// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="ChangeDirectory.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------


namespace Awesomni.Codes.Real.Net
{
    using System;
    using System.Collections.Generic;

    public interface IChangeDirectory<TKey> : IChangeDictionary<TKey, IEntity> { }

    public abstract class ChangeDirectoryBase<TKey> : ChangeDictionary<TKey, IEntity>, IChangeDirectory<object>
    {
        protected ChangeDirectoryBase(TKey key, IEnumerable<IChange<IEntity>> changes) : base(key, changes)
        {
        }
    }

    public class ChangeDirectory<TKey> : ChangeDirectoryBase<TKey>, IChangeDirectory<TKey>
    {
        public static new IChangeDirectory<TKey> Create(TKey key, IEnumerable<IChange<IEntity>> changes)
            => new ChangeDirectory<TKey>(key, changes);
        protected ChangeDirectory(TKey key, IEnumerable<IChange<IEntity>> changes) : base(key, changes) { }
    }
}