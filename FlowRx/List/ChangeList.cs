// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IChangeList.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System.Collections.Generic;

    public interface IChangeList<TDataObject> : IChange<IDataList<TDataObject>> where TDataObject : class, IDataObject
    {
        int Key { get; }
        IEnumerable<IChange<TDataObject>> Changes { get; }
    }

    public abstract class ChangeList : IChangeList<IDataObject>
    {
        public abstract int Key { get; }
        public abstract IEnumerable<IChange<IDataObject>> Changes { get; }
    }

    public class ChangeList<TDataObject> : ChangeList, IChangeList<TDataObject> where TDataObject : class, IDataObject
    {
        private readonly IEnumerable<IChange<TDataObject>> _changes;

        public static IChangeList<TDataObject> Create(int key, IEnumerable<IChange<TDataObject>> changes)
            => new ChangeList<TDataObject>(key, changes);
        protected ChangeList(int key, IEnumerable<IChange<TDataObject>> changes)
        {
            Key = key;
            _changes = changes;
        }
        public override int Key { get; }

        IEnumerable<IChange<TDataObject>> IChangeList<TDataObject>.Changes => _changes;
        public override IEnumerable<IChange<IDataObject>> Changes => _changes;
    }
}