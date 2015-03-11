using System.Collections.Generic;
using System.Diagnostics;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static Task<TValue> ReturnAsync<TValue>(this TValue value)
        {
            var tcs = new TaskCompletionSource<TValue>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        public static Task<TOutput> Then<TInput, TOutput>(this Task<TInput> task, Func<TInput, TOutput> continuation,
            TaskContinuationOptions options)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (continuation == null) throw new ArgumentNullException("continuation");

            var tcs = new TaskCompletionSource<TOutput>();

            task.ContinueWith(res =>
            {
                switch (res.Status)
                {
                    case TaskStatus.RanToCompletion:
                        try
                        {
                            tcs.SetResult(continuation(res.Result));
                        }
                        catch (Exception e)
                        {
                            tcs.SetException(e);
                        }
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.SetException(res.Exception.Flatten());
                        break;
                }
            },
                options);

            return tcs.Task;
        }

        public static Task<TOutput> Then<TInput, TOutput>(this Task<TInput> task, Func<TInput, TOutput> continuation)
        {
            return task.Then(continuation, TaskContinuationOptions.None);
        }

        public static Task Then<TInput>(this Task<TInput> task, Action<TInput> continuation,
            TaskContinuationOptions options)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (continuation == null) throw new ArgumentNullException("continuation");

            var tcs = new TaskCompletionSource<object>();

            task.ContinueWith(res =>
            {
                switch (res.Status)
                {
                    case TaskStatus.RanToCompletion:
                        try
                        {
                            continuation(res.Result);
                            tcs.SetResult(null);
                        }
                        catch (Exception e)
                        {
                            tcs.SetException(e);
                        }
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.SetException(res.Exception.Flatten());
                        break;
                }
            },
                options);

            return tcs.Task;
        }

        public static Task Then<TInput>(this Task<TInput> task, Action<TInput> continuation)
        {
            return task.Then(continuation, TaskContinuationOptions.None);
        }

        public static Task<TOutput> Then<TOutput>(this Task task, Func<TOutput> continuation,
            TaskContinuationOptions options)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (continuation == null) throw new ArgumentNullException("continuation");

            var tcs = new TaskCompletionSource<TOutput>();
            task.ContinueWith(res =>
            {
                switch (res.Status)
                {
                    case TaskStatus.RanToCompletion:
                        try
                        {
                            tcs.SetResult(continuation());
                        }
                        catch (Exception e)
                        {
                            tcs.SetException(e);
                        }
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.SetException(res.Exception.Flatten());
                        break;
                }
            },
                options);
            return tcs.Task;
        }

        public static Task<TOutput> Then<TOutput>(this Task task, Func<TOutput> continuation)
        {
            return task.Then(continuation, TaskContinuationOptions.None);
        }

        public static Task Then(this Task task, Action continuation, TaskContinuationOptions options)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (continuation == null) throw new ArgumentNullException("continuation");

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(res =>
            {
                switch (res.Status)
                {
                    case TaskStatus.RanToCompletion:
                        try
                        {
                            continuation();
                            tcs.SetResult(null);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.SetException(res.Exception.Flatten());
                        break;
                }
            }, options);
            return tcs.Task;
        }

        public static Task Then(this Task task, Action continuation)
        {
            return task.Then(continuation, TaskContinuationOptions.None);
        }

        public static Task<TOutput> ThenIfNotCancelled<TInput, TOutput>(this Task<TInput> task,
            CancellationToken cancellationToken, Func<TInput, TOutput> continuation,
            TaskContinuationOptions options = TaskContinuationOptions.None)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (continuation == null) throw new ArgumentNullException("continuation");

            var tcs = new TaskCompletionSource<TOutput>();

            task.ContinueWith(t =>
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            try
                            {
                                var result = continuation(t.Result);
                                tcs.SetResult(result);
                            }
                            catch (Exception e)
                            {
                                tcs.SetException(e);
                            }
                        }
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        var exception = task.Exception;
                        Debug.Assert(exception != null);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetException(exception.InnerExceptions);
                        }
                        break;
                    default:
                        tcs.SetException(new InvalidOperationException());
                        break;
                }
            },
                options);

            return tcs.Task;
        }

        public static Task ThenIfNotCancelled<TInput>(this Task<TInput> task, CancellationToken cancellationToken,
            Action<TInput> continuation, TaskContinuationOptions options = TaskContinuationOptions.None)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (continuation == null) throw new ArgumentNullException("continuation");

            var tcs = new TaskCompletionSource<object>();

            task.ContinueWith(t =>
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            try
                            {
                                continuation(t.Result);
                                tcs.SetResult(null);
                            }
                            catch (Exception e)
                            {
                                tcs.SetException(e);
                            }
                        }
                        break;
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        var exception = task.Exception;
                        Debug.Assert(exception != null);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetException(exception.InnerExceptions);
                        }
                        break;
                    default:
                        tcs.SetException(new InvalidOperationException());
                        break;
                }
            },
                options);

            return tcs.Task;
        }

        public static Task<T> WithExceptionHandling<T>(this Task<T> task, Func<Exception, Exception> exceptionHandler)
        {
            var tcs = new TaskCompletionSource<T>();
            task.ContinueWith(res =>
            {
                if (res.IsFaulted)
                    tcs.SetException(exceptionHandler(res.Exception.Flatten()));
                else if (res.IsCanceled)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(res.Result);
            });
            return tcs.Task;
        }

        public static Task<T> WithExceptionHandling<T>(this Task<T> task, Action<Exception> exceptionHandler)
        {
            var tcs = new TaskCompletionSource<T>();
            task.ContinueWith(res =>
            {
                if (res.IsFaulted)
                {
                    var ex = res.Exception.Flatten();
                    exceptionHandler(ex);
                    tcs.SetException(ex);
                }
                else if (res.IsCanceled)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(res.Result);
            });
            return tcs.Task;
        }

        public static Task WithExceptionHandling(this Task task, Action<Exception> exceptionHandler)
        {
            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(res =>
            {
                if (res.IsFaulted)
                {
                    var ex = res.Exception.Flatten();
                    exceptionHandler(ex);
                    tcs.SetException(ex);
                }
                else if (res.IsCanceled)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(null);
            });
            return tcs.Task;
        }

        public static Task WithExceptionHandling(this Task task, Func<Exception, Exception> exceptionHandler)
        {
            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(res =>
            {
                if (res.IsFaulted)
                    tcs.SetException(exceptionHandler(res.Exception.Flatten()));
                else if (res.IsCanceled)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(null);
            });
            return tcs.Task;
        }

        [Obsolete("Defeats the purpose of tasks")]
        public static void WaitToBeCompleted(this Task task, long waitTimeOut)
        {
            if (task != null && !task.IsCompleted)
            {
                try
                {
                    if (!task.Wait(TimeSpan.FromMilliseconds(waitTimeOut)))
                        throw new SystemException("Task is hanging");
                }
                catch (AggregateException aggrEx)
                {
                    throw aggrEx.InnerException;
                }
            }
        }

        public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, string errorMessage)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (timeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("timeout");
            if (timeout == TimeSpan.Zero) return task;

            var tcs = new TaskCompletionSource<T>();

            Timer timer = null;
            TimerCallback timerCallback = _ =>
            {
                timer.Dispose();
                tcs.TrySetException(new TimeoutException(errorMessage));
            };
            timer = new Timer(timerCallback, null, timeout, TimeSpan.FromMilliseconds(-1));

            task.ContinueWith(res =>
            {
                timer.Dispose();

                if (res.IsFaulted)
                    tcs.TrySetException(res.Exception);
                else if (res.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(res.Result);
            },
                TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            return task.WithTimeout(timeout, "Task is timed out");
        }

        public static Task<T> Defer<T>(this Task<T> task, TimeSpan successTimeout, TimeSpan errorTimeout,
            TimeSpan cancelTimeout)
        {
            return task.Defer(successTimeout, errorTimeout, cancelTimeout, CancellationToken.None);
        }

        public static Task<T> Defer<T>(this Task<T> task, TimeSpan successTimeout, TimeSpan errorTimeout,
            TimeSpan cancelTimeout, CancellationToken token)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (successTimeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("successTimeout");
            if (errorTimeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("errorTimeout");
            if (cancelTimeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("cancelTimeout");

            var tcs = new TaskCompletionSource<T>();

            task.ContinueWith(res =>
            {
                if (token.IsCancellationRequested(res))
                {
                    tcs.SetCanceled();
                    return;
                }

                var timeout = GetTimeout(successTimeout, errorTimeout, cancelTimeout, res);
                if (timeout == TimeSpan.Zero)
                {
                    tcs.SetFrom(res);
                }
                else
                {
                    Timer timer = null;
                    TimerCallback timerCallback = _ =>
                    {
                        timer.Dispose();

                        if (token.IsCancellationRequested(res))
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetFrom(res);
                        }
                    };
                    timer = new Timer(timerCallback, null, timeout, TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
            },
                TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static Task Defer(this Task task, TimeSpan successTimeout, TimeSpan errorTimeout, TimeSpan cancelTimeout)
        {
            return task.Defer(successTimeout, errorTimeout, cancelTimeout, CancellationToken.None);
        }

        public static Task Defer(this Task task, TimeSpan successTimeout, TimeSpan errorTimeout, TimeSpan cancelTimeout,
            CancellationToken token)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (successTimeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("successTimeout");
            if (errorTimeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("errorTimeout");
            if (cancelTimeout.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException("cancelTimeout");

            var tcs = new TaskCompletionSource<object>();

            task.ContinueWith(res =>
            {
                if (token.IsCancellationRequested(res))
                {
                    tcs.SetCanceled();
                    return;
                }

                var timeout = GetTimeout(successTimeout, errorTimeout, cancelTimeout, res);
                if (timeout == TimeSpan.Zero)
                {
                    tcs.SetFrom(res, null);
                }
                else
                {
                    Timer timer = null;
                    TimerCallback timerCallback = _ =>
                    {
                        timer.Dispose();

                        if (token.IsCancellationRequested(res))
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetFrom(res, null);
                        }
                    };
                    timer = new Timer(timerCallback, null, timeout, TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
            },
                TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static Task<T> DeferException<T>(this Task<T> task, TimeSpan errorTimeout)
        {
            return task.Defer(TimeSpan.Zero, errorTimeout, TimeSpan.Zero);
        }

        public static Task<T> DeferException<T>(this Task<T> task, TimeSpan errorTimeout, CancellationToken token)
        {
            return task.Defer(TimeSpan.Zero, errorTimeout, TimeSpan.Zero, token);
        }

        public static Task DeferException(this Task task, TimeSpan errorTimeout)
        {
            return task.DeferException(errorTimeout, CancellationToken.None);
        }

        public static Task DeferException(this Task task, TimeSpan errorTimeout, CancellationToken token)
        {
            return task.Defer(TimeSpan.Zero, errorTimeout, TimeSpan.Zero, token);
        }

        public static Task<T> ThrowAsync<T>(this Exception exception)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static Task ThrowAsync(this Exception exception)
        {
            return exception.ThrowAsync<object>();
        }

        public static Task<T> Retry<T>(this Func<Task<T>> createTask, Action<Exception> onError, TimeSpan initialTimeout,
            TimeSpan incrementBy)
        {
            return createTask.Retry(onError, GenerateTimeouts(initialTimeout, incrementBy));
        }

        public static Task Retry(this Func<Task> createTask, Action<Exception> onError, TimeSpan initialTimeout,
            TimeSpan incrementBy)
        {
            return createTask.Retry(onError, GenerateTimeouts(initialTimeout, incrementBy));
        }

        public static Task<T> Retry<T>(this Func<Task<T>> createTask, Action<Exception> onError,
            IEnumerable<TimeSpan> timeouts)
        {
            return createTask.Retry(onError, timeouts, CancellationToken.None);
        }

        public static Task<T> Retry<T>(this Func<Task<T>> createTask, Action<Exception> onError,
            IEnumerable<TimeSpan> timeouts, CancellationToken token)
        {
            if (createTask == null) throw new ArgumentNullException("createTask");
            if (onError == null) throw new ArgumentNullException("onError");
            if (timeouts == null) throw new ArgumentNullException("timeouts");

            var enumerator = timeouts.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return createTask();
            }

            var task = createTask().DeferException(enumerator.Current, token);

            var tcs = new TaskCompletionSource<T>();
            Action<Task<T>> retry = null;
            retry = res =>
            {
                switch (res.Status)
                {
                    case TaskStatus.RanToCompletion:
                        enumerator.Dispose();
                        if (token.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetResult(res.Result);
                        }
                        break;
                    case TaskStatus.Canceled:
                        enumerator.Dispose();
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        var exception = res.Exception;
                        Debug.Assert(exception != null);
                        if (token.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            onError(exception.Flatten());
                            if (!enumerator.MoveNext())
                            {
                                enumerator.Dispose();
                                tcs.SetException(exception);
                                return;
                            }

                            task = createTask().DeferException(enumerator.Current, token);
                            task.ContinueWith(retry);
                        }
                        break;
                }
            };

            task.ContinueWith(retry);

            return tcs.Task;
        }

        public static Task Retry(this Func<Task> createTask, Action<Exception> onError, IEnumerable<TimeSpan> timeouts)
        {
            return createTask.Retry(onError, timeouts, CancellationToken.None);
        }

        public static Task Retry(this Func<Task> createTask, Action<Exception> onError, IEnumerable<TimeSpan> timeouts,
            CancellationToken token)
        {
            if (createTask == null) throw new ArgumentNullException("createTask");
            if (onError == null) throw new ArgumentNullException("onError");
            if (timeouts == null) throw new ArgumentNullException("timeouts");

            var enumerator = timeouts.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return createTask();
            }

            var task = createTask().DeferException(enumerator.Current, token);

            var tcs = new TaskCompletionSource<object>();
            Action<Task> retry = null;
            retry = res =>
            {
                switch (res.Status)
                {
                    case TaskStatus.RanToCompletion:
                        enumerator.Dispose();
                        if (token.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetResult(null);
                        }
                        break;
                    case TaskStatus.Canceled:
                        enumerator.Dispose();
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        var exception = res.Exception;
                        Debug.Assert(exception != null);
                        if (token.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            onError(exception.Flatten());
                            if (!enumerator.MoveNext())
                            {
                                enumerator.Dispose();
                                tcs.SetException(exception);
                                return;
                            }

                            task = createTask().DeferException(enumerator.Current, token);
                            task.ContinueWith(retry);
                        }
                        break;
                }
            };

            task.ContinueWith(retry);

            return tcs.Task;
        }

        public static Task<T> Finally<T>(this Task<T> task, Action finallyAction)
        {
            return task.Finally(finallyAction, TaskContinuationOptions.None, TaskScheduler.Current);
        }

        public static Task<T> Finally<T>(this Task<T> task, Action finallyAction, TaskScheduler scheduler)
        {
            return task.Finally(finallyAction, TaskContinuationOptions.None, scheduler);
        }

        public static Task<T> Finally<T>(this Task<T> task, Action finallyAction,
            TaskContinuationOptions continuationOptions)
        {
            return task.Finally(finallyAction, continuationOptions, TaskScheduler.Current);
        }

        public static Task<T> Finally<T>(this Task<T> task, Action finallyAction,
            TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (finallyAction == null) throw new ArgumentNullException("finallyAction");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            var tcs = new TaskCompletionSource<T>();
            task.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        finallyAction();
                        tcs.SetFrom(t);
                        break;
                }
            },
                CancellationToken.None,
                continuationOptions,
                scheduler);

            return tcs.Task;
        }

        public static Task Finally(this Task task, Action finallyAction)
        {
            return task.Finally(finallyAction, TaskContinuationOptions.None, TaskScheduler.Current);
        }

        public static Task Finally(this Task task, Action finallyAction, TaskScheduler scheduler)
        {
            return task.Finally(finallyAction, TaskContinuationOptions.None, scheduler);
        }

        public static Task Finally(this Task task, Action finallyAction, TaskContinuationOptions continuationOptions)
        {
            return task.Finally(finallyAction, continuationOptions, TaskScheduler.Current);
        }

        public static Task Finally(this Task task, Action finallyAction, TaskContinuationOptions continuationOptions,
            TaskScheduler scheduler)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (finallyAction == null) throw new ArgumentNullException("finallyAction");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        finallyAction();
                        tcs.SetFrom(t, null);
                        break;
                }
            },
                CancellationToken.None,
                continuationOptions,
                scheduler);

            return tcs.Task;
        }

        private static TimeSpan GetTimeout(TimeSpan successTimeout, TimeSpan errorTimeout, TimeSpan cancelTimeout,
            Task task)
        {
            if (task.IsFaulted)
                return errorTimeout;
            if (task.IsCanceled)
                return cancelTimeout;
            return successTimeout;
        }

        private static IEnumerable<TimeSpan> GenerateTimeouts(TimeSpan initialTimeout, TimeSpan incrementBy)
        {
            var timeout = initialTimeout;

            while (true)
            {
                yield return timeout;
                timeout += incrementBy;
            }
        }

        private static bool IsCancellationRequested(this CancellationToken token, Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                case TaskStatus.Canceled:
                    if (token.IsCancellationRequested)
                    {
                        return true;
                    }
                    break;
                case TaskStatus.Faulted:
                    if (token.IsCancellationRequested)
                    {
                        var exception = task.Exception;
                        Debug.Assert(exception != null);

                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}