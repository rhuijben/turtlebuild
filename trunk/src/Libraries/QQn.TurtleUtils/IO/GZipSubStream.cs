using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.IO;

[module: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "QQn.TurtleUtils.IO.GZipSubStream.GetService():T")]

namespace QQn.TurtleUtils.IO
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
