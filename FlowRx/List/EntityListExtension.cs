// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntitySubjectListActor.cs" project="FlowRx" solution="FlowRx" />
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

    public static class EntityListExtension
    {
        public static IList<TValue> AsValueList<TValue>(this IEntityList<IEntitySubject<TValue>> list) => new EntitySubjectListActor<TValue>(list);
    }

    internal class EntitySubjectListActor<TValue> : IList<TValue>, IList, IReadOnlyCollection<TValue>
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
        {
            throw new NotImplementedException();
        }

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

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

    }
}