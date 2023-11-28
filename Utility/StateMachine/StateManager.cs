using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

namespace OptionalRule.Utility
{
    public abstract class StateManager<EState>: MonoBehaviour where EState : Enum
    {
        protected Dictionary<EState, BaseState<EState>> states = new Dictionary<EState, BaseState<EState>>();

        protected BaseState<EState> currentState;

        protected bool isTransitioningState = false;

        void Start()
        {
            currentState.EnterState();
        }

        void Update()
        {
            EState nextStateKey = currentState.GetNextState();
            if (!isTransitioningState && nextStateKey.Equals(currentState.StateKey))
            {
                currentState.UpdateState();
            } else if (!isTransitioningState)
            {
                TransitionToState(nextStateKey);
            }
        }

        public void TransitionToState(EState nextStateKey)
        {
            isTransitioningState = true;
            currentState.ExitState();
            currentState = states[nextStateKey];
            currentState.EnterState();
            isTransitioningState = false;
        }

        void OnTriggerEnter(Collider other)
        {
            currentState.OnTriggerEnter(other);
        }
        
        void OnTriggerExit(Collider other)
        {
            currentState.OnTriggerExit(other);
        }
        void OnTriggerStay(Collider other)
        {
            currentState.OnTriggerStay(other);
        }
    }
}