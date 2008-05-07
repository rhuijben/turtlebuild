using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleBuildUtils;

namespace QQn.TurtlePackager.Origins
{
	class ExternalFileOrigin : Origin
	{
        SortedFileList _files;

        public SortedFileList Files
        {
            get { return _files ?? (_files = new SortedFileList()); }
        }
	}
}
