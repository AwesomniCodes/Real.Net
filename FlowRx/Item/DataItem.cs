// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataItem.cs" project="FlowRx" solution="FlowRx" />
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

    public abstract class DataItem : DataObject, IDataItem<object?>
    {

        public abstract object? Value { get; }

        public abstract void Dispose();

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(object? value);

        public abstract IDisposable Subscribe(IObserver<object?> observer);
    }

    public class DataItem<TData> : DataItem, IDataItem<TData>
    {
        static DataItem() => DataObject.InterfaceToClassTypeMap[typeof(IDataItem<>)] = typeof(DataItem<>);
        public static IDataItem<TData> Create(TData initialValue = default) => new DataItem<TData>(initialValue);

        private readonly BehaviorSubject<TData> _subject;
        private bool _isDisposed;

        protected DataItem(TData initialValue = default)
        {
            _subject = new BehaviorSubject<TData>(initialValue);

            Changes = Subject.Create<IEnumerable<IChange>>(
                    Observer.Create<IEnumerable<IChange>>(changes =>
                    {
                        changes.Cast<IChangeItem<TData>>().ForEach(change =>
                        {
                            //Handle Errors
                            if (_isDisposed) OnError(new InvalidOperationException("DataItem is already disposed"));

                            if (change.ChangeType.HasFlag(ChangeType.Modify))
                            {
                                var data = change.Value is TData value ? value : default!;
                                if (!EqualityComparer<TData>.Default.Equals(_subject.Value, data))
                                {
                                    _subject.OnNext((TData)change.Value);
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
                        pub.Take(1).Select(value => ChangeItem<TData>.Create(ChangeType.Create, value).Yield())
                        .Merge(
                            pub.Skip(1).Select(value => ChangeItem<TData>.Create(ChangeType.Modify, value).Yield())))
                    .Concat(Observable.Return(ChangeItem<TData>.Create(ChangeType.Complete, _subject.Value).Yield())));
        }

        TData IDataItem<TData>.Value => _subject.Value;

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

        public void OnNext(TData value)
        {
            if (_isDisposed) throw new InvalidOperationException("DataItem is already disposed");

            _subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<TData> observer) => _subject.Subscribe(observer);

        public override void Dispose()
        {
            _subject.Dispose();
            _isDisposed = true;
        }

        public override void OnNext(object? value) => OnNext((TData) value!);

        public override IDisposable Subscribe(IObserver<object?> observer) => _subject.Select(i => (object?)i).Subscribe(observer);
    }
}