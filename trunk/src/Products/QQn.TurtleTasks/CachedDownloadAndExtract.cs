using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Net.Cache;
using System.Globalization;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Diagnostics;

namespace QQn.TurtleTasks
{
	public sealed class CachedDownloadAndExtract : QQnTaskBase
	{
		ITaskItem[] _downloadDir;
		ITaskItem[] _uris;
		ITaskItem[] _targetDirs;
		ITaskItem[] _ids;
		List<TaskItem> _filesWritten = new List<TaskItem>();
		List<TaskItem> _cacheFiles = new List<TaskItem>();

		[Required]
		public ITaskItem[] DownloadDir
		{
			get { return _downloadDir; }
			set { _downloadDir = value; }
		}

		[Required]
		public ITaskItem[] Uris
		{
			get { return _uris; }
			set { _uris = value; }
		}

		[Required]
		public ITaskItem[] TargetDir
		{
			get { return _targetDirs; }
			set { _targetDirs = value; }
		}

		[Required]
		public ITaskItem[] Prefix
		{
			get { return _ids; }
			set { _ids = value; }
		}

		[Output]
		public ITaskItem[] FilesWritten
		{
			get { return _filesWritten.ToArray(); }
		}

		[Output]
		public ITaskItem[] CacheFiles
		{
			get { return _cacheFiles.ToArray(); }
		}

		class ExtractItem
		{
			readonly Uri _uri;
			readonly string _file;
			readonly string _toDir;
			readonly string _prefix;
			readonly string _name;
			bool _isUpdated;

			public ExtractItem(Uri uri, string file, string toDir, string prefix, string filename)
			{
				_uri = uri;
				_file = file;
				_toDir = toDir;
				_prefix = prefix;
				_name = filename;
			}

			public Uri Uri
			{
				get { return _uri; }
			}

			public string TmpFile
			{
				get { return _file; }
			}

			public string ToDir
			{
				get { return _toDir; }
			}

			public string Prefix
			{
				get { return _prefix; }
			}

			public bool IsUpdated
			{
				get { return _isUpdated; }
				set { _isUpdated = value; }
			}

			public string Name
			{
				get { return _name; }
			}
		}

		public override bool Execute()
		{
			bool ok = false;
			if (Uris == null)
				Log.LogError("Uri value must be set");
			else if (TargetDir == null)
				Log.LogError("TargetDir must be set");
			else if (DownloadDir == null)
				Log.LogError("DownloadDir mus be set");
			else if (Prefix == null)
				Log.LogError("Prefix mus be set");
			else
				ok = true;

			if (!ok)
				return false;

			string[] downloadDir = ApplySecondaryValue(DownloadDir, "DownloadDir", Uris);
			string[] targetDir = ApplySecondaryValue(TargetDir, "TargetDir", Uris);
			string[] prefix = ApplySecondaryValue(Prefix, "Prefix", Uris);

			if (Uris.Length == 0)
				return true;

			List<ExtractItem> items = new List<ExtractItem>();

			for (int i = 0; i < Uris.Length; i++)
			{
				Uri uri;
				if (!Uri.TryCreate(Uris[i].ItemSpec, UriKind.Absolute, out uri))
				{
					Log.LogError("'{0}' is not a valid uri; skipped", Uris[i].ItemSpec);
				}
				else
				{
					string fileName = uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
					fileName = fileName.Substring(fileName.LastIndexOf('/') + 1); // Error of LastIndexOf = -1

					//string downloadDir = dirDownloadDir[DownloadDir.Length == 1 ? 0 : i].ItemSpec;

					if (!String.IsNullOrEmpty(prefix[i]) && !prefix[i].EndsWith("-", StringComparison.Ordinal))
						prefix[i] += "-";

					// TODO: Use QQnPath
					string dlDir = string.IsNullOrEmpty(downloadDir[i]) ? Path.Combine(Path.GetTempPath(), "tbDownloadCache") : downloadDir[i];
					string tmpPath = Path.GetFullPath(Path.Combine(dlDir, fileName));

					ExtractItem ei = new ExtractItem(uri, tmpPath, targetDir[i], prefix[i], fileName);

					items.Add(ei);

					_cacheFiles.Add(new TaskItem(tmpPath));
				}
			}

			if (!EnsureDownloads(items))
				return false;

			bool updateOne = false;
			foreach (ExtractItem i in items)
			{
				if (i.IsUpdated)
				{ }
				else if (!File.Exists(Path.Combine(i.ToDir, i.Prefix + i.Name + ".tick")))
				{
					i.IsUpdated = true;
				}

				if (i.IsUpdated)
				{
					updateOne = true;
				}
			}

			if (!updateOne)
			{
				Log.LogMessage(MessageImportance.High, "== No extraction required ==");
				return true;
			}

			// Ok, we have all items; let's extract them			

			foreach (ExtractItem i in items)
			{
				if (!i.IsUpdated)
					continue;

				string toDir = i.ToDir;
				string ext = Path.GetExtension(i.Name).ToUpperInvariant();

				Log.LogMessage(MessageImportance.Normal, "Extracting '{0}' to '{1}'", i.TmpFile, toDir);

				if (!File.Exists(i.TmpFile))
					Log.LogError("'{0}' does not exist", i.TmpFile);

				if (!Directory.Exists(toDir))
					Directory.CreateDirectory(toDir);

				if (ext == ".ZIP" || ext == ".TPZ")
				{
					FastZipEvents fze = new FastZipEvents();
					fze.CompletedFile +=
						delegate(object sender, ScanEventArgs e)
						{
							_filesWritten.Add(new TaskItem(Path.Combine(toDir, e.Name)));
							e.ContinueRunning = true;
						};

					FastZip fz = new FastZip(fze);
					fz.CreateEmptyDirectories = true;
					fz.RestoreAttributesOnExtract = true;

					fz.ExtractZip(i.TmpFile, toDir, FastZip.Overwrite.Always, null, null, null, false);
				}
				// TODO: Check other types
				else
				{
					string file = Path.Combine(toDir, i.Name);
					File.Copy(i.TmpFile, file);
					_filesWritten.Add(new TaskItem(file));
				}

				File.WriteAllText(Path.Combine(i.ToDir, i.Prefix + i.Name + ".tick"), "");
			}
			Log.LogMessage(MessageImportance.High, "== File extraction completed ==");

			return true;
		}

		sealed class DownloadData : EventArgs
		{
			readonly ExtractItem _item;
			readonly EventHandler<DownloadData> _handler;
			string _errorText;
			WebClient client;
			bool _completed;
			bool _ok;
			bool _canceled;

			public DownloadData(ExtractItem item, EventHandler<DownloadData> handler)
			{
				_item = item;
				_handler = handler;
			}

			public void Start()
			{
				client = new WebClient();
				client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);

				ThreadPool.QueueUserWorkItem(new WaitCallback(Run));
			}

			void Run(object value)
			{
				Exception ex = null;
				try
				{
					client.DownloadFile(_item.Uri, _item.TmpFile);
				}
				catch (Exception ee)
				{
					ex = ee;

					_errorText = string.Format("Downloading '{0}' failed: {1}", _item.Uri, ee.ToString());
				}

				_completed = true;

				if (!_canceled && ex == null)
				{
					_ok = true;
					_item.IsUpdated = true;
				}
				else
					try
					{
						File.Delete(_item.TmpFile);
					}
					catch { }

				if (_handler != null)
					_handler(this, this);
			}

			public void Cancel()
			{
				_canceled = true;
				if (!_ok && _errorText == null)
					_errorText = "* aborted *";
			}

			public ExtractItem ExtractItem
			{
				get { return _item; }
			}
			public string FileName
			{
				get { return _item.TmpFile; }
			}

			public bool Ok()
			{
				return _ok;
			}

			public bool Completed()
			{
				return _completed;
			}

			public string ErrorText
			{
				get { return _errorText; }
			}
		}

		private bool EnsureDownloads(List<ExtractItem> items)
		{
			List<DownloadData> clients = new List<DownloadData>();
			List<string> delOnError = new List<string>();
			int nCompleted;

			nCompleted = 0;
			int nError = 0;

			ManualResetEvent ev = new ManualResetEvent(false);
			EventHandler<DownloadData> handler =
				delegate(object sender, DownloadData e)
				{
					bool done = false;
					lock (delOnError)
					{
						nCompleted++;

						if (!e.Ok())
							nError++;

						done = (nCompleted >= clients.Count);
					}
					if (done)
						ev.Set();
				};

			try
			{
				foreach (ExtractItem i in items)
				{	
					FileInfo fif = new FileInfo(i.TmpFile);

					if (!fif.Exists)
					{
						if (!Directory.Exists(fif.DirectoryName))
							Directory.CreateDirectory(fif.DirectoryName);

						clients.Add(new DownloadData(i, handler));

						delOnError.Add(i.TmpFile);
					}
					else
					{
						try 
						{
							// Touch the file
							fif.LastWriteTime = DateTime.Now;
						}
						catch { }
					}
				}

				if (clients.Count == 0)
				{
					Log.LogMessage(MessageImportance.Normal, "== No downloads required ==");
					return true;
				}

				Log.LogMessage(MessageImportance.High, "== started dependency downloading ==");


				DateTime start = DateTime.Now;
				foreach (DownloadData d in clients)
					d.Start();

				int n;
				if (0 > (n = WaitAnyWithUI(new WaitHandle[] { ev }, new TimeSpan(0, 30, 0), true)) || n == WaitHandle.WaitTimeout)
				{
					foreach (DownloadData d in clients)
					{
						if (!d.Completed())
							d.Cancel();
					}

					Log.LogError("== Download timeout exceeded, aborted ==");
					return false;
				}

				GC.KeepAlive(ev);

				if (nError > 0)
				{
					foreach (DownloadData d in clients)
					{
						if (d.ErrorText != null)
							Log.LogError(d.ErrorText);
					}

					Log.LogError("== One or more downloads failed; aborting ==");
					return false;
				}

				Log.LogMessage(MessageImportance.High, "Downloads completed successfully in {0}", (DateTime.Now - start));

				return true;
			}
			catch (Exception)
			{
				foreach (string path in delOnError)
				{
					if (File.Exists(path))
					{
						try
						{
							File.Delete(path);
						}
						catch
						{ }
					}
				}
				throw;
			}
		}
	}
}
