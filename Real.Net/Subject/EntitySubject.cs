// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntitySubject.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;

    public abstract class EntitySubjectBase<TValue> : EntityObservable<TValue>, IEntitySubject<object?>
    {
        protected EntitySubjectBase(BehaviorSubject<TValue> subject) : base(subject) { }

        public abstract object? Value { get; }

        public abstract void Dispose();

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(object? value);
    }

    public class EntitySubject<TValue> : EntitySubjectBase<TValue>, IEntitySubject<TValue>
    {
        private readonly BehaviorSubject<TValue> _subject;
        public static IEntitySubject<TValue> Create(TValue initialValue = default) => new EntitySubject<TValue>(initialValue);

        private bool _isDisposed;

        protected EntitySubject(TValue initialValue = default) : this(new BehaviorSubject<TValue>(initialValue)) { }

        protected EntitySubject(BehaviorSubject<TValue> subject) : base(subject)
        {
            _subject = subject;
        }
        TValue IEntitySubject<TValue>.Value => _subject.Value;

        public override object? Value => _subject.Value;

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
            if (_isDisposed) throw new InvalidOperationException($"{nameof(EntitySubject<TValue>)} is already disposed");

            _subject.OnNext(value);
        }

        public override void Dispose()
        {
            _subject.Dispose();
            _isDisposed = true;
        }

        public override void OnNext(object? value) => OnNext((TValue) value!);

        protected override IObserver<IEnumerable<IChange>> CreateObserverForChangesSubject()
        => Observer.Create<IEnumerable<IChange>>(changes =>
        {
            changes.OfType<IChangeSubject<TValue>>().ForEach(change =>
            {
                //Handle Errors
                if (_isDisposed) OnError(new InvalidOperationException($"{nameof(EntitySubject<TValue>)} is already disposed"));

                if (change.ChangeType.HasFlag(ChangeType.Modification))
                {
                    var value = change.Value is TValue val ? val : default!;
                    if (!EqualityComparer<TValue>.Default.Equals(_subject.Value, value))
                    {
                        _subject.OnNext((TValue)change.Value);
                    }
                }

                if (change.ChangeType.HasFlag(ChangeType.Completion))
                {
                    _subject.OnCompleted();
                }
            });
        });
    }
}