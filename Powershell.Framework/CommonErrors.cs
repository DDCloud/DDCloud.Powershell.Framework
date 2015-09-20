using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management.Automation;

namespace DDCloud.Powershell.Framework
{
	/// <summary>
	///		Factory methods for common <see cref="ErrorRecord"/>s.
	/// </summary>
	public static class CommonErrors
	{
		/// <summary>
		///		Create an <see cref="ErrorRecord"/> for when an unrecognised parameter set is encountered by a Cmdlet.
		/// </summary>
		/// <param name="cmdlet">
		///		The Cmdlet.
		/// </param>
		/// <returns>
		///		The configured <see cref="ErrorRecord"/>.
		/// </returns>
		public static ErrorRecord UnrecognizedParameterSet(PSCmdlet cmdlet)
		{
			if (cmdlet == null)
				throw new ArgumentNullException(nameof(cmdlet));

			return new ErrorRecord(
				new ArgumentException(
					String.Format(
						"Unrecognised parameter-set: '{0}'.",
						cmdlet.ParameterSetName
					)
				),
				"UnrecognisedParameterSet",
				ErrorCategory.InvalidArgument,
				cmdlet.ParameterSetName
			);
		}

		/// <summary>
		///		Create an <see cref="ErrorRecord"/> for when requested functionality is not implemented.
		/// </summary>
		/// <param name="messageOrFormat">
		///		A message or message format specifier describing what is not implemented (and why).
		/// </param>
		/// <param name="formatArguments">
		///		Optional message format arguments.
		/// </param>
		/// <exception cref="ArgumentException">
		///		<paramref name="messageOrFormat"/> is <c>null</c>, empty, or entirely composed of whitespace.
		/// </exception>
		/// <returns>
		///		The configured <see cref="ErrorRecord"/>.
		/// </returns>
		public static ErrorRecord NotImplemented(string messageOrFormat, params object[] formatArguments)
		{
			if (String.IsNullOrWhiteSpace(messageOrFormat))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'messageOrFormat'.", nameof(messageOrFormat));

			return new ErrorRecord(
				new NotImplementedException(
					String.Format(
						messageOrFormat,
						formatArguments
					)
				),
				"NotImplemented",
				ErrorCategory.NotImplemented,
				null
			);
		}

		/// <summary>
		///		Create an <see cref="ErrorRecord"/> for when a file was not found.
		/// </summary>
		/// <param name="file">
		///		A <see cref="FileInfo"/> representing the file.
		/// </param>
		/// <param name="description">
		///		A short description (sentence fragment) of the file that was not found.
		/// </param>
		/// <param name="errorCodePrefix">
		///		An optional string to prepend to the error code.
		/// </param>
		/// <returns>
		///		The configured <see cref="ErrorRecord"/>.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We only support FileInfo for this factory method because it relates to files")]
		public static ErrorRecord FileNotFound(FileInfo file, string description, string errorCodePrefix = "")
		{
			if (file == null)
				throw new ArgumentNullException(nameof(file));

			if (String.IsNullOrWhiteSpace(description))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'description'.", nameof(description));

			return new ErrorRecord(
				new FileNotFoundException(
					String.Format(
						"Cannot find {0} file '{1}'.",
						description,
						file.FullName
					),
					file.FullName
				),
				errorCodePrefix + "FileNotFound",
				ErrorCategory.ObjectNotFound,
				file
			);
		}
	}
}
