using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Singleton;
using Net.FrameSync.Interface;
using UnityEngine;

namespace Net.FrameSync.Manager
{
    /// <summary>
    /// ������Ϸ������
    /// ���������������
    /// </summary>
    public class NetGameManager : SingletonBase<NetGameManager>
    {
        //����������Ŀͻ��˻��棺�����ͻ���ID��ֵ����Ҷ���
        private readonly Dictionary<int, INetObject> _idToPlayerMap = new Dictionary<int, INetObject>();

        private NetGameManager()
        {

        }

        /// <summary>
        /// ��ȡ��Ҷ���
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        public bool TryGetPlayer(int clientId, out INetObject netObject)
        {
            return _idToPlayerMap.TryGetValue(clientId, out netObject);
        }

        /// <summary>
        /// ������Ҷ���
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="playerCharacter"></param>
        public void AddPlayer(int clientId, INetObject netObject)
        {
            if (_idToPlayerMap.TryAdd(clientId, netObject))
            {
                Debug.Log($"��ң�{clientId}��������Ϸ");
            }
        }

        public override int InitPriority => throw new System.NotImplementedException();

        public override Task InitAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
