using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class GuideAction
    {
        protected GuideActionData mData;
        protected bool mIsFinish;
        protected float mWaitTime;
        protected GuideActionManager mParent;
        public bool IsFinish => mIsFinish;
        public virtual void Init(GuideActionManager parent, GuideActionData data) 
        { 
            mParent = parent; 
            mData = data; 
            mWaitTime = data.waitTime; 
        }
        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnFinish() { }
    }
}