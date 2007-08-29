using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "QQn.TurtleUtils.IO.ProxyStream.GetService():T")]

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Generic proxy class over an existing stream; forwards all calls to a parent <see cref="Stream"/> and leaves
	/// translation of the location to implementers of this abstract class
	/// </summary>
	public abstract class ProxyStream : Stream, IServiceProvider
	{
		readonly Stream _parentStream;
		readonly bool _closeParent;
		bool _closed;
		readonly bool _proxyAsyncRequests;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProxyStream"/> class.
		/// </summary>
		/// <param name="parentStream">The parent stream.</param>
		/// <param name="closeParent">if set to <c>true</c> close parent when closing this stream.</param>
		/// <param name="proxyAsyncRequest">if set to <c>true</c> [proxy async request].</param>
		protected ProxyStream(Stream parentStream, bool closeParent, bool proxyAsyncRequest)
		{
			if (parentStream == null)
				throw new ArgumentNullException("parentStream");

			_parentStream = parentStream;
			_closeParent = closeParent;
			_proxyAsyncRequests = proxyAsyncRequest;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProxyStream"/> class.
		/// </summary>
		/// <param name="parentStream">The parent stream.</param>
		/// <param name="closeParent">if set to <c>true</c> [close parent].</param>
		protected ProxyStream(Stream parentStream, bool closeParent)
			: this(parentStream, closeParent, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProxyStream"/> class.
		/// </summary>
		/// <param name="parentStream">The parent stream.</param>
		protected ProxyStream(Stream parentStream)
			: this(parentStream, true, false)
		{
		}

		/// <summary>
		/// Gets a boolean indicating whether the parent stream should be closed with this stream
		/// </summary>
		protected bool CloseParent
		{
			get { return _closeParent; }
		}

		/// <summary>
		/// Gets the inner stream
		/// </summary>
		protected Stream BaseStream
		{
			get { return _parentStream; }
		}


		/// <summary>
		/// Gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <value></value>
		/// <returns>true if the stream supports reading; otherwise, false.</returns>
		public override bool CanRead
		{
			get { return BaseStream.CanRead; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <value></value>
		/// <returns>true if the stream supports seeking; otherwise, false.</returns>
		public override bool CanSeek
		{
			get { return BaseStream.CanSeek; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing
		/// </summary>
		/// <value></value>
		/// <returns>true if the stream supports writing; otherwise, false.</returns>
		public override bool CanWrite
		{
			get { return BaseStream.CanWrite; }
		}

		/// <summary>
		/// Gets a value that determines whether the current stream can time out.
		/// </summary>
		/// <value></value>
		/// <returns>A value that determines whether the current stream can time out.</returns>
		public override bool CanTimeout
		{
			get { return BaseStream.CanTimeout; }
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device
		/// </summary>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override void Flush()
		{
			if (_closed)
				throw new InvalidOperationException();

			BaseStream.Flush();
		}

		/// <summary>
		/// Gets or sets the position within the current stream.
		/// </summary>
		/// <value></value>
		/// <returns>The current position within the stream.</returns>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Position
		{
			get { return PositionToSubStream(BaseStream.Position); }
			set 
			{
				if (_closed)
					throw new InvalidOperationException();
				
				BaseStream.Position = PositionToParent(value);
			}
		}

		/// <summary>
		/// Translates a position in the parent stream to one in the substream
		/// </summary>
		/// <param name="parentPosition">The parent position.</param>
		/// <returns></returns>
		/// <remarks>The default implementation returns the input value untranslated</remarks>
		protected virtual long PositionToSubStream(long parentPosition)
		{
			return parentPosition;
		}

		/// <summary>
		/// Translates a position in the substream to one in the parent stream
		/// </summary>
		/// <param name="subStreamPosition">The sub stream position.</param>
		/// <returns></returns>
		/// <remarks>The default implementation returns the input value untranslated</remarks>
		protected virtual long PositionToParent(long subStreamPosition)
		{
			return subStreamPosition;
		}


		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
		/// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (CanSeek)
			{
				if (count + Position > Length)
				{
					count = (int)Math.Max(0, Length - Position);

					if (count == 0)
						return 0;
				}
			}
			return BaseStream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Begins an asynchronous read operation.
		/// </summary>
		/// <param name="buffer">The buffer to read the data into.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data read from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
		/// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
		/// <returns>
		/// An <see cref="T:System.IAsyncResult"/> that represents the asynchronous read, which could still be pending.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">Attempted an asynchronous read past the end of the stream, or a disk error occurs. </exception>
		/// <exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="T:System.NotSupportedException">The current Stream implementation does not support the read operation. </exception>
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (_proxyAsyncRequests)
			{
				if (CanSeek)
				{
					if (count + Position > Length)
					{
						count = (int)Math.Max(0, Length - Position);
					}
				}
				return BaseStream.BeginRead(buffer, offset, count, callback, state);
			}
			else
				return base.BeginRead(buffer, offset, count, callback, state);
		}

		/// <summary>
		/// Waits for the pending asynchronous read to complete.
		/// </summary>
		/// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
		/// <returns>
		/// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="asyncResult"/> is null. </exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream. </exception>
		/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
		public override int EndRead(IAsyncResult asyncResult)
		{
			if (_proxyAsyncRequests)
				return BaseStream.EndRead(asyncResult);
			else
				return base.EndRead(asyncResult);
		}

		/// <summary>
		/// Begins an asynchronous write operation.
		/// </summary>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer"/> from which to begin writing.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
		/// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
		/// <returns>
		/// An IAsyncResult that represents the asynchronous write, which could still be pending.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">Attempted an asynchronous write past the end of the stream, or a disk error occurs. </exception>
		/// <exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="T:System.NotSupportedException">The current Stream implementation does not support the write operation. </exception>
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (_proxyAsyncRequests)
				return BaseStream.BeginWrite(buffer, offset, count, callback, state);
			else
				return base.BeginWrite(buffer, offset, count, callback, state);
		}

		/// <summary>
		/// Ends an asynchronous write operation.
		/// </summary>
		/// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="asyncResult"/> is null. </exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream. </exception>
		/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
		public override void EndWrite(IAsyncResult asyncResult)
		{
			if (_proxyAsyncRequests)
				BaseStream.EndWrite(asyncResult);
			else
				base.EndWrite(asyncResult);
		}

		

		

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
		/// <returns>
		/// The new position within the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek || _closed)
				throw new NotSupportedException();

			long toPosition;
			switch (origin)
			{
				case SeekOrigin.Begin:
					if ((offset < 0) || (offset > Length))
						throw new IOException("Seeking outside substream");

					toPosition = BaseStream.Seek(PositionToParent(offset), SeekOrigin.Begin);
					break;					
				case SeekOrigin.Current:
					long newPos = Position + offset;
					if (newPos < 0)
						newPos = 0;
					else if (newPos > Length)
						throw new IOException("Seeking outside substream");

					toPosition = BaseStream.Seek(newPos, SeekOrigin.Begin);
					break;
				case SeekOrigin.End:
					if((offset < 0) || (offset > Length))
						throw new IOException("Seeking outside substream");

					toPosition = BaseStream.Seek(Position + Length - offset, SeekOrigin.Begin);
					break;
				default:
					throw new ArgumentOutOfRangeException("origin", origin, "Invalid origin");
			}
			return PositionToSubStream(toPosition);
		}

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
		/// <exception cref="T:System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!CanWrite || _closed)
				throw new NotSupportedException();

			BaseStream.Write(buffer, offset, count);
		}

		#region IServiceProvider Members

		/// <summary>
		/// Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType">An object that specifies the type of service object to get.</param>
		/// <returns>
		/// A service object of type serviceType.-or- null if there is no service object of type serviceType.
		/// </returns>
		public virtual object GetService(Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException("serviceType");

			if (serviceType.IsAssignableFrom(GetType()))
				return this;

			IServiceProvider sp = BaseStream as IServiceProvider;
			if (sp != null)
				return sp.GetService(serviceType);
			else if (serviceType.IsAssignableFrom(BaseStream.GetType()))
				return BaseStream;
			else
				return null;
		}

		#endregion

		/// <summary>
		/// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
		/// </summary>
		public override void Close()
		{			
			base.Close();
			if(_closeParent)
				BaseStream.Close();
			_closed = true;
		}

		/// <summary>
		/// Gets or sets a value that determines how long the stream will attempt to read before timing out.
		/// </summary>
		/// <value></value>
		/// <returns>A value that determines how long the stream will attempt to read before timing out.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.IO.Stream.ReadTimeout"></see> method always throws an <see cref="T:System.InvalidOperationException"></see>. </exception>
		public override int ReadTimeout
		{
			get { return BaseStream.ReadTimeout; }
			set { BaseStream.ReadTimeout = value; }
		}

		/// <summary>
		/// Gets or sets a value that determines how long the stream will attempt to write before timing out.
		/// </summary>
		/// <value></value>
		/// <returns>A value that determines how long the stream will attempt to write before timing out.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.IO.Stream.WriteTimeout"></see> method always throws an <see cref="T:System.InvalidOperationException"></see>. </exception>
		public override int WriteTimeout
		{
			get { return BaseStream.WriteTimeout; }
			set { BaseStream.WriteTimeout = value; }			
		}

		/// <summary>
		/// Gets the length in bytes of the stream.
		/// </summary>
		/// <value></value>
		/// <returns>A long value representing the length of the stream in bytes.</returns>
		/// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Length
		{
			get { return PositionToSubStream(BaseStream.Length); }
		}

		/// <summary>
		/// Sets the length of the current stream
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override void SetLength(long value)
		{
			if (!CanWrite || _closed)
				throw new NotSupportedException();

			BaseStream.SetLength(PositionToParent(value));
		}

		/// <summary>
		/// Generic helper of <see cref="GetService(Type)"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected T GetService<T>()
		{
			return (T)GetService(typeof(T));
		}
	}
}
