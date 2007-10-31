using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using QQn.TurtleUtils.IO;

[module: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "QQn.TurtleBuildUtils.FileCollection`1")]
[module: SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "QQn.TurtleBuildUtils.FileCollection`1")]

namespace QQn.TurtleBuildUtils
{
	/// <summary>
	/// 
	/// </summary>
	public interface IHasFileName
	{
		/// <summary>
		/// Gets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		string FileName { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FileCollection<T> : KeyedFileCollection<T>
		where T : IHasFileName
	{
		/// <summary>
		/// Extracts the key from the specified element
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		protected override string GetKeyForItem(T item)
		{
			return (item != null) ? item.FileName : null;
		}
	}
}
