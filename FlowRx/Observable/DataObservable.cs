// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObservable.cs" project="FlowRx" solution="FlowRx" />
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


    public abstract class DataObservable : DataObject, IDataObservable<object?>
    {
        public abstract IDisposable Subscribe(IObserver<object?> observer);
    }

    public class DataObservable<TData> : DataObservable, IDataObservable<TData>
    {
        private readonly IObservable<TData> _observable;

        public static IDataObservable<TData> Create(IObservable<TData> observable, TData initialValue = default)
            => new DataObservable<TData>(observable, initialValue);
        protected DataObservable(IObservable<TData> observable, TData initialValue = default)
        {
            _observable = observable;
            Value = initialValue;
            var isFirst = true;

            Changes = Subject.Create<IEnumerable<IChange>>(
                    Observer.Create<IEnumerable<IChange>>(changes =>
                    {
                        //Handle Errors
                        throw new InvalidOperationException("DataObservable cannot be updated");
                    }),
                    Observable.Return(ChangeItem<TData>.Create(ChangeType.Connect, initialValue).Yield())
                    .Concat(observable.DistinctUntilChanged().SelectMany(value =>
                    {
                        if (isFirst && EqualityComparer<TData>.Default.Equals(Value, value))
                        {
                            return Observable.Empty<IEnumerable<IChangeItem<TData>>>();
                        }

                        isFirst = false;

                        return Observable.Return(ChangeItem<TData>.Create(ChangeType.Modify, value).Yield());
                    }))
                    .Concat(Observable.Return(ChangeItem<TData>.Create(ChangeType.Complete, Value).Yield()))
                    .Concat(Observable.Never<IEnumerable<IChangeItem<TData>>>())); //Avoid OnComplete
        }


        public TData Value { get; }

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        public IDisposable Subscribe(IObserver<TData> observer) => _observable.Subscribe(observer);

        public override IDisposable Subscribe(IObserver<object?> observer) => _observable.Select(tData => (object?) tData).Subscribe(observer);
    }
}