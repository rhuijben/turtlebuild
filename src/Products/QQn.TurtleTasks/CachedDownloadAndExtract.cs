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

namespace QQn.TurtleTasks
{
	public sealed class CachedDownloadAndExtract : Task
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

			ok = false;
			if (DownloadDir.Length != Uris.Length && DownloadDir.Length > 1)
				Log.LogError("DownloadDir.Length != Uri.Length && != 1");
			else if (TargetDir.Length != Uris.Length && TargetDir.Length == 1)
				Log.LogError("TargetDir.Length != Uri.Length && TargetDir.Length != 1");
			else if (Prefix.Length != Uris.Length && Prefix.Length > 1)
				Log.LogError("Prefix.Length != Uri.Length && Prefix.Length > 1");
			else
				ok = true;

			if (DownloadDir.Length == 0)
				DownloadDir = new ITaskItem[] { new TaskItem(Path.Combine(Path.GetTempPath(), "tbDownloadCache")) };

			if (!ok)
				return false;

			if (Uris.Length == 0)
				return true;

			List<Uri> uris = new List<Uri>();
			List<String> tmpPaths = new List<string>();
			List<String> names = new List<string>();
			List<String> extractorPaths = new List<string>();

			for (int i = 0; i < Uris.Length; i++)
			{
				Uri uri;
				if (!Uri.TryCreate(Uris[i].ItemSpec, UriKind.Absolute, out uri))
				{
					Log.LogError("'{0}' is not a valid uri; skipped", Uris[i].ItemSpec);
				}
				else
				{
					uris.Add(uri);
					string path = uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);

					string fileName = path.Substring(path.LastIndexOf('/') + 1); // Error of LastIndexOf = -1

					names.Add(fileName);

					string downloadDir = DownloadDir[DownloadDir.Length == 1 ? 0 : i].ItemSpec;
					string extractDir = TargetDir[TargetDir.Length == 1 ? 0 : i].ItemSpec;

					string prefix = (Prefix.Length > 0) ? Prefix[Prefix.Length == 1 ? 0 : i].ItemSpec : "";

					if(prefix.Length > 0 && !prefix.EndsWith("-", StringComparison.Ordinal))
						prefix += "-";

					// TODO: Use QQnPath
					string tmpPath = Path.GetFullPath(Path.Combine(downloadDir, fileName));
					tmpPaths.Add(tmpPath);
					extractorPaths.Add(Path.GetFullPath(extractDir));

					_cacheFiles.Add(new TaskItem(tmpPath));
				}
			}

			bool isUpdated;
			if (!EnsureDownloads(uris, tmpPaths, out isUpdated))
				return false;

			if(!isUpdated)
			{
				Log.LogMessage("No download actions performed, no extraction required");
				return false;
			}

			// Ok, we have all items; let's extract them			

			for (int i = 0; i < uris.Count; i++)
			{
				string tmp = tmpPaths[i];
				string toDir = extractorPaths[i];
				string name = names[i];
				string ext = Path.GetExtension(name).ToUpperInvariant();

				Log.LogMessage("Extracting {0} to {1}", tmp, toDir);

				if (!File.Exists(tmp))
					Log.LogError("'{0}' does not exist", tmp);

				if (!Directory.Exists(toDir))
					Directory.CreateDirectory(toDir);

				if (ext == ".ZIP")
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

					fz.ExtractZip(tmp, toDir, FastZip.Overwrite.Always, null, null, null, false);
				}
					// TODO: Check other types
				else
				{
					string file = Path.Combine(toDir, names[i]);
					File.Copy(tmp, file);
					_filesWritten.Add(new TaskItem(file));
				}
			}

			return true;
		}

		private bool EnsureDownloads(List<Uri> uris, List<String> tmpPaths, out bool isUpdated)
		{
			List<WebClient> clients;
			int nCompleted;

			isUpdated = false;
			clients = new List<WebClient>();
			nCompleted = 0;
			int nError = 0;

			AsyncCompletedEventHandler handler = delegate(object sender, AsyncCompletedEventArgs e)
			{
				Uri dlUri = (Uri)e.UserState;
				if (e.Cancelled)
				{
					Log.LogError("Download of {0} was canceled", dlUri);
					nError++;
				}
				else if (e.Error != null)
				{
					Log.LogError("Download of {0} failed: {1}", dlUri, e.Error);
					nError++;
				}
				else
				{
					Log.LogMessage("Download of {0} completed", dlUri);
					nCompleted++;
				}
			};
			List<string> delOnError = new List<string>() ;
			try
			{
				for (int i = 0; i < uris.Count; i++)
				{
					if (!File.Exists(tmpPaths[i]))
					{
						if (!Directory.Exists(Path.GetDirectoryName(tmpPaths[i])))
							Directory.CreateDirectory(Path.GetDirectoryName(tmpPaths[i]));

						if(delOnError.Count == 0)
							Log.LogMessage("== started dependency downloading ==");

						WebClient wc = new WebClient();
						wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.Revalidate);
						wc.DownloadFileCompleted += handler;
						wc.DownloadFileAsync(uris[i], tmpPaths[i], uris[i]);
						Log.LogMessage("Start downloading '{0}'...", uris[i], tmpPaths[i]);
						clients.Add(wc);
						delOnError.Add(tmpPaths[i]);
						isUpdated = true;
					}
				}

				DateTime now = DateTime.Now;
				DateTime start = now;
				DateTime next = DateTime.Now + new TimeSpan(0, 0, 10);
				DateTime err = DateTime.Now + new TimeSpan(1, 0, 0);

				while (nCompleted + nError < clients.Count)
				{
					Thread.Sleep(50); // TODO: Use some kind of sleep event
					now = DateTime.Now;
										
					if (now > next)
					{
						Log.LogMessage("Still downloading...");
						next = DateTime.Now + new TimeSpan(0, 0, 10);
					}
					else if (now > err)
					{
						Log.LogError("Hard limit of 30 minutes exceeded");
						throw new InvalidOperationException();
					}
				}

				if (nCompleted < clients.Count)
					return false;
				else if (nCompleted > 0)
					Log.LogMessage("= Downloads completed successfully in {0} =", now-start);
				else
					Log.LogMessage("= No downloads required =");

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
