using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleBuildUtils.Files.TBLog;
using System.Collections.ObjectModel;
using QQn.TurtlePackager.Origins;
using QQn.TurtlePackage;
using QQn.TurtleUtils.Cryptography;
using System.Reflection;
using QQn.TurtleUtils.IO;

namespace QQn.TurtlePackager
{
	sealed class PackageState
	{
		readonly TBLogCache _logCache;
		readonly Collection<TBLogFile> _logFiles = new Collection<TBLogFile>();
		readonly Collection<Origin> _origins = new Collection<Origin>();
		readonly FileDataList _files = new FileDataList();
		readonly string _buildRoot;
		readonly bool _dontUseProjectDependencies;

		public PackageState(PackageArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			_logCache = args.LogCache;

			foreach (string file in args.ProjectsToPackage.KeysAsFullPaths)
			{
				Logs.Add(LogCache.Get(file));
			}

			_buildRoot = args.BuildRoot;
			_dontUseProjectDependencies = !args.UseProjectDependencies;

			if (!string.IsNullOrEmpty(BuildRoot))
			{
				Files.BaseDirectory = args.BuildRoot;
			}
		}

		public string BuildRoot
		{
			get { return _buildRoot; }
		}

		public TBLogCache LogCache
		{
			get { return _logCache; }
		}

		public Collection<TBLogFile> Logs
		{
			get { return _logFiles; }
		}

		public Collection<Origin> Origins
		{
			get { return _origins; }
		}

		public FileDataList Files
		{
			get { return _files; }
		}

		public bool DontUseProjectDependencies
		{
			get { return _dontUseProjectDependencies; }
		}

		public void LoadExternalOrigins()
		{
		}

		public void CreateBuildOrigins()
		{
			foreach (TBLogFile log in Logs)
			{
				Origins.Add(new BuildOrigin(log));
			}

			foreach (Origin o in Origins)
			{
				o.PublishOriginalFiles(this);
			}
		}

		public void AddRequirements()
		{
			foreach (Origin o in Origins)
			{
				o.PublishRequiredFiles(this);
			}
		}

        public void CalculateDependencies()
        {
            int n;
            do
            {
                n = 0;
                foreach (FileData fd in Files)
                {
                    if (fd.FindOrigin)
                    {
                        FileData baseFile;
                        if (!string.IsNullOrEmpty(fd.CopiedFrom) && Files.TryGetValue(fd.CopiedFrom, out baseFile))
                        {
                            if (baseFile.CopiedFrom != null)
                            {
                                fd.CopiedFrom = baseFile.CopiedFrom;
                                n++;
                            }

                            if (!baseFile.FindOrigin)
                            {
                                fd.Origin = baseFile.Origin;
                                fd.FindOrigin = false;
                                n++;
                            }
                        }
                        else
                        {
                            GC.KeepAlive(fd);
                        }
                    }
                }
            }
            while (n > 0);

            ExternalFileOrigin efo = new ExternalFileOrigin();
            Origins.Add(efo);
            foreach (FileData fd in Files)
            {
                if (fd.FindOrigin)
                {
                    string fname = fd.CopiedFrom ?? fd.FileName;

                    if (!efo.Files.Contains(fname))
                        efo.Files.Add(fname);

                    fd.Origin = efo;
                }
            }

            foreach (Origin o in Origins)
            {
                o.ApplyProjectDependencies(this);
            }
        }

        Dictionary<TBLogFile, TBLogFile> _packaged = new Dictionary<TBLogFile, TBLogFile>();

        public Pack CreateDefinition(TBLogFile file)
        {
            BuildOrigin myOrigin = null;
            foreach (BuildOrigin bo in BuildOrigins)
            {
                if (bo.LogFile == file)
                {
                    myOrigin = bo;
                    break;
                }
            }

            Pack p = new Pack();

            TBLogConfiguration config = file.Configurations[0];
            TBLogTarget target = config.Target;
            TBLogAssembly asm = config.Assembly;

            if (!string.IsNullOrEmpty(target.KeySrc))
                p.StrongNameKey = StrongNameKey.LoadFrom(QQnPath.Combine(file.ProjectPath, target.KeySrc));
            else if(!string.IsNullOrEmpty(target.KeyContainer))
                p.StrongNameKey = StrongNameKey.LoadFromContainer(target.KeyContainer, false);

            if (asm != null && !string.IsNullOrEmpty(asm.AssemblyName))
            {
                AssemblyName name = new AssemblyName(asm.AssemblyName);
                p.Version = name.Version;
            }

            PackContainer po = p.Containers.AddItem("#ProjectOutput");
            po.ContainerDir = config.OutputPath;
            po.BaseDir = QQnPath.Combine(file.ProjectPath);
            foreach (TBLogItem item in file.AllProjectOutput)
            {
                if(item.IsShared)
                    continue;

                PackFile pf = po.Files.AddItem(QQnPath.EnsureRelativePath(po.BaseDir, item.FullSrc));
                pf.StreamName = item.Src;
            }

            PackContainer ct = p.Containers.AddItem("#Content");
            ct.ContainerDir = "";
            po.BaseDir = file.ProjectPath;

            foreach (TBLogItem item in file.AllContents)
            {
                if (item.IsShared)
                    continue;

                PackFile pf = po.Files.AddItem(QQnPath.EnsureRelativePath(po.BaseDir, item.FullSrc));
            }

            myOrigin.Pack = p;

            return p;
        }

        /// <summary>
        /// Checks whether all origins for this file are available
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        internal bool CanPackage(TBLogFile file)
        {
            foreach (TBLogItem item in file.AllPublishItems)
            {
                if (!item.IsShared)
                    continue;

                FileData fd;

                if (!Files.TryGetValue(item.FullSrc, out fd))
                    return false;

                if (fd.Origin == null)
                    return false;

                BuildOrigin bo = fd.Origin as BuildOrigin;

                if (bo != null && bo.Pack == null)
                    return false; // Package this one first!
            }

            return true;
        }

        internal IEnumerable<BuildOrigin> BuildOrigins
        {
            get
            {
                foreach (Origin o in Origins)
                {
                    BuildOrigin bo = o as BuildOrigin;

                    if (bo != null)
                        yield return bo;
                }
            }
        }


        internal void SetOriginPack(TBLogFile file, Pack pack)
        {
            foreach (BuildOrigin origin in BuildOrigins)
            {
                if(origin.LogFile == file)
                {
                    origin.Pack = pack;
                    return;
                }
            }            
        }
    }
}
