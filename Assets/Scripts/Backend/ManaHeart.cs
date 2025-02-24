using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using BackEnd;


namespace Lance
{
    // 장비 정보
    public class ManaHeartInst
    {
        public int Type;
        public int Level;

        ObscuredInt mType;       // 아이디
        ObscuredInt mLevel;       // 레벨
        ManaHeartData mData;
        public ManaHeartInst(ManaHeartType type, int level)
        {
            mType = (int)type;
            mLevel = level;
            mData = Lance.GameData.ManaHeartData.TryGet(type);
        }

        public void ReadyToSave()
        {
            Type = (int)mType;
            Level = mLevel;
        }

        public void RandomizeKey()
        {
            mType.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
        }

        public ObscuredInt GetLevel()
        {
            return mLevel;
        }

        public void SetLevel(int level)
        {
            mLevel = level;
        }

        public ManaHeartType GetManaHeartType()
        {
            return (ManaHeartType)(int)mType;
        }

        public int GetMaxLevel()
        {
            ManaHeartType type = GetManaHeartType();

            return DataUtil.GetManaHeartMaxLevel(type);
        }

        public bool IsMaxLevel()
        {
            return mLevel == GetMaxLevel();
        }

        public void LevelUp(int levelUpCount)
        {
            mLevel += levelUpCount;

            mLevel = Math.Min(mLevel, GetMaxLevel());
        }

        public int GetUpgradeRequire()
        {
            ManaHeartType type = GetManaHeartType();

            ManaHeartUpgradeData upgradeData = Lance.GameData.ManaHeartUpgradeData.TryGet(type);
            if (upgradeData == null)
                return int.MaxValue;

            if (upgradeData.require.Length <= mLevel)
                return int.MaxValue;

            return upgradeData.require[mLevel];
        }

        public StatType GetStatType()
        {
            return mData.valueType;
        }

        public (StatType type, double value) GetStatValueNType()
        {
            ManaHeartType type = GetManaHeartType();

            return (mData.valueType, DataUtil.GetManaHeartStatValue(type, mLevel));
        }

        public double GetStatValue()
        {
            ManaHeartType type = GetManaHeartType();

            return DataUtil.GetManaHeartStatValue(type, mLevel);
        }

        public double GetNextStatValue()
        {
            ManaHeartType type = GetManaHeartType();

            return DataUtil.GetManaHeartStatValue(type, mLevel + 1);
        }
    }

    public class ManaHeart : AccountBase
    {
        ObscuredInt mManaHeartStep;
        ObscuredInt mUpgradeStep;
        Dictionary<ManaHeartType, ManaHeartInst> mManaHeartInsts;
        protected override void InitializeData()
        {
            mManaHeartStep = 0;
            mUpgradeStep = 1;

            mManaHeartInsts = new Dictionary<ManaHeartType, ManaHeartInst>();

            foreach (var data in Lance.GameData.ManaHeartData.Values)
            {
                var ManaHeartInst = new ManaHeartInst(data.type, 0);

                mManaHeartInsts.Add(data.type, ManaHeartInst);
            }
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mManaHeartInsts = new Dictionary<ManaHeartType, ManaHeartInst>();

            var ManaHeartJsonDatas = gameDataJson["ManaHeartInsts"];

            for (int i = 0; i < ManaHeartJsonDatas.Count; i++)
            {
                var ManaHeartJsonData = ManaHeartJsonDatas[i];

                int typeTemp = 0;

                int.TryParse(ManaHeartJsonData["Type"].ToString(), out typeTemp);

                ManaHeartType type = (ManaHeartType)typeTemp;

                var ManaHeartData = Lance.GameData.ManaHeartData.TryGet(type);
                if (ManaHeartData == null)
                    continue;

                int levelTemp = 0;

                int.TryParse(ManaHeartJsonData["Level"].ToString(), out levelTemp);

                ManaHeartInst ManaHeartInst = new ManaHeartInst(type, levelTemp);

                mManaHeartInsts.Add(type, ManaHeartInst);
            }

            if (gameDataJson.ContainsKey("ManaHeartStep"))
            {
                int manaHeartStepTemp = 0;

                int.TryParse(gameDataJson["ManaHeartStep"].ToString(), out manaHeartStepTemp);

                mManaHeartStep = manaHeartStepTemp;
            }

            if (gameDataJson.ContainsKey("UpgradeStep"))
            {
                int upgradeStepTemp = 0;

                int.TryParse(gameDataJson["UpgradeStep"].ToString(), out upgradeStepTemp);

                mUpgradeStep = upgradeStepTemp;
            }
        }

        public override Param GetParam()
        {
            foreach (var ManaHeart in mManaHeartInsts.Values)
            {
                ManaHeart.ReadyToSave();
            }

            Param param = new Param();

            param.Add("ManaHeartStep", (int)mManaHeartStep);
            param.Add("UpgradeStep", (int)mUpgradeStep);
            param.Add("ManaHeartInsts", mManaHeartInsts);

            return param;
        }

        public override string GetTableName()
        {
            return "ManaHeart";
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mManaHeartStep.RandomizeCryptoKey();
            mUpgradeStep.RandomizeCryptoKey();

            foreach (var ManaHeart in GetManaHearts())
            {
                ManaHeart.RandomizeKey();
            }
        }

        public ManaHeartInst GetManaHeart(ManaHeartType type)
        {
            return mManaHeartInsts.TryGet(type);
        }

        public IEnumerable<ManaHeartInst> GetManaHearts()
        {
            return mManaHeartInsts.Values;
        }

        public int GetLevel(ManaHeartType ManaHeartType)
        {
            return GetManaHeart(ManaHeartType)?.GetLevel() ?? 0;
        }

        public bool IsMaxLevelManaHeart(ManaHeartType ManaHeartType)
        {
            var manaHeart = GetManaHeart(ManaHeartType);

            return manaHeart?.IsMaxLevel() ?? false;
        }

        public bool IsAllMaxLevelManaHearts()
        {
            foreach (var ManaHeart in GetManaHearts())
            {
                if (ManaHeart.IsMaxLevel() == false)
                    return false;
            }

            return true;
        }

        public bool UpgradeManaHeart()
        {
            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(mManaHeartStep);
            if (manaHeartStepData == null)
                return false;

            if (manaHeartStepData.maxUpgradeStep > mUpgradeStep)
                return false;

            NextStep();

            SetIsChangedData(true);

            return true;
        }

        public bool UpgradeManaHeartInst()
        {
            var manaHeartStepData = Lance.GameData.ManaHeartStepData.TryGet(mManaHeartStep);
            if (manaHeartStepData == null)
                return false;

            var stepData = Lance.GameData.ManaHeartUpgradeStepData.TryGet(mUpgradeStep);
            if (stepData == null)
                return false;

            if (manaHeartStepData.maxUpgradeStep < mUpgradeStep)
                return false;

            var manaHeartInst = GetManaHeart(stepData.type);
            if (manaHeartInst == null)
                return false;

            manaHeartInst.LevelUp(1);

            NextUpgradeStep();

            SetIsChangedData(true);

            return true;
        }

        public int GetStep()
        {
            return mManaHeartStep;
        }

        public int GetUpgradeStep()
        {
            return mUpgradeStep;
        }

        void NextStep()
        {
            mManaHeartStep += 1;

            mManaHeartStep = Math.Min(mManaHeartStep, DataUtil.GetManaHeartMaxStep());
        }

        void NextUpgradeStep()
        {
            mUpgradeStep += 1;

            mUpgradeStep = Math.Min(mUpgradeStep, DataUtil.GetManaHeartMaxUpgradeStep() + 1);
        }

        public int GetUpgradeRequire()
        {
            var stepData = Lance.GameData.ManaHeartUpgradeStepData.TryGet(mUpgradeStep);
            if (stepData == null)
                return int.MaxValue;

            return GetUpgradeRequire(stepData.type);
        }

        public int GetUpgradeRequire(ManaHeartType ManaHeartType)
        {
            var manaHeart = GetManaHeart(ManaHeartType);

            return manaHeart?.GetUpgradeRequire() ?? 0;
        }

        public double GetStatValue(StatType statType)
        {
            double total = 0;

            foreach (var manaHeart in GetManaHearts())
            {
                if (manaHeart.GetStatType() == statType)
                    total += manaHeart.GetStatValue();
            }

            return total;
        }

        public double GetStatValue(ManaHeartType ManaHeartType)
        {
            var manaHeart = GetManaHeart(ManaHeartType);

            return manaHeart?.GetStatValue() ?? 0;
        }

        public double GetNextStatValue(ManaHeartType ManaHeartType)
        {
            var manaHeart = GetManaHeart(ManaHeartType);

            return manaHeart?.GetNextStatValue() ?? 0;
        }
    }
}