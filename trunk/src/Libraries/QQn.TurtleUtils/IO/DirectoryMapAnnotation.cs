using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;

namespace QQn.TurtleUtils.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class DirectoryMapAnnotation
    {
        string _key;
        string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryMapAnnotation"/> class.
        /// </summary>
        public DirectoryMapAnnotation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryMapAnnotation"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public DirectoryMapAnnotation(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        [Token("key")]
        public string Key
        {
            get { return _key; }
            set
            {
                if (_key != null && value != _key)
                    throw new InvalidOperationException();

                _key = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [Token("value")]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class DirectoryMapAnnotationCollection : KeyedCollection<string, DirectoryMapAnnotation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryMapAnnotationCollection"/> class.
        /// </summary>
        public DirectoryMapAnnotationCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(DirectoryMapAnnotation item)
        {
            return item.Key;
        }
    }
}
