// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeExtensions.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------


namespace Awesomni.Codes.FlowRx.Utility
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;

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

        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IEnumerable<IChange> changes, IEnumerable<object>? curKeyChain = null)
        {
            return changes.SelectMany(change => change.Flattened(curKeyChain ?? Enumerable.Empty<object>()));
        }

        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IChange change, IEnumerable<object>? curKeyChain = null)
        {
            var keyChain = (curKeyChain ?? Enumerable.Empty<object>()).ToList();

            if (change is IChangeItem valueChange)
            {
                yield return (keyChain, valueChange.ChangeType, valueChange.Value);
            }

            if (change is IChangeDictionary childChange)
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
