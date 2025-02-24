using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class SkillInfo
    {
        SkillManager mParent;
        Player mPlayer;
        SkillData mData;
        ObscuredString mId;
        ObscuredInt mSlot;
        ObscuredInt mStackedCount;
        ObscuredFloat mCoolTime;
        ObscuredFloat mDuration;
        SkillConditionInst mCondition; 
        public string Id => mId;
        public int Slot => mSlot;
        public SkillCondition Condition => mData.condition;
        public SkillData Data => mData;
        public void Init(SkillManager parent, Player player, int slot, string id)
        {
            mParent = parent;
            mPlayer = player;
            mSlot = slot;
            mId = id;
            mStackedCount = 0;
            mDuration = 0;
            mData = DataUtil.GetSkillData(id);
            if (mData.condition != SkillCondition.None)
            {
                mCondition = SkillConditionUtil.Create(player, mData.condition, mData.conditionValue);
            }
        }

        public bool Envaluate(DamageInst inst = null)
        {
            return mCondition?.Evaluate(inst) ?? false;
        }

        public bool Apply(DamageInst inst = null)
        {
            bool satisfied = Envaluate(inst);
            if (satisfied)
            {
                bool isOver = mData.stackCount < mStackedCount + 1;
                mStackedCount = Mathf.Min(mData.stackCount, mStackedCount + 1);

                mDuration = mData.duration;

                if (isOver == false)
                    mPlayer.UpdatePlayerStat();

                return true;
            }

            return false;
        }

        public ObscuredInt GetStackCount()
        {
            return mStackedCount;
        }

        public void OnUpdate(float dt)
        {
            if (mCoolTime > 0)
                mCoolTime -= dt;

            if (mDuration > 0)
            {
                mDuration -= dt;
                if (mDuration <= 0)
                {
                    mPlayer.ReleaseFX(mId);
                    mPlayer.UpdatePlayerStat();
                }
            }
        }

        public bool IsApply()
        {
            if (mCondition != null)
            {
                return mStackedCount > 0 && mDuration > 0;
            }
            else
            {
                return true;
            }
        }

        public ObscuredDouble GetSkillValue(DamageInst inst = null)
        {
            double skillValue = Lance.Account.SkillInventory.GetSkillValue(mData.type, mId);

            if (mCondition != null)
            {
                if (IsApply() || Envaluate(inst))
                {
                    int stackedCount = Mathf.Max(1, mStackedCount);

                    return skillValue * stackedCount;
                }
                else
                {
                    return skillValue * mStackedCount;
                }
            }
            else
            {
                return skillValue;
            }
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mSlot.RandomizeCryptoKey();
            mCoolTime.RandomizeCryptoKey();
            mDuration.RandomizeCryptoKey();
            mStackedCount.RandomizeCryptoKey();
            mCondition?.RandomizeKey();
        }

        // 스킬 사용
        public bool Cast()
        {
            if (CanCast() == false)
                return false;

            return FillCoolTime();
        }

        public bool FillCoolTime()
        {
            float reduceValue = (float)mParent.GatherPassiveSkillValue(StatType.ReduceSkillCoolTime);
            float reduceCoolTime = mData.coolTime * reduceValue;

            mCoolTime = mData.coolTime - reduceCoolTime;

            return true;
        }

        public void ResetCoolTime()
        {
            mCoolTime = 0f;
        }

        public float GetCoolTime()
        {
            return mCoolTime;
        }

        public bool CanCast()
        {
            return mCoolTime <= 0;
        }
    }
}