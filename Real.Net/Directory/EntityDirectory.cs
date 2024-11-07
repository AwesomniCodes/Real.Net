// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDirectory.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using Awesomni.Codes.Real.Net.Utility;
    using DynamicData;
    using DynamicData.Kernel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;

    public abstract class EntityDirectoryBase<TKey> : EntityDictionary<TKey, IEntity>, IEntityDirectory<object> { }
    public class EntityDirectory<TKey> : EntityDirectoryBase<TKey>, IEntityDirectory<TKey>
    {
        public static new IEntityDirectory<TKey> Create() => new EntityDirectory<TKey>();

        protected EntityDirectory() { }
        protected override IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeSubject<IEntityDirectory<TKey>>.Create(ChangeType.Definition).Yield())
               .Concat<IEnumerable<IChange<IEntity>>>(
                    _item.Switch()
                    .MergeMany(kE =>
                        kE.Entity.Changes
                        .Select(changes => ChangeDirectory<TKey>.Create(kE.Key, changes.Cast<IChange<IEntity>>()).Yield())));
    }
}