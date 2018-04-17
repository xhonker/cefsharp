namespace newCRM.Tools
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
        public const string PHONE_DIALING = "PHONE_DIALING";
        /// <summary>
        /// 通话中
        /// </summary>
        public const string PHONE_CALLING = "PHONE_CALLING";
        /// <summary>
        /// 来电
        /// </summary>
        public const string PHONE_RINGING = "PHONE_RINGING";
        /// <summary>
        /// 挂断
        /// </summary>
        public const string PHONE_IDEL = "PHONE_IDEL";
        /// <summary>
        /// 判断线路忙音或者远程挂机信号
        /// </summary>
        public const string LINE_IS_BUSYORHANGUP = "LINE_IS_BUSYORHANGUP";
        #endregion

        #region js传过来的
        /// <summary>
        /// 接电话
        /// </summary>
        public const string PHONE_PICK_UP = "PHONE_PICK_UP";
        /// <summary>
        /// 打电话
        /// </summary>
        public const string PHONE_MAKE_CALL = "PHONE_MAKE_CALL";
        /// <summary>
        /// 挂电话
        /// </summary>
        public const string PHONE_HANG_UP = "PHONE_HANG_UP";
        /// <summary>
        ///系统消息
        /// </summary>
        public const string NOTIFICATION = "NOTIFICATION";
        /// <summary>
        /// 上一通是否评价
        /// </summary>
        public const string PHONE_IS_EVALUATE = "PHONE_IS_EVALUATE";
        /// <summary>
        /// 风险提示
        /// </summary>
        public const string PHONE_RISKPROMPT = "PHONE_RISKPROMPT";
        /// <summary>
        /// 设备是否正常
        /// </summary>
        public const string DEVICE_IS_NORMAL = "DEVICE_IS_NORMAL";
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
        public class resultFromServer
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
        public class retToJs
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
}
