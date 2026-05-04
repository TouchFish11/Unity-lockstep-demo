using System;

namespace HotUpdate.Base.Tip
{
    /// <summary>
    /// 确认数据
    /// </summary>
    public class ConfirmData
    {
        public string ConfirmTitle { get; set; }
        
        public string ConfirmMessage { get; set; }
        
        public object ContentData { get; set; }
        
        public EConfirmContent ConfirmContent { get; set; }
        
        public Action OnConfirm { get; set; }
    }
}
