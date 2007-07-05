using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
	public class MultiStreamReader : IDisposable
	{
		readonly VerificationMode _verificationMode;
		readonly Stream _input;
		readonly QQnBinaryReader _reader;
		readonly int _maxCount;
		readonly List<MultiStreamItemHeader> _items = new List<MultiStreamItemHeader>();
		MultiSubStreamReader _openReader;
		int _next = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="args"></param>
		public MultiStreamReader(Stream input, MultiStreamCreateArgs args)
		{
			if (input == null)
				throw new ArgumentNullException("input");
			else if (args == null)
				throw new ArgumentNullException("args");

			_input = input;
			_reader = new QQnBinaryReader(_input);
			_verificationMode = args.Verification;

			_maxCount = _reader.ReadInt32();
			int count = _reader.ReadInt32();

			long nextHeader = _reader.ReadInt64();

			for (int i = 0; i < count; i++)
			{
				_items.Add(new MultiStreamItemHeader(_reader));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		public MultiStreamReader(Stream input)
			: this(input, new MultiStreamCreateArgs())
		{
		}

		/// <summary>
		/// 
		/// </summary>
		protected Stream BaseStream
		{
			get { return _input; }
		}

		void IDisposable.Dispose()
		{
			Close();
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Close()
		{
			BaseStream.Close();
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Stream GetNextStream()
		{
			if (_openReader != null)
				throw new InvalidOperationException();
			else if (_next >= _items.Count)
				return null;

			MultiStreamItemHeader header = _items[_next++];
			MultiSubStreamReader reader = new MultiSubStreamReader(this, BaseStream, header);
			Stream s = reader;

			if(0 != (header.ItemType & MultiStreamItemHeader.AssuredFlag))
				s = new AssuredSubStream(s, _verificationMode);

			if (0 != (header.ItemType & MultiStreamItemHeader.GZippedFlag))
				s = new GZipSubStream(s, System.IO.Compression.CompressionMode.Decompress);

			_openReader = reader;
			return s;
		}

		internal void CloseStream(MultiSubStreamReader multiSubStreamReader)
		{
			if (_openReader != multiSubStreamReader)
				throw new InvalidOperationException();

			_openReader = null;
		}
	}
}
