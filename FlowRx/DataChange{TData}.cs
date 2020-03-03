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
    public enum ChangeType : int
    {
        Created = 1,
        Connected = 2,
        Modify = 4,
        Remove = 8,
        Request = 16,
    }



    public abstract class ValueChange
    {
        private List<object> _keyChain = new List<object>();

        protected ValueChange(ChangeType changeType, object key, object value = null) : this(changeType, new List<object> {key}, value)
        {
            ChangeType = changeType;
            ForwardUp(key);
            Value = value;
        }

        protected ValueChange(ChangeType changeType, IEnumerable<object> keyChain, object value = null)
        {
            ChangeType = changeType;
            _keyChain = keyChain.ToList();
            Value = value;
        }

        public ChangeType ChangeType { get; }
        public IReadOnlyList<object> KeyChain => _keyChain;
        public object Value { get; }

        public ValueChange ForwardUp(object key) { return ReplicateType(ChangeType, _keyChain.Prepend(key), Value); }

        public ValueChange ForwardDown(object key)
        {
            if (EqualityComparer<object>.Default.Equals(key, _keyChain[0]))
            {
                return ReplicateType(ChangeType, _keyChain.Skip(1), Value);
            }

            throw new InvalidOperationException();
        }

        public abstract ValueChange ReplicateType(ChangeType changeType, IEnumerable<object> keyChain, object value);
    }

    public class ValueChange<TData> : ValueChange
    {
        internal ValueChange(ChangeType changeType, object key, TData value = default(TData)) : this(changeType, new List<object> {key}, value) { }

        internal ValueChange(ChangeType changeType, IEnumerable<object> keyChain, TData value = default(TData)) : base(changeType, keyChain, value) { }

        public new TData Value => (TData) base.Value;

        public override ValueChange ReplicateType(ChangeType changeType, IEnumerable<object> keyChain, object value)
        {
            return Create(changeType, keyChain, value is TData genValue ? genValue : default(TData));
        }

        public static ValueChange<TData> Create(ChangeType changeType, object key, TData value = default(TData))
        {
            return new ValueChange<TData>(changeType, key, value);
        }
        public static ValueChange<TData> Create(ChangeType changeType, IEnumerable<object> keyChain, TData value = default(TData))
        {
            return new ValueChange<TData>(changeType, keyChain, value);
        }
    }
}