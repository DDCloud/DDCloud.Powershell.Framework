using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace DDCloud.Powershell.Framework
{
	/// <summary>
	///		The base class for Cmdlets.
	/// </summary>
	public abstract class CmdletBase
		: PSCmdlet, IDisposable
	{
		/// <summary>
		///		Initialise the <see cref="CmdletBase"/>.
		/// </summary>
		protected CmdletBase()
		{
		}

		/// <summary>
		///		Finaliser for <see cref="CmdletBase"/>.
		/// </summary>
		~CmdletBase()
		{
			Dispose(false);
		}

		/// <summary>
		///		Dispose of resources being used by the Cmdlet.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Dispose of resources being used by the Cmdlet.
		/// </summary>
		/// <param name="disposing">
		///		Explicit disposal?
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		///		Write a progress record to the output stream, and as a verbose message.
		/// </summary>
		/// <param name="progressRecord">
		///		The progress record to write.
		/// </param>
		protected void WriteVerboseProgress(ProgressRecord progressRecord)
		{
			if (progressRecord == null)
				throw new ArgumentNullException(nameof(progressRecord));

			WriteProgress(progressRecord);
			WriteVerbose(progressRecord.StatusDescription);
		}

		/// <summary>
		///		Write a progress record to the output stream, and as a verbose message.
		/// </summary>
		/// <param name="progressRecord">
		///		The progress record to write.
		/// </param>
		/// <param name="messageOrFormat">
		///		The message or message-format specifier.
		/// </param>
		/// <param name="formatArguments">
		///		Optional format arguments.
		/// </param>
		protected void WriteVerboseProgress(ProgressRecord progressRecord, string messageOrFormat, params object[] formatArguments)
		{
			if (progressRecord == null)
				throw new ArgumentNullException(nameof(progressRecord));

			if (String.IsNullOrWhiteSpace(messageOrFormat))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'messageOrFormat'.", nameof(messageOrFormat));

			if (formatArguments == null)
				throw new ArgumentNullException(nameof(formatArguments));

			progressRecord.StatusDescription = String.Format(messageOrFormat, formatArguments);
			WriteVerboseProgress(progressRecord);
		}

		/// <summary>
		///		Write a completed progress record to the output stream.
		/// </summary>
		/// <param name="progressRecord">
		///		The progress record to complete.
		/// </param>
		/// <param name="completionMessageOrFormat">
		///		The completion message or message-format specifier.
		/// </param>
		/// <param name="formatArguments">
		///		Optional format arguments.
		/// </param>
		protected void WriteProgressCompletion(ProgressRecord progressRecord, string completionMessageOrFormat, params object[] formatArguments)
		{
			if (progressRecord == null)
				throw new ArgumentNullException(nameof(progressRecord));

			if (String.IsNullOrWhiteSpace(completionMessageOrFormat))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'completionMessageOrFormat'.", nameof(completionMessageOrFormat));

			if (formatArguments == null)
				throw new ArgumentNullException(nameof(formatArguments));

			progressRecord.StatusDescription = String.Format(completionMessageOrFormat, formatArguments);
			progressRecord.PercentComplete = 100;
			progressRecord.RecordType = ProgressRecordType.Completed;
			WriteProgress(progressRecord);
			WriteVerbose(progressRecord.StatusDescription);
		}
	}
}
