// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityObservable.cs" project="FlowRx" solution="FlowRx" />
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


    public abstract class EntityObservable : Entity, IEntityObservable<object?>
    {
        static EntityObservable() => Entity.InterfaceToClassTypeMap[typeof(IEntityObservable<>)] = typeof(EntityObservable<>);
        public abstract IDisposable Subscribe(IObserver<object?> observer);
    }

    public class EntityObservable<TValue> : EntityObservable, IEntityObservable<TValue>
    {
        public static IEntityObservable<TValue> Create(IObservable<TValue> observable, TValue initialValue = default)
            => new EntityObservable<TValue>(observable, initialValue);

        private readonly IObservable<TValue> _observable;

        protected EntityObservable(IObservable<TValue> observable, TValue initialValue = default)
        {
            _observable = observable;
            Value = initialValue;
            var isFirst = true;

            Changes = Subject.Create<IEnumerable<IChange>>(
                    Observer.Create<IEnumerable<IChange>>(changes =>
                    {
                        //Handle Errors
                        throw new InvalidOperationException($"{nameof(EntityObservable)} cannot be updated");
                    }),
                    Observable.Return(ChangeValue<TValue>.Create(ChangeType.Connect, initialValue).Yield())
                    .Concat(observable.DistinctUntilChanged().SelectMany(value =>
                    {
                        if (isFirst && EqualityComparer<TValue>.Default.Equals(Value, value))
                        {
                            return Observable.Empty<IEnumerable<IChangeValue<TValue>>>();
                        }

                        isFirst = false;

                        return Observable.Return(ChangeValue<TValue>.Create(ChangeType.Modify, value).Yield());
                    }))
                    .Concat(Observable.Return(ChangeValue<TValue>.Create(ChangeType.Complete, Value).Yield()))
                    .Concat(Observable.Never<IEnumerable<IChangeValue<TValue>>>())); //Avoid OnComplete
        }


        public TValue Value { get; }

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        public IDisposable Subscribe(IObserver<TValue> observer) => _observable.Subscribe(observer);

        public override IDisposable Subscribe(IObserver<object?> observer) => _observable.Select(tValue => (object?)tValue).Subscribe(observer);
    }
}