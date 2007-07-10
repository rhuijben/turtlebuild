using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace QQn.TurtleUtils.ItemSets
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TRoot">The type of the root.</typeparam>
	public class ItemSetRoot<TRoot> : ItemSetBase<TRoot>
		where TRoot: ItemSetRoot<TRoot>
	{
		bool _readOnly;
		
		/// <summary>
		/// 
		/// </summary>
		protected ItemSetRoot()
		{
			Package = (TRoot)this;
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsReadOnly
		{
			get { return _readOnly; }
		}
		
		/// <summary>
		/// 
		/// </summary>
		protected void SetReadOnly()
		{
			_readOnly = true;
		}				

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xmlWriter"></param>
		public void WriteTo(XmlWriter xmlWriter)
		{
			XmlSerializer xs = new XmlSerializer(GetType());
			xs.Serialize(xmlWriter, this);
		}
	}
}
