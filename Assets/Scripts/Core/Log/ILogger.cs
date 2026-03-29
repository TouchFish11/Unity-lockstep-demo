using Core.Net;

namespace Core.Log
{
    public interface ILogger
    {
        bool EnableLog { get; set; }

        void UploadLog(UploadProgressCallBack progressCallBack);
    }
}
