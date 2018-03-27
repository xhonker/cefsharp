using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                VoipHelper.WriteLog(string.Format("电话机摘机{0}", tools.GetTimeStamp()));
                VoipHelper.StopVoice(VoipHelper.playHandle);
                VoipHelper.OffOnHook(1);

                ConstDefault.resultToJs phoneHook = new ConstDefault.resultToJs();
                phoneHook.action = ConstDefault.phone_calling;
                phoneHook.isOffHook = true;
                tools.resultToJavascript(phoneHook);
            }
            if (VoipHelper.callState == VoipHelper.telState.OUT)
            {
                //VoipHelper.OffOnHook(1);
            }

        }

        /// <summary>
        /// 电话机挂机处理
        /// </summary>
        public static void phoneHang()
        {
            VoipHelper.WriteLog(string.Format("电话机挂机{0}", tools.GetTimeStamp()));
            if (VoipHelper.isOffHookCall)
            {
                VoipHelper.isOffHookCall = false;
                VoipHelper.offHookCallNumber = string.Empty;
            }
            VoipHelper.OffOnHook(0);

            ConstDefault.resultToJs phonehang = new ConstDefault.resultToJs();
            phonehang.action = ConstDefault.phone_idel;
            phonehang.isOffHook = true;
            tools.resultToJavascript(phonehang);
        }

        /// <summary>
        /// 接收到来电号码
        /// </summary>
        /// <param name="phone"> 手机号码</param>
        public static void getCallID(string phone)
        {
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
            #region 号码小于7位屏蔽
            //if (phone.Length < 7)
            //{
            //    VoipHelper.StopVoice(VoipHelper.playHandle);
            //    VoipHelper.OffOnHook(0);
            //    VoipHelper.lineToSpk(0);
            //    return;
            //}
            #endregion
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
                        MainWindow.form.Topmost = false;
                        ConstDefault.isCalling = false; // 重置状态
                        VoipHelper.StopVoice(VoipHelper.playHandle);
                        VoipHelper.OffOnHook(0);
                        VoipHelper.lineToSpk(0);

                        ConstDefault.resultToJs getCallIdThree = new ConstDefault.resultToJs();
                        getCallIdThree.action = ConstDefault.phone_idel;
                        tools.resultToJavascript(getCallIdThree);
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
            VoipHelper.WriteLog(string.Format("忙音{0}", tools.GetTimeStamp()));
            ConstDefault.resultToJs busy = new ConstDefault.resultToJs();
            busy.action = ConstDefault.phone_idel;
            tools.resultToJavascript(busy);
        }

        /// <summary>
        /// 对方接听
        /// </summary>
        public static void remoteHook()
        {
            VoipHelper.WriteLog(string.Format("对方接听{0}", tools.GetTimeStamp()));
            ConstDefault.resultToJs remoteHook = new ConstDefault.resultToJs();
            remoteHook.action = ConstDefault.phone_calling;
            tools.resultToJavascript(remoteHook);
        }

        /// <summary>
        /// 远程挂机
        /// </summary>
        public static void remoteHang()
        {
            VoipHelper.playHandle = VoipHelper.StopVoice(VoipHelper.playHandle);
            VoipHelper.WriteLog(string.Format("远程挂机{0}", tools.GetTimeStamp()));
            //传给前端一个信号 
            ConstDefault.isBySelf = false;

            ConstDefault.resultToJs remoteHang = new ConstDefault.resultToJs();
            remoteHang.action = ConstDefault.phone_idel;
            tools.resultToJavascript(remoteHang);
            VoipHelper.OffOnHook(0);
            VoipHelper.lineToSpk(0);
        }

        /// <summary>
        /// 摘机手动拨号
        /// </summary>
        /// <param name="phone">手机号码</param>
        public static void phoneDial(string phone)
        {
            VoipHelper.WriteLog(string.Format("摘机手动拨号{0}", tools.GetTimeStamp()));
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
                VoipHelper.WriteLog(string.Format("电话摘机检查拨号结束{0}", tools.GetTimeStamp()));
                VoipHelper.OffOnHook(1);
                VoipHelper.callId = tools.GetCallId();

                ConstDefault.resultToJs ringBack = new ConstDefault.resultToJs();
                ringBack.action = ConstDefault.phone_dialing;
                ringBack.phoneNumber = VoipHelper.offHookCallNumber;
                tools.resultToJavascript(ringBack);
                VoipHelper.isOffHookCall = true;
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
                    VoipHelper.WriteLog(string.Format("软摘机{0}", tools.GetTimeStamp()));
                    VoipHelper.StopVoice(VoipHelper.playHandle);
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
                    VoipHelper.WriteLog(string.Format("软挂机{0}", tools.GetTimeStamp()));
                    VoipHelper.StopVoice(VoipHelper.playHandle);
                    ConstDefault.isBySelf = true;
                    ConstDefault.isCalling = false;

                    ConstDefault.resultToJs resultToJs = new ConstDefault.resultToJs();
                    resultToJs.action = ConstDefault.phone_idel;
                    tools.resultToJavascript(resultToJs);

                    if (VoipHelper.callState == VoipHelper.telState.IN)
                    {
                        MainWindow.form.Topmost = false;
                    }
                }
            }
            catch (Exception err)
            {
                VoipHelper.WriteLog(string.Format("软摘软挂错误{0}==>>{1}", tools.GetTimeStamp(), err));
            }
        }
    }
}
