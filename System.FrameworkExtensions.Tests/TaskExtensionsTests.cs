using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace System.FrameworkExtensions.Tests
{
	[TestFixture]
	public sealed class TaskExtensionsTests
	{
		[Test]
		public void CheckFinallyCallOrderWhenNoException()
		{
			var i = 0;

			var finallyInvoked = -1;
			var continuationInvoked = -1;

			const int value = 0;
			value.ReturnAsync()
				.Finally(() => finallyInvoked = Interlocked.Increment(ref i))
					.ContinueWith(t => continuationInvoked = Interlocked.Increment(ref i))
					.Wait();

			Assert.AreEqual(1, finallyInvoked);
			Assert.AreEqual(2, continuationInvoked);
		}

		[Test]
		public void CheckFinallyCallOrderWhenException()
		{
			var i = 0;

			var finallyInvoked = -1;
			var continuationInvoked = -1;

			new Exception().ThrowAsync()
				.Finally(() => finallyInvoked = Interlocked.Increment(ref i))
					.ContinueWith(t =>
					              {
						Assert.NotNull(t.Exception);
						return continuationInvoked = Interlocked.Increment(ref i);
					})
					.Wait();

			Assert.AreEqual(1, finallyInvoked);
			Assert.AreEqual(2, continuationInvoked);
		}

		[Test]
		public void CheckFinallyCallOrderWhenCancelled()
		{
			var i = 0;

			var finallyInvoked = -1;
			var continuationInvoked = -1;

			var tcs = new TaskCompletionSource<object>();
			var task = tcs.Task
				.Finally(() => finallyInvoked = Interlocked.Increment(ref i))
					.ContinueWith(t =>
					              {
						Assert.AreEqual(TaskStatus.Canceled, t.Status);
						return continuationInvoked = Interlocked.Increment(ref i);
					});

			tcs.SetCanceled();
			task.Wait();

			Assert.AreEqual(1, finallyInvoked);
			Assert.AreEqual(2, continuationInvoked);
		}
	}
}
