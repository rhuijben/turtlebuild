using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace QQn.TurtleBuildUtils
{
	/// <summary>
	/// Debug information container
	/// </summary>
	public class DebugReference
	{
		readonly string _pdbFile;
		readonly Guid _guid;
		readonly int _age;
		string _debugId;
		/// <summary>
		/// Initializes a new instance of the <see cref="DebugReference"/> class.
		/// </summary>
		/// <param name="pdbFile">The PDB file.</param>
		/// <param name="guid">The GUID.</param>
		/// <param name="age">The age.</param>
		public DebugReference(string pdbFile, Guid guid, int age)
		{
			if (string.IsNullOrEmpty(pdbFile))
				throw new ArgumentNullException("pdbFile");

			_pdbFile = pdbFile;
			_guid = guid;
			_age = age;
		}

		/// <summary>
		/// Gets the unique debug id of the file for retrieving the debug information from a symbol server
		/// </summary>
		/// <value>A string containing a guid as a 32 character uppercase hexadecimal string and an 
		/// age (lowercase hexadecimal) suffix without separation in between</value>
		public string DebugId
		{
			get
			{
				if (_debugId == null)
					_debugId = Guid.ToString("N").ToUpperInvariant() + Age.ToString("x", CultureInfo.InvariantCulture);

				return _debugId;
			}
		}

		/// <summary>
		/// Gets the file containing the debug information.
		/// </summary>
		/// <value>The PDB file.</value>
		public string PdbFile
		{
			get { return _pdbFile; }
		}

		/// <summary>
		/// Gets the age of the debug information
		/// </summary>
		/// <value>The age.</value>
		public int Age
		{
			get { return _age; }
		}

		/// <summary>
		/// Gets the GUID of the debug information
		/// </summary>
		/// <value>The GUID.</value>
		public Guid Guid
		{
			get { return _guid; }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return DebugId;
		}
	}
}
