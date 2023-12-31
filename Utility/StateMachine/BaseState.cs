using System;
using UnityEngine;

namespace OptionalRule.Utility
{
    public abstract class BaseState<EState> where EState : Enum
    {
        public BaseState(EState stateKey)
        {
            StateKey = stateKey;
        }

        public EState StateKey { get; private set; }

        public abstract void EnterState();
        public abstract void ExitState();
        public abstract void UpdateState();
        public abstract EState GetNextState();
        public abstract void SetNextState(EState nextState);
        public abstract void OnTriggerEnter(Collider other);
        public abstract void OnTriggerExit(Collider other);
        public abstract void OnTriggerStay(Collider other);
    }
}