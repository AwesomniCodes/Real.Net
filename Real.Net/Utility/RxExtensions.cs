using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Awesomni.Codes.FlowRx.Utility
{
    public static class RxExtensions
    {
        public static IList<T> Snapshot<T>(this IObservable<T> observable)
        {
            List<T> snapshot = new List<T>();
            using (observable.Subscribe(item => snapshot.Add(item)))
            {
                // Deliberately empty; subscribing will add everything to the list.
            }
            return snapshot;
        }
        public static IObservable<long> CreateAutoResetInterval<TSource>(IObservable<TSource> resetter,
            TimeSpan timeSpan, bool immediate = false)
        {
            return resetter.Select(_ =>
                immediate ? Observable.Interval(timeSpan).StartWith(0) : Observable.Interval(timeSpan)).Switch();
        }

        public static IObserver<TSource> Transform<TSource, TResult>(
            this IObserver<TResult> observer,
            Func<TSource, TResult> transform)
        {
            return Observer.Create<TSource>(
                source => { observer.OnNext(transform(source)); },
                observer.OnError,
                observer.OnCompleted);
        }


        public static IObservable<IList<TSource>> SlidingBuffer<TSource>(this IObservable<TSource> observable, TimeSpan timeSpan)
        {
            var published = observable.Publish().RefCount();
            return published.Buffer(() => published.Throttle(TimeSpan.FromMilliseconds(500)));
        }


        /// <summary>
        /// Always returns the previous value too
        /// </summary>
        /// <typeparam name="TValue">The type of observable</typeparam>
        /// <param name="source">The source</param>
        /// <returns>The paired observable</returns>
        public static IObservable<(TValue Previous, TValue Next)> PairWithPrevious<TValue>(
            this IObservable<TValue> source)
        {
            return source.Scan((Previous: default(TValue), New: default(TValue)),
                (acc, current) => (Previous: acc.New, New: current));
        }


        public static IObservable<IList<T>> SynchronousBuffer<T>(this IObservable<T> observable)
        {
            return observable.Select(_ => new List<T>());
        }
    }
}
