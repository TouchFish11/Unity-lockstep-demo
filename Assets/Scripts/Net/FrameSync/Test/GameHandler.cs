using System.Threading.Tasks;
using Core.Singleton;

namespace Net.FrameSync.Test
{
    public class GameHandler : SingletonBase<GameHandler>
    {
        /// <summary>
        /// �Ƿ�ֹͣͬ����ģ����ߣ�
        /// </summary>
        public bool IsStop { get; set; }
        
        private GameHandler()
        {

        }

        public override int InitPriority => throw new System.NotImplementedException();

        public override Task InitAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
