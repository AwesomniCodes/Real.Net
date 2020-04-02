// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeFactory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Awesomni.Codes.FlowRx
{
    public interface IChangeFactory
    {
        IChangeDictionary<TKey, TDataObject> Dictionary<TKey, TDataObject>(TKey key, IEnumerable<IChange<TDataObject>> changes) where TDataObject : class, IDataObject;
        IChangeDirectory Directory(string key, IEnumerable<IChange<IDataObject>> changes);
        IChangeItem<TData> Item<TData>(ChangeType changeType, TData value = default);
        IChangeList<TDataObject> List<TDataObject>(int index, IEnumerable<IChange<TDataObject>> changes) where TDataObject : class, IDataObject;
    }
}