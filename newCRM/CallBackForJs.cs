using System.Diagnostics;
using newCRM.Tools;
using 上海CRM管理系统.Tools;
using Newtonsoft.Json;

namespace newCRM
{
    internal class CallBackForJs
    {
        /// <summary>
        /// 开始通话
        /// code =200 成功 其他失败
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="callId">通话ID</param>
        /// <returns>{code:number,msg:string}</returns>
        public void startTalk(string phone)
        {
            VoipHelper.callId = tools.GetCallId();
            VoipHelper.callState = VoipHelper.telState.OUT;
            VoipHelper.callNumber = phone;
            VoipHelper.Call(phone);
            VoipHelper.OffOnHook(1);
        }
        /// <summary>
        /// 接听来电
        /// code =200 成功 其他失败
        /// </summary>
        /// <param name="callId">通话ID</param>
        /// <returns>{code:number,msg:string}</returns>
        public void answerCall()
        {
            VoipHelper.OffOnHook(1);
        }

        /// <summary>
        /// 挂断 
        /// code =200 成功 其他失败
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
            ConstDefault.resultToJs deviceIdNormal = new ConstDefault.resultToJs();
            deviceIdNormal.action = ConstDefault.device_is_normal;
            deviceIdNormal.deviceIsNormal = VoipHelper.deviceState;
            tools.resultToJavascript(deviceIdNormal);
        }
        /// <summary>
        /// 统一消息接收
        /// </summary>
        /// <param name="msg">DispacthMsg</param>
        public void dispacthMsg(string msg)
        {

            if (!string.IsNullOrEmpty(msg))
            {
                //var jsonMsg = JsonHelper.JsonDeserialize<DispacthMsg>(msg);
                var jsonMsg = JsonConvert.DeserializeObject<DispacthMsg>(msg);
                Debug.WriteLine(JsonConvert.DeserializeObject<DispacthMsg>(msg));
                VoipHelper.WriteLog(string.Format("Js To Client ==>> {0}", msg));
                if (jsonMsg.action != null)
                {
                    switch (jsonMsg.action)
                    {
                        case ConstDefault.phone_hang_up:
                            stopTalk();
                            break;
                        case ConstDefault.phone_make_call:
                            startTalk(jsonMsg.payload.phoneNumber);
                            break;
                        case ConstDefault.phone_pick_up:
                            answerCall();
                            break;
                        case ConstDefault.notification:
                            if (!string.IsNullOrEmpty(jsonMsg.payload.content))
                            {
                                systemNewMessage(jsonMsg.payload.content);
                            }
                            break;
                        case ConstDefault.phone_is_evaluate:
                            phoneIsEvaluate(jsonMsg.payload.isEvaluate);
                            break;
                        case ConstDefault.phone_riskprompt:
                            riskprompt();
                            break;
                        case ConstDefault.device_is_normal:
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