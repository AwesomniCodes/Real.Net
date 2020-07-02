// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------


namespace Awesomni.Codes.FlowRx
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