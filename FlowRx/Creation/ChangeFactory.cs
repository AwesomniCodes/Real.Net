// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeFactory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;
    using System.Reflection;

    public class ChangeFactory : IChangeFactory
    {
        public IChangeDictionary<TKey, TDataObject> Dictionary<TKey, TDataObject>(TKey key, IEnumerable<IChange<TDataObject>> changes) where TDataObject : class, IDataObject
            => (typeof(TKey) == typeof(string) && typeof(TDataObject) == typeof(IDataObject)) ?
            (IChangeDictionary<TKey, TDataObject>)Directory(key as string ?? string.Empty, changes) :
            ChangeDictionary<TKey, TDataObject>.Create(key, changes);

        public IChangeDirectory Directory(string key, IEnumerable<IChange<IDataObject>> changes)
            => ChangeDirectory.Create(key, changes);

    }
}