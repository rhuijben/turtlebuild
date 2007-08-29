using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.Collections.Specialized;
using System.Collections;

namespace QQn.TurtleMSBuild
{
	class SimpleTaskItem : ITaskItem
	{
		NameValueCollection _metaData;
		string _itemSpec;

		public SimpleTaskItem(string itemSpec)
		{
			if (string.IsNullOrEmpty(itemSpec))
				throw new ArgumentNullException("itemSpec");

			_itemSpec = itemSpec;
			_metaData = new NameValueCollection();
		}
		#region ITaskItem Members

		/// <summary>
		/// Gets the collection of custom metadata.
		/// </summary>
		/// <returns>The collection of custom metadata.</returns>
		public System.Collections.IDictionary CloneCustomMetadata()
		{
			Hashtable ht = new Hashtable();

			foreach (string name in _metaData.Keys)
			{
				ht[name] = _metaData[name];

			}
			return ht;
		}

		/// <summary>
		/// Copies the custom metadata entries to another item.
		/// </summary>
		/// <param name="destinationItem">The item to copy the metadata entries to.</param>
		public void CopyMetadataTo(ITaskItem destinationItem)
		{
			foreach (string name in _metaData.Keys)
			{
				destinationItem.SetMetadata(name, _metaData[name]);

			}
		}

		/// <summary>
		/// Gets the value of the specified metadata entry.
		/// </summary>
		/// <param name="metadataName">The name of the metadata entry.</param>
		/// <returns>
		/// The value of the <paramref name="attributeName"/> metadata.
		/// </returns>
		public string GetMetadata(string metadataName)
		{
			return _metaData[metadataName];
		}

		/// <summary>
		/// Gets or sets the item specification.
		/// </summary>
		/// <value></value>
		/// <returns>The item specification.</returns>
		public string ItemSpec
		{
			get { return _itemSpec; }
			set { _itemSpec = value; }
		}

		public int MetadataCount
		{
			get { return _metaData.Count; }
		}

		public System.Collections.ICollection MetadataNames
		{
			get { return _metaData.Keys; }
		}

		public void RemoveMetadata(string metadataName)
		{
			_metaData.Remove(metadataName);
			
		}

		public void SetMetadata(string metadataName, string metadataValue)
		{
			_metaData[metadataName] = metadataValue;
		}

		#endregion
	}
}
