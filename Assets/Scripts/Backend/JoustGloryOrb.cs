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
    public class JoustGloryOrbInst
    {
        public int Type;
        public int Level;

        JoustingGloryOrbData mData;
        ObscuredInt mType;       // 아이디
        ObscuredInt mLevel;       // 레벨
        public JoustGloryOrbInst(JoustGloryOrbType type, int level)
        {
            mType = (int)type;
            mLevel = level;
            mData = Lance.GameData.JoustingGloryOrbData.TryGet(type);
        }

        public void ReadyToSave()
        {
            Type = (int)mType;
            Level = mLevel;
        }

        public StatType GetStatType()
        {
            return mData.valueType;
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

        public JoustGloryOrbType GetJoustGloryOrbType()
        {
            return (JoustGloryOrbType)(int)mType;
        }

        public int GetMaxLevel()
        {
            JoustGloryOrbType type = GetJoustGloryOrbType();

            return DataUtil.GetJoustGloryOrbMaxLevel(type);
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
            JoustGloryOrbType type = GetJoustGloryOrbType();

            JoustingGloryOrbUpgradeData upgradeData = Lance.GameData.JoustingGloryOrbUpgradeData.TryGet(type);
            if (upgradeData == null)
                return int.MaxValue;

            if (upgradeData.require.Length <= mLevel)
                return int.MaxValue;

            return upgradeData.require[mLevel];
        }

        public (StatType type, double value) GetStatValueNType()
        {
            JoustGloryOrbType type = GetJoustGloryOrbType();

            return (mData.valueType, DataUtil.GetJoustGloryOrbStatValue(type, mLevel));
        }

        public double GetStatValue()
        {
            JoustGloryOrbType type = GetJoustGloryOrbType();

            return DataUtil.GetJoustGloryOrbStatValue(type, mLevel);
        }

        public double GetNextStatValue()
        {
            JoustGloryOrbType type = GetJoustGloryOrbType();

            return DataUtil.GetJoustGloryOrbStatValue(type, mLevel + 1);
        }
    }

    public class JoustGloryOrb : AccountBase
    {
        ObscuredInt mStep;
        Dictionary<JoustGloryOrbType, JoustGloryOrbInst> mJoustGloryOrbInsts;
        protected override void InitializeData()
        {
            mStep = 1;

            mJoustGloryOrbInsts = new Dictionary<JoustGloryOrbType, JoustGloryOrbInst>();

            foreach (var data in Lance.GameData.JoustingGloryOrbData.Values)
            {
                var JoustGloryOrbInst = new JoustGloryOrbInst(data.type, 0);

                mJoustGloryOrbInsts.Add(data.type, JoustGloryOrbInst);
            }
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mJoustGloryOrbInsts = new Dictionary<JoustGloryOrbType, JoustGloryOrbInst>();

            var joustGloryOrbJsonDatas = gameDataJson["JoustGloryOrbInsts"];

            for (int i = 0; i < joustGloryOrbJsonDatas.Count; i++)
            {
                var joustGloryOrbJsonData = joustGloryOrbJsonDatas[i];

                int typeTemp = 0;

                int.TryParse(joustGloryOrbJsonData["Type"].ToString(), out typeTemp);

                JoustGloryOrbType type = (JoustGloryOrbType)typeTemp;

                var joustGloryOrbData = Lance.GameData.JoustingGloryOrbData.TryGet(type);
                if (joustGloryOrbData == null)
                    continue;

                int levelTemp = 0;

                int.TryParse(joustGloryOrbJsonData["Level"].ToString(), out levelTemp);

                JoustGloryOrbInst joustGloryOrbInst = new JoustGloryOrbInst(type, levelTemp);

                mJoustGloryOrbInsts.Add(type, joustGloryOrbInst);
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
            foreach (var joustGloryOrb in mJoustGloryOrbInsts.Values)
            {
                joustGloryOrb.ReadyToSave();
            }

            Param param = new Param();

            param.Add("Step", (int)mStep);
            param.Add("JoustGloryOrbInsts", mJoustGloryOrbInsts);

            return param;
        }

        public override string GetTableName()
        {
            return "JoustGloryOrb";
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mStep.RandomizeCryptoKey();

            foreach (var joustGloryOrb in GetJoustGloryOrbs())
            {
                joustGloryOrb.RandomizeKey();
            }
        }

        public JoustGloryOrbInst GetJoustGloryOrb(JoustGloryOrbType type)
        {
            return mJoustGloryOrbInsts.TryGet(type);
        }

        public IEnumerable<JoustGloryOrbInst> GetJoustGloryOrbs()
        {
            return mJoustGloryOrbInsts.Values;
        }

        public int GetLevel(JoustGloryOrbType JoustGloryOrbType)
        {
            return GetJoustGloryOrb(JoustGloryOrbType)?.GetLevel() ?? 0;
        }

        public bool IsMaxLevelJoustGloryOrb(JoustGloryOrbType JoustGloryOrbType)
        {
            var JoustGloryOrb = GetJoustGloryOrb(JoustGloryOrbType);

            return JoustGloryOrb?.IsMaxLevel() ?? false;
        }

        public bool IsAllMaxLevelJoustGloryOrbs()
        {
            foreach (var JoustGloryOrb in GetJoustGloryOrbs())
            {
                if (JoustGloryOrb.IsMaxLevel() == false)
                    return false;
            }

            return true;
        }

        public bool UpgradeJoustGloryOrb()
        {
            var stepData = Lance.GameData.JoustingGloryOrbStepData.TryGet(mStep);
            if (stepData == null)
                return false;

            var gloryOrb = GetJoustGloryOrb(stepData.type);
            if (gloryOrb == null)
                return false;

            gloryOrb.LevelUp(1);

            NextStep();

            SetIsChangedData(true);

            return true;
        }

        public int GetStep()
        {
            return mStep;
        }

        void NextStep()
        {
            mStep += 1;

            mStep = Math.Min(mStep, DataUtil.GetJoustGloryOrbMaxStep() + 1);
        }

        public bool IsMaxStep()
        {
            return mStep >= DataUtil.GetJoustGloryOrbMaxStep();
        }

        public int GetUpgradeRequire()
        {
            var stepData = Lance.GameData.JoustingGloryOrbStepData.TryGet(mStep);
            if (stepData == null)
                return int.MaxValue;

            return GetUpgradeRequire(stepData.type);
        }

        public int GetUpgradeRequire(JoustGloryOrbType JoustGloryOrbType)
        {
            var JoustGloryOrb = GetJoustGloryOrb(JoustGloryOrbType);

            return JoustGloryOrb?.GetUpgradeRequire() ?? 0;
        }

        public double GetStatValue(StatType statType)
        {
            double total = 0;

            foreach (var joustGloryOrb in GetJoustGloryOrbs())
            {
                if (joustGloryOrb.GetStatType() == statType)
                {
                    total += joustGloryOrb.GetStatValue();
                }
            }

            return total;
        }

        public double GetStatValue(JoustGloryOrbType JoustGloryOrbType)
        {
            var JoustGloryOrb = GetJoustGloryOrb(JoustGloryOrbType);

            return JoustGloryOrb?.GetStatValue() ?? 0;
        }

        public double GetNextStatValue(JoustGloryOrbType JoustGloryOrbType)
        {
            var JoustGloryOrb = GetJoustGloryOrb(JoustGloryOrbType);

            return JoustGloryOrb?.GetNextStatValue() ?? 0;
        }
    }
}