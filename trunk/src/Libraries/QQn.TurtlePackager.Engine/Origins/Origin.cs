using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackager.Origins
{
	class Origin
	{
		Collection<Origin> _dependencies = new Collection<Origin>();
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

		public Collection<Origin> Dependencies
		{
			get { return _dependencies; }
		}

		public virtual void ApplyProjectDependencies(PackageState state)
		{
			
		}
	}
}
