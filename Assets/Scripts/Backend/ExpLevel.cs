using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;
using System;
using System.Linq;

namespace Lance
{
    public class ExpLevel : AccountBase
    {
        ObscuredDouble mExp;
        ObscuredInt mLevel = 1;
        ObscuredInt mAP;
        ObscuredDouble mStackedMonsterKillCount;
        ObscuredDouble mStackedBossKillCount;
        ObscuredInt mLimitBreak;
        ObscuredInt mUltimateLimitBreak;

        public double GetCurrentRequireExp()
        {
            int nextLevel = mLevel + 1;
            var nextLevelUpData = DataUtil.GetPlayerLevelUpData(nextLevel);
            if (nextLevelUpData != null)
            {
                return nextLevelUpData.requireExp;
            }
            else
            {
                var levelUpData = DataUtil.GetPlayerLevelUpData(mLevel);

                return levelUpData?.requireExp ?? 0;
            }
        }

        public double GetExp()
        {
            return mExp;
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public void StackMonsterKillCount(int killCount)
        {
            mStackedMonsterKillCount += killCount;

            SetIsChangedData(true);
        }

        public void StackBossKillCount(int killCount)
        {
            mStackedBossKillCount += killCount;

            SetIsChangedData(true);
        }

        public double GetStackedMonsterKillCount()
        {
            return mStackedMonsterKillCount;
        }

        public double GetStackedBossKillCount()
        {
            return mStackedBossKillCount;
        }

        public int StackExp(double exp)
        {
            int levelUpCount = 0;

            if ( exp > 0 )
            {
                mExp += exp;

                SetIsChangedData(true);

                // 레벨업을 할 수 있으면 한번에 하자
                while(CanLevelUp())
                {
                    if (LevelUp())
                        levelUpCount++;
                }
            }

            return levelUpCount;
        }

        public bool LevelUp()
        {
            int nextLevel = mLevel + 1;

            var levelUpData = DataUtil.GetPlayerLevelUpData(nextLevel);
            if (levelUpData == null)
                return false;

            if (levelUpData.requireExp > mExp)
                return false;

            mExp -= levelUpData.requireExp;
            mLevel += 1;
            AddAP(levelUpData.ap);

            SetIsChangedData(true);

            return true;
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public void SetLevel(int level)
        {
            mLevel = level;
        }
#endif

        public bool IsEnoughLevel(int requireLevel)
        {
            return requireLevel <= mLevel;
        }

        public bool CanLevelUp()
        {
            int nextLevel = mLevel + 1;

            var levelUpData = DataUtil.GetPlayerLevelUpData(nextLevel);

            return levelUpData != null && levelUpData.requireExp <= mExp;
        }

        public bool IsEnoughAP(int ap)
        {
            return mAP >= ap;
        }

        public bool UseAP(int ap)
        {
            if (ap < 0)
                return false;

            if (IsEnoughAP(ap) == false)
                return false;

            mAP -= ap;

            return true;
        }

        public int GetAP()
        {
            return mAP;
        }

        public void AddAP(int ap)
        {
            if (ap <= 0)
                return;

            mAP += ap;
        }

#if UNITY_EDITOR
        public void SetLimitBreak(int limitBreak)
        {
            mLimitBreak = limitBreak;

            SetIsChangedData(true);
        }

        public void SetUltimateLimitBreak(int limitBreak)
        {
            mUltimateLimitBreak = limitBreak;

            SetIsChangedData(true);
        }
#endif

        public int GetLimitBreak()
        {
            return mLimitBreak;
        }

        public bool IsMaxLimitBreak()
        {
            int nextStep = mLimitBreak + 1;

            var nextStepData = DataUtil.GetLimitBreakDataByStep(nextStep);

            return nextStepData == null;
        }

        public bool CanTryLimitBreak()
        {
            return IsMaxLimitBreak() == false && IsEnoughLimitBreakLevel();
        }

        public bool IsEnoughLimitBreakLevel()
        {
            int nextStep = mLimitBreak + 1;

            var nextStepData = DataUtil.GetLimitBreakDataByStep(nextStep);

            int requireLevel = nextStepData?.requireLevel ?? int.MaxValue;

            return requireLevel <= mLevel;
        }

        public double GetLimitBreakStatValue(StatType statType)
        {
            return DataUtil.GetLimitBreakStatValue(statType, mLimitBreak);
        }

        public double GetNextLimitBreakStatValue(StatType statType)
        {
            return DataUtil.GetLimitBreakStatValue(statType, mLimitBreak + 1);
        }

        public int GetUltimateLimitBreak()
        {
            return mUltimateLimitBreak;
        }

        public bool IsMaxUltimateLimitBreak()
        {
            int nextStep = mUltimateLimitBreak + 1;

            var nextStepData = DataUtil.GetUltimateLimitBreakDataByStep(nextStep);

            return nextStepData == null;
        }

        public bool CanTryUltimateLimitBreak()
        {
            return IsMaxLimitBreak() && IsEnoughUltimateLimitBreakLevel();
        }

        public bool IsEnoughUltimateLimitBreakLevel()
        {
            int nextStep = mUltimateLimitBreak + 1;

            var nextStepData = DataUtil.GetUltimateLimitBreakDataByStep(nextStep);

            int requireLevel = nextStepData?.requireLevel ?? int.MaxValue;

            return requireLevel <= mLevel;
        }

        public double GetUltimateLimitBreakStatValue(StatType statType)
        {
            return DataUtil.GetUltimateLimitBreakStatValue(statType, mUltimateLimitBreak);
        }

        public double GetNextUltimateLimitBreakStatValue(StatType statType)
        {
            return DataUtil.GetUltimateLimitBreakStatValue(statType, mUltimateLimitBreak + 1);
        }

        public override bool CanUpdateRankScore()
        {
            return mLevel > 0;
        }

        public override string GetTableName()
        {
            return "ExpLevel";
        }

        public override Param GetParam()
        {
            var param = new Param();
            param.Add("Exp", (double)mExp);
            param.Add("Level", (int)mLevel);
            param.Add("AP", (int)mAP);
            param.Add("StackedMonsterKillCount", (double)mStackedMonsterKillCount);
            param.Add("StackedBossKillCount", (double)mStackedBossKillCount);
            param.Add("LimitBreak", (int)mLimitBreak);
            param.Add("UltimateLimitBreak", (int)mUltimateLimitBreak);

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            double expTemp = 0;
            double.TryParse(gameDataJson["Exp"].ToString(), out expTemp);
            mExp = expTemp;

            int levelTemp = 1;
            int.TryParse(gameDataJson["Level"].ToString(), out levelTemp);
            mLevel = levelTemp;

            int apTemp = 0;
            int.TryParse(gameDataJson["AP"].ToString(), out apTemp);
            mAP = apTemp;

            double monsterKillCountTemp = 0;
            double.TryParse(gameDataJson["StackedMonsterKillCount"].ToString(), out monsterKillCountTemp);
            mStackedMonsterKillCount = monsterKillCountTemp;

            double bossKillCountTemp = 0;
            double.TryParse(gameDataJson["StackedBossKillCount"].ToString(), out bossKillCountTemp);
            mStackedBossKillCount = bossKillCountTemp;

            if (gameDataJson.ContainsKey("LimitBreak"))
            {
                int limitBreakTemp = 0;

                int.TryParse(gameDataJson["LimitBreak"].ToString(), out limitBreakTemp);

                mLimitBreak = limitBreakTemp;
            }

            if (gameDataJson.ContainsKey("UltimateLimitBreak"))
            {
                int ultimateLimitBreakTemp = 0;

                int.TryParse(gameDataJson["UltimateLimitBreak"].ToString(), out ultimateLimitBreakTemp);

                mUltimateLimitBreak = ultimateLimitBreakTemp;
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mExp.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mAP.RandomizeCryptoKey();
            mStackedMonsterKillCount.RandomizeCryptoKey();
            mStackedBossKillCount.RandomizeCryptoKey();
            mLimitBreak.RandomizeCryptoKey();
        }

        public void NextLimitBreak()
        {
            if (IsMaxLimitBreak())
                return;

            if (IsEnoughLimitBreakLevel() == false)
                return;

            mLimitBreak += 1;

            SetIsChangedData(true);
        }

        public void NextUltimateLimitBreak()
        {
            if (IsMaxLimitBreak() == false)
                return;

            if (IsMaxUltimateLimitBreak())
                return;

            if (IsEnoughUltimateLimitBreakLevel() == false)
                return;

            mUltimateLimitBreak += 1;

            SetIsChangedData(true);
        }
    }
}