using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class SkillInst
    {
        public string Id;
        public int Level;
        public int Count;

        ObscuredString mId;
        ObscuredInt mLevel;
        ObscuredInt mCount;

        public SkillInst(string id, int level, int count)
        {
            mId = id;
            mLevel = level;
            mCount = count;
        }

        public bool IsMaxLevel()
        {
            return DataUtil.GetSkillMaxLevel(mId) == mLevel;
        }

        public void LevelUp(int count)
        {
            mLevel += count;

            mLevel = Math.Min(mLevel, DataUtil.GetSkillMaxLevel(mId));
        }

        public void SetLevel(int level)
        {
            mLevel = level;

            mLevel = Math.Min(mLevel, DataUtil.GetSkillMaxLevel(mId));
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public bool Dismantle(int count)
        {
            var skillData = DataUtil.GetSkillData(mId);
            if (skillData == null)
                return false;

            if (Lance.GameData.SkillDismantleData.ContainsKey(skillData.grade) == false)
                return false;

            return UseCount(count);
        }

        public ObscuredInt GetCount()
        {
            return mCount;
        }

        public void AddCount(int count)
        {
            mCount += count;
        }

        public bool UseCount(int count)
        {
            if (IsEnoughCount(count) == false)
                return false;

            mCount -= count;

            return true;
        }

        public bool IsEnoughCount(int count)
        {
            return mCount >= count;
        }

        public int GetUpgradeRequireCount(int count)
        {
            string id = mId;

            if (id.ToString().IsValid() == false)
                return int.MaxValue;

            return DataUtil.GetSkillLevelUpRequireCount(mId, mLevel, count);
        }

        public double GetValue()
        {
            string id = mId;

            if (id.IsValid() == false)
                return 0;

            return DataUtil.GetSkillValue(id, mLevel);
        }

        public bool IsActiveSkill()
        {
            SkillData data = DataUtil.GetSkillData(mId);

            return data?.type == SkillType.Active;
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            Count = mCount;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mCount.RandomizeCryptoKey();
        }

        public string GetSkillId()
        {
            return mId;
        }
    }
}
