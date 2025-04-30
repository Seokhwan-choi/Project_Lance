using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    class Action_Idle : ActionInst
    {
        public override void OnStart(Character parent)
        {
            base.OnStart(parent);

            mDuration = 0.5f;

            parent.Anim.PlayIdle();

            if (parent is Player)
            {
                Lance.GameManager.StageManager.SetPlayerMoving(false);
            }
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            if (!mParent.IsPlayer)
                mDuration -= dt;
        }
    }
}