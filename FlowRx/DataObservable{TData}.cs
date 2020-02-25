// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObservable{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.DataSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DataObservable<TData> : DataObject, IObservable<TData>
    {
        private readonly IObservable<DataChange> _dataChangeObservable;
        private readonly IObservable<TData> _observable;

        internal DataObservable(object key, IObservable<TData> observable, TData initialValue = default(TData)) : base(key)
        {
            _observable = observable;
            Value = initialValue;
            var isFirst = true;
            _dataChangeObservable = Observable.Return(new DataChange<TData>(DataChangeType.Connected, Key, initialValue))
                .Concat(observable.DistinctUntilChanged().SelectMany(value =>
                {
                    if (isFirst && EqualityComparer<TData>.Default.Equals(Value, value))
                    {
                        return Observable.Empty<DataChange<TData>>();
                    }

                    isFirst = false;

                    return Observable.Return(new DataChange<TData>(DataChangeType.Modify, Key, value));
                }))
                .Concat(Observable.Return(new DataChange<TData>(DataChangeType.Remove, Key, Value))) //When completed it means for DataChange item is removed
                .Concat(Observable.Never<DataChange<TData>>()); //Avoid OnComplete

            Changes = Subject.Create<DataChange>(Observer.Create<DataChange>(OnDataLinkNext), _dataChangeObservable);
        }


        public TData Value { get; }

        public override ISubject<DataChange> Changes { get; }
 
        public IDisposable Subscribe(IObserver<TData> observer) => _observable.Subscribe();

        private void OnDataLinkNext(DataChange change)
        {
            //Handle Errors
            throw new InvalidOperationException("DataObservable cannot be updated");
        }
    }
}