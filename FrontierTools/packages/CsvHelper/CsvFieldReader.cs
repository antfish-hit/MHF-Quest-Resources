﻿// Copyright 2009-2019 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;
using System.IO;
using CsvHelper.Configuration;
using System.Threading.Tasks;

// This file is generated from a T4 template.
// Modifying it directly won't do you any good.

namespace CsvHelper
{
	/// <summary>
	/// Reads fields from a <see cref="TextReader"/>.
	/// </summary>
	public partial class CsvFieldReader : IFieldReader
	{
		private ReadingContext context;
		private bool disposed;

		/// <summary>
		/// Gets the reading context.
		/// </summary>
		public virtual ReadingContext Context => context;

		/// <summary>
		/// Gets a value indicating if the buffer is empty.
		/// True if the buffer is empty, otherwise false.
		/// </summary>
		public virtual bool IsBufferEmpty => context.BufferPosition >= context.CharsRead;

		/// <summary>
		/// Fills the buffer.
		/// </summary>
		/// <returns>True if there is more data left.
		/// False if all the data has been read.</returns>
		public virtual bool FillBuffer()
		{
			if (!IsBufferEmpty)
			{
				throw new InvalidOperationException($"The buffer can't be filled if it's not empty.");
			}

			if (context.Buffer.Length == 0)
			{
				context.Buffer = new char[context.ParserConfiguration.BufferSize];
			}

			if (context.CharsRead > 0)
			{
				// Create a new buffer with extra room for what is left from
				// the old buffer. Copy the remaining contents onto the new buffer.
				var charactersUsed = Math.Min(context.FieldStartPosition, context.RawRecordStartPosition);
				var bufferLeft = context.CharsRead - charactersUsed;
				var bufferUsed = context.CharsRead - bufferLeft;
				var tempBuffer = new char[bufferLeft + context.ParserConfiguration.BufferSize];
				Array.Copy(context.Buffer, charactersUsed, tempBuffer, 0, bufferLeft);
				context.Buffer = tempBuffer;

				context.BufferPosition = context.BufferPosition - bufferUsed;
				context.FieldStartPosition = context.FieldStartPosition - bufferUsed;
				context.FieldEndPosition = Math.Max(context.FieldEndPosition - bufferUsed, 0);
				context.RawRecordStartPosition = context.RawRecordStartPosition - bufferUsed;
				context.RawRecordEndPosition = context.RawRecordEndPosition - bufferUsed;
			}

			context.CharsRead = context.Reader.Read(context.Buffer, context.BufferPosition, context.ParserConfiguration.BufferSize);
			if (context.CharsRead == 0)
			{
				// End of file
				return false;
			}

			// Add the char count from the previous buffer that was copied onto this one.
			context.CharsRead += context.BufferPosition;

			return true;
		}

		/// <summary>
		/// Fills the buffer.
		/// </summary>
		/// <returns>True if there is more data left.
		/// False if all the data has been read.</returns>
		public virtual async Task<bool> FillBufferAsync()
		{
			if (!IsBufferEmpty)
			{
				throw new InvalidOperationException($"The buffer can't be filled if it's not empty.");
			}

			if (context.Buffer.Length == 0)
			{
				context.Buffer = new char[context.ParserConfiguration.BufferSize];
			}

			if (context.CharsRead > 0)
			{
				// Create a new buffer with extra room for what is left from
				// the old buffer. Copy the remaining contents onto the new buffer.
				var charactersUsed = Math.Min(context.FieldStartPosition, context.RawRecordStartPosition);
				var bufferLeft = context.CharsRead - charactersUsed;
				var bufferUsed = context.CharsRead - bufferLeft;
				var tempBuffer = new char[bufferLeft + context.ParserConfiguration.BufferSize];
				Array.Copy(context.Buffer, charactersUsed, tempBuffer, 0, bufferLeft);
				context.Buffer = tempBuffer;

				context.BufferPosition = context.BufferPosition - bufferUsed;
				context.FieldStartPosition = context.FieldStartPosition - bufferUsed;
				context.FieldEndPosition = Math.Max(context.FieldEndPosition - bufferUsed, 0);
				context.RawRecordStartPosition = context.RawRecordStartPosition - bufferUsed;
				context.RawRecordEndPosition = context.RawRecordEndPosition - bufferUsed;
			}

			context.CharsRead = await context.Reader.ReadAsync(context.Buffer, context.BufferPosition, context.ParserConfiguration.BufferSize).ConfigureAwait(false);
			if (context.CharsRead == 0)
			{
				// End of file
				return false;
			}

			// Add the char count from the previous buffer that was copied onto this one.
			context.CharsRead += context.BufferPosition;

			return true;
		}

		/// <summary>
		/// Creates a new <see cref="CsvFieldReader"/> using the given
		/// <see cref="TextReader"/> and <see cref="Configuration.Configuration"/>.
		/// </summary>
		/// <param name="reader">The text reader.</param>
		/// <param name="configuration">The configuration.</param>
		public CsvFieldReader(TextReader reader, Configuration.Configuration configuration) : this(reader, configuration, false) { }

		/// <summary>
		/// Creates a new <see cref="CsvFieldReader"/> using the given
		/// <see cref="TextReader"/>, <see cref="Configuration.Configuration"/>
		/// and leaveOpen flag.
		/// </summary>
		/// <param name="reader">The text reader.</param>
		/// <param name="configuration">The configuration.</param>
		/// <param name="leaveOpen">A value indicating if the <see cref="TextReader"/> should be left open when disposing.</param>
		public CsvFieldReader(TextReader reader, Configuration.Configuration configuration, bool leaveOpen)
		{
			context = new ReadingContext(reader, configuration, leaveOpen);
		}

		/// <summary>
		/// Gets the next char as an <see cref="int"/>.
		/// </summary>
		public virtual int GetChar()
		{
			var c = context.Buffer[context.BufferPosition];
			context.BufferPosition++;
			context.RawRecordEndPosition = context.BufferPosition;

			context.CharPosition++;

			return c;
		}

		/// <summary>
		/// Gets the field. This will append any reading progress.
		/// </summary>
		/// <returns>The current field.</returns>
		public virtual string GetField()
		{
			AppendField();

			if (context.IsFieldBad)
			{
				context.ParserConfiguration.BadDataFound?.Invoke(context);
			}

			context.IsFieldBad = false;

			var result = context.FieldBuilder.ToString();
			context.FieldBuilder.Clear();

			return result;
		}

		/// <summary>
		/// Appends the current reading progress.
		/// </summary>
		public virtual void AppendField()
		{
			if (context.ParserConfiguration.CountBytes)
			{
				context.BytePosition += context.ParserConfiguration.Encoding.GetByteCount(context.Buffer, context.RawRecordStartPosition, context.BufferPosition - context.RawRecordStartPosition);
			}

			context.RawRecordBuilder.Append(new string(context.Buffer, context.RawRecordStartPosition, context.RawRecordEndPosition - context.RawRecordStartPosition));
			context.RawRecordStartPosition = context.RawRecordEndPosition;

			var length = context.FieldEndPosition - context.FieldStartPosition;
			context.FieldBuilder.Append(new string(context.Buffer, context.FieldStartPosition, length));
			context.FieldStartPosition = context.BufferPosition;
			context.FieldEndPosition = 0;
		}

		/// <summary>
		/// Move's the buffer position according to the given offset.
		/// </summary>
		/// <param name="offset">The offset to move the buffer.</param>
		public virtual void SetBufferPosition(int offset = 0)
		{
			var position = context.BufferPosition + offset;
			if (position >= 0)
			{
				context.BufferPosition = position;
			}
		}

		/// <summary>
		/// Sets the start of the field to the current buffer position.
		/// </summary>
		/// <param name="offset">An offset for the field start.
		/// The offset should be less than 1.</param>
		public virtual void SetFieldStart(int offset = 0)
		{
			var position = context.BufferPosition + offset;
			if (position >= 0)
			{
				context.FieldStartPosition = position;
			}
		}

		/// <summary>
		/// Sets the end of the field to the current buffer position.
		/// </summary>
		/// <param name="offset">An offset for the field start.
		/// The offset should be less than 1.</param>
		public virtual void SetFieldEnd(int offset = 0)
		{
			var position = context.BufferPosition + offset;
			if (position >= 0)
			{
				context.FieldEndPosition = position;
			}
		}

		/// <summary>
		/// Sets the raw record start to the current buffer position;
		/// </summary>
		/// <param name="offset">An offset for the raw record start.
		/// The offset should be less than 1.</param>
		public virtual void SetRawRecordStart(int offset)
		{
			var position = context.BufferPosition + offset;
			if (position >= 0)
			{
				context.RawRecordStartPosition = position;
			}
		}

		/// <summary>
		/// Sets the raw record end to the current buffer position.
		/// </summary>
		/// <param name="offset">An offset for the raw record end.
		/// The offset should be less than 1.</param>
		public virtual void SetRawRecordEnd(int offset)
		{
			var position = context.BufferPosition + offset;
			if (position >= 0)
			{
				context.RawRecordEndPosition = position;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public virtual void Dispose()
		{
			Dispose(!context?.LeaveOpen ?? true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">True if the instance needs to be disposed of.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				context?.Dispose();
			}

			context = null;
			disposed = true;
		}
	}
}
