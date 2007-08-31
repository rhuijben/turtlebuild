using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using Microsoft.Build.Utilities;
using System.Xml.Xsl;
using System.Xml;

namespace QQn.ReportViewer.Tasks
{
	public class ConvertReportProjectForMSBuild : Task
	{
		ITaskItem[] _projects;
		ITaskItem _revertFile;

		/// <summary>
		/// Gets or sets the projects to convert.
		/// </summary>
		/// <value>The projects to convert.</value>
		[Required]
		public ITaskItem[] ProjectsToConvert
		{
			get { return _projects; }
			set { _projects = value; }
		}


		/// <summary>
		/// Gets or sets the revert file.
		/// </summary>
		/// <value>The revert file.</value>
		public ITaskItem RevertFile
		{
			get { return _revertFile; }
			set { _revertFile = value; }
		}

		XslCompiledTransform _rptTransform;
		XslCompiledTransform XslTransform
		{
			get
			{
				if (_rptTransform == null)
				{
					_rptTransform = new XslCompiledTransform();
					using (XmlReader reader = XmlReader.Create(typeof(ConvertReportProjectForMSBuild).Assembly.GetManifestResourceStream(typeof(ConvertReportProjectForMSBuild).Namespace + ".RptProjToMSBuild.xsll")))
					{
						_rptTransform.Load(reader);
					}
				}
				return _rptTransform;
			}
		}

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// true if the task successfully executed; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			if(ProjectsToConvert == null || ProjectsToConvert.Length == 0)
				return true;

			using (StreamWriter revertWriter = (RevertFile != null) ? File.AppendText(RevertFile.ItemSpec) : null)
			{
				if (revertWriter != null)
					revertWriter.WriteLine("# ConvertReportProjectForMSBuild - {0}", DateTime.Now);

				foreach (ITaskItem project in ProjectsToConvert ?? new ITaskItem[0])
				{
					string file = project.GetMetadata("FullPath");					

					if(!File.Exists(file))
						throw new FileNotFoundException(string.Format("File {0} not found for conversion", file), file);

					string revertFile = file + ".tbReverted";

					if (File.Exists(revertFile))
						File.SetAttributes(revertFile, FileAttributes.Normal);

					File.Copy(file, revertFile, true);

					if (revertWriter != null)
						revertWriter.WriteLine(revertFile);

					File.Delete(file);

					XslTransform.Transform(revertFile, file); // Retransforming a converted file will give exactly the same file, see xsl
				}
			}
			return true;
		}

	}
}
