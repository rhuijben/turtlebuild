using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Allows writing multiple substreams to one parent stream. The individual substream can be read with a <see cref="MultiStreamReader"/>
	/// </summary>
	public class MultiStreamWriter : IDisposable
	{
		readonly Stream _output;
		readonly QQnBinaryWriter _writer;
		readonly List<MultiStreamItemHeader> _items = new List<MultiStreamItemHeader>();
		readonly int _maxCount;
		readonly long _startPosition;
		bool _updated;
		Stream _openStream;

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiStreamWriter"/> class.
		/// </summary>
		/// <param name="output">The output.</param>
		/// <param name="e">The e.</param>
		public MultiStreamWriter(Stream output, MultiStreamCreateArgs e)
		{
			if (output == null)
				throw new ArgumentNullException("output");

			_output = output;
			_writer = new QQnBinaryWriter(BaseStream);
			_maxCount = e.MaximumNumberOfStreams;

			// sizeof(_maxCount) + sizeof(_items) + sizeof(_nextHeaderPosition) + sizeof(item)*maxItems
			_startPosition = 4 + 4 + 8 + MultiStreamItemHeader.ItemSize * _maxCount; 

			WriteHeader();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiStreamWriter"/> class.
		/// </summary>
		/// <param name="output">The output.</param>
		public MultiStreamWriter(Stream output)
			: this(output, new MultiStreamCreateArgs())
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

			if(BaseStream.Length < _startPosition)
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

		void IDisposable.Dispose()
		{
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
		public Stream CreateStream(short type)
		{
			if (type != (type & MultiStreamItemHeader.TypeMask))
				throw new ArgumentOutOfRangeException("type", type, "Type must be between 0 and 4096");

			MultiStreamArgs args = new MultiStreamArgs();
			args.StreamType = type;
			return CreateStream(args);
		}

		/// <summary>
		/// Creates the stream.
		/// </summary>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		public Stream CreateStream(MultiStreamArgs args)
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
			header.Offset = BaseStream.Position;
			header.ItemType = args.StreamType;
			_updated = true;

			MultiSubStreamWriter innerStream = new MultiSubStreamWriter(this, BaseStream, header);
			Stream result = innerStream;

			if (args.Assured)
			{
				result = new AssuredSubStream(result, VerificationMode.Full);
				header.ItemType |= MultiStreamItemHeader.AssuredFlag;
			}

			if (args.GZipped)
			{
				result = new GZipSubStream(result, CompressionMode.Compress);
				header.ItemType |= MultiStreamItemHeader.GZippedFlag;
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

			BaseStream.Close();
		}

		internal void CloseStream(MultiSubStreamWriter multiSubStream)
		{
			if(_openStream != multiSubStream)
				throw new InvalidOperationException();

			_items.Add(multiSubStream.Header);

			_openStream = null;
		}
	}
}
