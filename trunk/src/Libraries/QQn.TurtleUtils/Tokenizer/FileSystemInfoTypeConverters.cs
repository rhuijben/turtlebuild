using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// TypeConverter which can only convert a string in a FileSystemInfo instance
	/// </summary>
	sealed class FileSystemInfoTypeConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string name = value as string;
			if (name != null)
			{
				if (name.IndexOfAny(new char[] { '*', '?' }) >= 0)
				{
					List<FileSystemInfo> fsi = new List<FileSystemInfo>();
					List<string> items = new List<string>();
					items.Add(name);

					FileExpandArgs args = new FileExpandArgs();
					args.FileExpandMode = FileExpandMode.DirectoryWildCards;
					args.RemoveNonExistingFiles = true;
					args.MatchDirectories = true;
					args.MatchFiles = false;
					if (Tokenizer.TryExpandFileList(items, args))
						foreach (string directory in items)
						{
							fsi.Add(new DirectoryInfo(directory));
						}

					items.Clear();
					args.MatchDirectories = false;
					args.MatchFiles = true;
					if (Tokenizer.TryExpandFileList(items, args))
						foreach (string file in items)
						{
							fsi.Add(new FileInfo(file));
						}

					return new ExpandableTokenValue(fsi);
				}
				else if (File.Exists(name))
					return new FileInfo(name);
				else if (Directory.Exists(name))
					return new DirectoryInfo(name);
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}
	}

	sealed class FileInfoTypeConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string name = value as string;
			if (name != null)
			{
				if (name.IndexOfAny(new char[] { '*', '?' }) >= 0)
				{
					List<FileInfo> fsi = new List<FileInfo>();
					List<string> items = new List<string>();
					items.Add(name);

					FileExpandArgs args = new FileExpandArgs();
					args.FileExpandMode = FileExpandMode.DirectoryWildCards;
					args.RemoveNonExistingFiles = true;
					if (Tokenizer.TryExpandFileList(items, args))
						foreach (string file in items)
						{
							fsi.Add(new FileInfo(file));
						}

					return new ExpandableTokenValue(fsi);
				}
				else
					return new FileInfo(name);
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}
	}

	sealed class DirectoryInfoTypeConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string name = value as string;
			if (name != null)
			{
				if (name.IndexOfAny(new char[] { '*', '?' }) >= 0)
				{
					List<DirectoryInfo> fsi = new List<DirectoryInfo>();
					List<string> items = new List<string>();
					items.Add(name);

					FileExpandArgs args = new FileExpandArgs();
					args.FileExpandMode = FileExpandMode.DirectoryWildCards;
					args.RemoveNonExistingFiles = true;
					args.MatchDirectories = true;
					args.MatchFiles = false;
					if (Tokenizer.TryExpandFileList(items, args))
						foreach (string directory in items)
						{
							fsi.Add(new DirectoryInfo(directory));
						}

					return new ExpandableTokenValue(fsi);
				}
				else
					return new DirectoryInfo(name);
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public sealed class ExpandableTokenValue : System.Collections.IEnumerable
	{
		readonly System.Collections.IEnumerable _collection;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpandableTokenValue"/> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		public ExpandableTokenValue(System.Collections.IEnumerable collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			_collection = collection;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
		/// </returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return _collection.GetEnumerator();
		}
	}
}
