using System;
using newCRM.Tools;
using newCRM;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace 上海CRM管理系统.Tools
{
    /// <summary>
    /// 常量及对象
    /// </summary>
    public class ConstDefault
    {
        #region 传给JS
        /// <summary>
        /// 正在拨号中
        /// </summary>
        public const string phone_dialing = "PHONE_DIALING";
        /// <summary>
        /// 通话中
        /// </summary>
        public const string phone_calling = "PHONE_CALLING";
        /// <summary>
        /// 来电
        /// </summary>
        public const string phone_ringing = "PHONE_RINGING";
        /// <summary>
        /// 挂断
        /// </summary>
        public const string phone_idel = "PHONE_IDEL";
        /// <summary>
        /// 判断线路忙音或者远程挂机信号
        /// </summary>
        public const string line_is_BusyOrHangup = "LINE_IS_BUSYORHANGUP";
        #endregion

        #region js传过来的
        /// <summary>
        /// 接电话
        /// </summary>
        public const string phone_pick_up = "PHONE_PICK_UP";
        /// <summary>
        /// 打电话
        /// </summary>
        public const string phone_make_call = "PHONE_MAKE_CALL";
        /// <summary>
        /// 挂电话
        /// </summary>
        public const string phone_hang_up = "PHONE_HANG_UP";
        /// <summary>
        ///系统消息
        /// </summary>
        public const string notification = "NOTIFICATION";
        /// <summary>
        /// 上一通是否评价
        /// </summary>
        public const string phone_is_evaluate = "PHONE_IS_EVALUATE";
        /// <summary>
        /// 风险提示
        /// </summary>
        public const string phone_riskprompt = "PHONE_RISKPROMPT";
        /// <summary>
        /// 设备是否正常
        /// </summary>
        public const string device_is_normal = "DEVICE_IS_NORMAL";
        #endregion

        /// <summary>
        /// 是否是自己挂断
        /// </summary>
        public static bool isBySelf = true;
        /// <summary>
        /// 是否是未接
        /// </summary>
        public static bool isMissed = true;
        /// <summary>
        /// 是否来电中
        /// </summary>
        public static bool isCalling = false;
        /// <summary>
        /// 服务器返回结构
        /// </summary>
        public class result
        {
            /// <summary>
            /// 1 全部上传完成   200 分片上传完成  400 分片需要重新上传 500 上传失败 501 通话ID不存在
            /// </summary>
            public int code;
            /// <summary>
            /// 消息
            /// </summary>
            public string msg;
        }
        /// <summary>
        /// 返回给JS的消息体
        /// </summary>
        public class resultToJs
        {
            /// <summary>
            /// 行为
            /// </summary>
            public string action;
            /// <summary>
            /// 电话号码
            /// </summary>
            public string phoneNumber;
            /// <summary>
            /// 时间戳
            /// </summary>
            public long time;
            /// <summary>
            /// 是否硬摘
            /// </summary>
            public bool isOffHook;
            /// <summary>
            /// 设备是否正常
            /// </summary>
            public bool deviceIsNormal;
        }

        /// <summary>
        /// 分块上传
        /// </summary>
        public class chunkFile
        {
            /// <summary>
            /// 文件名 含后缀
            /// </summary>
            public string fileName;
            /// <summary>
            /// 通话ID
            /// </summary>
            public string call_id;
            /// <summary>
            /// 文件总大小
            /// </summary>
            public long totalSize;
            /// <summary>
            /// 总分片数
            /// </summary>
            public int totalChunk;
            /// <summary>
            /// 分片大小 4M
            /// </summary>
            public long chunkSize;
            /// <summary>
            /// 当前 分片索引
            /// </summary>
            public int index;
            /// <summary>
            /// 数据
            /// </summary>
            public byte[] data;
            /// <summary>
            /// 是否合并
            /// </summary>
            public int merge;
            /// <summary>
            /// 通话开始时间
            /// </summary>
            public string call_start;
            /// <summary>
            /// 通话结束时间
            /// </summary>
            public string call_end;
        }
    }

    /// <summary>
    /// 常用工具库
    /// </summary>
    public class tools
    {
        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static long GetTimeStamp()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
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
        public static void resultToJavascript(ConstDefault.resultToJs result)
        {
            var msg = new DispacthMsg();
            var payload = new Payload();
            msg.action = result.action;
            if (result.action == ConstDefault.phone_dialing || result.action == ConstDefault.phone_ringing)
            {
                payload.callId = VoipHelper.callId.ToString();
            }
            else if (result.action == ConstDefault.phone_idel)
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
            VoipHelper.WriteLog(string.Format("Client To Js ==>> {0}", resultToJs));
            MainWindow.browser.GetBrowser().MainFrame.EvaluateScriptAsync(string.Format("lyJsBridge.dispacthMsg({0})", resultToJs));
        }

        /// <summary>
        /// C# timeout
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
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display | ExecutionFlag.Continus);
            else
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Continus);
        }
        /// <summary>
        /// 恢复系统电源计划
        /// </summary>
        public static void ResotreSleep()
        {
            SetThreadExecutionState(ExecutionFlag.Continus);
        }

    }
}
