﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="ChangeExtensions.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------


namespace Awesomni.Codes.Real.Net.Utility
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

            flatChange.KeyChain.ForEach(key => sb.Append($"/{key}"));

            return $"{sb} - {flatChange.changeType}: {flatChange.Value}";
        }

        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IEnumerable<IChange> changes, IEnumerable<object>? curKeyChain = null)
        {
            return changes.SelectMany(change => change.Flattened(curKeyChain ?? Enumerable.Empty<object>()));
        }

        public static IEnumerable<(List<object> KeyChain, ChangeType changeType, object? Value)> Flattened(this IChange change, IEnumerable<object>? curKeyChain = null)
        {
            var keyChain = (curKeyChain ?? Enumerable.Empty<object>()).ToList();

            if (change is IChangeSubject<object?> subjectChange)
            {
                yield return (keyChain, subjectChange.ChangeType, subjectChange.Value);
            }

            if (change is IChangeDictionary<object?, IEntity> dictionaryChange)
            {
                keyChain = keyChain.Concat(dictionaryChange.Key!.Yield()).ToList();

                foreach (var flattenedChildItem in dictionaryChange.Changes.Flattened(keyChain))
                {
                    yield return flattenedChildItem;
                }
            }

            if (change is IChangeList<IEntity> listChange)
            {
                keyChain = keyChain.Concat((listChange.Key as object).Yield()).ToList();

                foreach (var flattenedChildItem in listChange.Changes.Flattened(keyChain))
                {
                    yield return flattenedChildItem;
                }
            }
        }

        public static List<string> ToDebugStringList(this IEnumerable<IEnumerable<IChange>> snapShot)
        {
            return snapShot
            .SelectMany(changes => changes)
            .Flattened()
            .Select(fC => fC.ToDebugString())
            .ToList();
        }
    }
}
