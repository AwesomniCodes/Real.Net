// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityValue.cs" project="FlowRx" solution="FlowRx" />
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
    using System.Reflection;

    public abstract class EntityValue : Entity, IEntityValue<object?>
    {

        public abstract object? Value { get; }

        public abstract void Dispose();

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(object? value);

        public abstract IDisposable Subscribe(IObserver<object?> observer);
    }

    public class EntityValue<TValue> : EntityValue, IEntityValue<TValue>
    {
        static EntityValue() => Entity.InterfaceToClassTypeMap[typeof(IEntityValue<>)] = typeof(EntityValue<>);
        public static IEntityValue<TValue> Create(TValue initialValue = default) => new EntityValue<TValue>(initialValue);

        private readonly BehaviorSubject<TValue> _subject;
        private bool _isDisposed;

        protected EntityValue(TValue initialValue = default)
        {
            _subject = new BehaviorSubject<TValue>(initialValue);

            Changes = Subject.Create<IEnumerable<IChange>>(
                    Observer.Create<IEnumerable<IChange>>(changes =>
                    {
                        changes.Cast<IChangeValue<TValue>>().ForEach(change =>
                        {
                            //Handle Errors
                            if (_isDisposed) OnError(new InvalidOperationException($"{nameof(EntityValue)} is already disposed"));

                            if (change.ChangeType.HasFlag(ChangeType.Modify))
                            {
                                var value = change.Value is TValue val ? val : default!;
                                if (!EqualityComparer<TValue>.Default.Equals(_subject.Value, value))
                                {
                                    _subject.OnNext((TValue)change.Value);
                                }
                            }

                            if (change.ChangeType.HasFlag(ChangeType.Complete))
                            {
                                _subject.OnCompleted();
                            }
                        });
                    }),
                    _subject.DistinctUntilChanged()
                    .Publish(pub =>
                        pub.Take(1).Select(value => ChangeValue<TValue>.Create(ChangeType.Create, value).Yield())
                        .Merge(
                            pub.Skip(1).Select(value => ChangeValue<TValue>.Create(ChangeType.Modify, value).Yield())))
                    .Concat(Observable.Return(ChangeValue<TValue>.Create(ChangeType.Complete, _subject.Value).Yield())));
        }

        TValue IEntityValue<TValue>.Value => _subject.Value;

        public override object? Value => _subject.Value;

        public override ISubject<IEnumerable<IChange>> Changes { get; }

        public override void OnCompleted() { _subject.OnCompleted(); }

        public override void OnError(Exception error)
        {
            if (_isDisposed)
            {
                throw error;
            }

            _subject.OnError(error);
        }

        public void OnNext(TValue value)
        {
            if (_isDisposed) throw new InvalidOperationException($"{nameof(EntityValue)} is already disposed");

            _subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<TValue> observer) => _subject.Subscribe(observer);

        public override void Dispose()
        {
            _subject.Dispose();
            _isDisposed = true;
        }

        public override void OnNext(object? value) => OnNext((TValue) value!);

        public override IDisposable Subscribe(IObserver<object?> observer) => _subject.Select(i => (object?)i).Subscribe(observer);
    }
}