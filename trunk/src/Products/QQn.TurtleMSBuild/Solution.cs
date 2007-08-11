using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace QQn.TurtleMSBuild
{
	class Solution : MSBuildProject
	{
		public Solution(string projectFile, string targetNames, IEnumerable properties, IEnumerable items, TurtleParameters parameters)
			: base(projectFile, targetNames, properties, items, parameters)
		{
		}

		public override bool IsSolution
		{
			get { return true; }
		}

		public override void ParseBuildResult(Project parentProject)
		{
			Refresh();

			VCBuildHandler.HandleProject(this);
		}
	}
}
