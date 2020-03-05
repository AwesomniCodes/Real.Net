// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataChange{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
    }

    public class ChildChange : SomeChange
    {
        public object Key { get; }

        private ChildChange(object key, IEnumerable<SomeChange> changes)
        {
            Key = key;
            Changes = changes;
        }

        public IEnumerable<SomeChange> Changes { get; private set; }

        public static ChildChange Create(object key, IEnumerable<SomeChange> changes) => new ChildChange(key, changes);
    }

    public abstract class ValueChange : SomeChange
    {
        protected ValueChange(ChangeType changeType, object value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object Value { get; }
    }

    public class ValueChange<TData> : ValueChange
    {
        internal ValueChange(ChangeType changeType, TData value = default(TData)) : base(changeType, value) { }

        public new TData Value => (TData) base.Value;

        public static ValueChange<TData> Create(ChangeType changeType, TData value = default(TData))
        {
            return new ValueChange<TData>(changeType, value);
        }
    }

    public static class ChangeExtensions
    {

        public static string ToDebugString(this (List<object> KeyChain, ChangeType changeType, object Value) flatChange)
        {
            var sb = new StringBuilder();
            sb.Append('.');
            foreach (var key in flatChange.KeyChain)
            {
                sb.Append($"/{key.ToString()}");
            }
            
            return $"{sb.ToString()} - {flatChange.changeType}: {flatChange.Value}";
        }
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object Value)> Flattened(this IEnumerable<SomeChange> changes, IEnumerable<object> curKeyChain = null)
        {
            return changes.SelectMany(change => change.Flattened(curKeyChain ?? Enumerable.Empty<object>()));
        }
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object Value)> Flattened(this SomeChange change, IEnumerable<object> curKeyChain = null)
        {
            var keyChain = (curKeyChain ?? Enumerable.Empty<object>()).ToList();

            if (change is ValueChange valueChange)
            {
                yield return (keyChain, valueChange.ChangeType, valueChange.Value);
            }

            if (change is ChildChange childChange)
            {
                keyChain = keyChain.Concat(childChange.Key.Yield()).ToList();

                foreach (var flattenedChildItem in childChange.Changes.Flattened(keyChain))
                {
                    yield return flattenedChildItem;
                }
            }
        }

    }
}