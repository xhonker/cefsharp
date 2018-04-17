using System.Diagnostics;
using newCRM.Tools;
using Newtonsoft.Json;

namespace newCRM
{
    /// <summary>
    /// 注册JS回调
    /// </summary>
    internal class CallBackForJs
    {
        /// <summary>
        /// 开始通话
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="callId">通话ID</param>
        /// <returns>{code:number,msg:string}</returns>
        public void startTalk(string phone)
        {
            VoipHelper.callId = Utils.GetCallId();
            VoipHelper.callState = VoipHelper.telState.OUT;
            VoipHelper.callNumber = phone;
            VoipHelper.Call(phone);
            VoipHelper.OffOnHook(1);
        }
        /// <summary>
        /// 接听来电
        /// </summary>
        /// <param name="callId">通话ID</param>
        /// <returns>{code:number,msg:string}</returns>
        public void answerCall()
        {
            VoipHelper.OffOnHook(1);
        }

        /// <summary>
        /// 挂断
        /// </summary>
        /// <returns>{code:number,msg:string}</returns>
        public void stopTalk()
        {
            VoipHelper.OffOnHook(0);
        }
        /// <summary>
        /// 风险提示
        /// </summary>
        public void riskprompt()
        {
            if (VoipHelper.playHandle <= 0)
            {
                VoipHelper.domicToLine(0);
                VoipHelper.playHandle = VoipHelper.PlayVoice(VoipHelper.PLAYFILEPATH);
            }
        }
        /// <summary>
        /// 系统消息
        /// </summary>
        /// <param name="message">传入消息对象</param>
        /// <returns>{code:number,msg:string}</returns>
        public static void systemNewMessage(string message)
        {
            if (!MainWindow.isActivation)
            {
                MainWindow.form.newMessage(message);
            }
        }
        /// <summary>
        /// 如果评价中 则拒接来电
        /// </summary>
        /// <param name="isEvaluate"></param>
        public void phoneIsEvaluate(bool isEvaluate)
        {
            //if (isEvaluate)
            //{
            //    VoipHelper.stopCusttentIncoming();
            //}
            //else
            //{
            //    var t = VoipHelper.refuseCurrentIncoming();
            //    Debug.WriteLine("拒接来电" + t);
            //}
        }
        /// <summary>
        /// 判断设备是否正常
        /// </summary>
        public void deviceIsNormal(string userID)
        {
            VoipHelper.userID = userID;
            ConstDefault.retToJs deviceIdNormal = new ConstDefault.retToJs();
            deviceIdNormal.action = ConstDefault.DEVICE_IS_NORMAL;
            deviceIdNormal.deviceIsNormal = VoipHelper.deviceState;
            Utils.resultToJavascript(deviceIdNormal);
        }
        /// <summary>
        /// 统一消息接收
        /// </summary>
        /// <param name="msg">DispacthMsg</param>
        public void dispacthMsg(string msg)
        {

            if (!string.IsNullOrEmpty(msg))
            {
                var jsonMsg = JsonConvert.DeserializeObject<DispacthMsg>(msg);
                Debug.WriteLine(jsonMsg);
                Utils.WriteLog(string.Format("Js To Client ==>> {0}", msg));
                if (jsonMsg.action != null)
                {
                    switch (jsonMsg.action)
                    {
                        case ConstDefault.PHONE_HANG_UP:
                            stopTalk();
                            break;
                        case ConstDefault.PHONE_MAKE_CALL:
                            startTalk(jsonMsg.payload.phoneNumber);
                            break;
                        case ConstDefault.PHONE_PICK_UP:
                            answerCall();
                            break;
                        case ConstDefault.NOTIFICATION:
                            if (!string.IsNullOrEmpty(jsonMsg.payload.content))
                            {
                                systemNewMessage(jsonMsg.payload.content);
                            }
                            break;
                        case ConstDefault.PHONE_IS_EVALUATE:
                            phoneIsEvaluate(jsonMsg.payload.isEvaluate);
                            break;
                        case ConstDefault.PHONE_RISKPROMPT:
                            riskprompt();
                            break;
                        case ConstDefault.DEVICE_IS_NORMAL:
                            deviceIsNormal(jsonMsg.payload.userID);
                            break;
                        default:
                            break;
                    }
                }

            }

        }
    }
}