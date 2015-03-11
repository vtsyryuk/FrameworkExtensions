namespace System.Threading.Tasks
{
	public static class TaskCompletionSourceExtensions
	{
		public static void SetFrom<T>(this TaskCompletionSource<T> tcs, Task<T> res)
		{
			if (res.IsFaulted)
				tcs.SetException(res.Exception);
			else if (res.IsCanceled)
				tcs.SetCanceled();
			else
				tcs.SetResult(res.Result);
		}

		public static void SetFrom<T>(this TaskCompletionSource<T> tcs, Task res, T result)
		{
			if (res.IsFaulted)
				tcs.SetException(res.Exception);
			else if (res.IsCanceled)
				tcs.SetCanceled();
			else
				tcs.SetResult(result);
		}
	}
}

