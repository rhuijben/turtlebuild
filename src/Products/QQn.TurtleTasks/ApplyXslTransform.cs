using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using System.Xml.Xsl;
using System.IO;
using Microsoft.Build.Framework;

namespace QQn.TurtleTasks
{
	public partial class ApplyXslTransform : QQnTaskBase
	{
		bool _debug;
		ITaskItem[] _sources = new ITaskItem[0];
		ITaskItem[] _transform = new ITaskItem[0];
		ITaskItem[] _outputs = new ITaskItem[0];
		internal List<ITaskItem> _filesWritten = new List<ITaskItem>();
		string _attributes = "";
		string _targetDir;
		string _intermediateDir;

		[Required]
		public ITaskItem[] Sources
		{
			get { return _sources; }
			set { _sources = value ?? new ITaskItem[0]; }
		}

		[Required]
		public ITaskItem[] Transform
		{
			get { return _transform; }
			set { _transform = value ?? new ITaskItem[0]; }
		}

		[Required]
		public ITaskItem[] Outputs
		{
			get { return _outputs; }
			set { _outputs = value ?? new ITaskItem[0]; }
		}

		public string TargetDir
		{
			get { return _targetDir; }
			set { _targetDir = value; }
		}

		public string IntermediateDir
		{
			get { return _intermediateDir; }
			set { _intermediateDir = value; }
		}

		public ITaskItem[] FilesWritten
		{
			get { return _filesWritten.ToArray(); }
		}

		public string Attributes
		{
			get { return _attributes; }
			set { _attributes = value ?? ""; }
		}

		public String Debug
		{
			get { return _debug.ToString(); }
			set
			{
				bool v;
				if (bool.TryParse(value, out v))
					_debug = v;
				else
					throw new InvalidOperationException();
			}
		}

		public class ApplyHelper
		{
			ApplyXslTransform _task;
			string _filename;

			public ApplyHelper(string filename, ApplyXslTransform task)
			{
				if (task == null)
					throw new ArgumentNullException("task");

				_task = task;
				_filename = filename;
			}

			public void DeclareOutput(string filename)
			{
				_task._filesWritten.Add(new TaskItem(filename));
			}

			public void LogMessage(string message)
			{
				_task.Log.LogMessage(null, null, null, _filename, 0, 0, 0, 0, message);
			}

			public void LogError(string message)
			{
				_task.Log.LogError(null, null, null, _filename, 0, 0, 0, 0, message);
			}

			public void LogWarning(string message)
			{
				_task.Log.LogWarning(null, null, null, _filename, 0,0,0,0, message);
			}

			public ApplyXslTransform Task
			{
				get { return _task; }
			}
		}

		static Dictionary<XslFilename, XslFile> _files = new Dictionary<XslFilename, XslFile>();

		public override bool Execute()
		{
			string[] templates = ApplySecondaryValue(Transform, "Transform", Sources);
			string[] outputs = ApplySecondaryValue(Outputs, "Output", Sources);

			if (Sources.Length == 0)
				return true;

			for(int i = 0; i < Sources.Length; i++)
			{
				XslFilename name = new XslFilename(Sources[i].ItemSpec, _debug);
				XslFile xsl;
				lock(_files)
				{
					if(!_files.TryGetValue(name, out xsl))
					{
						xsl = new XslFile(name);
						_files.Add(xsl, xsl);
					}
				}

				try
				{
					xsl.EnsureCompiled();
				}
				catch (XsltCompileException xc)
				{
					Log.LogError(null, null, null, xc.SourceUri, xc.LineNumber, xc.LinePosition, xc.LineNumber, xc.LinePosition, xc.Message);
					Log.LogError(null, null, null, xc.SourceUri, xc.LineNumber, xc.LinePosition, xc.LineNumber, xc.LinePosition, xc.ToString());
					throw;
				}

				string srcFile = Sources[i].ItemSpec;
				FileInfo fOut = new FileInfo(outputs[i]);

				if (fOut.Exists &&
					fOut.LastWriteTime > File.GetLastWriteTime(srcFile) &&
					fOut.LastWriteTime > File.GetLastWriteTime(xsl.Filename))
				{
					continue; // Skip processing
				}

				XsltArgumentList al = new XsltArgumentList();
				al.AddExtensionObject("http://schemas.qqn.nl/2008/02/ApplyXslTransform", new ApplyHelper(srcFile, this)); // Allows calling back into MSBuild
				al.AddParam("Src", "", srcFile);
				al.AddParam("SrcFilename", "", Path.GetFileName(srcFile));
				al.AddParam("SrcFileTitle", "", Path.GetFileNameWithoutExtension(srcFile));
				al.AddParam("SrcDir", "", Path.GetDirectoryName(Path.GetFullPath(srcFile)));
				al.AddParam("Dest", "", outputs[i]);
				al.AddParam("DestFilename", "", Path.GetFileName(outputs[i]));
				al.AddParam("DestFileTitle", "", Path.GetFileNameWithoutExtension(outputs[i]));
				al.AddParam("DestDir", "", Path.GetDirectoryName(Path.GetFullPath(outputs[i])));
				al.AddParam("BaseDir", "", Environment.CurrentDirectory);
				al.AddParam("ProjectFile", "", BuildEngine.ProjectFileOfTaskNode);
				al.AddParam("TargetDir", "", TargetDir);
				al.AddParam("IntermediateDir", "", IntermediateDir);
				
				foreach(string iAttr in Attributes.Split(';'))
				{
					if (string.IsNullOrEmpty(iAttr.Trim()))
						continue;

					string[] q = iAttr.Split('=');
					al.AddParam(q[0].Trim(), "", q[1].Trim());
				}

				Log.LogMessage(MessageImportance.High, "Transforming '{0}' into '{1}' using '{2}'", Sources[i].ItemSpec, outputs[i], Path.GetFileName(xsl.Filename));

				using(Stream wtr = File.Create(outputs[i]))
				{
					try
					{
						xsl.Transform.Transform(srcFile, al, wtr);
					}
					catch (XsltException xe)
					{
						Log.LogError(null, null, null, xe.SourceUri, xe.LineNumber, xe.LinePosition, xe.LineNumber, xe.LinePosition, xe.Message);
						Log.LogError(null, null, null, xe.SourceUri, xe.LineNumber, xe.LinePosition, xe.LineNumber, xe.LinePosition, xe.ToString());
					}

					_filesWritten.Add(new TaskItem(outputs[i]));
				}
			}

			return true;
		}
	}
}
