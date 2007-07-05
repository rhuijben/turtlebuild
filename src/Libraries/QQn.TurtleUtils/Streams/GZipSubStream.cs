using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// GZip stream wrapper with <see cref="IServiceProvider"/> implementation to find parent streams
	/// </summary>
	public class GZipSubStream : GZipStream, IServiceProvider
	{
		/// <summary>
		/// Initializes a new instance of the System.IO.Compression.GZipStream class
		///    using the specified stream and System.IO.Compression.CompressionMode value.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode"></param>
		public GZipSubStream(Stream stream, CompressionMode mode)
			: base(stream, mode)
		{
		}

		/// <summary>
		/// Initializes a new instance of the System.IO.Compression.GZipStream class
		///     using the specified stream and System.IO.Compression.CompressionMode value,
		///     and a value that specifies whether to leave the stream open.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode"></param>
		/// <param name="leaveOpen"></param>
		public GZipSubStream(Stream stream, CompressionMode mode, bool leaveOpen)
			: base(stream, mode, leaveOpen)
		{
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
