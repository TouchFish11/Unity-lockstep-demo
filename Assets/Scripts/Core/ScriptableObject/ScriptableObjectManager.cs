using System.Threading.Tasks;
using Core.Res;
using Core.Service;
using Core.Singleton;

namespace Core.ScriptableObject
{
    public class ScriptableObjectManager : IScriptableObjectManager, IInitializable
    {
        public int InitPriority => 0;

        private ScriptableObjectManager()
        {

        }

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 加载ScriptableObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T LoadSO<T>(string path) where T : UnityEngine.ScriptableObject
        {
            return ServiceLocator.Get<IResourcesManager>().Load<T>(path);
        }
    }
}
