using System;
using System.Threading.Tasks;

namespace System.Threading
{
    public sealed class AsyncQueue : IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly CancellationTokenSource _cts;
        private volatile Task<object> _current;

        public AsyncQueue()
        {
            _cts = new CancellationTokenSource();

            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            _current = tcs.Task;
        }

        public Task Enqueue(Action action, CancellationToken token, TaskScheduler scheduler)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            return EnqueueImpl(action, token, scheduler);
        }

        public Task Enqueue(Action action, CancellationToken token)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            return EnqueueImpl(action, token, TaskScheduler.Current);
        }

        public Task Enqueue(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            return EnqueueImpl(action, CancellationToken.None, TaskScheduler.Current);
        }

        public Task<T> Enqueue<T>(Func<T> func, CancellationToken token, TaskScheduler scheduler)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            return EnqueueImpl(func, token, scheduler);
        }

        public Task<T> Enqueue<T>(Func<T> func, CancellationToken token)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            return EnqueueImpl(func, token, TaskScheduler.Current);
        }

        public Task<T> Enqueue<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            return EnqueueImpl(func, CancellationToken.None, TaskScheduler.Current);
        }

        private Task EnqueueImpl(Action action, CancellationToken token, TaskScheduler scheduler)
        {
            lock (_syncRoot)
            {
                if (_current == null)
                    throw new ObjectDisposedException(typeof (AsyncQueue).Name);

                var outer = new TaskCompletionSource<object>();
                _current = _current.ContinueWith(_ =>
                {
                    var inner = new TaskCompletionSource<object>();
                    var actionCts = CancellationTokenSource.CreateLinkedTokenSource(new[] {_cts.Token, token});
                    Task.Factory.StartNew(action, actionCts.Token, TaskCreationOptions.None, scheduler)
                        .ContinueWith(res =>
                        {
                            actionCts.Dispose();

                            switch (res.Status)
                            {
                                case TaskStatus.RanToCompletion:
                                    outer.SetResult(null);
                                    break;
                                case TaskStatus.Canceled:
                                    outer.SetCanceled();
                                    break;
                                case TaskStatus.Faulted:
                                    outer.SetException(res.Exception.Flatten());
                                    break;
                            }
                            inner.SetResult(null);
                        },
                            _cts.Token,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Current);

                    return inner.Task;
                }, _cts.Token)
                    .Unwrap();

                return outer.Task;
            }
        }

        private Task<T> EnqueueImpl<T>(Func<T> func, CancellationToken token, TaskScheduler scheduler)
        {
            lock (_syncRoot)
            {
                if (_current == null)
                {
                    throw new ObjectDisposedException(typeof (AsyncQueue).Name);
                }

                var outer = new TaskCompletionSource<T>();
                _current = _current.ContinueWith(_ =>
                {
                    var inner = new TaskCompletionSource<object>();
                    var actionCts = CancellationTokenSource.CreateLinkedTokenSource(new[] {_cts.Token, token});
                    Task<T>.Factory.StartNew(func, actionCts.Token, TaskCreationOptions.None, scheduler)
                        .ContinueWith(res =>
                        {
                            actionCts.Dispose();

                            switch (res.Status)
                            {
                                case TaskStatus.RanToCompletion:
                                    outer.SetResult(res.Result);
                                    break;
                                case TaskStatus.Canceled:
                                    outer.SetCanceled();
                                    break;
                                case TaskStatus.Faulted:
                                    outer.SetException(res.Exception.Flatten());
                                    break;
                            }
                            inner.SetResult(null);
                        },
                            _cts.Token,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Current);

                    return inner.Task;
                },
                    _cts.Token)
                    .Unwrap();

                return outer.Task;
            }
        }

        public void Dispose()
        {
            _current = null;
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}