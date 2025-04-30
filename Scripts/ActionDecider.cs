using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class ActionDecider
    {
        protected Character mParent;
        protected ObscuredFloat mAttackCoolTime;
        public virtual void Init(Character parent) { mParent = parent;}
        public virtual void OnUpdate(float dt) 
        {
            if (mParent.Action.IsIdle && mAttackCoolTime > 0)
            {
                mAttackCoolTime -= dt;
            }
        }

        public virtual void RandomizeKey()
        {
            mAttackCoolTime.RandomizeCryptoKey();
        }

        public virtual ActionInst Decide()
        {
            if (mParent.IsMonster)
            {
                bool anyInAttackRange = mParent.AnyInAttackRangeOpponent();
                if (anyInAttackRange && mAttackCoolTime <= 0)
                {
                    mAttackCoolTime = 0.5f;

                    return new Action_Attack();
                }
                else
                {
                    return new Action_Idle();
                }
            }
            else
            {
                bool anyInAttackRange = mParent.AnyInAttackRangeOpponent();
                if (anyInAttackRange && mAttackCoolTime <= 0)
                    return new Action_Attack();
                else
                    return new Action_Move();
            }
        }
    }

    static class ActionDeciderCreator
    {
        public static ActionDecider Create(Character parent)
        {
            if (parent.IsRaidBoss)
            {
                return new ActionDecider_RaidBoss();
            }
            else
            {
                return new ActionDecider();
            }
        }
    }
}
