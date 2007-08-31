using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.XPath;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using QQn.ReportViewer.Tasks.ReportService2005;

namespace QQn.ReportViewer.Tasks
{
	public class PublishDataSources : Microsoft.Build.Utilities.Task
	{
		ITaskItem[] _dataSources;
		String _reportServer;
		String _targetFolder;
		bool _overwrite;
		bool _dontUseDefaultCredentials;

		[Required]
		public ITaskItem[] DataSources
		{
			get { return _dataSources; }
			set { _dataSources = value; }
		}

		[Required]
		public String ReportServerUrl
		{
			get { return _reportServer; }
			set { _reportServer = value; }
		}

		public String TargetFolder
		{
			get { return _targetFolder; }
			set { _targetFolder = value; }
		}

		public bool Overwrite
		{
			get { return _overwrite; }
			set { _overwrite = value; }
		}

		public bool UseDefaultCredentials
		{
			get { return !_dontUseDefaultCredentials; }
			set { _dontUseDefaultCredentials = value; }
		}

		public override bool Execute()
		{
#if DEBUG
			ReportingService2005 rs2005 = new ReportingService2005();
			rs2005.Url = ReportServerUrl;
			rs2005.UseDefaultCredentials = UseDefaultCredentials;

			SortedList<string, string> ExistingDataSources = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			if (!Overwrite)
			{
				foreach (CatalogItem item in rs2005.ListChildren(TargetFolder, false))
				{
					if (item.Type == ItemTypeEnum.DataSource)
					{
						ExistingDataSources[item.Name] = item.Name;
					}
				}
			}

			foreach (ITaskItem ds in DataSources ?? new ITaskItem[0])
			{
				XPathDocument doc = new XPathDocument(ds.ItemSpec);
				XPathNavigator nav = doc.CreateNavigator();
				nav.MoveToRoot();

				DataSourceDefinition dsd = new DataSourceDefinition();
				dsd.CredentialRetrieval = CredentialRetrievalEnum.Integrated;
				dsd.Enabled = true;
				dsd.EnabledSpecified = true;
				dsd.Prompt = null;
				dsd.WindowsCredentials = false;

				foreach (XPathNavigator n in nav.Select("/RptDataSource/ConnecionProperties/*"))
				{
					typeof(DataSetDefinition).InvokeMember(n.LocalName, System.Reflection.BindingFlags.SetProperty, null, dsd, new object[] { n.Value }, CultureInfo.InvariantCulture);
				}

				rs2005.CreateDataSource(nav.SelectSingleNode("/RptDataSource/Name").Value, TargetFolder, Overwrite, dsd, null);
			}
#endif
			return true;
		}
	}
}
