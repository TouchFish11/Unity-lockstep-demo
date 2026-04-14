using UnityEngine;
using Logger = Core.Log.Logger;

namespace Core.DI.Test
{
    public class Boss : MonoBehaviour
    {
        public void Attack()
        {
            Logger.Log($"{nameof(Boss)}: Boss Attacked");
        }
    }
}
