// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IChangeList.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System.Collections.Generic;

    public interface ChangeList : IChange<IDataList>
    {
        int Key { get; }
        IEnumerable<IChange> Changes { get; }
    }

    public interface IChangeList<TDataObject> : ChangeList, IChange<IDataList<TDataObject>> where TDataObject : class, IDataObject
    {
        new IEnumerable<IChange<TDataObject>> Changes { get; }
    }
    public class ChangeList<TDataObject> : IChangeList<TDataObject> where TDataObject : class, IDataObject
    {
        public int Key { get; }

        internal ChangeList(int key, IEnumerable<IChange<TDataObject>> changes)
        {
            Key = key;
            Changes = changes;
        }

        public IEnumerable<IChange<TDataObject>> Changes { get; private set; }

        IEnumerable<IChange> ChangeList.Changes => Changes;
    }
}