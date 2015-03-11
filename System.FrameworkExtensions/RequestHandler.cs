using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    public sealed class RequestHandler<TRequestId, TResponse> : IEnumerable<TRequestId>
    {
        private readonly ConcurrentDictionary<TRequestId, Tuple<TaskCompletionSource<TResponse>, Timer>>
            _pendingRequests;

        private readonly TimeSpan _timeout;

        public RequestHandler(TimeSpan timeout, IEqualityComparer<TRequestId> comparer)
        {
            if (timeout.TotalMilliseconds < Timeout.Infinite)
                throw new ArgumentOutOfRangeException("timeout");

            _pendingRequests =
                new ConcurrentDictionary<TRequestId, Tuple<TaskCompletionSource<TResponse>, Timer>>(comparer);
            _timeout = timeout;
        }

        public RequestHandler(TimeSpan timeout)
            : this(timeout, EqualityComparer<TRequestId>.Default)
        {
        }

        public Task<TResponse> RegisterRequest(TRequestId requestId, TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds < Timeout.Infinite)
                throw new ArgumentOutOfRangeException("timeout");

            var requestCompletion = new TaskCompletionSource<TResponse>();

            var timer = ((int)timeout.TotalMilliseconds == Timeout.Infinite)
                ? default(Timer)
                : new Timer(_ =>
                {
                    Tuple<TaskCompletionSource<TResponse>, Timer> value;
                    if (_pendingRequests.TryRemove(requestId, out value))
                    {
                        value.Item2.Dispose();
                        value.Item1.TrySetException(new TimeoutException("Request with Id " + requestId + " timed out"));
                    }
                    else
                    {
                        //Log.WarnFormat("Can't find request with Id {0}, looks like it has been completed", requestId);
                    }
                });

            if (_pendingRequests.TryAdd(requestId, Tuple.Create(requestCompletion, timer)))
            {
                if (timer != null)
                {
                    timer.Change(timeout, TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
            }
            else
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
                throw new ArgumentException("Request with Id " + requestId + " has already been added");
            }
            return requestCompletion.Task;
        }

        public Task<TResponse> RegisterRequest(TRequestId requestId)
        {
            return RegisterRequest(requestId, _timeout);
        }

        public bool OnRequestCompleted(TRequestId requestId, TResponse response)
        {
            var cs = TryRemoveRequestCompletion(requestId);
            if (cs == null) return false;

            return cs.TrySetResult(response);
        }

        public bool OnRequestFailed(TRequestId requestId, Exception ex)
        {
            var cs = TryRemoveRequestCompletion(requestId);
            if (cs == null) return false;

            return cs.TrySetException(ex);
        }

        public bool OnRequestCancelled(TRequestId requestId)
        {
            var cs = TryRemoveRequestCompletion(requestId);
            if (cs == null) return false;

            return cs.TrySetCanceled();
        }

        public int CancelPendingRequests()
        {
            return _pendingRequests.Keys
                .Select(TryRemoveRequestCompletion)
                .Count(completion => completion != null && completion.TrySetCanceled());
        }

        private TaskCompletionSource<TResponse> TryRemoveRequestCompletion(TRequestId requestId)
        {
            Tuple<TaskCompletionSource<TResponse>, Timer> value;
            if (!_pendingRequests.TryRemove(requestId, out value))
            {
                //Log.WarnFormat("Can't find request with Id {0}, looks like it has timed out", response.RequestId);
                return null;
            }
            if (value.Item2 != null)
            {
                value.Item2.Dispose();
            }
            return value.Item1;
        }

        public IEnumerator<TRequestId> GetEnumerator()
        {
            return _pendingRequests.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}