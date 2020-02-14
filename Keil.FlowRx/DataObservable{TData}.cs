// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObservable{TData}.cs" project="Keil.FlowRx" solution="Keil.FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.FlowRx.DataSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DataObservable<TData> : DataObject, IObservable<TData>
    {
        private readonly IObservable<DataUpdateInfo> _dataUpdateInfoObservable;
        private readonly IObservable<TData> _observable;

        internal DataObservable(object key, IObservable<TData> observable, TData initialValue = default(TData)) : base(key)
        {
            _observable = observable;
            Value = initialValue;
            var isFirst = true;
            _dataUpdateInfoObservable = Observable.Return(new DataUpdateInfo<TData>(DataUpdateType.Connected, Key, initialValue))
                .Concat(observable.DistinctUntilChanged().SelectMany(value =>
                {
                    if (isFirst && EqualityComparer<TData>.Default.Equals(Value, value))
                    {
                        return Observable.Empty<DataUpdateInfo<TData>>();
                    }

                    isFirst = false;

                    return Observable.Return(new DataUpdateInfo<TData>(DataUpdateType.Modify, Key, value));
                }))
                .Concat(Observable.Return(new DataUpdateInfo<TData>(DataUpdateType.Remove, Key, Value))) //When completed it means for DataUpdateInfo item is removed
                .Concat(Observable.Never<DataUpdateInfo<TData>>()); //Avoid OnComplete

            Link = Subject.Create<DataUpdateInfo>(Observer.Create<DataUpdateInfo>(OnDataLinkNext), _dataUpdateInfoObservable);
        }


        public TData Value { get; }

        public override ISubject<DataUpdateInfo> Link { get; }

        public override IDataObject Clone() { throw new NotImplementedException(); }

        public IDisposable Subscribe(IObserver<TData> observer) => _observable.Subscribe();

        private void OnDataLinkNext(DataUpdateInfo updateInfo)
        {
            //Handle Errors
            throw new InvalidOperationException("DataObservable cannot be updated");
        }
    }
}