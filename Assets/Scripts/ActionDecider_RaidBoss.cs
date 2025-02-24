using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Linq;


namespace Lance
{
    class ActionDecider_RaidBoss : ActionDecider
    {
        const float RageInterval = 10f;
        const float AttackCoolTime = 0.125f;
        ObscuredFloat mRageTime;
        ObscuredInt mRageStep;

        public override void Init(Character parent)
        {
            base.Init(parent);

            mRageTime = RageInterval;
            mRageStep = 0;
            mAttackCoolTime = AttackCoolTime;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mRageTime.RandomizeCryptoKey();
            mRageStep.RandomizeCryptoKey();
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            if (mAttackCoolTime > 0)
            {
                mAttackCoolTime -= dt;
            }

            if (mRageTime > 0)
            {
                mRageTime -= dt;
                if (mRageTime <= 0f)
                {
                    mRageTime = RageInterval;

                    mRageStep++;

                    // 레이드 보스의 레벨을 높여주자
                    var data = Lance.GameData.RaidBossRageLevelData.Where(x => x.step == mRageStep).FirstOrDefault();
                    if (data != null)
                    {
                        var monster = mParent as Monster;

                        mParent.Stat.InitMonster(monster, monster.Data, data.level);
                    }

                    mParent.Action.PlayRageAction(mParent.SpriteLibraryAssetData.rageActionId);
                }
            }
        }

        public override ActionInst Decide()
        {
            if (mAttackCoolTime <= 0)
            {
                bool anyInAttackRange = mParent.AnyInAttackRangeOpponent();
                if (anyInAttackRange)
                {
                    float atkSpeed = Mathf.Min(Lance.GameData.PlayerStatMaxData.atkSpeedMax, mParent.Stat.AtkSpeed);

                    mAttackCoolTime = AttackCoolTime * (1 / atkSpeed);

                    return new Action_Attack();
                }
                else
                {
                    return new Action_Idle();
                }
            }
            else
            {
                return new Action_Idle();
            }
        }
    }
}