using System;

namespace Core.Service.Login
{
    /// <summary>
    /// 登录数据
    /// </summary>
    [Serializable]
    public struct LoginData
    {
        // 账号
        public string account;
        // 密码
        public string password;

        public LoginData(string account, string password)
        {
            this.account = account;
            this.password = password;
        }
    }
}
