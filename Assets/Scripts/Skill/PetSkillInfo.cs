using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class PetSkillInfo
    {
        PetSkillManager mParent;
        SkillData mData;
        ObscuredString mId;
        ObscuredFloat mCoolTime;
        ObscuredFloat mDuration;
        SkillValueCalculatorInst mSkillValueCalc;

        public StatType PassiveType => mData.passiveType;
        public string Id => mId;
        public void Init(PetSkillManager parent, Player player, string id)
        {
            mParent = parent;
            mId = id;
            mDuration = 0;
            mData = DataUtil.GetPetSkillData(id);
            if (mData.skillValueCalcType != SkillValueCalcType.None)
            {
                mSkillValueCalc = SkillValueCalculatorUtil.Create(player, mData.skillValueCalcType, mData.skillValue);
            }

            FillCoolTime();
        }

        public void OnUpdate(float dt)
        {
            if (mCoolTime > 0)
                mCoolTime -= dt;

            if (mDuration <= 0)
            {
                mParent?.ReleaseFX(mId);
            }
            else
            {
                mDuration -= dt;
            }
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mCoolTime.RandomizeCryptoKey();
            mDuration.RandomizeCryptoKey();
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
            mCoolTime = mData.coolTime;

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

        public ObscuredDouble GetSkillValue(DamageInst inst = null)
        {
            if (mSkillValueCalc != null)
            {
                return mSkillValueCalc.Calc(inst);
            }
            else
            {
                return mData.skillValue;
            }
        }
    }
}