using System.Diagnostics;
using newCRM.Tools;
using 上海CRM管理系统.Tools;
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
            VoipHelper.lineToSpk(1);

        }
        /// <summary>
        /// 接听来电
        /// code =200 成功 其他失败
        /// </summary>
        /// <param name="callId">通话ID</param>
        /// <returns>{code:number,msg:string}</returns>
        public void answerCall()
        {
            VoipHelper.lineToSpk(1);
            VoipHelper.OffOnHook(1);
        }
   
        /// <summary>
        /// 挂断 
        /// code =200 成功 其他失败
        /// </summary>
        /// <returns>{code:number,msg:string}</returns>
        public string stopTalk()
        {
            try
            {
                if (VoipHelper.OffOnHook(0) < 0)
                {
                    //result.code = -1;
                    //result.msg = "挂机失败";
                }
                VoipHelper.lineToSpk(0);
            }
            catch (System.Exception err)
            {
                //result.code = -1;
                //result.msg = err.Message;
            }
            //return JsonHelper.Jsons(result);
            return "";
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
            // var t =   VoipHelper.refuseCurrentIncoming();
            //    Debug.WriteLine("拒接来电" + t);
            //}
        }
        /// <summary>
        /// 统一消息接收
        /// </summary>
        /// <param name="msg"></param>
        public void dispacthMsg(string msg)
        {

            if (!string.IsNullOrEmpty(msg))
            {
                var jsonMsg = JsonHelper.JsonDeserialize<DispacthMsg>(msg);
                Debug.WriteLine("[action]==>>" + msg);
                //Debug.WriteLine("[payload]==>>"+jsonMsg.payload);
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
                        default:
                            break;
                    }
                }

            }

        }
    }
}