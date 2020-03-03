// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataChange{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.DataSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Flags]
    public enum DataChangeType : int
    {
        Created = 1,
        Connected = 2,
        Modify = 4,
        Remove = 8,
        Request = 16,
    }



    public abstract class DataChange
    {
        private List<object> _keyChain = new List<object>();

        protected DataChange(DataChangeType changeType, object key, object value = null) : this(changeType, new List<object> {key}, value)
        {
            ChangeType = changeType;
            ForwardUp(key);
            Value = value;
        }

        protected DataChange(DataChangeType changeType, IEnumerable<object> keyChain, object value = null)
        {
            ChangeType = changeType;
            _keyChain = keyChain.ToList();
            Value = value;
        }

        public DataChangeType ChangeType { get; }
        public IReadOnlyList<object> KeyChain => _keyChain;
        public object Value { get; }

        public DataChange ForwardUp(object key) { return ReplicateType(ChangeType, _keyChain.Prepend(key), Value); }

        public DataChange ForwardDown(object key)
        {
            if (EqualityComparer<object>.Default.Equals(key, _keyChain[0]))
            {
                return ReplicateType(ChangeType, _keyChain.Skip(1), Value);
            }

            throw new InvalidOperationException();
        }

        public abstract DataChange ReplicateType(DataChangeType changeType, IEnumerable<object> keyChain, object value);
    }

    public class DataChange<TData> : DataChange
    {
        internal DataChange(DataChangeType changeType, object key, TData value = default(TData)) : this(changeType, new List<object> {key}, value) { }

        internal DataChange(DataChangeType changeType, IEnumerable<object> keyChain, TData value = default(TData)) : base(changeType, keyChain, value) { }

        public new TData Value => (TData) base.Value;

        public override DataChange ReplicateType(DataChangeType changeType, IEnumerable<object> keyChain, object value)
        {
            return Create(changeType, keyChain, value is TData genValue ? genValue : default(TData));
        }

        public static DataChange<TData> Create(DataChangeType changeType, object key, TData value = default(TData))
        {
            return new DataChange<TData>(changeType, key, value);
        }
        public static DataChange<TData> Create(DataChangeType changeType, IEnumerable<object> keyChain, TData value = default(TData))
        {
            return new DataChange<TData>(changeType, keyChain, value);
        }
    }
}