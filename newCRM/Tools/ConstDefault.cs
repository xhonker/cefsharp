using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using CefSharp.Wpf;
using newCRM.Tools;
using System.Configuration;
namespace 上海CRM管理系统.Tools
{
    public class ConstDefault
    {
        public const string upFileUrl = "http://crm-test.lanyife.com.cn";

        //public const string upFileUrl = "http://crmb-fanx.dev.lanyicj.cn";
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
        #endregion

        /// <summary>
        /// 是否是自己挂断
        /// </summary>
        public static bool isBySelf = true;

        /// <summary>
        /// 服务器返回结构
        /// </summary>
        public class result
        {
            public int code = 200;
            public string msg = "成功";
            public string[] data;
        }
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
        }
        /// <summary>
        /// 消息通知
        /// </summary>
        public class newsMessage
        {
            /// <summary>
            /// 标题
            /// </summary>
            public string title;
            /// <summary>
            /// 内容
            /// </summary>
            public string content;
            /// <summary>
            /// 消息ID
            /// </summary>
            public int id;
            /// <summary>
            /// 链接
            /// </summary>
            public string link;
            /// <summary>
            /// 消息时间
            /// </summary>
            public string time;
            /// <summary>
            /// 消息来源
            /// </summary>
            public string from_user;
        }
    }
    /// <summary>
    /// 序列化
    /// </summary>
    public class JsonHelper
    {

        /// <summary>
        /// Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string Jsons<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }
        /// <summary>
        /// 反Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

            T obj = (T)ser.ReadObject(ms);
            return obj;
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
        /// <param name="browser">浏览器实例</param>
        /// <param name="action">行为</param>
        /// <param name="phoneNumber">电话号码</param>
        /// <param name="time">时间</param>
        /// <param name="isOffHook">是否是硬摘 硬挂</param>
        public static void resultToJavascript(ChromiumWebBrowser browser, string action, string phoneNumber, long time, bool isOffHook)
        {
            var msg = new DispacthMsg();
            var paload = new Payload();
            msg.action = action;
            if (action == ConstDefault.phone_dialing || action == ConstDefault.phone_ringing)
            {
                paload.callId = VoipHelper.callId.ToString();
            }
            if (action == ConstDefault.phone_idel)
            {
                paload.isBySelf = ConstDefault.isBySelf;
            }
            msg.payload = paload;
            paload.phoneNumber = phoneNumber;
            if (time > 0)
            {
                paload.time = time;
            }
            else
            {
                paload.time = GetTimeStamp();
            }
            paload.isOffHook = isOffHook;
            System.Diagnostics.Debug.WriteLine("[回传js]==>>" + JsonHelper.Jsons(msg));
            browser.GetBrowser().MainFrame.EvaluateScriptAsync("lyJsBridge.dispacthMsg(" + JsonHelper.Jsons(msg) + ")");

            //return "lyJsBridge.dispacthMsg(" + JsonHelper.Jsons(msg) + ")";
        }
    }
}
