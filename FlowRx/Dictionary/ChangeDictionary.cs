// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeDictionary.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System.Collections.Generic;

    public interface IChangeDictionary<TKey, TDataObject> : IChange<IDataDictionary<TKey, TDataObject>> where TDataObject : class, IDataObject
    {
        TKey Key { get; }
        IEnumerable<IChange<TDataObject>> Changes { get; }
    }

    public abstract class ChangeDictionary : IChangeDictionary<object?, IDataObject>
    {
        public abstract object? Key { get; }
        public abstract IEnumerable<IChange<IDataObject>> Changes { get; }
    }

    public class ChangeDictionary<TKey, TDataObject> : ChangeDictionary, IChangeDictionary<TKey, TDataObject> where TDataObject : class, IDataObject
    {
        private readonly TKey _key;
        private readonly IEnumerable<IChange<TDataObject>> _changes;

        public static IChangeDictionary<TKey, TDataObject> Create(TKey key, IEnumerable<IChange<TDataObject>> changes)
             => (typeof(TKey) == typeof(string) && typeof(TDataObject) == typeof(IDataObject)) ?
            (IChangeDictionary<TKey, TDataObject>) ChangeDirectory.Create(key as string ?? string.Empty, changes) :
            new ChangeDictionary<TKey, TDataObject>(key, changes);

        protected ChangeDictionary(TKey key, IEnumerable<IChange<TDataObject>> changes)
        {
            _key = key;
            _changes = changes;
        }

        TKey IChangeDictionary<TKey, TDataObject>.Key => _key;
        public override object? Key => _key;
        IEnumerable<IChange<TDataObject>> IChangeDictionary<TKey, TDataObject>.Changes => _changes;
        public override IEnumerable<IChange<IDataObject>> Changes => _changes;
    }
}