using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class JoustPlayer : Player
    {
        public override void Init()
        {
            mAnim = new PlayerAnim();
            mAnim.Init(this);

            mPhysics = new CharacterPhysics();
            mPhysics.Init(this);

            mActionManager = new ActionManager();
            mActionManager.Init(this);

            mStat = new CharacterStat();

            float moveSpeedRatio = (float)Lance.Account.GatherStat(StatType.MoveSpeedRatio);
            float moveSpeed = (float)Lance.Account.GatherStat(StatType.MoveSpeed) * (1 + moveSpeedRatio);

            mStat.SetMoveSpeed(2.5f, 2f);

            mActionManager.PlayAction(ActionType.Idle, true);

            mWeaponRenderer = gameObject.FindComponent<SpriteRenderer>("Weapon");

            SetPosition(new Vector2(-1.15f, 1f));
        }

        public override void OnUpdate(float dt)
        {
            mActionManager.OnUpdate(dt);
            mPhysics.OnUpdate(dt);

            // 마상 전투 상단의 이동하는 UI 갱신...?

            if (IsMoving)
            {
                if (mCurMoveSpeed < GetMoveSpeed())
                {
                    mCurMoveSpeed += (dt * Const.Acceleration);

                    mAnim.SetMoveSpeed(mCurMoveSpeed / GetMoveSpeed());
                }
                else
                {
                    if (mCurMoveSpeed != GetMoveSpeed())
                    {
                        mCurMoveSpeed = GetMoveSpeed();

                        mAnim.SetMoveSpeed(1f);

                        Lance.GameManager.StageManager.SetActiveWindlines(true);
                    }
                }
            }
            else
            {
                mCurMoveSpeed = 0f;
            }
        }

        public override bool AnyInAttackRangeOpponent()
        {
            return false;
        }
        public override float GetMoveSpeed() { return mStat.MoveSpeed * (1 + mStat.MoveSpeedRatio); }
        public override float GetMoveSpeedRatio() { return mStat.MoveSpeedRatio; }
    }
}