using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace DDCloud.Powershell.Framework.Threading
{
	/// <summary>
	///		Helper methods for working with <see cref="Task"/>s.
	/// </summary>
	public static class TaskHelper
	{
		/// <summary>
		///		A pre-completed task.
		/// </summary>
		public static readonly Task CompletedTask = Task.FromResult<object>(null);

		/// <summary>
		///		Synchronously <see cref="Task.Wait()">wait</see> for a <see cref="Task"/> to complete (and unwrap any resulting <see cref="AggregateException"/>, if practical).
		/// </summary>
		/// <param name="task">
		///		The <see cref="Task"/> to unwrap.
		/// </param>
		public static void SyncUnwrap(this Task task)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait();
			}
			catch (AggregateException aggregateException)
			{
				AggregateException flattened = aggregateException.Flatten();
				if (flattened.InnerExceptions.Count == 1)
					ExceptionDispatchInfo.Capture(flattened.InnerExceptions[0]).Throw();

				throw; // Genuine aggregate.
			}
		}

		/// <summary>
		///		Synchronously wait for A <see cref="Task{TResult}"/>'s <see cref="Task{TResult}.Result">result</see> (and unwrap any resulting <see cref="AggregateException"/>, if practical).
		/// </summary>
		/// <typeparam name="TResult">
		///		The task result type.
		/// </typeparam>
		/// <param name="task">
		///		The <see cref="Task{TResult}"/> to unwrap.
		/// </param>
		/// <returns>
		///		The task result.
		/// </returns>
		public static TResult SyncUnwrap<TResult>(this Task<TResult> task)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			try
			{
				return task.Result;
			}
			catch (AggregateException aggregateException)
			{
				AggregateException flattened = aggregateException.Flatten();
				if (flattened.InnerExceptions.Count == 1)
					ExceptionDispatchInfo.Capture(flattened.InnerExceptions[0]).Throw();

				throw; // Genuine aggregate.
			}
		}

		/// <summary>
		///		Wrap the specified <see cref="Task"/> in a <see cref="Task{TResult}"/> that, when the original <see cref="Task"/> completes, returns the specified result.
		/// </summary>
		/// <typeparam name="TResult">
		///		The task result type.
		/// </typeparam>
		/// <param name="task">
		///		The <see cref="Task"/> to wrap.
		/// </param>
		/// <param name="getResult">
		///		A delegate that returns the new <see cref="Task{TResult}"/>'s result.
		/// </param>
		/// <returns>
		///		The new <see cref="Task{TResult}"/>.
		/// </returns>
		public static Task<TResult> WithResult<TResult>(this Task task, Func<TResult> getResult)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			if (getResult == null)
				throw new ArgumentNullException(nameof(getResult));

			TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();
			task.ContinueWith(
				completedTask =>
				{
					if (completedTask.IsCompleted)
					{
						completionSource.TrySetResult(
							getResult()
						);
					}

					if (completedTask.IsCanceled)
						completionSource.TrySetCanceled();

					if (completedTask.IsFaulted)
						completionSource.TrySetException(completedTask.Exception.InnerExceptions);
				}
			);

			return completionSource.Task;
		}

		/// <summary>
		///		Wrap the specified <see cref="Task"/> in a <see cref="Task{TResult}"/> that, when the original <see cref="Task"/> completes, returns the specified result.
		/// </summary>
		/// <typeparam name="TResult">
		///		The task result type.
		/// </typeparam>
		/// <param name="task">
		///		The <see cref="Task"/> to wrap.
		/// </param>
		/// <param name="result">
		///		The result that the new <see cref="Task{TResult}"/> should return.
		/// </param>
		/// <returns>
		///		The new <see cref="Task{TResult}"/>.
		/// </returns>
		public static Task<TResult> WithResult<TResult>(this Task task, TResult result)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			return task.WithResult(
				() => result
			);
		}

		/// <summary>
		///		Wrap the task in an adaptor task that implements the classic Asynchronous Programming Model (APM) design pattern using the specified asynchronous callback and state data.
		/// </summary>
		/// <typeparam name="TResult">
		///		The task result type.
		/// </typeparam>
		/// <param name="task">
		///		The task.
		/// </param>
		/// <param name="callback">
		///		The asynchronous callback (if any) to invoke when the task is complete.
		/// </param>
		/// <param name="state">
		///		The state data (if any) to pass to the callback.
		/// </param>
		/// <returns>
		///		The APM wrapper task.
		/// </returns>
		public static Task<TResult> ToApm<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			// No need for TaskCompletionSource if the supplied async state already matches the existing task's async state.
			if (task.AsyncState == state)
			{
				if (callback != null)
				{
					task.ContinueWith(
						taskState => 
							callback(task),
						CancellationToken.None,
						TaskContinuationOptions.None,
						TaskScheduler.Default
					);
				}

				return task;
			} 

			TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(state);

			task.ContinueWith(
				taskState =>
				{
					if (task.IsFaulted)
						completionSource.TrySetException(task.Exception.InnerExceptions);
					else if (task.IsCanceled)
						completionSource.TrySetCanceled();
					else
						completionSource.TrySetResult(task.Result);

					if (callback != null)
						callback(completionSource.Task);
				},
				CancellationToken.None,
				TaskContinuationOptions.None,
				TaskScheduler.Default
			);

			return completionSource.Task;
		}

		/// <summary>
		///		Wrap the task in an adaptor task that implements the classic Asynchronous Programming Model (APM) design pattern using the specified asynchronous callback and state data.
		/// </summary>
		/// <param name="task">
		///		The task.
		/// </param>
		/// <param name="callback">
		///		The asynchronous callback (if any) to invoke when the task is complete.
		/// </param>
		/// <param name="state">
		///		The state data (if any) to pass to the callback.
		/// </param>
		/// <returns>
		///		The APM wrapper task.
		/// </returns>
		public static Task ToApm(this Task task, AsyncCallback callback, object state)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			// No need for TaskCompletionSource if the supplied async state already matches the existing task's async state.
			if (task.AsyncState == state)
			{
				if (callback != null)
				{
					task.ContinueWith(
						taskState => 							callback(task),
						CancellationToken.None,
						TaskContinuationOptions.None,
						TaskScheduler.Default
					);
				}

				return task;
			}

			TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(state);

			task.ContinueWith(
				taskState =>
				{
					if (task.IsFaulted)
						completionSource.TrySetException(task.Exception.InnerExceptions);
					else if (task.IsCanceled)
						completionSource.TrySetCanceled();
					else
						completionSource.TrySetResult(true);

					if (callback != null)
						callback(completionSource.Task);
				},
				CancellationToken.None,
				TaskContinuationOptions.None,
				TaskScheduler.Default
			);

			return completionSource.Task;
		}
	}
}
