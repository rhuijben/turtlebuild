using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace QQn.TurtleTasks
{
	public abstract class QQnTaskBase : Task
	{
		public string[] ApplySecondaryValue(ITaskItem[] values, string valueName, ITaskItem[] primaryValues)
		{
			if(primaryValues == null)
				throw new ArgumentNullException("primaryValues");

			if(values == null)
				values = new ITaskItem[0];

			string[] vals = new string[primaryValues.Length];

			if(values.Length <= 1)
			{
				string baseValue = (values.Length == 1) ? values[0].ItemSpec : null;

				for(int i = 0; i < primaryValues.Length; i++)
				{
					vals[i] = baseValue ?? ((valueName != null) ? primaryValues[i].GetMetadata(valueName) : null);
				}
			}
			else if(values.Length != primaryValues.Length)
				throw new ArgumentException(string.Format("The number of values in {0} must be 0, 1 or the number of primary items", valueName), "values");
			else
				for(int i = 0; i < values.Length; i++)
				{
					vals[i] = values[i].ItemSpec;
				}

			return vals;
		}
	}
}
