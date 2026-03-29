using System;
using System.Threading.Tasks;
using Core.Singleton;
using UnityEngine;
using UnityEngine.Video;

namespace Core.Video
{
    public class VideoManager : IVideoManager, IInitializable
    {
        public int InitPriority => 0;

        private VideoPlayer videoPlayer;

        public event Action OnPrePlay;

        /// <summary>
        /// 
        /// </summary>
        public event Action OnPostPlay;

        private VideoManager()
        {

        }

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// ������Ƶ
        /// </summary>
        /// <param name="videoClip"></param>
        /// <param name="renderTexture"></param>
        public void PlayVideo(VideoClip videoClip, RenderTexture renderTexture)
        {
            // ��ʼ����Ƶ������
            if (videoPlayer == null)
            {
                GameObject videoObj = new GameObject("VideoPlayer");
                videoPlayer = videoObj.AddComponent<VideoPlayer>();

                videoPlayer.playOnAwake = false;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            }

            videoPlayer.targetTexture = renderTexture;
            videoPlayer.clip = videoClip;
            videoPlayer.prepareCompleted += PrepareCompleted;

            // ׼��
            videoPlayer.Prepare();
        }

        /// <summary>
        /// ׼�����
        /// </summary>
        /// <param name="source"></param>
        private void PrepareCompleted(VideoPlayer source)
        {
            OnPrePlay?.Invoke();
            // ����
            source.Play();
        }
    }
}
