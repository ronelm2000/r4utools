﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Utilities.Components
{
    /// <summary>
    /// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
    /// </summary>
    /// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
    public sealed class AsyncLazy<T>
    {
        /// <summary>
        /// The underlying lazy task.
        /// </summary>
        private readonly Lazy<Task<T>> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The delegate that is invoked on a background thread to produce the value when it is needed.</param>
        public AsyncLazy(Func<T> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(async() => await Process(factory)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed.</param>
        public AsyncLazy(Func<Task<T>> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(async() => await Process(factory)));
        }

        
        private async Task<T> Process(Func<T> factory)
        {
            await OnChanging();
            var result = factory();
            await OnChanged();
            return result;
        }
        private async Task<T> Process(Func<Task<T>> factory)
        {
            await OnChanging();
            var result = await factory();
            await OnChanged();
            return result;
        }
        

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
        /// </summary>
        public TaskAwaiter<T> GetAwaiter()
        {
            return instance.Value.GetAwaiter();
        }
        
        /// <summary>
        /// Starts the asynchronous initialization, if it has not already started.
        /// </summary>
        public void Start()
        {
            var unused = instance.Value;
        }

        /// <summary>
        /// Returns null if the underlying task for this one is still ongoing, otherwise rturns the instance value synchronously.
        /// </summary>
        public T Value {
            get {
                // return instance.Value.Result;
                if (instance.IsValueCreated && instance.Value.IsCompleted)
                    return instance.Value.Result;
                else
                {
                    Start();
                    return default(T);
                }
            }
        }

        /// <summary>
        /// Returns null if the underlying task for this one is still ongoing, otherwise rturns the instance value synchronously.
        /// </summary>
        public T DesignValue
        {
            get
            {
                return instance.Value.Result;
            }
        }

        public Func<Task> OnChanging { get; set; } = () => Task.CompletedTask;
        public Func<Task> OnChanged { get; set; } = () => Task.CompletedTask;
    }
}
