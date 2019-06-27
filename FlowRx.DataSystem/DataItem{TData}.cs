// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataItem{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.DataSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DataItem<TData> : DataObject, IDataItem<TData>
    {
        private readonly BehaviorSubject<TData> _subject;
        private readonly IObservable<DataUpdateInfo> _dataUpdateInfoObservable;
        private bool _isSyncUpdate;
        private bool _isDisposed;

        internal DataItem(object key, TData initialValue = default(TData)) : base(key)
        {
            _subject = new BehaviorSubject<TData>(initialValue);

            var isFirst = true;
            _dataUpdateInfoObservable = _subject.DistinctUntilChanged().SelectMany(value =>
                {
                    var dataUpdateType = isFirst ? DataUpdateType.Created : DataUpdateType.Modify;
                    if (_isSyncUpdate)
                    {
                        dataUpdateType |= DataUpdateType.Sync;
                    }

                    isFirst = false;
                    return Observable.Return(new DataUpdateInfo<TData>(dataUpdateType, Key, value));
                })
                .Concat(Observable.Return(new DataUpdateInfo<TData>(DataUpdateType.Remove, Key, _subject.Value))) //When completed it means for DataUpdateInfo item is removed
                .Concat(Observable.Never<DataUpdateInfo<TData>>()); //Avoid OnComplete

            Link = Subject.Create<DataUpdateInfo>(Observer.Create<DataUpdateInfo>(OnDataLinkNext), _dataUpdateInfoObservable);
        }

        public TData Value => _subject.Value;

        public override ISubject<DataUpdateInfo> Link { get; }
        public override DataObject Clone() { throw new NotImplementedException(); }

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

        private void OnDataLinkNext(DataUpdateInfo updateInfo)
        {
            //Handle Errors
            if (_isDisposed) OnError(new InvalidOperationException("DataItem is already disposed"));
            if (!EqualityComparer<object>.Default.Equals(Key, updateInfo.KeyChain[0]) || updateInfo.KeyChain.Count != 1)
                OnError(new InvalidOperationException("Invalid key routing. KeyChain is invalid for this DataItem"));

            if (updateInfo.UpdateType.HasFlag(DataUpdateType.Sync))
            {
                _isSyncUpdate = true;
            }

            if (updateInfo.UpdateType.HasFlag(DataUpdateType.Modify))
            {
                var data = updateInfo.Value is TData value ? value : default(TData);
                if (!EqualityComparer<TData>.Default.Equals(_subject.Value, data))
                {
                    _subject.OnNext((TData) updateInfo.Value);
                }
            }

            if (updateInfo.UpdateType.HasFlag(DataUpdateType.Remove))
            {
                _subject.OnCompleted();
            }

            _isSyncUpdate = false;
        }
    }
}