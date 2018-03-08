using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 上海CRM管理系统.Tools
{
    /// <summary>
    /// 返回给前端的json
    /// </summary>
    public class DispacthMsg
    {
        /// <summary>
        /// 方法名
        /// </summary>
        public string action;
        /// <summary>
        /// 消息体
        /// </summary>
        public Payload payload;
    }
    /// <summary>
    /// 消息体
    /// </summary>
    public class Payload
    {
        /// <summary>
        /// 通话ID
        /// </summary>
        public string callId;
        /// <summary>
        /// 电话号码
        /// </summary>
        public string phoneNumber;
        /// <summary>
        /// 通话时间戳
        /// </summary>
        public long time;
        /// <summary>
        /// 系统消息内容
        /// </summary>
        public string content;
        /// <summary>
        /// 是否是自己挂断的
        /// </summary>
        public bool isBySelf;
        /// <summary>
        /// 是否硬摘 硬挂
        /// </summary>
        public bool isOffHook;
        /// <summary>
        /// 是否评价中
        /// </summary>
        public bool isEvaluate;
    }
}
