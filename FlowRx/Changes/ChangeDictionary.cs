// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeDictionary.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System.Collections.Generic;

    public interface IChangeDictionary : IChange<IDataDictionary>
    {
        object Key { get; }
        IEnumerable<IChange> Changes { get; }
    }

    public interface IChangeDictionary<TKey, TDataObject> : IChangeDictionary, IChange<IDataDictionary<TKey, TDataObject>> where TDataObject : class, IDataObject
    {
        new TKey Key { get; }
        new IEnumerable<IChange<TDataObject>> Changes { get; }
    }

    public class ChangeDictionary<TKey, TDataObject> : IChangeDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        public TKey Key { get; }

        internal ChangeDictionary(TKey key, IEnumerable<IChange<TDataObject>> changes)
        {
            Key = key;
            Changes = changes;
        }

        public IEnumerable<IChange<TDataObject>> Changes { get; private set; }

        object IChangeDictionary.Key => Key!;

        IEnumerable<IChange> IChangeDictionary.Changes => Changes;
    }
}