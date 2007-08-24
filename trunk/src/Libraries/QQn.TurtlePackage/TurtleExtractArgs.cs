using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace QQn.TurtlePackage
{
	class KeyedStringCollection : KeyedCollection<string, string>
	{
		public KeyedStringCollection()
			: base(StringComparer.InvariantCultureIgnoreCase)
		{
		}
		
		protected override string GetKeyForItem(string item)
		{
			return item;
		}
	}
	/// <summary>
	/// 
	/// </summary>
	public class TurtleExtractArgs
	{
		KeyedCollection<string, string> _containerList;
		bool _dontUseDirectoryMap;

		//SortedList<string, string> _containerList = null;
		/// <summary>
		/// Initializes a new instance of the <see cref="TurtleExtractArgs"/> class.
		/// </summary>
		public TurtleExtractArgs()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TurtleExtractArgs"/> class.
		/// </summary>
		/// <param name="containers">The containers.</param>
		public TurtleExtractArgs(ICollection<string> containers)
		{
			Containers = containers;
		}

		/// <summary>
		/// Gets or sets the containers.
		/// </summary>
		/// <value>The containers.</value>
		public ICollection<string> Containers
		{
			get { return _containerList; }
			set
			{
				if(value != null)
				{
					KeyedStringCollection ksc = new KeyedStringCollection();
					foreach(string s in value)
					{
						ksc.Add(s);
					}

					_containerList = ksc;
				}
			}
		}

		internal bool ExtractContainer(string containerName)
		{
			return (_containerList == null) || _containerList.Contains(containerName);
		}

		/// <summary>
		/// Gets or sets a value indicating whether [use directory map].
		/// </summary>
		/// <value><c>true</c> if [use directory map]; otherwise, <c>false</c>.</value>
		public bool UseDirectoryMap
		{
			get { return !_dontUseDirectoryMap; }
			set { _dontUseDirectoryMap = !value; }
		}
	}
}
