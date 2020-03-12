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
    using System.Reflection;
    using System.Text;

    [Flags]
    public enum ChangeType : int
    {
        Create = 1,
        Complete = 2,
        Connect = 4,
        Disconnect = 8,
        Modify = 16,
        Error = 32,
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

        public static Func<ChildChange> Creation(object key, IEnumerable<SomeChange> changes) => () => new ChildChange(key, changes);
    }

    public abstract class ValueChange : SomeChange
    {
        protected ValueChange(ChangeType changeType, object? value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object? Value { get; }

        public static Func<ValueChange> Creation(ChangeType changeType, object value)
        {
            MethodInfo method = typeof(ValueChange<>).MakeGenericType(value.GetType()).GetMethod(nameof(ValueChange.Creation), BindingFlags.Static | BindingFlags.Public);
            return (Func<ValueChange>)method.Invoke(null, new object[] { changeType, value });
        }

        public static Func<ValueChange> Creation(ChangeType changeType, Type type)
        {
            MethodInfo method = typeof(ValueChange<>).MakeGenericType(type).GetMethod(nameof(ValueChange.Creation), BindingFlags.Static | BindingFlags.Public);
            return (Func<ValueChange>)method.Invoke(null, new object?[] { changeType, type.GetDefault() });
        }
    }

    public class ValueChange<TData> : ValueChange
    {
        internal ValueChange(ChangeType changeType, TData value = default) : base(changeType, value) { }

        public new TData Value => base.Value is TData tValue ? tValue : default;

        public static Func<ValueChange<TData>> Creation(ChangeType changeType, TData value = default)
            => () => new ValueChange<TData>(changeType, value);
    }

    public static class ChangeExtensions
    {

        public static string ToDebugString(this (List<object> KeyChain, ChangeType changeType, object? Value) flatChange)
        {
            var sb = new StringBuilder();
            sb.Append('.');
            foreach (var key in flatChange.KeyChain)
            {
                sb.Append($"/{key.ToString()}");
            }
            
            return $"{sb.ToString()} - {flatChange.changeType}: {flatChange.Value}";
        }
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IEnumerable<SomeChange> changes, IEnumerable<object>? curKeyChain = null)
        {
            return changes.SelectMany(change => change.Flattened(curKeyChain ?? Enumerable.Empty<object>()));
        }
        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this SomeChange change, IEnumerable<object>? curKeyChain = null)
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