using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    // 플레이어가 이동중인지 확인하는 액션
    // 플레이어만이 이 액션을 사용할 수 있다..
    class Action_Move : ActionInst
    {
        public override void OnStart(Character parent)
        {
            base.OnStart(parent);

            mDuration = 999f;

            parent.Anim.PlayMove();

            if (parent.IsPlayer)
            {
                Lance.GameManager.StageManager.SetPlayerMoving(true);
            }
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            
            bool anyInAttackRange = mParent.AnyInAttackRangeOpponent();

            if (anyInAttackRange)
            {
                mDuration = 0f;
            }
        }

        public override void OnFinish()
        {
            if (mParent.IsPlayer)
            {
                Lance.GameManager.StageManager.SetPlayerMoving(false);
            }
        }
    }
}