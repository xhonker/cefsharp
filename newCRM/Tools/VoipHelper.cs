using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using newCRM.Tools;
using System.IO;
namespace newCRM.Tools
{
    public class VoipHelper
    {
        /// <summary>
        /// 设备状态
        /// </summary>
        public static bool deviceState = true;
        /// <summary>
        /// 来电号码
        /// </summary>
        public static string callNumber;
        /// <summary>
        /// 电话拨打类型
        /// </summary>
        public enum telState
        {
            OUT, IN
        }
        /// <summary>
        /// 电话拨打状态
        /// </summary>
        public static telState callState;
        /// <summary>
        /// 是否是摘机拨打
        /// </summary>
        public static bool isOffHookCall = false;
        /// <summary>
        /// 摘机拨打号码
        /// </summary>
        public static string offHookCallNumber;
        /// <summary>
        /// 播放语音的句柄
        /// </summary>
        public static int playHandle;
        /// <summary>
        /// 来电铃声
        /// </summary>
        public static string callBell = AppDomain.CurrentDomain.BaseDirectory + "//1670.wav";
        /// <summary>
        /// 播放风险提示
        /// </summary>
        public static string PLAYFILEPATH = AppDomain.CurrentDomain.BaseDirectory + "remind.wav";
        /// <summary>
        /// 录音句柄
        /// </summary>
        public static int recordingHanle;
        /// <summary>
        /// 录音文件路径
        /// </summary>
        public static string recordPath;
        /// <summary>
        /// 通话ID
        /// </summary>
        public static string callId;
        /// <summary>
        /// 初始化通道
        /// </summary>
        public static short VoipIndex = 0;
        /// <summary>
        /// 初始化盒子 设置相关配置
        /// </summary>
        public static void init()
        {
            BriSDKLib.QNV_SetParam(VoipIndex, BriSDKLib.QNV_PARAM_BUSY, 10);//修复 意外远程挂机
            lineToSpk(0);//初始化线路声音到耳机 关闭  因为来电有干扰声音 很刺耳
            BriSDKLib.QNV_SetDevCtrl(VoipIndex, BriSDKLib.QNV_CTRL_DOMICTOLINE, 1);//打开麦克风到电话线
            BriSDKLib.QNV_SetDevCtrl(VoipIndex, BriSDKLib.QNV_CTRL_LINEOUT, 1);//打开线路输出功能
            BriSDKLib.QNV_SetDevCtrl(VoipIndex, BriSDKLib.QNV_CTRL_DOPLAYTOSPK, 1);//打开播放的语音到耳机
            BriSDKLib.QNV_SetDevCtrl(VoipIndex, BriSDKLib.QNV_CTRL_PLAYTOLINE, 1);//打开播放的语音到线路

            BriSDKLib.QNV_SetParam(VoipIndex, BriSDKLib.QNV_PARAM_AM_MIC, 0);//获取插在设备上的麦克风增益大小//
            BriSDKLib.QNV_SetParam(VoipIndex, BriSDKLib.QNV_PARAM_AM_SPKOUT, 10);//设置插在设备上的耳机音量等级大小
            BriSDKLib.QNV_SetParam(VoipIndex, BriSDKLib.QNV_PARAM_AM_LINEOUT, 15);//设置播放语音到线路的音量等级大小//
            BriSDKLib.QNV_SetParam(VoipIndex, BriSDKLib.QNV_PARAM_AM_LINEIN, 7);//电话线路信号强

            BriSDKLib.QNV_SetParam(VoipIndex, BriSDKLib.QNV_PARAM_AM_DOPLAY, 15);
            recordPath = AppDomain.CurrentDomain.BaseDirectory + "record";
            if (Directory.Exists(recordPath))
            {
                Directory.CreateDirectory(recordPath);
            }
        }
        /// <summary>
        /// 窗口关闭
        /// </summary>
        public static void windowClose()
        {
            lineToSpk(0);
            OffOnHook(0);
            EndRecord();
            CloseDevice();
        }
        /// <summary>
        /// 打开1/关闭0 麦克风到电话线
        /// </summary>
        /// <param name="domic">1打开/0关闭</param>
        public static void domicToLine(int domic)
        {
            BriSDKLib.QNV_SetDevCtrl(VoipIndex, BriSDKLib.QNV_CTRL_DOMICTOLINE, domic);
        }
        /// <summary>
        /// 打开线路声音到耳机  1打开 0 关闭
        /// </summary>
        public static void lineToSpk(int spk)
        {
            BriSDKLib.QNV_SetDevCtrl(VoipIndex, BriSDKLib.QNV_CTRL_DOLINETOSPK, spk);
        }
        /// <summary>
        /// 打开设备
        /// </summary>
        public static int OpenDevice()
        {
            return BriSDKLib.QNV_OpenDevice(BriSDKLib.ODT_LBRIDGE, 0, "0");
        }
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns></returns>
        public static int CloseDevice()
        {
            return BriSDKLib.QNV_CloseDevice(BriSDKLib.ODT_ALL, 0);
        }
        /// <summary>
        /// 拨打电话
        /// </summary>
        /// <param name="telephone">手机号码</param>
        /// <returns></returns>
        public static int Call(string telephone)
        {
            return BriSDKLib.QNV_General(0, BriSDKLib.QNV_GENERAL_STARTDIAL, BriSDKLib.DIALTYPE_DTMF, telephone);
        }
        /// <summary>
        /// 取得来电号码
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        public static string FromASCIIByteArray(byte[] characters)
        {

            ASCIIEncoding encoding = new ASCIIEncoding();
            var constructedString = encoding.GetString(characters);
            var phoneNumber = constructedString.Substring(0, constructedString.IndexOf("\0"));
            return phoneNumber;
        }
        /// <summary>
        ///  软摘/挂机 1 摘机  0 挂机
        /// </summary>
        /// <param name="hook"></param>
        /// <returns></returns>
        public static void OffOnHook(int hook)
        {
            if (hook == 1)
            {
                StartRecord();
            }
            if (hook == 0)
            {
                EndRecord();
            }
            BriSDKLib.QNV_SetDevCtrl((short)VoipIndex, BriSDKLib.QNV_CTRL_DOHOOK, hook);
        }
        /// <summary>
        /// 播放语音文件  
        /// </summary>
        /// <param name="path">语音文件路径</param>
        /// <returns>播放语音的句柄</returns>
        public static int PlayVoice(string path)
        {
            return BriSDKLib.QNV_PlayFile(0, BriSDKLib.QNV_PLAY_FILE_START, 0, BriSDKLib.PLAYFILE_MASK_REPEAT, path);
        }
        /// <summary>
        /// 停止播放语音文件
        /// </summary>
        /// <param name="handle">语音句柄</param>
        /// <returns>大于0表示成功，其它表示失败</returns>
        public static int StopVoice(int handle)
        {
            return BriSDKLib.QNV_PlayFile(0, BriSDKLib.QNV_PLAY_FILE_STOP, handle, 0, "");
        }
        /// <summary>
        /// 停止录音
        /// </summary>
        /// <param name="handle"></param>
        public static void EndRecord()
        {
            BriSDKLib.QNV_RecordFile(VoipIndex, BriSDKLib.QNV_RECORD_FILE_STOP, recordingHanle, 0, "");
            if (MainWindow.uploadRecordingFile != null)
            {
                if (!MainWindow.uploadRecordingFile.IsBusy)
                {
                    MainWindow.uploadRecordingFile.RunWorkerAsync();
                }
            }

        }
        /// <summary>
        /// 开始录音
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>返回录音句柄</returns>
        public static int StartRecord()
        {
            if (!string.IsNullOrEmpty(callId))
            {
                var path = string.Format(recordPath + "\\{0}.wav", callId.ToString().Trim());
                recordingHanle = BriSDKLib.QNV_RecordFile(VoipIndex, BriSDKLib.QNV_RECORD_FILE_START, BriSDKLib.BRI_WAV_FORMAT_PCM8K16B, BriSDKLib.RECORD_MASK_ECHO | BriSDKLib.RECORD_MASK_AGC, path);

                if (recordingHanle < 0)//录音失败
                {
                    BriSDKLib.QNV_RecordFile(VoipIndex, BriSDKLib.QNV_RECORD_FILE_STOPALL, 0, 0, "0");
                    lineToSpk(0);
                    OffOnHook(0);
                    return -1;
                }
            }

            return recordingHanle;
        }
        /// <summary>
        /// 拒绝当前呼入来电 大于零=成功
        /// </summary>
        /// <returns></returns>
        public static int refuseCurrentIncoming()
        {
            return BriSDKLib.QNV_General(0, BriSDKLib.QNV_GENERAL_STARTREFUSE, BriSDKLib.REFUSE_SYN, "");
        }
        /// <summary>
        /// 停止拒接
        /// </summary>
        /// <returns></returns>
        public static int stopCusttentIncoming()
        {
            return BriSDKLib.QNV_General(0, BriSDKLib.QNV_GENERAL_STOPREFUSE, 0, "");
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="strLog"></param>
        public static void WriteLog(string strLog)
        {
            FileStream fs;
            StreamWriter sw;
            string FilePath = AppDomain.CurrentDomain.BaseDirectory + "log\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month;
            string FileName = DateTime.Now.ToString("dd") + ".log";
            FileName = FilePath + "\\" + FileName;
            if (!System.IO.Directory.Exists(FilePath))
            {
                System.IO.Directory.CreateDirectory(FilePath);
            }
            if (File.Exists(FileName))
            {
                fs = new FileStream(FileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ---- " + strLog);
            sw.Close();
            fs.Close();
        }
    }
}
