using System;
using System.Collections.Generic;
using Dev.Scripts.Consumables;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Scripts.GameManager
{
    public class GameManager:MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        public AState[] states;
        public AState  TopState { get { if (_stateStack.Count == 0) return null; return _stateStack[_stateStack.Count - 1]; }}

        public ConsumableDatabase consumableDatabase;
        private List<AState> _stateStack = new List<AState>();
        private Dictionary<string, AState> _stateDict = new Dictionary<string, AState>();

        protected void OnEnable()
        {
            PlayerData.Create();

            _instance = this;
            consumableDatabase.Load();
            _stateDict.Clear();

            if (states.Length==0)
                return;
            
            for (int i = 0; i < states.Length; i++)
            {
                states[i].manager = this;
                _stateDict.Add(states[i].name,states[i]);
            }
            
            _stateStack.Clear();
            PushState(states[0].GetName());
        }


        protected void Update()
        {
            if (_stateStack.Count>0)
            {
                _stateStack[_stateStack.Count-1].Tick();
            }
        }
    }
    

    public abstract class AState : MonoBehaviour
    {
        [HideInInspector]
        public GameManager manager;

        public abstract void Enter(AState from);
        public abstract void Exit(AState to);
        public abstract void Tick();

        public abstract string GetName();
    }
}