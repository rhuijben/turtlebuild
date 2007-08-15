using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	/// <summary>
	/// Contains the parser state while tokanizing
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TokenizerState<T> : Hashtable, IDisposable, ITypeDescriptorContext
		where T : class, new()
	{
		readonly T _instance;
		readonly TokenizerDefinition _definition;
		readonly TokenizerArgs _args;
		ISupportInitialize _initialize;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerState&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="definition">The definition.</param>
		/// <param name="args">The args.</param>
		/// <remarks>Calls <see cref="ISupportInitialize.BeginInit"/> if the instance implements <see cref="ISupportInitialize"/></remarks>
		public TokenizerState(T instance, TokenizerDefinition definition, TokenizerArgs args)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			else if (definition == null)
				throw new ArgumentNullException("definition");
			else if (args == null)
				throw new ArgumentNullException("args");

			_instance = instance;
			_definition = definition;
			_args = args;
			_initialize = instance as ISupportInitialize;

			if (_initialize != null)
				_initialize.BeginInit();
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public T Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// Gets the definition.
		/// </summary>
		/// <value>The definition.</value>
		public TokenizerDefinition Definition
		{
			get { return _definition; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is complete.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplete
		{
			get
			{
				// TODO: Verify required arguments

				return true;
			}
		}

		/// <summary>
		/// Gets the tokenizer args.
		/// </summary>
		/// <value>The tokenizer args.</value>
		public TokenizerArgs TokenizerArgs
		{
			get { return _args; }
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>Calls <see cref="ISupportInitialize.EndInit"/> if the instance implements <see cref="ISupportInitialize"/></remarks>
		public void Dispose()
		{
			ISupportInitialize init = _initialize;
			if (init != null)
			{
				_initialize = null;

				init.EndInit();
			}
		}

		#endregion

		#region ITypeDescriptorContext Members

		/// <summary>
		/// Gets the container representing this <see cref="T:System.ComponentModel.TypeDescriptor"/> request.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.ComponentModel.IContainer"/> with the set of objects for this <see cref="T:System.ComponentModel.TypeDescriptor"/>; otherwise, null if there is no container or if the <see cref="T:System.ComponentModel.TypeDescriptor"/> does not use outside objects.</returns>
		IContainer ITypeDescriptorContext.Container
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the object that is connected with this type descriptor request.
		/// </summary>
		/// <value></value>
		/// <returns>The object that invokes the method on the <see cref="T:System.ComponentModel.TypeDescriptor"/>; otherwise, null if there is no object responsible for the call.</returns>
		object ITypeDescriptorContext.Instance
		{
			get { return Instance; }
		}

		/// <summary>
		/// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanged"/> event.
		/// </summary>
		void ITypeDescriptorContext.OnComponentChanged()
		{			
		}

		/// <summary>
		/// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanging"/> event.
		/// </summary>
		/// <returns>
		/// true if this object can be changed; otherwise, false.
		/// </returns>
		bool ITypeDescriptorContext.OnComponentChanging()
		{
			return false;
		}

		/// <summary>
		/// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"/> that is associated with the given context item.
		/// </summary>
		/// <value></value>
		/// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor"/> that describes the given context item; otherwise, null if there is no <see cref="T:System.ComponentModel.PropertyDescriptor"/> responsible for the call.</returns>
		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
		{
			get { return null; }
		}

		#endregion

		#region IServiceProvider Members

		/// <summary>
		/// Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType">An object that specifies the type of service object to get.</param>
		/// <returns>
		/// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
		/// </returns>
		object IServiceProvider.GetService(Type serviceType)
		{
			return null;
		}

		#endregion

		public CultureInfo CultureInfo
		{
			get { return _args.CultureInfo; }
		}
	}
}
