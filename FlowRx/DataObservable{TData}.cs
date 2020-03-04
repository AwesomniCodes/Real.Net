// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObservable{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DataObservable<TData> : DataObject, IObservable<TData>
    {
        private readonly IObservable<IEnumerable<ValueChange>> _dataChangeObservable;
        private readonly IObservable<TData> _observable;

        internal DataObservable(object key, IObservable<TData> observable, TData initialValue = default(TData)) : base(key)
        {
            _observable = observable;
            Value = initialValue;
            var isFirst = true;
            _dataChangeObservable = Observable.Return(ValueChange<TData>.Create(ChangeType.Connected, Key, initialValue).Yield())
                .Concat(observable.DistinctUntilChanged().SelectMany(value =>
                {
                    if (isFirst && EqualityComparer<TData>.Default.Equals(Value, value))
                    {
                        return Observable.Empty<IEnumerable<ValueChange<TData>>>();
                    }

                    isFirst = false;

                    return Observable.Return(ValueChange<TData>.Create(ChangeType.Modify, Key, value).Yield());
                }))
                .Concat(Observable.Return(ValueChange<TData>.Create(ChangeType.Remove, Key, Value).Yield())) //When completed it means for DataChange item is removed
                .Concat(Observable.Never<IEnumerable<ValueChange<TData>>>()); //Avoid OnComplete

            Changes = Subject.Create<IEnumerable<SomeChange>>(Observer.Create<IEnumerable<SomeChange>>(OnChangesIn), _dataChangeObservable);
        }


        public TData Value { get; }

        public override ISubject<IEnumerable<SomeChange>> Changes { get; }
 
        public IDisposable Subscribe(IObserver<TData> observer) => _observable.Subscribe();

        private void OnChangesIn(IEnumerable<SomeChange> changes)
        {
            //Handle Errors
            throw new InvalidOperationException("DataObservable cannot be updated");
        }
    }
}