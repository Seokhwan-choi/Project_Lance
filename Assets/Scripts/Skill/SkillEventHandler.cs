using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    class SkillEventHandler : MonoBehaviour
    {
        Action mOnReady;
        Action mOnAttack;
        Action mOnFinish;
        public void SetReadyEvent(Action onReady)
        {
            mOnReady = onReady;
        }

        public void OnReadyEvent()
        {
            mOnReady?.Invoke();
        }

        public void SetAttackEvent(Action onAttack)
        {
            mOnAttack = onAttack;
        }

        public void OnAttackEvent()
        {
            mOnAttack?.Invoke();
        }

        public void SetFinishEvent(Action onFinish)
        {
            mOnFinish = onFinish;
        }

        public void OnFinishEvent()
        {
            mOnFinish?.Invoke();
        }
    }
}