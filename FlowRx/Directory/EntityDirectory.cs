// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
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

    public abstract class EntityDirectoryBase<TKey> : EntityDictionary<TKey, IEntity>, IEntityDirectory<object>
    {
        static EntityDirectoryBase() => Entity.InterfaceToClassTypeMap[typeof(IEntityDirectory<>)] = typeof(EntityDirectory<>);
    }
    public class EntityDirectory<TKey> : EntityDirectoryBase<TKey>, IEntityDirectory<TKey>
    {
        public static new IEntityDirectory<TKey> Create() => new EntityDirectory<TKey>();

        protected EntityDirectory() { }
        protected override IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeValue<IEntityDirectory<TKey>>.Create(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IEntity>>>(
                    _item.Switch()
                    .MergeMany(kE =>
                        kE.Entity.Changes
                        .Select(changes => ChangeDirectory<TKey>.Create(kE.Key, changes.Cast<IChange<IEntity>>()).Yield())));
    }
}