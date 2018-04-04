using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CefSharp.Wpf;
using newCRM.Tools;
using System.IO;
using System.Diagnostics;
using 上海CRM管理系统.Tools;
using System.Threading;
using System.ComponentModel;
using CefSharp;
using System.Configuration;
using Newtonsoft.Json;

namespace newCRM
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 当前窗口是否激活
        /// </summary>
        public static bool isActivation;
        /// <summary>
        /// 上传通话录音
        /// </summary>
        public static BackgroundWorker uploadRecordingFile;
        /// <summary>
        /// voip事件消息处理
        /// </summary>
        private IntPtr hwnd;
        /// <summary>
        /// 浏览器
        /// </summary>
        public static ChromiumWebBrowser browser;
        /// <summary>
        /// 处理CRM系统消息 引用this
        /// </summary>
        public static MainWindow form;

        public MainWindow()
        {
            InitializeComponent();
            form = this;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.SourceInitialized += MainWindow_SourceInitialized;//注册盒子监听事件
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string serverAddress = ConfigurationManager.AppSettings["server"];
            #region browser
            #region 初始化环境 禁用gpu 防止闪烁
            var setting = new CefSharp.CefSettings();
            setting.CefCommandLineArgs.Add("disable-gpu", "1");
            setting.Locale = "zh-CN";
            setting.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36 LY_CRM_BOX";
            CefSharp.Cef.Initialize(setting);
            #endregion
            browser = new ChromiumWebBrowser();
            //browser.Address = System.AppDomain.CurrentDomain.BaseDirectory + "index.html";
            // 解决input框无法输入中文的BUG
            browser.PreviewTextInput += Browser_PreviewTextInput;

            browser.Address = serverAddress;

            browser.LifeSpanHandler = new prohibitNewPageJump();
            //browser.Address = "http://ie.icoa.cn";
            //注册JS调用函数
            browser.RegisterJsObject("__phone", new CallBackForJs());
            // 菜单处理事件
            browser.MenuHandler = new MenuHandler();
            // 下载处理事件
            browser.DownloadHandler = new browerDownLoad();

            this.main.Children.Add(browser);
            // 浏览器按键事件
            browser.KeyDown += Browser_KeyDown;

            #endregion

            #region 盒子初始化
            if (VoipHelper.OpenDevice() == 0)
            {
                VoipHelper.deviceState = false;
                MessageBox.Show("电话硬件设备连接存在问题，请关闭软件检查硬件后重试。");
            }

            hwnd = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;//盒子事件注册
            for (Int16 i = 0; i < BriSDKLib.QNV_DevInfo(-1, BriSDKLib.QNV_DEVINFO_GETCHANNELS); i++)
            {
                //在windowproc处理接收到的消息
                BriSDKLib.QNV_Event(i, BriSDKLib.QNV_EVENT_REGWND, (int)hwnd, "", new StringBuilder(0), 0);
            }
            VoipHelper.init();
            #endregion

            #region 上传录音
            uploadRecordingFile = new BackgroundWorker();
            uploadRecordingFile.DoWork += uploadRecordingFile_DoWork;
            uploadRecordingFile.WorkerSupportsCancellation = true;
            uploadRecordingFile.RunWorkerAsync();
            #endregion


        }



        /// <summary>
        /// 解决input框无法输入中文的BUG
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var character in e.Text)
            {
                browser.GetBrowser().GetHost().SendKeyEvent((int)WM.CHAR, (int)character, 0);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 禁止页面跳转
        /// </summary>
        public class prohibitNewPageJump : CefSharp.ILifeSpanHandler
        {
            public bool DoClose(IWebBrowser browserControl, IBrowser browser)
            {
                return true;
            }

            public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
            {
                //throw new NotImplementedException();
            }

            public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
            {
                //throw new NotImplementedException();
            }

            public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                newBrowser = null;
                var chromWebBrowser = (ChromiumWebBrowser)browserControl;
                chromWebBrowser.Load(targetUrl);

                return true;

            }
        }
        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            VoipHelper.windowClose();
            BriSDKLib.QNV_Event(0, BriSDKLib.QNV_EVENT_UNREGWND, (int)hwnd, "", new StringBuilder(0), 0);
        }
        /// <summary>
        /// 上传目录下的录音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uploadRecordingFile_DoWork(object sender, DoWorkEventArgs e)
        {
            //updateOldFileRecordDirectory();
            updateNewFileRecordDirectory();
            #region 遍历工作目录、录音文件目录
            //var fileList = Directory.GetFiles(VoipHelper.recordPath);

            //DirectoryInfo dir = new DirectoryInfo(VoipHelper.recordPath);

            //FileInfo[] fil = dir.GetFiles();
            //if (fileList.Length > 0)
            //{
            //    foreach (FileInfo item in fil)
            //    {
            //        Debug.WriteLine(item.FullName);
            //        Debug.WriteLine(System.IO.Path.GetFileNameWithoutExtension(item.FullName));
            //        //httpHellper.PostRequest(item.FullName);
            //        var json = httpHellper.PostRequest(item.FullName);
            //        if (!string.IsNullOrEmpty(json))
            //        {
            //            var result = JsonHelper.JsonDeserialize<ConstDefault.result>(json);
            //            if (result.code == 1)
            //            {
            //                File.Delete(item.FullName);
            //            }
            //            else
            //            {
            //                Debug.WriteLine(result.msg);
            //            }
            //        }
            //        else
            //        {
            //            Debug.WriteLine("[uploadRecordingFile_DoWork]==>>上传失败");
            //        }

            //    }
            //}
            #endregion

        }
        /// <summary>
        /// 遍历新目录
        /// </summary>
        private static void updateNewFileRecordDirectory()
        {
            VoipHelper.WriteLog(string.Format("开始上传新目录"));

            var fileLists = Directory.GetFiles(VoipHelper.recordPath);
            DirectoryInfo dir = new DirectoryInfo(VoipHelper.recordPath);
            FileInfo[] fils = dir.GetFiles();

            if (fileLists.Length > 0)
            {
                foreach (FileInfo item in fils)
                {
                    updateFile(item.FullName);

                }
            }
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileName">文件路径</param>
        public static void updateFile(string fileName)
        {
            var json = httpHellper.PostRequest(fileName);
            var call_id = Path.GetFileNameWithoutExtension(fileName);

            if (!string.IsNullOrEmpty(json))
            {
                var result = JsonConvert.DeserializeObject<ConstDefault.result>(json);
                if (result.code == 1)
                {
                    VoipHelper.WriteLog(string.Format("上传成功 call_id ==>> {0}", call_id));
                    File.Delete(fileName);
                }
                else if (result.code == 501)
                {
                    VoipHelper.WriteLog(string.Format("通话ID不存在==>> {0}", call_id));
                    File.Delete(fileName);
                }
                else
                {
                    VoipHelper.WriteLog(string.Format("上传失败==>> {0} call_id ==>> {1}", result.msg, call_id));
                    return;
                }
            }
            else
            {
                VoipHelper.WriteLog(string.Format("上传失败 call_id ==>> {0}", call_id));
                return;
            }
        }
        /// <summary>
        /// 遍历旧目录
        /// </summary>
        public static void updateOldFileRecordDirectory()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(".."));
            VoipHelper.WriteLog(string.Format("开始上传旧目录"));
            try
            {
                foreach (DirectoryInfo info in dir.GetDirectories())
                {
                    foreach (DirectoryInfo item in info.GetDirectories())
                    {
                        var isRecord = item.ToString().IndexOf("record");
                        if (isRecord == 0)
                        {
                            foreach (FileInfo fil in item.GetFiles())
                            {
                                updateFile(fil.FullName);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                VoipHelper.WriteLog(string.Format("上传出错==>> {0}", err));
                return;
            }
        }

        #region 盒子事件监听
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source != null)
            {
                source.AddHook(new System.Windows.Interop.HwndSourceHook(WindowProc));
            }
        }
        protected virtual System.IntPtr WindowProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case BriSDKLib.BRI_EVENT_MESSAGE:
                    HandleWindowProc(lParam);
                    handled = true;
                    break;
                default:
                    break;
            }
            return (System.IntPtr)0;
        }
        #endregion 

        /// <summary>
        /// 盒子事件
        /// </summary>
        /// <param name="lParam"></param>
        private void HandleWindowProc(System.IntPtr lParam)
        {
            BriSDKLib.TBriEvent_Data EventData = (BriSDKLib.TBriEvent_Data)Marshal.PtrToStructure(lParam, typeof(BriSDKLib.TBriEvent_Data));
            Debug.WriteLine("event==>>" + EventData.lEventType);

            switch (EventData.lEventType)
            {
                case BriSDKLib.BriEvent_PhoneHook://电话机摘机  
                    boxHandler.phoneHook();
                    break;
                case BriSDKLib.BriEvent_PhoneHang://电话机挂机
                    boxHandler.phoneHang();
                    break;
                case BriSDKLib.BriEvent_GetCallID://接收到来电号码 
                    boxHandler.getCallID(VoipHelper.FromASCIIByteArray(EventData.szData));
                    break;
                case BriSDKLib.BriEvent_Busy://忙音
                    boxHandler.busy();
                    break;
                case BriSDKLib.BriEvent_RemoteHook: //对方接听 
                    boxHandler.remoteHook();
                    break;
                case BriSDKLib.BriEvent_RemoteHang://远程挂机 
                    boxHandler.remoteHang();
                    break;
                case BriSDKLib.BriEvent_PhoneDial://摘机手动拨号
                    boxHandler.phoneDial(VoipHelper.FromASCIIByteArray(EventData.szData));
                    break;
                case BriSDKLib.BriEvent_RingBack://电话机检查拨号结束
                    boxHandler.ringBack(EventData.lResult);
                    break;
                case BriSDKLib.BriEvent_EnableHook: // 软摘/挂机 1是摘机  0是挂机
                    boxHandler.enableHook(EventData.lResult);
                    break;
                default:
                    break;
            }
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            return;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            return;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            isActivation = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            isActivation = false;
        }
        /// <summary>
        /// CRM系统消息
        /// </summary>
        /// <param name="content"></param>
        public void newMessage(string content)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
             (ThreadStart)delegate ()
             {
                 popup.HorizontalOffset = SystemParameters.WorkArea.Width;
                 popup.VerticalOffset = SystemParameters.WorkArea.Height;

                 msg_content.Text = content;
                 popup.IsOpen = true;
             }
             );
            SetTimeOut(1000 * 60 * 5, new Action(() =>
              {
                  this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 (ThreadStart)delegate ()
                 {
                     popup.IsOpen = false;
                 }
                 );
              }));
        }
        /// <summary>
        /// CRM消息提示框关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void close_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

        /// <summary>
        /// C# timeout
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        void SetTimeOut(double interval, Action action)
        {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                timer.Enabled = false;
                action();
            };
            timer.Enabled = true;
        }
        /// <summary>
        /// CRM消息提示框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            popup.IsOpen = false;
            this.Activate();
        }
        /// <summary>
        /// 浏览器按键事件 调用开发者工具/打开日志目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.I))
            {
                browser.GetBrowser().ShowDevTools();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.O))
            {
                Process.Start("explorer.exe", VoipHelper.crmRoot);
            }
        }
    }
}
