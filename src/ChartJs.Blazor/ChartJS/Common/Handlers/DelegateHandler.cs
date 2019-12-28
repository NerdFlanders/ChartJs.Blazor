﻿using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChartJs.Blazor.ChartJS.Common.Handlers
{
    /// <summary>
    /// Wraps a C#-delegate to make it callable by Javascript.
    /// </summary>
    /// <typeparam name="T">The type of the delegate you want to invoke from Javascript.</typeparam>
    public class DelegateHandler<T> : IMethodHandler<T>, IDisposable
        where T : Delegate
    {
        private readonly T _function;

        /// <summary>
        /// The name of the method which should be called from Javascript. In this case it's always the name of the <see cref="Invoke(object[])"/>-method.
        /// </summary>
        public string MethodName => nameof(Invoke);

        /// <summary>
        /// Keeps a reference to this object which is used to invoke the stored delegate from Javascript.
        /// </summary>
        public DotNetObjectReference<DelegateHandler<T>> HandlerReference { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DelegateHandler{T}"/>.
        /// </summary>
        /// <param name="function">The delegate you want to invoke from Javascript.</param>
        public DelegateHandler(T function)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
            HandlerReference = DotNetObjectReference.Create(this);
        }

        /// <summary>
        /// Invokes the delegate dynamically.
        /// </summary>
        /// <param name="args">All the arguments for the method as array.</param>
        [JSInvokable]
        public virtual object Invoke(params object[] args) => _function.DynamicInvoke(args);

        /// <inheritdoc/>
        public void Dispose()
        {
            HandlerReference.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The <see cref="Dispose"/> method doesn't have any unmanaged resources to free BUT once this object is finalized
        /// we need to prevent any further use of the <see cref="DotNetObjectReference"/> to this object. Since the <see cref="HandlerReference"/>
        /// will only be disposed if this <see cref="DelegateHandler{T}"/> instance is disposed or when <c>dispose</c> is called from Javascript
        /// (which shouldn't happen) we HAVE to dispose the reference when this instance is finalized.
        /// </summary>
        ~DelegateHandler()
        {
            Dispose();
        }

        /// <summary>
        /// Converts a delegate of type <typeparamref name="T"/> to a <see cref="DelegateHandler{T}"/> implicitly.
        /// </summary>
        /// <param name="function"></param>
        public static implicit operator DelegateHandler<T>(T function) => new DelegateHandler<T>(function);
    }
}