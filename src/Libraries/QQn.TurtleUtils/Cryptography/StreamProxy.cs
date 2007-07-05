using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QQn.TurtleUtils.Cryptography
{
	public abstract class StreamProxy : Stream, IServiceProvider
	{
		readonly Stream _parentStream;
		readonly bool _closeParent;

		public StreamProxy(Stream parentStream, bool closeParent)
		{
			if (parentStream == null)
				throw new ArgumentNullException("parentStream");

			_parentStream = parentStream;
			_closeParent = closeParent;
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
		protected Stream ParentStream
		{
			get { return _parentStream; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports reading. 
		/// </summary>
		public override bool CanRead
		{
			get { return ParentStream.CanRead; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking
		/// </summary>
		public override bool CanSeek
		{
			get { return ParentStream.CanSeek; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing
		/// </summary>
		public override bool CanWrite
		{
			get { return ParentStream.CanWrite; }
		}

		/// <summary>
		/// Gets a value that determines whether the current stream can time out. 
		/// </summary>
		public override bool CanTimeout
		{
			get { return ParentStream.CanTimeout; }
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device
		/// </summary>
		public override void Flush()
		{
			ParentStream.Flush();
		}

		/// <summary>
		/// Gets or sets the position within the current stream. 
		/// </summary>
		public override long Position
		{
			get { return PositionToSubStream(ParentStream.Position); }
			set { ParentStream.Position = PositionToParent(value); }
		}

		/// <summary>
		/// When overridden translates a position in the parent stream to one in the substream
		/// </summary>
		/// <param name="parentPosition"></param>
		/// <returns></returns>
		protected abstract long PositionToSubStream(long parentPosition);

		/// <summary>
		/// When overridden translates a position in the substream to one in the parent stream
		/// </summary>
		/// <param name="subStreamPosition"></param>
		/// <returns></returns>
		protected abstract long PositionToParent(long subStreamPosition);

		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read. 
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
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
			return ParentStream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek)
				throw new NotSupportedException();

			long toPosition;
			switch (origin)
			{
				case SeekOrigin.Begin:
					if ((offset < 0) || (offset > Length))
						throw new IOException("Seeking outside substream");

					toPosition = ParentStream.Seek(PositionToParent(offset), SeekOrigin.Begin);
					break;					
				case SeekOrigin.Current:
					long newPos = Position + offset;
					if (newPos < 0)
						newPos = 0;
					else if (newPos > Length)
						throw new IOException("Seeking outside substream");

					toPosition = ParentStream.Seek(newPos, SeekOrigin.Begin);
					break;
				case SeekOrigin.End:
					if((offset < 0) || (offset > Length))
						throw new IOException("Seeking outside substream");

					toPosition = ParentStream.Seek(Position + Length - offset, SeekOrigin.Begin);
					break;
				default:
					throw new ArgumentOutOfRangeException("origin", origin, "Invalid origin");
			}
			return PositionToSubStream(toPosition);
		}

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written. 
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!CanWrite)
				throw new NotSupportedException();

			ParentStream.Write(buffer, offset, count);
		}

		#region IServiceProvider Members

		/// <summary>
		/// Gets the service object of the specified type.  
		/// </summary>
		/// <param name="serviceType"></param>
		/// <returns></returns>
		public virtual object GetService(Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException("serviceType");

			if (serviceType.IsAssignableFrom(GetType()))
				return this;

			IServiceProvider sp = ParentStream as IServiceProvider;
			if (sp != null)
				return sp.GetService(serviceType);
			else if (serviceType.IsAssignableFrom(ParentStream.GetType()))
				return ParentStream;
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
				ParentStream.Close();
		}

		/// <summary>
		/// Gets or sets a value that determines how long the stream will attempt to read before timing out.
		/// </summary>
		public override int ReadTimeout
		{
			get { return ParentStream.ReadTimeout; }
			set { ParentStream.ReadTimeout = value; }
		}

		/// <summary>
		/// Gets or sets a value that determines how long the stream will attempt to write before timing out. 
		/// </summary>
		public override int WriteTimeout
		{
			get { return ParentStream.WriteTimeout; }
			set { ParentStream.WriteTimeout = value; }			
		}

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream. 
		/// </summary>
		public override long Length
		{
			get { return PositionToSubStream(ParentStream.Length); }
		}

		/// <summary>
		/// When overridden in a derived class, sets the length of the current stream
		/// </summary>
		/// <param name="value"></param>
		public override void SetLength(long value)
		{
			ParentStream.SetLength(PositionToParent(value));
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
