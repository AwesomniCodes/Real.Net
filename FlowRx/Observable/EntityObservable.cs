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
        public abstract IDisposable Subscribe(IObserver<object?> observer);
    }

    public class EntityObservable<TValue> : EntityObservable, IEntityObservable<TValue>
    {
        public static IEntityObservable<TValue> Create(IObservable<TValue> observable)
            => new EntityObservable<TValue>(observable);

        private readonly IObservable<TValue> _observable;

        protected EntityObservable(IObservable<TValue> observable)
        {
            _observable = observable;
        }


        protected override IObserver<IEnumerable<IChange>> CreateObserverForChangesSubject()
            => Observer.Create<IEnumerable<IChange>>(changes =>
            {
                //Handle Errors
                throw new InvalidOperationException($"{nameof(EntityObservable)} cannot be updated");
            });

        protected override IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject()
            => _observable
                .DistinctUntilChanged()
                .Publish(pub =>
                    pub.Take(1).Select(value => ChangeValue<TValue>.Create(ChangeType.Create, value).Yield())
                        .Merge(pub.Skip(1).Select(value => ChangeValue<TValue>.Create(ChangeType.Modify, value).Yield())))
                .Concat(Observable.Return(ChangeValue<TValue>.Create(ChangeType.Complete).Yield()));

        public IDisposable Subscribe(IObserver<TValue> observer) => _observable.Subscribe(observer);

        public override IDisposable Subscribe(IObserver<object?> observer) => _observable.Select(tValue => (object?)tValue).Subscribe(observer);
    }
}