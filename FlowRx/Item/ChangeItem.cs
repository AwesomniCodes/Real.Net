// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeItem.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections.Generic;

    public interface IChangeItem : IChange<IDataItem>
    {
        ChangeType ChangeType { get; }
        object? Value { get; }
    }

    public interface IChangeItem<TData> : IChange<IDataItem<TData>>, IChangeItem
    {
        new TData Value { get; }
    }

    public abstract class ChangeItem : IChangeItem
    {
        protected ChangeItem(ChangeType changeType, object? value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object? Value { get; }
    }

    public class ChangeItem<TData> : ChangeItem, IChangeItem<TData>
    {
        public static IChangeItem<TData> Create(ChangeType changeType, TData value = default)
            => new ChangeItem<TData>(changeType, value);

        internal ChangeItem(ChangeType changeType, TData value = default) : base(changeType, value) { }

        public new TData Value => base.Value is TData tValue ? tValue : default!;

    }
}