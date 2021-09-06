using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    public static class RxExtensions
    {
        /// <summary>
        /// Subscribes to a Task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="onNext"></param>
        /// <param name="onError"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> onNext, Action<Exception> onError, Action onCompleted)
        {
            return source
                .Select(e => Observable.Defer(() => onNext(e).ToObservable()))
                .Concat()
                .Subscribe(e => { }, onError, onCompleted);
        }

        public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> onNext)
        {
            return source
                .Select(e => Observable.Defer(() => onNext(e).ToObservable()))
                .Concat()
                .Subscribe(e => { });
        }
    }
}
