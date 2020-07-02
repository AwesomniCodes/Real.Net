// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeValue.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections.Generic;

    public interface IChangeValue<TValue> : IChange<IEntityValue<TValue>>
    {
        ChangeType ChangeType { get; }
        TValue Value { get; }
    }

    public abstract class ChangeValue : IChangeValue<object?>
    {
        protected ChangeValue(ChangeType changeType, object? value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object? Value { get; }
    }

    public class ChangeValue<TValue> : ChangeValue, IChangeValue<TValue>
    {
        public static IChangeValue<TValue> Create(ChangeType changeType, TValue value = default)
            => new ChangeValue<TValue>(changeType, value);

        protected ChangeValue(ChangeType changeType, TValue value = default) : base(changeType, value) { }

        public new TValue Value => base.Value is TValue tValue ? tValue : default!;

    }
}