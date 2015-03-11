namespace System.Threading.Tasks
{
    //This is not great, but unfortunately there isn't a base class for TaskCompletionSource
    //here we pay the cost of re-direction and a interface-call
    public interface ITaskCompletionSourceProxy
    {
        void SetCanceled();
        void SetException(Exception ex);

        void TrySetCanceled();
        bool TrySetException(Exception ex);
    }

    //sadly c# doesn't support template specialization
    public static class TaskCompletionSourceProxyExtension
    {
        public static void SetResult<T>(this ITaskCompletionSourceProxy tcs, T o)
        {
            if (tcs is TaskCompletionSourceProxy<T>)
                (tcs as TaskCompletionSourceProxy<T>).SetResult(o);
        }

        public static bool TrySetResult<T>(this ITaskCompletionSourceProxy tcs, T o)
        {
            if (tcs is TaskCompletionSourceProxy<T>)
                return (tcs as TaskCompletionSourceProxy<T>).TrySetResult(o);
            return false;
        }
    }

    public class TaskCompletionSourceProxy<T> : ITaskCompletionSourceProxy
    {
        private readonly TaskCompletionSource<T> _tcs = null;

        public TaskCompletionSourceProxy(TaskCompletionSource<T> tcs)
        {
            _tcs = tcs;
        }

        public void SetResult(T result)
        {
            _tcs.SetResult(result);
        }

        public void SetCanceled()
        {
            _tcs.SetCanceled();
        }

        public void SetException(Exception ex)
        {
            _tcs.SetException(ex);
        }

        public bool TrySetResult(T result)
        {
            return _tcs.TrySetResult(result);
        }

        public void TrySetCanceled()
        {
            _tcs.TrySetCanceled();
        }

        public bool TrySetException(Exception ex)
        {
            return _tcs.TrySetException(ex);
        }
    }
}