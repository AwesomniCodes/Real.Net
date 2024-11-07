// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="EntityListExtension.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using Awesomni.Codes.Real.Net.Utility;
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

    public static class EntityListExtension
    {
        public static IList<TValue> AsValueList<TValue>(this IEntityList<IEntitySubject<TValue>> list) => new EntitySubjectListActor<TValue>(list);

        private class EntitySubjectListActor<TValue> : IList<TValue>, IList, IReadOnlyCollection<TValue>, IEntity
        {
            private readonly IEntityList<IEntitySubject<TValue>> _list;

            internal EntitySubjectListActor(IEntityList<IEntitySubject<TValue>> list) => _list = list;

            public int Count => _list.Count;

            public bool IsReadOnly => _list.IsReadOnly;

            bool IList.IsFixedSize => ((IList)_list).IsFixedSize;

            bool IList.IsReadOnly => ((IList)_list).IsReadOnly;

            int ICollection.Count => _list.Count;

            bool ICollection.IsSynchronized => ((ICollection)_list).IsSynchronized;

            object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;

            public ISubject<IEnumerable<IChange>> Changes => _list.Changes;

            object IList.this[int index] { get => _list[index].Value!; set => _list[index].OnNext((TValue)value); }
            public TValue this[int index] { get => _list[index].Value; set => _list[index].OnNext(value); }

            public int IndexOf(TValue item)
                => _list.Select((subject, index) => new { Subject = subject, Index = index }).FirstOrDefault(si => EqualityComparer<TValue>.Default.Equals(si.Subject.Value, item))?.Index ?? -1;

            public void Insert(int index, TValue item)
                => _list.Insert(index, EntitySubject<TValue>.Create(item));

            public void RemoveAt(int index)
                => _list.RemoveAt(index);

            public void Add(TValue item)
                => _list.Add(EntitySubject<TValue>.Create(item));

            public void Clear()
                => _list.Clear();

            public bool Contains(TValue item)
                => _list.Any(subject => EqualityComparer<TValue>.Default.Equals(subject.Value, item));

            public void CopyTo(TValue[] array, int arrayIndex)
                => this.Select((value, index) => (value, index)).ForEach(item => array[arrayIndex + item.index] = item.value);

            public bool Remove(TValue item)
            {
                var index = IndexOf(item);
                if (index == -1)
                    return false;

                _list.RemoveAt(index);
                return true;
            }

            public IEnumerator<TValue> GetEnumerator()
                => _list.Select(subject => subject.Value).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList.Add(object value)
            {
                if (value is TValue tValue)
                {
                    _list.Add(EntitySubject<TValue>.Create(tValue));
                    return _list.Count - 1;
                }
                return -1;
            }

            void IList.Clear() => Clear();

            bool IList.Contains(object value)
                => value is TValue tValue && Contains(tValue);

            int IList.IndexOf(object value) => value is TValue tValue ? IndexOf(tValue) : -1;

            void IList.Insert(int index, object value) => Insert(index, (TValue)value);

            void IList.Remove(object value) => Remove((TValue)value);

            void IList.RemoveAt(int index) => RemoveAt(index);

            void ICollection.CopyTo(Array array, int index)
            {
                if (!(array is TValue[] tdArray)) throw new ArgumentException(nameof(array));
                CopyTo(tdArray, index);
            }
        }
    }
}