// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataItem{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.DataSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DataItem<TData> : DataObject, IDataItem<TData>
    {
        private readonly BehaviorSubject<TData> _subject;
        private readonly IObservable<DataChange> _dataChangeObservable;
        private bool _isSyncUpdate;
        private bool _isDisposed;

        internal DataItem(object key, TData initialValue = default(TData)) : base(key)
        {
            _subject = new BehaviorSubject<TData>(initialValue);

            var isFirst = true;
            _dataChangeObservable = _subject.DistinctUntilChanged().SelectMany(value =>
                {
                    var changeType = isFirst ? DataChangeType.Created : DataChangeType.Modify;
                    if (_isSyncUpdate)
                    {
                        changeType |= DataChangeType.Sync;
                    }

                    isFirst = false;
                    return Observable.Return(new DataChange<TData>(changeType, Key, value));
                })
                .Concat(Observable.Return(new DataChange<TData>(DataChangeType.Remove, Key, _subject.Value))) //When completed it means for DataChange item is removed
                .Concat(Observable.Never<DataChange<TData>>()); //Avoid OnComplete

            Link = Subject.Create<DataChange>(Observer.Create<DataChange>(OnDataLinkNext), _dataChangeObservable);
        }

        public TData Value => _subject.Value;

        public override ISubject<DataChange> Link { get; }
        public override IDataObject Clone() { throw new NotImplementedException(); }

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

        private void OnDataLinkNext(DataChange change)
        {
            //Handle Errors
            if (_isDisposed) OnError(new InvalidOperationException("DataItem is already disposed"));
            if (!EqualityComparer<object>.Default.Equals(Key, change.KeyChain[0]) || change.KeyChain.Count != 1)
                OnError(new InvalidOperationException("Invalid key routing. KeyChain is invalid for this DataItem"));

            if (change.ChangeType.HasFlag(DataChangeType.Sync))
            {
                _isSyncUpdate = true;
            }

            if (change.ChangeType.HasFlag(DataChangeType.Modify))
            {
                var data = change.Value is TData value ? value : default(TData);
                if (!EqualityComparer<TData>.Default.Equals(_subject.Value, data))
                {
                    _subject.OnNext((TData) change.Value);
                }
            }

            if (change.ChangeType.HasFlag(DataChangeType.Remove))
            {
                _subject.OnCompleted();
            }

            _isSyncUpdate = false;
        }
    }
}