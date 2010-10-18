using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;

namespace QQn.TurtleTasks
{
	sealed class DownloadData : EventArgs
	{
		readonly ExtractItem _item;
		readonly EventHandler<DownloadData> _handler;
		string _errorText;
		DownloadWebClient client;
		bool _completed;
		bool _ok;
		bool _canceled;
		readonly CookieContainer _cookies = new CookieContainer();

		public DownloadData(ExtractItem item, EventHandler<DownloadData> handler)
		{
			_item = item;
			_handler = handler;
		}

		public void Start()
		{
			client = new DownloadWebClient(_cookies);

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

		sealed class DownloadWebClient : WebClient
		{
			readonly CookieContainer _cookies;
			public DownloadWebClient(CookieContainer cookies)
				: base()
			{
				_cookies = cookies;
				CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);
			}
			protected override WebRequest GetWebRequest(Uri address)
			{
				WebRequest wr = base.GetWebRequest(address);

				HttpWebRequest httpRq = wr as HttpWebRequest;

				if (httpRq != null)
				{
					httpRq.AllowAutoRedirect = true;
					if (_cookies != null)
						httpRq.CookieContainer = _cookies;
				}

				return wr;
			}
		}
	}
}
