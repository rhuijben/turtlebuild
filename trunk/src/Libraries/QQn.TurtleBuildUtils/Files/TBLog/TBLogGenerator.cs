using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;
using QQn.TurtleUtils.Tokens.Converters;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogGenerator
	{
		string _name;
		string _product;
		Version _version;
		DateTime _date;

		/// <summary>
		/// The name of the generator
		/// </summary>
		[Token("name")]
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// The product name of the generator
		/// </summary>
		[Token("product")]
		public string Product
		{
			get { return _product; }
			set { _product = value; }
		}

		/// <summary>
		/// The version of the generator
		/// </summary>
		[Token("version", TypeConverter = typeof(VersionConverter))]
		public Version Version
		{
			get { return _version; }
			set { _version = value; }
		}

		/// <summary>
		/// The date-time of generation
		/// </summary>
		[Token("date", TypeConverter = typeof(UtcDateTimeConverter))]
		public DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}
	}
}
