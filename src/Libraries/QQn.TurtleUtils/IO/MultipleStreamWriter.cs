using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Allows writing multiple substreams to one parent stream. The individual substream can be read with a <see cref="MultipleStreamReader"/>
	/// </summary>
	public class MultipleStreamWriter : IDisposable
	{
		readonly Stream _output;
		readonly QQnBinaryWriter _writer;
		readonly List<MultiStreamItemHeader> _items = new List<MultiStreamItemHeader>();
		readonly int _maxCount;
		readonly long _startPosition;
		bool _updated;
		Stream _openStream;

		/// <summary>
		/// Initializes a new instance of the <see cref="MultipleStreamWriter"/> class.
		/// </summary>
		/// <param name="output">The output.</param>
		/// <param name="args">The args.</param>
		public MultipleStreamWriter(Stream output, MultipleStreamCreateArgs args)
		{
			if (output == null)
				throw new ArgumentNullException("output");

			_output = output;
			_writer = new QQnBinaryWriter(BaseStream);
			_maxCount = args.MaximumNumberOfStreams;

			// sizeof(_maxCount) + sizeof(_items) + sizeof(_nextHeaderPosition) + sizeof(item)*maxItems
			_startPosition = 4 + 4 + 8 + MultiStreamItemHeader.ItemSize * _maxCount;

			WriteHeader();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MultipleStreamWriter"/> class.
		/// </summary>
		/// <param name="output">The output.</param>
		public MultipleStreamWriter(Stream output)
			: this(output, new MultipleStreamCreateArgs())
		{
		}

		void WriteHeader()
		{
			BaseStream.Position = 0;
			_writer.Write((int)_maxCount);
			_writer.Write((int)_items.Count);

			_writer.Write((long)0); // Next header position; not used for now

			for (int i = 0; (i < _items.Count) && (i < _maxCount); i++)
			{
				_items[i].WriteTo(_writer);
			}

			if (BaseStream.Length < _startPosition)
			{
				BaseStream.SetLength(_startPosition);
				BaseStream.Position = _startPosition;
			}
		}

		/// <summary>
		/// Gets the base stream.
		/// </summary>
		/// <value>The base stream.</value>
		protected Stream BaseStream
		{
			get { return _output; }
		}

		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Close();
		}

		/// <summary>
		/// Creates a new SubStream
		/// </summary>
		/// <returns></returns>
		public Stream CreateStream()
		{
			return CreateStream(0);
		}

		/// <summary>
		/// Creates a new substream
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public Stream CreateStream(int type)
		{
			if ((type < 0) || (type > 0xFFFFFF))
				throw new ArgumentOutOfRangeException("type", type, "Value must be between 0 and 16777216");

			MultipleStreamArgs args = new MultipleStreamArgs();
			args.StreamType = type;
			return CreateStream(args);
		}

		/// <summary>
		/// Creates the stream.
		/// </summary>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		public Stream CreateStream(MultipleStreamArgs args)
		{
			if (_openStream != null)
				throw new InvalidOperationException("Only one substream can be open at a time");
			else if (args == null)
				throw new ArgumentNullException("args");

			if (_items.Count == _maxCount)
			{
				// TODO: Implement end header support
				throw new InvalidOperationException("Can't create any more substreams");
			}
			MultiStreamItemHeader header = new MultiStreamItemHeader();
			header.Offset = BaseStream.Position = BaseStream.Length;
			header.ItemType = args.StreamType << 4;
			_updated = true;

			MultipleSubStreamWriter innerStream = new MultipleSubStreamWriter(this, BaseStream, header);
			Stream result = innerStream;

			if (args.Assured)
			{
				result = new AssuredSubStream(result, VerificationMode.Full);
				header.ItemType |= MultiStreamItemHeader.AssuredFlag;
			}

			if (args.GZipped)
			{
				result = new ZLibSubStream(result, CompressionMode.Compress);
				header.ItemType |= MultiStreamItemHeader.ZippedFlag;
			}

			_openStream = innerStream;
			return result;
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public virtual void Close()
		{
			if (_updated)
				WriteHeader();

			_writer.Close();

			BaseStream.Close();
		}

		internal void CloseStream(MultipleSubStreamWriter multiSubStream)
		{
			if (_openStream != multiSubStream)
				throw new InvalidOperationException();

			_items.Add(multiSubStream.Header);

			_openStream = null;
		}
	}
}
