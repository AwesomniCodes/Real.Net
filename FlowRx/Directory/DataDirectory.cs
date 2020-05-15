// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDictionary.cs" project="FlowRx" solution="FlowRx" />
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

    public abstract class DataDirectoryBase<TKey> : DataDictionary<TKey, IDataObject>, IDataDirectory<object>
    {

    }
    public class DataDirectory<TKey> : DataDirectoryBase<TKey>, IDataDirectory<TKey>
    {
        public static new IDataDirectory<TKey> Create() => new DataDirectory<TKey>();
        static DataDirectory() => DataObject.InterfaceToClassTypeMap[typeof(IDataDirectory<>)] = typeof(DataDirectory<>);

        protected DataDirectory() { }
        protected override IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeItem<IDataDirectory<TKey>>.Create(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IDataObject>>>(
                    _item.Switch()
                    .MergeMany(dO =>
                        dO.DataObject.Changes
                        .Select(changes => ChangeDirectory<TKey>.Create(dO.Key, changes.Cast<IChange<IDataObject>>()).Yield())));
    }
}