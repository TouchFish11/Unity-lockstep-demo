namespace Net.Sync
{
    /// <summary>
    /// 消息工厂
    /// </summary>
    public static class MessageFactory
    {
        /// <summary>
        /// 创建消息
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="bytes"></param>
        /// <param name="nowIndex"></param>
        /// <returns></returns>
        public static Message CreateMessage(int msgId, byte[] bytes, int nowIndex)
        {
            Message message = null;
            //������Ϣ��
            switch (msgId)
            {
                case 2000:
                    message = new S2C_HeartMessage();
                    break;
                case 2001:
                    message = new S2C_ConnectMessage();
                    break;
                case 2002:
                    message = new S2C_MatchSuccessMessage();
                    break;
                case 2003:
                    message = new S2C_ConfirmMessage();
                    break;
                case 2004:
                    message = new S2C_PrepareReceMessage();
                    break;
                case 2005:
                    message = new S2C_StartRaceMessage();
                    break;
                case 2006:
                    message = new S2C_LeaveRaceMessage();
                    break;
                case 2007:
                    message = new S2C_ReconnecRaceMessage();
                    break;
                case 2008:
                    message = new S2C_ConnectConfirmMessage();
                    break;
                default:
                    Debug.LogError($"δ�������Ϣ���ͣ�{msgId}");
                    break;
            }

            // ������Ϣ
            message.Deserialize(bytes, nowIndex);
            return message;
        }
    }
}
