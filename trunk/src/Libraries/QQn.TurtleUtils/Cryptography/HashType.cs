using System;
using System.Collections.Generic;
using System.Text;

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
		Null	= 0x00,

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
		PlusSize = 0x100,
		/// <summary>
		/// 
		/// </summary>
		PlusType = 0x200,
	}
}