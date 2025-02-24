using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    // �÷��̾ �̵������� Ȯ���ϴ� �׼�
    // �÷��̾�� �� �׼��� ����� �� �ִ�..
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