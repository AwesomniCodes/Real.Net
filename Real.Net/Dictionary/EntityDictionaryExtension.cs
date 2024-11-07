// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDictionaryExtension.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using DynamicData;
    using DynamicData.Kernel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;

    public static class EntityDictionaryExtension
    {
        public static IDictionary<TKey,TValue> AsValueDictionary<TKey,TValue>(this IEntityDictionary<TKey, IEntitySubject<TValue>> dictionary) => new EntitySubjectDictionaryActor<TKey, TValue>(dictionary);

        private class EntitySubjectDictionaryActor<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEntity
        {
            private readonly IEntityDictionary<TKey, IEntitySubject<TValue>> _dictionary;

            internal EntitySubjectDictionaryActor(IEntityDictionary<TKey, IEntitySubject<TValue>> dictionary) => _dictionary = dictionary;

            public TValue this[TKey key] { get => _dictionary[key].Value; set => _dictionary[key] = EntitySubject<TValue>.Create(value); }

            TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

            public ICollection<TKey> Keys => _dictionary.Keys;

            public ICollection<TValue> Values => _dictionary.Values.Select(subject => subject.Value).ToList();

            public int Count => _dictionary.Count;

            public bool IsReadOnly => _dictionary.IsReadOnly;

            IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

            IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values.Select(subject => subject.Value);

            int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => Count;

            public ISubject<IEnumerable<IChange>> Changes => _dictionary.Changes;

            public void Add(TKey key, TValue value) => _dictionary.Add(key, EntitySubject<TValue>.Create(value));

            public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

            public void Clear() => _dictionary.Clear();

            public bool Contains(KeyValuePair<TKey, TValue> item)
            => TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

            public bool ContainsKey(TKey key)
            => _dictionary.ContainsKey(key);

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => this.Select((kvp, index) => (kvp, index)).ForEach(item => array[arrayIndex + item.index] = item.kvp);

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => _dictionary.Select(kvp => new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value)).GetEnumerator();

            public bool Remove(TKey key) => _dictionary.Remove(key);

            public bool Remove(KeyValuePair<TKey, TValue> item)
            => TryGetValue(item.Key, out var value)
                && EqualityComparer<TValue>.Default.Equals(value, item.Value)
                && Remove(item.Key);

            public bool TryGetValue(TKey key, out TValue value)
            {
                if (_dictionary.TryGetValue(key, out var subject))
                {
                    value = subject.Value;
                    return true;
                }
                value = default!;
                return false;
            }

            bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value);
        }
    }
}