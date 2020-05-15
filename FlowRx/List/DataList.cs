// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataList.cs" project="FlowRx" solution="FlowRx" />
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

    public abstract class DataList : DataObject, IDataList<IDataObject>
    {
        public abstract IDataObject this[int index] { get; set; }
        object IList.this[int index] { get => this[index]; set => this[index] = (IDataObject) value; }

        public abstract bool IsFixedSize { get; }
        public abstract bool IsReadOnly { get; }
        public abstract int Count { get; }
        public abstract bool IsSynchronized { get; }
        public abstract object SyncRoot { get; }
        public abstract int Add(object value);
        public abstract void Add(IDataObject item);
        public abstract void Clear();
        public abstract bool Contains(object value);
        public abstract bool Contains(IDataObject item);
        public abstract void CopyTo(Array array, int index);
        public abstract void CopyTo(IDataObject[] array, int arrayIndex);
        public abstract IEnumerator<IDataObject> GetEnumerator();
        public abstract int IndexOf(object value);
        public abstract int IndexOf(IDataObject item);
        public abstract void Insert(int index, object value);
        public abstract void Insert(int index, IDataObject item);
        public abstract void Remove(object value);
        public abstract bool Remove(IDataObject item);
        public abstract void RemoveAt(int index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class DataList<TDataObject> : DataList, IDataList<TDataObject> where TDataObject : class, IDataObject
    {
        public static IDataList<TDataObject> Create() => new DataList<TDataObject>();
        static DataList() => DataObject.InterfaceToClassTypeMap[typeof(IDataList<>)] = typeof(DataList<>); 
        
        private IDataList<TDataObject> @this => this;
        private object? _syncRoot;
        protected readonly BehaviorSubject<SourceList<TDataObject>> _item;

        protected DataList()
        {
            _item = new BehaviorSubject<SourceList<TDataObject>>(new SourceList<TDataObject>());

            Changes = CreateChangesSubject();

            //Subscription to remove completed childs from list
            Changes.Subscribe(childChanges =>
            {
                var completedKeys = childChanges
                .OfType<IChangeList<TDataObject>>()
                .SelectMany(childChange =>
                                childChange
                                .Changes
                                .OfType<IChangeItem<object?>>()
                                .Where(ccI => ccI.ChangeType == ChangeType.Complete)
                                .Select(_ => childChange.Key));

                completedKeys.ForEach(key =>
                {
                    _item.Value.RemoveAt(key);
                });
            });
        }

        private ISubject<IEnumerable<IChange>> CreateChangesSubject()
            => Subject.Create<IEnumerable<IChange>>(
                    CreateObserverForChangesSubject(),
                    CreateObservableForChangesSubject());

        private IObserver<IEnumerable<IChange>> CreateObserverForChangesSubject()
            => Observer.Create<IEnumerable<IChange>>(changes =>
            {
                changes.ForEach(change =>
                {
                    if (change is IChangeItem<TDataObject>)
                    {
                        //TODO: The whole list gets replaced
                        //item.OnNext(change);
                    }
                    else if (change is IChangeList<TDataObject> childChange)
                    {
                        childChange.Changes.ForEach(innerChange =>
                        {
                            if (innerChange is IChangeItem<object?> innerValueChange && innerValueChange.ChangeType == ChangeType.Create)
                            {
                                var changeType = innerChange.GetType().GetTypesIfImplemented(typeof(IChange<>)).Last().GetGenericArguments().Single();
                                if (changeType != null)
                                {
                                    var dataObject = (TDataObject)DataObject.Create(changeType, innerValueChange.Value);
                                    Insert(childChange.Key, dataObject);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Received an invalid IChangeItem that is not implementing IChange<>");
                                }
                            }
                            else
                            {
                                @this[childChange.Key].Changes.OnNext(innerChange.Yield());
                            }
                        });
                    }
                });
            });

        private IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => Observable.Return(ChangeItem<IDataList<TDataObject>>.Create(ChangeType.Create).Yield())
               .Concat<IEnumerable<IChange<IDataObject>>>(
                    _item.Switch()
                    .Transform((dO, index) => (DataObject: dO, Index: index))
                    .MergeMany(dOWithIndex => 
                        dOWithIndex.DataObject.Changes
                        .Select(changes => ChangeList<TDataObject>.Create(dOWithIndex.Index, changes.Cast<IChange<TDataObject>>()).Yield())));

        public override int Add(object value)
        {
            if (value is TDataObject dataObjectValue)
            {
                _item.Value.Add(dataObjectValue);
                return _item.Value.Count - 1;
            }
            return -1;
        }
        public override void Add(IDataObject item) => @this.Add((TDataObject)item);
        public void Add(TDataObject item) => _item.Value.Add(item);
        public override void Clear() => _item.Value.Clear();
        public override bool Contains(object value) => value is TDataObject dataObjectValue ? Contains(dataObjectValue) : false;
        public override bool Contains(IDataObject item) => throw new NotImplementedException();
        public bool Contains(TDataObject item) => _item.Value.Items.Contains(item);
        public override void CopyTo(Array array, int index)
        {
            if (!(array is TDataObject[] tdArray)) throw new ArgumentException("array");
            CopyTo(tdArray, index);
        }
        public override void CopyTo(IDataObject[] array, int arrayIndex) => throw new NotImplementedException();
        public void CopyTo(TDataObject[] array, int arrayIndex) => _item.Value.Items.Select((dO, index) => (dO, index)).ForEach(item => array[arrayIndex + item.index] = item.dO);
        public override IEnumerator<IDataObject> GetEnumerator() => throw new NotImplementedException();
        public override int IndexOf(object value) => value is TDataObject dataObjectValue ? IndexOf(dataObjectValue) : -1;
        public override int IndexOf(IDataObject item) => item is TDataObject dataObjectValue ? IndexOf(dataObjectValue) : -1;
        public int IndexOf(TDataObject item) => _item.Value.Items.IndexOf(item);
        public override void Insert(int index, object value) => Insert(index, (TDataObject)value);
        public override void Insert(int index, IDataObject item) => Insert(index, (TDataObject)item);
        public void Insert(int index, TDataObject item) => _item.Value.Insert(index, item);
        public override void Remove(object value) => @this.Remove((TDataObject)value);
        public override bool Remove(IDataObject item) => @this.Remove((TDataObject)item);
        public bool Remove(TDataObject item) => _item.Value.Remove(item);
        public override void RemoveAt(int index) => _item.Value.RemoveAt(index);
        IEnumerator<TDataObject> IEnumerable<TDataObject>.GetEnumerator() => _item.Value.Items.Select(dO => dO).GetEnumerator();
        public override ISubject<IEnumerable<IChange>> Changes { get; }
        public override bool IsFixedSize => false;
        public override bool IsReadOnly => false;
        public override int Count => _item.Value.Count;
        public override bool IsSynchronized => false;
        public override object SyncRoot => _syncRoot ?? (_syncRoot = new object());
        TDataObject IList<TDataObject>.this[int index] { get => @this[index]; set => @this[index] = value; }
        TDataObject IDataList<TDataObject>.this[int index]
        {
            get => _item.Value.Items.ElementAt(index);
            set => _item.Value.ReplaceAt(index, value);
        }
        public override IDataObject this[int index] { get => @this[index]; set => @this[index] = (TDataObject)value; }
    }
}