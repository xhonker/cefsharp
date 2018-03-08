using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp.Wpf;
using newCRM.Tools;
using System.IO;
using System.Diagnostics;
using 上海CRM管理系统.Tools;
using System.Threading;
using System.ComponentModel;
using CefSharp;
using System.Configuration;

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
        static ChromiumWebBrowser browser;
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
            setting.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            CefSharp.Cef.Initialize(setting);
            #endregion
            browser = new ChromiumWebBrowser();
            //browser.Address = System.AppDomain.CurrentDomain.BaseDirectory + "index.html";
            browser.PreviewTextInput += Browser_PreviewTextInput; // 解决input框无法输入中文的BUG
            browser.Address = serverAddress;
            browser.LifeSpanHandler = new prohibitNewPageJump();
            //browser.Address = "http://ie.icoa.cn";
            //注册JS调用函数
            browser.RegisterJsObject("__phone", new CallBackForJs());

            browser.MenuHandler = new MenuHandler();

            browser.DownloadHandler = new browerDownLoad();

            this.main.Children.Add(browser);
            //屏蔽右键菜单
            // 调用JS方法
            // browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("test()");
            #endregion

            #region 盒子初始化
            if (VoipHelper.OpenDevice() == -1)
            {
                //MessageBox.Show("打开设备失败");
                browser.Address = System.AppDomain.CurrentDomain.BaseDirectory + "index.html";
                return;
            }
            //CallBackForJs.systemNewMessage("的风格");
            //SetTimeOut(1000 * 5, new Action(() =>
            //{
            //    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            //   (ThreadStart)delegate ()
            //   {
            //       CallBackForJs.systemNewMessage("sdfsffsdfsd ");
            //   }
            //   );
            //}));
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
        }
        /// <summary>
        /// 上传目录下的录音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uploadRecordingFile_DoWork(object sender, DoWorkEventArgs e)
        {
            var fileList = Directory.GetFiles(VoipHelper.recordPath);
            DirectoryInfo dir = new DirectoryInfo(VoipHelper.recordPath);
            FileInfo[] fil = dir.GetFiles();
            if (fileList.Length > 0)
            {
                foreach (FileInfo item in fil)
                {
                    Debug.WriteLine(item.FullName);
                    Debug.WriteLine(System.IO.Path.GetFileNameWithoutExtension(item.FullName));
                    //httpHellper.PostRequest(item.FullName);
                    var json = httpHellper.PostRequest(item.FullName);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var result = JsonHelper.JsonDeserialize<ConstDefault.result>(json);
                        if (result.code == 1)
                        {
                            File.Delete(item.FullName);
                        }
                        else
                        {
                            Debug.WriteLine(result.msg);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("[uploadRecordingFile_DoWork]==>>上传失败");
                    }

                }
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

        private void HandleWindowProc(System.IntPtr lParam)
        {
            BriSDKLib.TBriEvent_Data EventData = (BriSDKLib.TBriEvent_Data)Marshal.PtrToStructure(lParam, typeof(BriSDKLib.TBriEvent_Data));

            System.Diagnostics.Debug.WriteLine("event==>>" + EventData.lEventType);

            switch (EventData.lEventType)
            {
                case BriSDKLib.BriEvent_PhoneHook://电话机摘机  
                    if (VoipHelper.callState == VoipHelper.telState.IN)
                    {
                        VoipHelper.StopVoice(VoipHelper.playHandle);
                        VoipHelper.OffOnHook(1);
                        tools.resultToJavascript(browser, ConstDefault.phone_calling, null, -1, true);
                    }
                    if (VoipHelper.callState == VoipHelper.telState.OUT)
                    {
                        //VoipHelper.OffOnHook(1);
                    }

                    break;
                case BriSDKLib.BriEvent_PhoneHang://电话机挂机 
                    if (VoipHelper.isOffHookCall)
                    {
                        VoipHelper.isOffHookCall = false;
                        VoipHelper.offHookCallNumber = string.Empty;
                    }
                    VoipHelper.OffOnHook(0);
                    tools.resultToJavascript(browser, ConstDefault.phone_idel, null, -1, true);
                    break;
                case BriSDKLib.BriEvent_GetCallID://接收到来电号码 
                    VoipHelper.callState = VoipHelper.telState.IN;
                    this.Activate();
                    this.Topmost = true;
                    VoipHelper.playHandle = VoipHelper.PlayVoice(VoipHelper.callBell);
                    var phone = VoipHelper.FromASCIIByteArray(EventData.szData);
                    if (phone.Length < 7)
                    {
                        VoipHelper.OffOnHook(0);
                        VoipHelper.lineToSpk(0);
                        return;
                    }
                    VoipHelper.callId = tools.GetCallId();
                    tools.resultToJavascript(browser, ConstDefault.phone_ringing, phone, -1, false);
                    break;
                case BriSDKLib.BriEvent_StopCallIn://停止呼入，产生一个未接电话   
                    VoipHelper.StopVoice(VoipHelper.playHandle);
                    Debug.WriteLine("未接时间==>>" + BriSDKLib.QNV_CallLog(0, BriSDKLib.QNV_CALLLOG_ENDTIME, "", 0) * 1000);
                    long MissedCallTime = (long)BriSDKLib.QNV_CallLog(0, BriSDKLib.QNV_CALLLOG_ENDTIME, "", 0) * 1000;
                    ConstDefault.isBySelf = false;
                    tools.resultToJavascript(browser, ConstDefault.phone_idel, null, MissedCallTime, false);
                    this.Topmost = false;
                    VoipHelper.OffOnHook(0);

                    break;
                case BriSDKLib.BriEvent_Busy://忙音
                    tools.resultToJavascript(browser, ConstDefault.phone_idel, null, -1, false);
                    break;
                case BriSDKLib.BriEvent_RemoteHook: //对方接听
                    tools.resultToJavascript(browser, ConstDefault.phone_calling, null, -1, false);
                    break;
                case BriSDKLib.BriEvent_RemoteHang://远程挂机   
                    //传给前端一个信号 
                    ConstDefault.isBySelf = false;
                    tools.resultToJavascript(browser, ConstDefault.phone_idel, null, -1, false);
                    VoipHelper.OffOnHook(0);
                    VoipHelper.lineToSpk(0);
                    break;
                case BriSDKLib.BriEvent_PhoneDial://摘机手动拨号
                    Debug.WriteLine("[摘机手动拨号==>>]" + VoipHelper.FromASCIIByteArray(EventData.szData));
                    VoipHelper.offHookCallNumber = VoipHelper.FromASCIIByteArray(EventData.szData);
                    break;
                case BriSDKLib.BriEvent_RingBack://电话机检查拨号结束

                    Debug.WriteLine("[摘机拨号状态]==>>" + EventData.lResult);
                    if (EventData.lResult == 0 && VoipHelper.offHookCallNumber != null)
                    {
                        VoipHelper.callId = tools.GetCallId();
                        tools.resultToJavascript(browser, ConstDefault.phone_dialing, VoipHelper.offHookCallNumber, -1, true);
                        VoipHelper.isOffHookCall = true;
                        //传给前端手动摘机拨打的号码。
                        Debug.WriteLine("电话机检查拨号结束" + VoipHelper.offHookCallNumber);
                    }
                    break;
                case BriSDKLib.BriEvent_EnableHook: // 软摘/挂机 1是摘机  0是挂机
                    try
                    {
                        if (EventData.lResult == 1)//摘机
                        {
                            Debug.WriteLine("[软摘机]");
                            if (VoipHelper.callState == VoipHelper.telState.IN)
                            {
                                Debug.WriteLine("[摘机时间]" + tools.GetTimeStamp());
                                tools.resultToJavascript(browser, ConstDefault.phone_calling, null, -1, false);
                                VoipHelper.StopVoice(VoipHelper.playHandle);
                            }
                            else
                            {
                                tools.resultToJavascript(browser, ConstDefault.phone_dialing, VoipHelper.callNumber, -1, false);

                            }
                        }
                        else //挂机
                        {
                            Debug.WriteLine("[软挂机]");
                            ConstDefault.isBySelf = true;
                            tools.resultToJavascript(browser, ConstDefault.phone_idel, null, -1, false);
                            if (VoipHelper.callState == VoipHelper.telState.IN)
                            {
                                this.Topmost = false;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine(err);
                    }
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

        private void close_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

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

        private void popup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            popup.IsOpen = false;
            this.Activate();
        }
    }
}
