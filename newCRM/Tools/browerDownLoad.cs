using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CefSharp;
using CefSharp.Wpf;

namespace 上海CRM管理系统.Tools
{
    public class browerDownLoad : IDownloadHandler
    {
        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            System.Diagnostics.Debug.WriteLine(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(@"c:\Users\" +
                        System.Security.Principal.WindowsIdentity.GetCurrent().Name +
                        @"\download\"
                            + downloadItem.SuggestedFileName, true);
                }
            }

        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {

        }
        public bool OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem)
        {
            return false;
        }
    }
}
