using System;
using System.Collections;
using System.Collections.Generic;


namespace QQn.TurtleMSBuild
{
	class Solution : MSBuildProject
	{
		bool _usedVcBuild;

		public Solution(string projectFile, string targetNames, IEnumerable properties, IEnumerable items, TurtleParameters parameters)
			: base(projectFile, targetNames, properties, items, parameters)
		{
			ProjectType = "Solution";
		}

		internal bool UsedVCBuild
		{
			get { return _usedVcBuild; }
			set { _usedVcBuild = value; }
		}

		public override bool IsSolution
		{
			get { return true; }
		}

		public override void ParseBuildResult(Project parentProject)
		{
			Refresh();

			if(UsedVCBuild)
				VCBuildHandler.HandleProject(this);
		}
	}
}
