// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataItem{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DataItem<TData> : DataObject, IDataItem<TData>
    {
        private readonly BehaviorSubject<TData> _subject;
        private readonly IObservable<IEnumerable<ValueChange>> _outObservable;
        private bool _isDisposed;

        internal DataItem(object key, TData initialValue = default(TData)) : base(key)
        {
            _subject = new BehaviorSubject<TData>(initialValue);

            _outObservable = _subject.DistinctUntilChanged()
                .Publish(pub =>
                    pub.Take(1).Select(value => ValueChange<TData>.Create(ChangeType.Created, Key, value).Yield())
                    .Merge(
                        pub.Skip(1).Select(value => ValueChange<TData>.Create(ChangeType.Modify, Key, value).Yield())))
                .Concat(Observable.Return(ValueChange<TData>.Create(ChangeType.Remove, Key, _subject.Value).Yield())); //When completed it means for DataChange item is removed

            Changes = Subject.Create<IEnumerable<SomeChange>>(Observer.Create<IEnumerable<SomeChange>>(OnChangesIn), _outObservable);
        }

        public TData Value => _subject.Value;

        public override ISubject<IEnumerable<SomeChange>> Changes { get; }
        public void OnCompleted() { _subject.OnCompleted(); }

        public void OnError(Exception error)
        {
            if (_isDisposed)
            {
                throw error;
            }

            _subject.OnError(error);
        }

        public void OnNext(TData value)
        {
            if (_isDisposed) throw new InvalidOperationException("DataItem is already disposed");

            _subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<TData> observer) => _subject.Subscribe();

        public void Dispose()
        {
            _subject.Dispose();
            _isDisposed = true;
        }

        private void OnChangesIn(IEnumerable<SomeChange> changes)
        {
            changes.Cast<ValueChange<TData>>().ForEach(change =>
            {
                //Handle Errors
                if (_isDisposed) OnError(new InvalidOperationException("DataItem is already disposed"));
                if (!EqualityComparer<object>.Default.Equals(Key, change.Key))
                    OnError(new InvalidOperationException("Invalid key routing. KeyChain is invalid for this DataItem"));

                if (change.ChangeType.HasFlag(ChangeType.Modify))
                {
                    var data = change.Value is TData value ? value : default(TData);
                    if (!EqualityComparer<TData>.Default.Equals(_subject.Value, data))
                    {
                        _subject.OnNext((TData)change.Value);
                    }
                }

                if (change.ChangeType.HasFlag(ChangeType.Remove))
                {
                    _subject.OnCompleted();
                }
            });
        }
    }
}