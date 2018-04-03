using System;
using System.Threading;
using newCRM.Tools;
using newCRM;
namespace 上海CRM管理系统.Tools
{
    /// <summary>
    /// 录音盒 消息处理
    /// </summary>
    public class boxHandler
    {
        /// <summary>
        /// 电话机摘机处理
        /// </summary>
        public static void phoneHook()
        {
            if (VoipHelper.callState == VoipHelper.telState.IN)
            {
                VoipHelper.WriteLog(string.Format("电话机摘机"));
                VoipHelper.StopVoice(VoipHelper.playHandle);
                ConstDefault.isMissed = false;
                VoipHelper.OffOnHook(1);
            }
        }

        /// <summary>
        /// 电话机挂机处理
        /// </summary>
        public static void phoneHang()
        {
            VoipHelper.WriteLog(string.Format("电话机挂机"));
            if (VoipHelper.isOffHookCall)
            {
                VoipHelper.isOffHookCall = false;
                VoipHelper.offHookCallNumber = null;
            }
            if (VoipHelper.callState == VoipHelper.telState.IN)
            {
                MainWindow.form.Topmost = false;
            }
            VoipHelper.OffOnHook(0);
        }

        /// <summary>
        /// 接收到来电号码
        /// </summary>
        /// <param name="phone"> 手机号码</param>
        public static void getCallID(string phone)
        {
            VoipHelper.WriteLog(string.Format("获取到来电号码 ==>> {0}", phone));
            #region 号码小于7位屏蔽
            if (phone.Length < 7)
            {
                VoipHelper.WriteLog(string.Format("来电小于7位号码  号码为 ==>> {0}", phone));
                VoipHelper.StopVoice(VoipHelper.playHandle);
                VoipHelper.OffOnHook(0);
                return;
            }
            #endregion
            if (ConstDefault.isCalling)
            {
                return;
            }
            ConstDefault.isCalling = true;
            ConstDefault.isMissed = true;
            VoipHelper.callState = VoipHelper.telState.IN;
            MainWindow.form.Activate();
            MainWindow.form.Topmost = true;
            VoipHelper.playHandle = VoipHelper.PlayVoice(VoipHelper.callBell);

            VoipHelper.callId = tools.GetCallId();

            ConstDefault.resultToJs getCallId = new ConstDefault.resultToJs();
            getCallId.action = ConstDefault.phone_ringing;
            getCallId.phoneNumber = phone;
            tools.resultToJavascript(getCallId);

            tools.SetTimeOut(1000 * 30, new Action(() =>
             {
                 MainWindow.form.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    if (ConstDefault.isMissed) // 如果是未接，30秒后 挂断。 因为 停止呼入时间有问题。 
                    {
                        VoipHelper.WriteLog(string.Format("来电30秒未处理"));
                        VoipHelper.StopVoice(VoipHelper.playHandle);
                        VoipHelper.OffOnHook(0);
                    }
                }
                );
             }));
        }

        /// <summary>
        /// 忙音
        /// </summary>
        public static void busy()
        {
            VoipHelper.WriteLog(string.Format("忙音"));
            ConstDefault.resultToJs busy = new ConstDefault.resultToJs();
            busy.action = ConstDefault.line_is_BusyOrHangup;
            tools.resultToJavascript(busy);
        }

        /// <summary>
        /// 对方接听
        /// </summary>
        public static void remoteHook()
        {
            VoipHelper.WriteLog(string.Format("对方接听"));
            ConstDefault.resultToJs remoteHook = new ConstDefault.resultToJs();
            remoteHook.action = ConstDefault.phone_calling;
            tools.resultToJavascript(remoteHook);
        }

        /// <summary>
        /// 远程挂机
        /// </summary>
        public static void remoteHang()
        {
            VoipHelper.WriteLog(string.Format("远程挂机"));
            VoipHelper.playHandle = VoipHelper.StopVoice(VoipHelper.playHandle);
            ConstDefault.isBySelf = false;
            ConstDefault.resultToJs remoteHang = new Tools.ConstDefault.resultToJs();
            remoteHang.action = ConstDefault.line_is_BusyOrHangup;
            tools.resultToJavascript(remoteHang);
        }

        /// <summary>
        /// 摘机手动拨号
        /// </summary>
        /// <param name="phone">手机号码</param>
        public static void phoneDial(string phone)
        {
            VoipHelper.WriteLog(string.Format("摘机手动拨号"));
            VoipHelper.offHookCallNumber = phone;
        }

        /// <summary>
        /// 电话机检测拨号结束
        /// </summary>
        /// <param name="result"> 状态 是否回铃</param>
        public static void ringBack(int result)
        {
            if (result == 0 && VoipHelper.offHookCallNumber != null)
            {
                VoipHelper.WriteLog(string.Format("电话摘机检查拨号结束==>> {0}", VoipHelper.offHookCallNumber));
                VoipHelper.callId = tools.GetCallId();
                ConstDefault.resultToJs ringBack = new ConstDefault.resultToJs();
                ringBack.action = ConstDefault.phone_dialing;
                ringBack.phoneNumber = VoipHelper.offHookCallNumber;
                ringBack.isOffHook = true;
                VoipHelper.isOffHookCall = true;
                tools.resultToJavascript(ringBack);
            }
        }

        /// <summary>
        /// 软摘机/挂机
        /// </summary>
        /// <param name="result">1摘机 0挂机</param>
        public static void enableHook(int result)
        {
            try
            {

                if (result == 1)//摘机
                {
                    VoipHelper.WriteLog(string.Format("软摘机"));
                    VoipHelper.StopVoice(VoipHelper.playHandle);
                    VoipHelper.lineToSpk(1);
                    VoipHelper.domicToLine(1);
                    if (VoipHelper.callState == VoipHelper.telState.IN)
                    {
                        ConstDefault.isMissed = false;

                        ConstDefault.resultToJs resultToJs = new ConstDefault.resultToJs();
                        resultToJs.action = ConstDefault.phone_calling;
                        tools.resultToJavascript(resultToJs);
                    }
                    else
                    {
                        ConstDefault.resultToJs resultToJs = new ConstDefault.resultToJs();
                        resultToJs.action = ConstDefault.phone_dialing;
                        resultToJs.phoneNumber = VoipHelper.callNumber;
                        tools.resultToJavascript(resultToJs);
                    }
                }
                else //挂机
                {
                    VoipHelper.WriteLog(string.Format("软挂机"));
                    VoipHelper.lineToSpk(0);
                    VoipHelper.offHookCallNumber = null;
                    VoipHelper.playHandle = VoipHelper.StopVoice(VoipHelper.playHandle);
                    ConstDefault.isBySelf = true;
                    ConstDefault.isCalling = false;

                    if (VoipHelper.callState == VoipHelper.telState.IN)
                    {
                        MainWindow.form.Topmost = false;
                    }
                    if (VoipHelper.isHookError)
                    {
                        VoipHelper.isHookError = false;
                        return;
                    }

                    ConstDefault.resultToJs resultToJs = new ConstDefault.resultToJs();
                    resultToJs.action = ConstDefault.phone_idel;
                    tools.resultToJavascript(resultToJs);
                }
            }
            catch (Exception err)
            {
                VoipHelper.isHookError = true;
                ConstDefault.resultToJs errToJs = new ConstDefault.resultToJs();
                errToJs.action = ConstDefault.phone_idel;
                tools.resultToJavascript(errToJs);
                VoipHelper.OffOnHook(0);
                VoipHelper.WriteLog(string.Format("软摘软挂错误==>>{0}", err));
                return;
            }
        }
    }
}
