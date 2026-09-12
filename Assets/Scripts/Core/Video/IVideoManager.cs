using System;
using UnityEngine;
using UnityEngine.Video;

namespace Core.Video
{
    /// <summary>
    /// 视频播放管理器接口
    /// </summary>
    public interface IVideoManager
    {
        event Action OnPrePlay;
        event Action OnPostPlay;

        void PlayVideo(VideoClip videoClip, RenderTexture renderTexture);
    }
}
