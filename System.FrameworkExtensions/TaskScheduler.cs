using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Threading.Tasks
{
	//Doesn't make sense to support work stealing!
	public class N1TaskScheduler : TaskScheduler
	{
		private readonly Queue<Task> _pendingTasks = new Queue<Task>();

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			lock (_pendingTasks)
				return _pendingTasks.ToList();
		}
		protected override void QueueTask(Task task)
		{
			bool empty = false;
			lock (_pendingTasks)
			{
				empty = _pendingTasks.Count == 0;
				_pendingTasks.Enqueue(task);
			}

			if (empty)
			{
				ThreadPool.UnsafeQueueUserWorkItem(_ =>
				                                   {
					bool result = true;
					while (result)
					{
						Task t = null;
						lock (_pendingTasks)
						{
							result = (_pendingTasks.Count > 0);
							if (result)
								t = _pendingTasks.Dequeue();
						}
						if (result)
							TryExecuteTask(t);
					}
				}, null);
			}
		}
		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{   
			return false;
		}

		public override int MaximumConcurrencyLevel
		{
			get
			{
				return 1;
			}
		}
	}

	//Much of the code inherited from msdn
	//TryExecuteTaskInline.taskWasPreviouslyQueued will only be true IF the task is passed into waitall,
	//This is to support work stealing.
	public class NmTaskScheduler : TaskScheduler
	{
		[ThreadStatic] 
		private static bool _isProcessing;

		private readonly LinkedList<Task> _pendingTasks = new LinkedList<Task>();
		private readonly int _maxThreads;

		private int _threadCount = 0;

		public NmTaskScheduler(int maxThreads)
		{
			if (maxThreads < 2)
				throw new Exception("for sync execute, please use the N1TaskScheduler");

			_maxThreads = maxThreads;
		}

		protected override sealed void QueueTask(Task task)
		{
			lock (_pendingTasks)
			{
				_pendingTasks.AddLast(task);
				if (_threadCount < _maxThreads)
				{
					++_threadCount;
					ThreadPool.UnsafeQueueUserWorkItem(_ =>
					                                   {
						_isProcessing = true;
						try
						{
							while (true)
							{
								Task item;
								lock (_pendingTasks)
								{
									if (_pendingTasks.Count == 0)
									{
										--_threadCount;
										break;
									}
									item = _pendingTasks.First.Value;
									_pendingTasks.RemoveFirst();
								}
								base.TryExecuteTask(item);
							}
						}
						finally
						{
							_isProcessing = false;
						}
					}, null);
				}
			}
		}

		protected sealed override bool TryDequeue(Task task)
		{
			lock (_pendingTasks)
			{
				return _pendingTasks.Remove(task);
			}
		}

		protected override sealed bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			if (!_isProcessing) 
				return false;


			if (taskWasPreviouslyQueued) 
				if (TryDequeue(task)) // here it is trying to jump the queue, bad!!
					return base.TryExecuteTask(task);
			else
				return false;
			else
				return base.TryExecuteTask(task);
		}

		public override sealed int MaximumConcurrencyLevel
		{
			get { return _maxThreads; }
		}

		protected override sealed IEnumerable<Task> GetScheduledTasks()
		{
			lock (_pendingTasks)
				return _pendingTasks.ToList();
		}
	}
}
