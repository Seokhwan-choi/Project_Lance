using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    // 장비 정보
    public class EssenceInst
    {
        public int Type;
        public int Level;
        public int Step;

        ObscuredInt mType;       // 아이디
        ObscuredInt mLevel;       // 레벨
        ObscuredInt mStep;
        public EssenceInst(EssenceType type, int level, int step)
        {
            mType = (int)type;
            mLevel = level;
            mStep = step;
        }

        public void ReadyToSave()
        {
            Type = (int)mType;
            Level = mLevel;
            Step = mStep;
        }

        public void RandomizeKey()
        {
            mType.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mStep.RandomizeCryptoKey();
        }

        public ObscuredInt GetLevel()
        {
            return mLevel;
        }

        public int GetStep()
        {
            return mStep;
        }

        public EssenceType GetEssenceType()
        {
            return (EssenceType)(int)mType;
        }

        public int GetMaxLevel()
        {
            EssenceData data = Lance.GameData.EssenceData.TryGet(GetEssenceType());
            if (data == null)
                return 0;

            if (data.type == EssenceType.Central)
                return 0;

            var stepData = DataUtil.GetEssenceStepData(data.type, mStep);

            return stepData?.maxLevel ?? 0;
        }

        public int GetMaxStep()
        {
            EssenceData data = Lance.GameData.EssenceData.TryGet(GetEssenceType());
            if (data == null)
                return 0;

            return DataUtil.GetEssenceMaxStep(data.type);
        }

        public bool IsMaxStep()
        {
            return mStep == GetMaxStep();
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

        public void NextStep()
        {
            mStep += 1;

            mStep = Math.Min(mStep, GetMaxStep());
        }

        public int GetUpgradeRequireElements()
        {
            EssenceData data = Lance.GameData.EssenceData.TryGet(GetEssenceType());
            if (data == null)
                return int.MaxValue;

            EssenceUpgradeData upgradeData = Lance.GameData.EssenceUpgradeData.TryGet(data.type);
            if (upgradeData == null)
                return int.MaxValue;

            if (upgradeData.require.Length <= mLevel)
                return int.MaxValue;

            return upgradeData.require[mLevel];
        }

        public (StatType type, double value) GetStatValueNType()
        {
            EssenceData data = Lance.GameData.EssenceData.TryGet(GetEssenceType());
            if (data == null)
                return (StatType.AtkRatio, 0);

            return (data.valueType, DataUtil.GetEssenceStatValue(data.type, mStep, mLevel));
        }

        public double GetStatValue()
        {
            EssenceData data = Lance.GameData.EssenceData.TryGet(GetEssenceType());
            if (data == null)
                return 0;

            return DataUtil.GetEssenceStatValue(data.type, mStep, mLevel);
        }

        public double GetNextStatValue()
        {
            EssenceData data = Lance.GameData.EssenceData.TryGet(GetEssenceType());
            if (data == null)
                return 0;

            if (data.type == EssenceType.Central)
            {
                return DataUtil.GetEssenceStatValue(data.type, mStep + 1, mLevel + 1);
            }
            else
            {
                return DataUtil.GetEssenceStatValue(data.type, mStep, mLevel + 1);
            }
        }
    }

    public class Essence : AccountBase
    {
        ObscuredInt mStackedActiveCount;    // 태초의 정수 누적 활성 횟수
        ObscuredFloat mDurationTime;        // 태초의 정수 활성 시간
        DailyCounter mDailyActiveCounter;   // 태초의 정수 일일 활성 횟수 제한
        Dictionary<EssenceType, EssenceInst> mEssenceInsts;
        protected override void InitializeData()
        {
            mEssenceInsts = new Dictionary<EssenceType, EssenceInst>();

            foreach (var data in Lance.GameData.EssenceData.Values)
            {
                var essenceInst = new EssenceInst(data.type, 0, 0);

                mEssenceInsts.Add(data.type, essenceInst);
            }

            mDailyActiveCounter = new DailyCounter();
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mEssenceInsts = new Dictionary<EssenceType, EssenceInst>();

            var essenceJsonDatas = gameDataJson["EssenceInsts"];

            for (int i = 0; i < essenceJsonDatas.Count; i++)
            {
                var essenceJsonData = essenceJsonDatas[i];

                int typeTemp = 0;

                int.TryParse(essenceJsonData["Type"].ToString(), out typeTemp);

                EssenceType type = (EssenceType)typeTemp;

                var essenceData = Lance.GameData.EssenceData.TryGet(type);
                if (essenceData == null)
                    continue;

                int levelTemp = 0;

                int.TryParse(essenceJsonData["Level"].ToString(), out levelTemp);

                int stepTemp = 0;

                int.TryParse(essenceJsonData["Step"].ToString(), out stepTemp);

                EssenceInst essenceInst = new EssenceInst(type, levelTemp, stepTemp);

                mEssenceInsts.Add(type, essenceInst);
            }

            if (gameDataJson.ContainsKey("StackedActiveCount"))
            {
                int stackedActiveCountTemp = 0;

                int.TryParse(gameDataJson["StackedActiveCount"].ToString(), out stackedActiveCountTemp);

                mStackedActiveCount = stackedActiveCountTemp;
            }

            if (gameDataJson.ContainsKey("DurationTime"))
            {
                float durationTimeTemp = 0;

                float.TryParse(gameDataJson["DurationTime"].ToString(), out durationTimeTemp);

                mDurationTime = durationTimeTemp;
            }

            mDailyActiveCounter = new DailyCounter();
            if (gameDataJson.ContainsKey("DailyActiveCounter"))
            {
                mDailyActiveCounter.SetServerDataToLocal(gameDataJson["DailyActiveCounter"]);
            }
        }

        public override Param GetParam()
        {
            foreach (var essence in mEssenceInsts.Values)
            {
                essence.ReadyToSave();
            }

            mDailyActiveCounter.ReadyToSave();

            Param param = new Param();

            param.Add("EssenceInsts", mEssenceInsts);
            param.Add("StackedActiveCount", (int)mStackedActiveCount);
            param.Add("DurationTime", (float)mDurationTime);
            param.Add("DailyActiveCounter", mDailyActiveCounter);

            return param;
        }

        public override string GetTableName()
        {
            return "Essence";
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var essence in GetEssences())
            {
                essence.RandomizeKey();
            }
        }

        public EssenceInst GetEssence(EssenceType type)
        {
            return mEssenceInsts.TryGet(type);
        }

        public IEnumerable<EssenceInst> GetEssences()
        {
            return mEssenceInsts.Values;
        }

        public int GetLevel(EssenceType essenceType)
        {
            return GetEssence(essenceType)?.GetLevel() ?? 0;
        }

        public int GetStep(EssenceType essenceType)
        {
            return GetEssence(essenceType)?.GetStep() ?? 0;
        }

        public bool IsMaxLevel(EssenceType essenceType)
        {
            var essence = GetEssence(essenceType);

            return essence?.IsMaxLevel() ?? false;
        }

        public bool IsMaxStep(EssenceType essenceType)
        {
            var essence = GetEssence(essenceType);

            return essence?.IsMaxStep() ?? false;
        }

        public bool UpgradeEssence(EssenceType essenceType)
        {
            var upgradeEssence = GetEssence(essenceType);
            if (upgradeEssence == null)
                return false;

            if (essenceType == EssenceType.Central)
            {
                foreach(var essence in GetEssences())
                {
                    essence.NextStep();
                }

                SetIsChangedData(true);

                return true;
            }
            else
            {
                upgradeEssence.LevelUp(1);

                SetIsChangedData(true);

                return true;
            }
        }

        public int GetUpgradeRequireElements(EssenceType essenceType)
        {
            var essence = GetEssence(essenceType);

            return essence?.GetUpgradeRequireElements() ?? 0;
        }

        public double GetStatValue(StatType statType)
        {
            double total = 0;

            foreach(var essence in GetEssences())
            {
                var result = essence.GetStatValueNType();
                if (result.type == statType)
                {
                    if (essence.GetEssenceType() == EssenceType.Central)
                    {
                        if (IsActiveCentralEssence())
                        {
                            total += result.value * Lance.GameData.EssenceCommonData.centralEssenceActiveValue;

                            continue;
                        }
                    }

                    total += result.value;
                }
            }

            return total;
        }

        public double GetStatValue(EssenceType essenceType)
        {
            var essence = GetEssence(essenceType);

            double statValue = essence?.GetStatValue() ?? 0;

            if (essenceType == EssenceType.Central)
            {
                if (IsActiveCentralEssence())
                {
                    return statValue * Lance.GameData.EssenceCommonData.centralEssenceActiveValue;
                }
            }

            return statValue;
        }

        public double GetNextStatValue(EssenceType essenceType)
        {
            var essence = GetEssence(essenceType);

            return essence?.GetNextStatValue() ?? 0;
        }

        public float GetDurationTime()
        {
            return mDurationTime;
        }

        public void OnUpdate(float time)
        {
            if (mDurationTime > 0f)
            {
                mDurationTime -= time;

                if (mDurationTime <= 0f)
                {
                    SetIsChangedData(true);
                }
            }
        }

        public int GetRemainActiveCount()
        {
            return mDailyActiveCounter.GetRemainCount(Lance.GameData.EssenceCommonData.centralEssenceDailyLimitCount);
        }

        public bool IsActiveCentralEssence()
        {
            return mDurationTime > 0;
        }

        public bool ActiveCentralEssence()
        {
            var essence = GetEssence(EssenceType.Central);
            int step = essence?.GetStep() ?? 0;

            var data = Lance.GameData.CentralEssenceActiveRequireData.TryGet(step);
            if (data == null)
                return false;

            if (data.requireAllEssenceAmount <= 0)
                return false;

            if (mDailyActiveCounter.StackCount(Lance.GameData.EssenceCommonData.centralEssenceDailyLimitCount))
            {
                mDurationTime = Lance.GameData.EssenceCommonData.centralEssenceDurationTime;

                mStackedActiveCount += 1;

                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public void Reset()
        {
            InitializeData();
        }
    }
}