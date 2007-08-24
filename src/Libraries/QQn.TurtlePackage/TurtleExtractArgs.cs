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
	public class TurtleExtractArgs
	{
		KeyedCollection<string, string> _containerList;
		bool _dontUseDirectoryMap;

		//SortedList<string, string> _containerList = null;
		public TurtleExtractArgs()
		{
		}

		public TurtleExtractArgs(ICollection<string> containers)
		{
			Containers = containers;
		}

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

		public bool UseDirectoryMap
		{
			get { return !_dontUseDirectoryMap; }
			set { _dontUseDirectoryMap = !value; }
		}
	}
}
