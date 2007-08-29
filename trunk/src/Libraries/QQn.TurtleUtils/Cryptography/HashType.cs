using System;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames", Scope = "type", Target = "QQn.TurtleUtils.Cryptography.HashType")]

namespace QQn.TurtleUtils.Cryptography
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum HashType
	{
		/// <summary>
		/// 
		/// </summary>
		None	= 0x00,

		/// <summary>
		/// 
		/// </summary>
		MD5		= 0x01,
		/// <summary>
		/// 
		/// </summary>
		SHA1	= 0x02,
		/// <summary>
		/// 
		/// </summary>
		SHA256	= 0x03,
		/// <summary>
		/// 
		/// </summary>
		SHA512  = 0x04,

		/// <summary>
		/// 
		/// </summary>
		SHA384	= 0x05,

		/// <summary>
		/// Ripe's extended version of MD5. Autodetect of the hashvalue does not work for this type
		/// </summary>
		RipeMD160 = 0x06,

		/// <summary>
		/// Flag in addition to the type which appends the number of hashed bytes to the hash in strings
		/// </summary>
		PlusSize = 0x100,
		/// <summary>
		/// Flag in addition to the type which appends the number of hashtype to the hash in strings
		/// </summary>
		PlusType = 0x200,
	}
}
