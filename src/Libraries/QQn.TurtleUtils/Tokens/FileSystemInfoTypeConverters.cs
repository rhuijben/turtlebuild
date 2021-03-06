using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace QQn.TurtleUtils.Tokens
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
					TokenizerExpandCollection<FileSystemInfo> fsi = new TokenizerExpandCollection<FileSystemInfo>();
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

					return fsi;
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
					TokenizerExpandCollection<FileInfo> fsi = new TokenizerExpandCollection<FileInfo>();
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

					return fsi;
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
					TokenizerExpandCollection<DirectoryInfo> fsi = new TokenizerExpandCollection<DirectoryInfo>();
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

					return fsi;
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
	public sealed class TokenizerExpandCollection<T> : Collection<T>, ITokenizerExpandCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerExpandCollection&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="collection">a collection to copy</param>
		public TokenizerExpandCollection(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			foreach(T i in collection)
				Add(i);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerExpandCollection&lt;T&gt;"/> class.
		/// </summary>
		public TokenizerExpandCollection()
		{
		}
	}
}
