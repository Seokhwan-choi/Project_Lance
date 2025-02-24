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
    public class ExcaliburForceInst
    {
        public int Type;
        public int Level;
        public int Step;

        ObscuredInt mType;       // 아이디
        ObscuredInt mLevel;       // 레벨
        ObscuredInt mStep;
        public ExcaliburForceInst(ExcaliburForceType type, int level, int step)
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

        public void SetLevel(int level)
        {
            mLevel = level;
        }

        public int GetStep()
        {
            return mStep;
        }

        public void SetStep(int step)
        {
            mStep = step;
        }

        public ExcaliburForceType GetExcaliburForceType()
        {
            return (ExcaliburForceType)(int)mType;
        }

        public int GetMaxLevel()
        {
            ExcaliburForceType type = GetExcaliburForceType();

            var stepData = DataUtil.GetExcaliburForceStepData(type, mStep);

            return stepData?.maxLevel ?? 0;
        }

        public int GetMaxStep()
        {
            ExcaliburForceType type = GetExcaliburForceType();

            return DataUtil.GetExcaliburForceMaxStep(type);
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

        public int GetUpgradeRequire()
        {
            ExcaliburForceType type = GetExcaliburForceType();

            ExcaliburUpgradeData upgradeData = Lance.GameData.ExcaliburUpgradeData.TryGet(type);
            if (upgradeData == null)
                return int.MaxValue;

            if (upgradeData.require.Length <= mStep)
                return int.MaxValue;

            return upgradeData.require[mStep];
        }

        public int GetTotalUsedAncientEssence()
        {
            return DataUtil.GetTotalUsedAncientEssence(GetExcaliburForceType(), mStep, mLevel);
        }

        public (StatType type, double value) GetStatValueNType()
        {
            ExcaliburForceType type = GetExcaliburForceType();

            var data = Lance.GameData.ExcaliburData.TryGet(type);
            if (data == null)
                return (StatType.Atk, 0);

            return (data.valueType, DataUtil.GetExcaliburForceStatValue(type, mStep, mLevel));
        }

        public double GetStatValue()
        {
            ExcaliburForceType type = GetExcaliburForceType();

            return DataUtil.GetExcaliburForceStatValue(type, mStep, mLevel);
        }

        public double GetNextStatValue()
        {
            ExcaliburForceType type = GetExcaliburForceType();

            return DataUtil.GetExcaliburForceStatValue(type, mStep, mLevel + 1);
        }
    }

    public class Excalibur : AccountBase
    {
        ObscuredInt mStep;
        Dictionary<ExcaliburForceType, ExcaliburForceInst> mExcaliburForceInsts;
        protected override void InitializeData()
        {
            mStep = 0;

            mExcaliburForceInsts = new Dictionary<ExcaliburForceType, ExcaliburForceInst>();

            foreach (var data in Lance.GameData.ExcaliburData.Values)
            {
                var excaliburForceInst = new ExcaliburForceInst(data.type, 0, 0);

                mExcaliburForceInsts.Add(data.type, excaliburForceInst);
            }
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mExcaliburForceInsts = new Dictionary<ExcaliburForceType, ExcaliburForceInst>();

            var excaliburJsonDatas = gameDataJson["ExcaliburForceInsts"];

            for (int i = 0; i < excaliburJsonDatas.Count; i++)
            {
                var excaliburJsonData = excaliburJsonDatas[i];

                int typeTemp = 0;

                int.TryParse(excaliburJsonData["Type"].ToString(), out typeTemp);

                ExcaliburForceType type = (ExcaliburForceType)typeTemp;

                var excaliburData = Lance.GameData.ExcaliburData.TryGet(type);
                if (excaliburData == null)
                    continue;

                int levelTemp = 0;

                int.TryParse(excaliburJsonData["Level"].ToString(), out levelTemp);

                int stepTemp = 0;

                int.TryParse(excaliburJsonData["Step"].ToString(), out stepTemp);

                ExcaliburForceInst excaliburInst = new ExcaliburForceInst(type, levelTemp, stepTemp);

                mExcaliburForceInsts.Add(type, excaliburInst);
            }

            if (gameDataJson.ContainsKey("Step"))
            {
                int stepTemp = 0;

                int.TryParse(gameDataJson["Step"].ToString(), out stepTemp);

                mStep = stepTemp;
            }
        }

        public override Param GetParam()
        {
            foreach (var excalibur in mExcaliburForceInsts.Values)
            {
                excalibur.ReadyToSave();
            }

            Param param = new Param();

            param.Add("Step", (int)mStep);
            param.Add("ExcaliburForceInsts", mExcaliburForceInsts);

            return param;
        }

        public override string GetTableName()
        {
            return "Excalibur";
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mStep.RandomizeCryptoKey();

            foreach (var excaliburForce in GetExcaliburForces())
            {
                excaliburForce.RandomizeKey();
            }
        }

        public ExcaliburForceInst GetExcaliburForce(ExcaliburForceType type)
        {
            return mExcaliburForceInsts.TryGet(type);
        }

        public IEnumerable<ExcaliburForceInst> GetExcaliburForces()
        {
            return mExcaliburForceInsts.Values;
        }

        public int GetLevel(ExcaliburForceType excaliburForceType)
        {
            return GetExcaliburForce(excaliburForceType)?.GetLevel() ?? 0;
        }

        public int GetStep()
        {
            return mStep;
        }

        public void SetStep(int step)
        {
            mStep = step;

            SetIsChangedData(true);
        }

        public bool AnyForceUpgraded()
        {
            foreach(var inst in mExcaliburForceInsts.Values)
            {
                if (inst.GetLevel() > 0)
                    return true;
            }

            return false;
        }

        public int GetExcaliburForceStep(ExcaliburForceType excaliburForceType)
        {
            return GetExcaliburForce(excaliburForceType)?.GetStep() ?? 0;
        }

        public bool IsMaxLevelExcaliburForce(ExcaliburForceType excaliburForceType)
        {
            var excaliburForce = GetExcaliburForce(excaliburForceType);

            return excaliburForce?.IsMaxLevel() ?? false;
        }

        public bool IsMaxStepExcalibur()
        {
            return mStep == DataUtil.GetExcaliburMaxStep();
        }

        public bool IsMaxStepExcaliburForce(ExcaliburForceType excaliburForceType)
        {
            var excaliburForce = GetExcaliburForce(excaliburForceType);

            return excaliburForce?.IsMaxStep() ?? false;
        }

        public bool IsAllMaxLevelExcaliburForces()
        {
            foreach (var excaliburForce in GetExcaliburForces())
            {
                if (excaliburForce.IsMaxLevel() == false)
                    return false;
            }

            return true;
        }

        public bool UpgradeExcalibur()
        {
            if (IsAllMaxLevelExcaliburForces() == false)
                return false;

            if (IsMaxStepExcalibur())
                return false;

            foreach (var excaliburForce in GetExcaliburForces())
            {
                excaliburForce.NextStep();
            }

            NextStep();

            SetIsChangedData(true);

            return true;
        }

        void NextStep()
        {
            mStep += 1;

            mStep = Math.Min(mStep, DataUtil.GetExcaliburMaxStep());
        }

        public bool UpgradeExcaliburForce(ExcaliburForceType excaliburForceType)
        {
            var upgradeExcaliburForce = GetExcaliburForce(excaliburForceType);
            if (upgradeExcaliburForce == null)
                return false;

            upgradeExcaliburForce.LevelUp(1);

            SetIsChangedData(true);

            return true;
        }

        public int GetUpgradeRequire(ExcaliburForceType excaliburForceType)
        {
            var excaliburForce = GetExcaliburForce(excaliburForceType);

            return excaliburForce?.GetUpgradeRequire() ?? 0;
        }

        public double GetStatValue(StatType statType)
        {
            double total = 0;

            foreach (var excaliburForce in GetExcaliburForces())
            {
                var result = excaliburForce.GetStatValueNType();
                if (result.type == statType)
                {
                    total += result.value;
                }
            }

            return total;
        }

        public double GetStatValue(ExcaliburForceType excaliburForceType)
        {
            var excaliburForce = GetExcaliburForce(excaliburForceType);

            return excaliburForce?.GetStatValue() ?? 0;
        }

        public double GetNextStatValue(ExcaliburForceType excaliburForceType)
        {
            var excaliburForce = GetExcaliburForce(excaliburForceType);

            return excaliburForce?.GetNextStatValue() ?? 0;
        }
    }
}