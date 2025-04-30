using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    class Action_Knockback : ActionInst
    {
        public override void OnStart(Character parent)
        {
            base.OnStart(parent);

            mDuration = Const.DefaultKnockbackTime;

            parent.Anim.PlayKnockback();

            Lance.GameManager.StageManager.SetPlayerMoving(true);
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            mDuration -= dt;
        }

        public override void OnFinish()
        {
            base.OnFinish();

            Lance.GameManager.StageManager.SetPlayerMoving(false);
        }
    }
}