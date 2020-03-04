// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataChange{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
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

    public abstract class SomeChange
    {
        public object Key { get; }

        protected SomeChange(object key) => Key = key;
    }

    public class ChildChange : SomeChange
    {
        private ChildChange(object key, IEnumerable<SomeChange> changes) : base(key)
        {
            Changes = changes;
        }

        public IEnumerable<SomeChange> Changes { get; private set; }

        public static ChildChange Create(object key, IEnumerable<SomeChange> changes) => new ChildChange(key, changes);
    }

    public abstract class ValueChange : SomeChange
    {
        protected ValueChange(ChangeType changeType, object key, object value = null) : base(key)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object Value { get; }
    }

    public class ValueChange<TData> : ValueChange
    {
        internal ValueChange(ChangeType changeType, object key, TData value = default(TData)) : base(changeType, key, value) { }

        public new TData Value => (TData) base.Value;

        public static ValueChange<TData> Create(ChangeType changeType, object key, TData value = default(TData))
        {
            return new ValueChange<TData>(changeType, key, value);
        }
    }
}