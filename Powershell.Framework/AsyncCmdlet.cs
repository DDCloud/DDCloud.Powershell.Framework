using System;
using System.Threading;
using System.Threading.Tasks;

namespace DDCloud.Powershell.Framework
{
	using Threading;
	
	/// <summary>
	///		Base class for Cmdlets that run asynchronously.
	/// </summary>
	/// <remarks>
	///		Inherit from this class if your Cmdlet needs to use <c>async</c> / <c>await</c> functionality.
	/// </remarks>
	public abstract class AsyncCmdlet
		: CmdletBase
	{
		/// <summary>
		///		The source for cancellation tokens that can be used to cancel Cmdlet execution.
		/// </summary>
		readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

		/// <summary>
		///		Initialise the <see cref="AsyncCmdlet"/>.
		/// </summary>
		protected AsyncCmdlet()
		{
		}

		/// <summary>
		///		Dispose of resources being used by the Cmdlet.
		/// </summary>
		/// <param name="disposing">
		///		Explicit disposal?
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_cancellationSource.Dispose();

			base.Dispose(disposing);
		}
		
		/// <summary>
		///		Asynchronously perform Cmdlet pre-processing.
		/// </summary>
		/// <returns>
		///		A <see cref="Task"/> representing asynchronous Cmdlet execution.
		/// </returns>
		protected virtual Task BeginProcessingAsync()
		{
			return BeginProcessingAsync(_cancellationSource.Token);
		}

		/// <summary>
		///		Asynchronously perform Cmdlet pre-processing.
		/// </summary>
		/// <param name="cancellationToken">
		///		A <see cref="CancellationToken"/> that can be used to cancel Cmdlet execution.
		/// </param>
		/// <returns>
		///		A <see cref="Task"/> representing asynchronous Cmdlet execution.
		/// </returns>
		protected virtual Task BeginProcessingAsync(CancellationToken cancellationToken)
		{
			return TaskHelper.CompletedTask;
		}

		/// <summary>
		///		Asynchronously perform Cmdlet processing.
		/// </summary>
		/// <returns>
		///		A <see cref="Task"/> representing asynchronous Cmdlet execution.
		/// </returns>
		protected virtual Task ProcessRecordAsync()
		{
			return ProcessRecordAsync(_cancellationSource.Token);
		}

		/// <summary>
		///		Asynchronously perform Cmdlet processing.
		/// </summary>
		/// <param name="cancellationToken">
		///		A <see cref="CancellationToken"/> that can be used to cancel Cmdlet execution.
		/// </param>
		/// <returns>
		///		A <see cref="Task"/> representing asynchronous Cmdlet execution.
		/// </returns>
		protected virtual Task ProcessRecordAsync(CancellationToken cancellationToken)
		{
			return TaskHelper.CompletedTask;
		}

		/// <summary>
		///		Asynchronously perform Cmdlet post-processing.
		/// </summary>
		/// <returns>
		///		A <see cref="Task"/> representing asynchronous Cmdlet execution.
		/// </returns>
		protected virtual Task EndProcessingAsync()
		{
			return EndProcessingAsync(_cancellationSource.Token);
		}

		/// <summary>
		///		Asynchronously perform Cmdlet post-processing.
		/// </summary>
		/// <param name="cancellationToken">
		///		A <see cref="CancellationToken"/> that can be used to cancel Cmdlet execution.
		/// </param>
		/// <returns>
		///		A <see cref="Task"/> representing asynchronous Cmdlet execution.
		/// </returns>
		protected virtual Task EndProcessingAsync(CancellationToken cancellationToken)
		{
			return TaskHelper.CompletedTask;
		}

		/// <summary>
		///		Perform Cmdlet pre-processing.
		/// </summary>
		protected sealed override void BeginProcessing()
		{
			ThreadAffinitiveSynchronizationContext.RunSynchronized(
				async () => await BeginProcessingAsync()
			);
		}

		/// <summary>
		///		Perform Cmdlet processing.
		/// </summary>
		protected sealed override void ProcessRecord()
		{
			ThreadAffinitiveSynchronizationContext.RunSynchronized(
				async () => await ProcessRecordAsync()
			);
		}

		/// <summary>
		///		Perform Cmdlet post-processing.
		/// </summary>
		protected sealed override void EndProcessing()
		{
			ThreadAffinitiveSynchronizationContext.RunSynchronized(
				async () => await EndProcessingAsync()
			);
		}

		/// <summary>
		///		Interrupt Cmdlet processing (if possible).
		/// </summary>
		protected sealed override void StopProcessing()
		{
			try
			{
				_cancellationSource.Cancel();
			}
			catch (ObjectDisposedException)
			{
				// Nothing we can do if cancellation source has already been disposed.
			}

			base.StopProcessing();
		}
	}
}
