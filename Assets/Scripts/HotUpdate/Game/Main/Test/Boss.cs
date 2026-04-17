using System;
using Core.DI;
using Core.GlobalEvent;
using Core.Input.ActionAsset;
using Core.Serialize.Json;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.Game.Main.Test
{
    public class Boss : MonoBehaviour
    {
        [Inject] private IJsonManager _jsonManager;
        [Inject] private IInputSystem _inputSystem;
        [Inject] private IEventCenter _eventCenter;

        private void Awake()
        {
            DIContainer.InjectIntoInstance(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            Logger.Log($"{nameof(Boss)} has been started");
            Logger.Log($"{nameof(_jsonManager)} has been started");
            Logger.Log($"{nameof(_inputSystem)} has been started");
            Logger.Log($"{nameof(_eventCenter)} has been started");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
