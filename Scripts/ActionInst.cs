using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class ActionInst
    {
        protected Character mParent;
        protected float mDuration;
        public bool IsFinish => mDuration <= 0;
        public virtual void OnStart(Character parent) { mParent = parent; }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnFinish() { }
    }
}