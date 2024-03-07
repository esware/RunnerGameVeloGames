using System;
using System.Collections.Generic;
using Dev.Scripts.Consumables;
using Dev.Scripts.Track;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Scripts.GameManager
{
    public class GameManager:MonoBehaviour
    {
        public AState[] states;
        private List<AState> _stateStack = new List<AState>();
        private Dictionary<string, AState> _stateDict = new Dictionary<string, AState>();

        protected void OnEnable()
        {
            PlayerData.Create();
            
            _stateDict.Clear();

            if (states.Length==0)
                return;
            
            for (int i = 0; i < states.Length; i++)
            {
                states[i].manager = this;
                _stateDict.Add(states[i].GetName(),states[i]);
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

        public void SwitchState(string newState)
        {
            AState state = FindState(newState);
            if (state==null)
            {
                Debug.LogError("Cant find the state named"+newState);
                return;
            }
            
            _stateStack[_stateStack.Count-1].Exit(state);
            state.Enter(_stateStack[_stateStack.Count-1]);
            _stateStack.RemoveAt(_stateStack.Count-1);
            _stateStack.Add(state);
        }

        private AState FindState(string stateName)
        {
            AState state;
            if (!_stateDict.TryGetValue(stateName,out state))
            {
                return null;
            }

            return state;
        }

        private void PushState(string stateName)
        {
            AState state;
            if (!_stateDict.TryGetValue(stateName,out state))
            {
                Debug.LogError("cant find the state named"+stateName);
                return;
            }

            if (_stateStack.Count>0)
            {
                _stateStack[_stateStack.Count-1].Exit(state);
                state.Enter(_stateStack[_stateStack.Count-1]);
            }
            else
            {
                state.Enter(null);
            }
            _stateStack.Add(state);
        }
    }
    
    

    public abstract class AState : MonoBehaviour
    {
        [HideInInspector]
        public GameManager manager;
        public TrackManager trackManager;
        
        public abstract void Enter(AState from);
        public abstract void Exit(AState to);
        public abstract void Tick();

        public abstract string GetName();
    }
}