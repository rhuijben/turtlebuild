using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackager.Origins
{
	enum DependencyType
	{
		/// <summary>
		/// Hard linked to a specific version
		/// </summary>
		HardLinked,

		/// <summary>
		/// Linked to the library
		/// </summary>
		LinkedTo,

		/// <summary>
		/// A compatible version of the dependency is required
		/// </summary>
		Required
	}

	class Origin
	{
		Dictionary<Origin, DependencyType> _dependencies = new Dictionary<Origin,DependencyType>();
		/// <summary>
		/// Publishes the output files.
		/// </summary>
		/// <param name="state">The state.</param>
		public virtual void PublishOriginalFiles(PackageState state)
		{
		}

		public virtual void PublishRequiredFiles(PackageState state)
		{
			
		}

		public IDictionary<Origin, DependencyType> Dependencies
		{
			get { return _dependencies; }
		}

		public virtual void ApplyProjectDependencies(PackageState state)
		{
			
		}
	}
}
