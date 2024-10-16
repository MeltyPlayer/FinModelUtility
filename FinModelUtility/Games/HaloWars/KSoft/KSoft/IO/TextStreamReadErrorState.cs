﻿using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	public interface ICanThrowReadExceptionsWithExtraDetails
	{
		void ThrowReadExeception(Exception detailsException);
	};

	public sealed class TextStreamReadErrorState
		: Text.IHandleTextParseError
		, ICanThrowReadExceptionsWithExtraDetails
	{
		readonly IKSoftStream mStream;
		Text.ITextLineInfo mReadLineInfo;
		public Func<Exception> GetLineInfoException { get; private set; }

		public TextStreamReadErrorState(IKSoftStream textStream)
		{
			Contract.Requires<ArgumentNullException>(textStream != null);

			this.mStream = textStream;
			this.mReadLineInfo = null;
			this.GetLineInfoException = this.GetLineInfoExceptionInternal;
		}

		/// <summary>Line info of the last read that took place</summary>
		/// <remarks>Rather, about to take place. Should be set before a read with a possible error executes</remarks>
		public Text.ITextLineInfo LastReadLineInfo
		{
			get { return this.mReadLineInfo; }
			set { this.mReadLineInfo = value; }
		}

		const string kReadLineInfoIsNullMsg =
			"A Text stream reader implementation failed to set the LastReadLineInfo before a read took place. " +
			"Guess what? Said read just failed";

		private Exception GetLineInfoExceptionInternal()
		{
			Contract.Assert(this.mReadLineInfo != null, kReadLineInfoIsNullMsg);

			return new Text.TextLineInfoException(this.mReadLineInfo, this.mStream.StreamName);
		}

		private Text.TextLineInfoException GetReadException(Exception detailsException)
		{
			return new Text.TextLineInfoException(detailsException, this.mReadLineInfo, this.mStream.StreamName);
		}

		/// <summary>Throws a <see cref="Text.TextLineInfoException"/> using <see cref="LastReadLineInfo"/></summary>
		/// <param name="detailsException">The details (inner) exception of what went wrong</param>
		public void ThrowReadExeception(Exception detailsException)
		{
			Contract.Assert(this.mReadLineInfo != null, kReadLineInfoIsNullMsg);

			throw this.GetReadException(detailsException);
		}

		public void LogReadExceptionWarning(Exception detailsException)
		{
			Contract.Assert(this.mReadLineInfo != null, kReadLineInfoIsNullMsg);

			Debug.Trace.IO.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"Failed to parse tag value: {0}",
				this.GetReadException(detailsException));
		}
	};
}
