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
        private readonly IObservable<IEnumerable<IDataItemChange>> _dataChangeObservable;
        private readonly IObservable<TData> _observable;

        public static Func<DataObservable<TData>> Creation(IObservable<TData> observable, TData initialValue = default)
            => () => new DataObservable<TData>(observable, initialValue);
        private DataObservable(IObservable<TData> observable, TData initialValue = default)
        {
            _observable = observable;
            Value = initialValue;
            var isFirst = true;
            _dataChangeObservable = Observable.Return(DataItemChange<TData>.Creation(ChangeType.Connect, initialValue)().Yield())
                .Concat(observable.DistinctUntilChanged().SelectMany(value =>
                {
                    if (isFirst && EqualityComparer<TData>.Default.Equals(Value, value))
                    {
                        return Observable.Empty<IEnumerable<IDataItemChange<TData>>>();
                    }

                    isFirst = false;

                    return Observable.Return(DataItemChange<TData>.Creation(ChangeType.Modify, value)().Yield());
                }))
                .Concat(Observable.Return(DataItemChange<TData>.Creation(ChangeType.Complete, Value)().Yield())) //When completed it means for DataChange item is removed
                .Concat(Observable.Never<IEnumerable<IDataItemChange<TData>>>()); //Avoid OnComplete

            Changes = Subject.Create<IEnumerable<IChange<IDataObject>>>(Observer.Create<IEnumerable<IChange<IDataObject>>>(OnChangesIn), _dataChangeObservable);
        }


        public TData Value { get; }

        public override ISubject<IEnumerable<IChange<IDataObject>>> Changes { get; }
 
        public IDisposable Subscribe(IObserver<TData> observer) => _observable.Subscribe();

        private void OnChangesIn(IEnumerable<IChange<IDataObject>> changes)
        {
            //Handle Errors
            throw new InvalidOperationException("DataObservable cannot be updated");
        }
    }
}