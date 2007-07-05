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
	/// <typeparam name="TPackage"></typeparam>
	/// <typeparam name="TContainer"></typeparam>
	/// <typeparam name="TItem"></typeparam>
	public class Package<TPackage, TContainer, TItem> : ItemSetList<TContainer, TPackage, TContainer, TItem>
		where TPackage : Package<TPackage, TContainer, TItem>
		where TContainer : Container<TPackage, TContainer, TItem>, new()
		where TItem : Item<TPackage, TContainer, TItem>, new()
	{
		bool _readOnly;
		
		/// <summary>
		/// 
		/// </summary>
		protected Package()
		{
			Package = (TPackage)this;
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsReadOnly
		{
			get { return _readOnly; }
		}

		/// <summary>
		/// Adds or retrieves a container with the specified name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public TContainer AddContainer(string name)
		{
			if(string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			foreach (TContainer c in this)
			{
				if (c.Name == name)
					return c;
			}

			TContainer newC = new TContainer();
			newC.Name = name;

			Add(newC);
			return newC;
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
		/// <param name="name"></param>
		/// <returns></returns>
		public TContainer this[string name]
		{
			get
			{
				foreach (TContainer c in this)
				{
					if (c.Name == name)
						return c;
				}
				throw new KeyNotFoundException();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		public bool TryGetContainer(string name, out TContainer container)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			foreach (TContainer c in this)
			{
				if (c.Name == name)
				{
					container = c;
					return true;
				}
			}

			container = null;
			return false;
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
