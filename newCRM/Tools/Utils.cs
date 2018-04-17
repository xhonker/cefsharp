using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace newCRM.Tools
{
    /// <summary>
    /// 工具库
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 日志写入锁
        /// </summary>
        public static System.Threading.ReaderWriterLockSlim logWriteLock = new System.Threading.ReaderWriterLockSlim();
        /// <summary>
        /// 所有交互日志
        /// </summary>
        public static string crmLog = VoipHelper.crmRoot + "\\log";
        /// <summary>
        /// 日志文件目录
        /// </summary>
        public static string logFilePath = string.Format("{0}\\{1}\\{2}", crmLog, DateTime.Now.Year, DateTime.Now.Month);
        /// <summary>
        /// 日志文件
        /// </summary>
        public static string logFileName = string.Format("{0}\\{1}", logFilePath, DateTime.Now.ToString("dd") + ".log");
        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static long GetTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long time = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return time;
        }
        /// <summary>
        /// 获取通话ID
        /// </summary>
        /// <returns></returns>
        public static string GetCallId()
        {
            return Guid.NewGuid().ToString("N");
        }
        /// <summary>
        /// 返回给JS
        /// </summary>
        /// <param name="result"></param>
        public static void resultToJavascript(ConstDefault.retToJs result)
        {
            var msg = new DispacthMsg();
            var payload = new Payload();
            msg.action = result.action;
            if (result.action == ConstDefault.PHONE_DIALING || result.action == ConstDefault.PHONE_RINGING)
            {
                payload.callId = VoipHelper.callId.ToString();
            }
            else if (result.action == ConstDefault.PHONE_IDEL)
            {
                payload.isBySelf = ConstDefault.isBySelf;
            }
            msg.payload = payload;

            payload.phoneNumber = result.phoneNumber;

            payload.time = GetTimeStamp();

            payload.isOffHook = result.isOffHook;
            if (result.deviceIsNormal)
            {
                payload.deviceIsNormal = true;
            }
            else
            {
                payload.deviceIsNormal = false;
            }
            string resultToJs = JsonConvert.SerializeObject(msg);
            System.Diagnostics.Debug.WriteLine("[回传js test]==>>" + resultToJs);
            WriteLog(string.Format("Client To Js ==>> {0}", resultToJs));
            MainWindow.browser.GetBrowser().MainFrame.EvaluateScriptAsync(string.Format("lyJsBridge.dispacthMsg({0})", resultToJs));
        }
        /// <summary>
        /// C# setTimeOut
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        public static void SetTimeOut(double interval, Action action)
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
        /// 阻止系统休眠||关闭显示器||强制系统处于工作状态
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);
        [Flags]
        enum ExecutionFlag : uint
        {
            /// <summary>
            /// 强制系统处于工作状态
            /// </summary>
            System = 0x00000001,
            /// <summary>
            /// 强制开启显示器
            /// </summary>
            Display = 0x00000002,
            /// <summary>
            /// 重置状态
            /// </summary>
            Continus = 0x80000000,
        }
        /// <summary>
        /// 阻止系统电源计划
        /// </summary>
        /// <param name="includeDisplay">是否阻止关闭显示器</param>
        public static void PreventSleep(bool includeDisplay = true)
        {
            if (includeDisplay)
            {
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display | ExecutionFlag.Continus);
            }
            else
            {
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Continus);
            }
        }
        /// <summary>
        /// 恢复系统电源计划
        /// </summary>
        public static void ResotreSleep()
        {
            SetThreadExecutionState(ExecutionFlag.Continus);
        }
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path">路径</param>
        public static void createDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                File.SetAttributes(path, FileAttributes.Hidden);
            }
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="strLog"></param>
        public static void WriteLog(string strLog)
        {
            try
            {
                if (crmLog == null)
                {
                    createDirectory(crmLog);
                }

                createDirectory(logFilePath);
                string logContent = string.Format("{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + " ---- " + strLog);
                try
                {
                    logWriteLock.EnterWriteLock();
                    File.AppendAllText(logFileName, logContent);
                }
                finally
                {
                    logWriteLock.ExitWriteLock();
                }
            }
            catch (Exception err)
            {
                WriteLog(string.Format("写入错误 ==>> {0}", err.ToString()));
                return;
            }
        }
    }
}
