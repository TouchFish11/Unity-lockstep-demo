using Core.Net.SyncModule.Interface;
using Net.Protocols.FSync.Messages;
using UnityEngine;

namespace HotUpdate.Game.Race
{
    public class RaceRoleController : MonoBehaviour, INetObject
    {
        public int ClientId { get; private set; }
        
        public void Init(int clientId)
        {
            ClientId = clientId;
        }

        public void CollectInput(OptMessage optMessage)
        {
            
        }

        public void SyncFrame(OptMessage optMessage)
        {

        }
    }
}
