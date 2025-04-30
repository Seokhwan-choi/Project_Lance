using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class GoldTrainLevel : AccountBase
    {
        ObscuredInt mAtkLevel;
        ObscuredInt mHpLevel;
        ObscuredInt mCriProbLevel;
        ObscuredInt mCriDmgLevel;
        ObscuredInt mSuperCriProbLevel;
        ObscuredInt mSuperCriDmgLevel;
        ObscuredInt mPowerAtkLevel;
        ObscuredInt mPowerHpLevel;

        ObscuredInt mAmplifyAtkLevel;
        ObscuredInt mAmplifyHpLevel;
        ObscuredInt mGoldAmountLevel;
        ObscuredInt mExpAmountLevel;
        ObscuredInt mFireAddDmgLevel;
        ObscuredInt mWaterAddDmgLevel;
        ObscuredInt mGrassAddDmgLevel;

        ObscuredBool mReworkAtkNHp;

        public int GetTotalTrainLevel()
        {
            return mAtkLevel + mHpLevel +
                mCriProbLevel + mCriDmgLevel +
                mSuperCriProbLevel + mSuperCriDmgLevel +
                mPowerAtkLevel + mPowerHpLevel +
                mAmplifyAtkLevel + mAmplifyHpLevel +
                mGoldAmountLevel + mExpAmountLevel +
                mFireAddDmgLevel + mWaterAddDmgLevel + mGrassAddDmgLevel;
        }
        public int GetTrainLevel(StatType type)
        {
            switch (type)
            {
                case StatType.Atk:
                    return mAtkLevel;
                case StatType.Hp:
                    return mHpLevel;
                case StatType.CriProb:
                    return mCriProbLevel;
                case StatType.CriDmg:
                    return mCriDmgLevel;
                case StatType.SuperCriProb:
                    return mSuperCriProbLevel;
                case StatType.SuperCriDmg:
                    return mSuperCriDmgLevel;
                case StatType.PowerAtk:
                    return mPowerAtkLevel;
                case StatType.PowerHp:
                    return mPowerHpLevel;
                case StatType.AmplifyAtk:
                    return mAmplifyAtkLevel;
                case StatType.AmplifyHp:
                    return mAmplifyHpLevel;
                case StatType.GoldAmount:
                    return mGoldAmountLevel;
                case StatType.ExpAmount:
                    return mExpAmountLevel;
                case StatType.FireAddDmg:
                    return mFireAddDmgLevel;
                case StatType.WaterAddDmg:
                    return mWaterAddDmgLevel;
                case StatType.GrassAddDmg:
                    return mGrassAddDmgLevel;
                default:
                    return 0;
            }
        }

        public bool Train(StatType type, int trainCount = 1)
        {
            // 외부에서 확인했으니까 그냥 실행해주자
            switch(type)
            {
                case StatType.Atk:
                    mAtkLevel += trainCount;
                    break;
                case StatType.Hp:
                    mHpLevel += trainCount;
                    break;
                case StatType.CriProb:
                    mCriProbLevel += trainCount;
                    break;
                case StatType.CriDmg:
                    mCriDmgLevel += trainCount;
                    break;
                case StatType.SuperCriProb:
                    mSuperCriProbLevel += trainCount;
                    break;
                case StatType.SuperCriDmg:
                    mSuperCriDmgLevel += trainCount;
                    break;
                case StatType.PowerAtk:
                    mPowerAtkLevel += trainCount;
                    break;
                case StatType.PowerHp:
                    mPowerHpLevel += trainCount;
                    break;
                case StatType.AmplifyAtk:
                    mAmplifyAtkLevel += trainCount;
                    break;
                case StatType.AmplifyHp:
                    mAmplifyHpLevel += trainCount;
                    break;
                case StatType.GoldAmount:
                    mGoldAmountLevel += trainCount;
                    break;
                case StatType.ExpAmount:
                    mExpAmountLevel += trainCount;
                    break;
                case StatType.FireAddDmg:
                    mFireAddDmgLevel += trainCount;
                    break;
                case StatType.WaterAddDmg:
                    mWaterAddDmgLevel += trainCount;
                    break;
                case StatType.GrassAddDmg:
                    mGrassAddDmgLevel += trainCount;
                    break;
            }

            SetIsChangedData(true);

            return false;
        }

#if UNITY_EDITOR
        public bool TestTrain(StatType type, int trainCount = 1)
        {
            // 외부에서 확인했으니까 그냥 실행해주자
            switch (type)
            {
                case StatType.Atk:
                    mAtkLevel = trainCount;
                    break;
                case StatType.Hp:
                    mHpLevel = trainCount;
                    break;
                case StatType.CriProb:
                    mCriProbLevel = trainCount;
                    break;
                case StatType.CriDmg:
                    mCriDmgLevel = trainCount;
                    break;
                case StatType.SuperCriProb:
                    mSuperCriProbLevel = trainCount;
                    break;
                case StatType.SuperCriDmg:
                    mSuperCriDmgLevel = trainCount;
                    break;
                case StatType.PowerAtk:
                    mPowerAtkLevel = trainCount;
                    break;
                case StatType.PowerHp:
                    mPowerHpLevel = trainCount;
                    break;
            }

            SetIsChangedData(true);

            return false;
        }
#endif

        public (bool canTrain, double totalRequireGold) CanTrain(StatType type, int trainCount = 1)
        {
            int curLevel = GetTrainLevel(type);

            var result = DataUtil.GetGoldTrainTotalRequireGold(type, curLevel, trainCount);

            return result;
        }

        public double GetTrainStatValue(StatType type)
        {
            return DataUtil.GetGoldTrainStatValue(type, GetTrainLevel(type));
        }

        public override string GetTableName()
        {
            return "GoldTrainLevel";
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            mReworkAtkNHp = true;
        }

        public override Param GetParam()
        {
            var param = new Param();
            param.Add("AtkLevel", (int)mAtkLevel);
            param.Add("HpLevel", (int)mHpLevel);
            param.Add("CriProbLevel", (int)mCriProbLevel);
            param.Add("CriDmgLevel", (int)mCriDmgLevel);
            param.Add("SuperCriProbLevel", (int)mSuperCriProbLevel);
            param.Add("SuperCriDmgLevel", (int)mSuperCriDmgLevel);
            param.Add("PowerAtkLevel", (int)mPowerAtkLevel);
            param.Add("PowerHpLevel", (int)mPowerHpLevel);
            param.Add("AmplifyAtkLevel", (int)mAmplifyAtkLevel);
            param.Add("AmplifyHpLevel", (int)mAmplifyHpLevel);
            param.Add("GoldAmountLevel", (int)mGoldAmountLevel);
            param.Add("ExpAmountLevel", (int)mExpAmountLevel);
            param.Add("FireAddDmgLevel", (int)mFireAddDmgLevel);
            param.Add("WaterAddDmgLevel", (int)mWaterAddDmgLevel);
            param.Add("GrassAddDmgLevel", (int)mGrassAddDmgLevel);
            param.Add("ReworkAtkNHp", (bool)mReworkAtkNHp);
            
            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int atkLevelTemp = 0;
            int.TryParse(gameDataJson["AtkLevel"].ToString(), out atkLevelTemp);
            mAtkLevel = atkLevelTemp;

            int hpLevelTemp = 0;
            int.TryParse(gameDataJson["HpLevel"].ToString(), out hpLevelTemp);
            mHpLevel = hpLevelTemp;

            int cripProbLevelTemp = 0;
            int.TryParse(gameDataJson["CriProbLevel"].ToString(), out cripProbLevelTemp);
            mCriProbLevel = cripProbLevelTemp;

            int criDmgLevelTemp = 0;
            int.TryParse(gameDataJson["CriDmgLevel"].ToString(), out criDmgLevelTemp);
            mCriDmgLevel = criDmgLevelTemp;

            if (gameDataJson.ContainsKey("SuperCriProbLevel"))
            {
                int superCripProbLevelTemp = 0;
                int.TryParse(gameDataJson["SuperCriProbLevel"].ToString(), out superCripProbLevelTemp);
                mSuperCriProbLevel = superCripProbLevelTemp;
            }

            if (gameDataJson.ContainsKey("SuperCriDmgLevel"))
            {
                int superCripDmgLevelTemp = 0;
                int.TryParse(gameDataJson["SuperCriDmgLevel"].ToString(), out superCripDmgLevelTemp);
                mSuperCriDmgLevel = superCripDmgLevelTemp;
            }

            if (gameDataJson.ContainsKey("PowerAtkLevel"))
            {
                int powerAtkLevelTemp = 0;
                int.TryParse(gameDataJson["PowerAtkLevel"].ToString(), out powerAtkLevelTemp);
                mPowerAtkLevel = powerAtkLevelTemp;
            }

            if (gameDataJson.ContainsKey("PowerHpLevel"))
            {
                int powerHpLevelTemp = 0;
                int.TryParse(gameDataJson["PowerHpLevel"].ToString(), out powerHpLevelTemp);
                mPowerHpLevel = powerHpLevelTemp;
            }

            if (gameDataJson.ContainsKey("AmplifyAtkLevel"))
            {
                int amplifyAtkLevelTemp = 0;
                int.TryParse(gameDataJson["AmplifyAtkLevel"].ToString(), out amplifyAtkLevelTemp);
                mAmplifyAtkLevel = amplifyAtkLevelTemp;
            }

            if (gameDataJson.ContainsKey("AmplifyHpLevel"))
            {
                int amplifyHpLevelTemp = 0;
                int.TryParse(gameDataJson["AmplifyHpLevel"].ToString(), out amplifyHpLevelTemp);
                mAmplifyHpLevel = amplifyHpLevelTemp;
            }

            if (gameDataJson.ContainsKey("GoldAmountLevel"))
            {
                int goldAmountLevelTemp = 0;
                int.TryParse(gameDataJson["GoldAmountLevel"].ToString(), out goldAmountLevelTemp);
                mGoldAmountLevel = goldAmountLevelTemp;
            }

            if (gameDataJson.ContainsKey("ExpAmountLevel"))
            {
                int expAmountLevelTemp = 0;
                int.TryParse(gameDataJson["ExpAmountLevel"].ToString(), out expAmountLevelTemp);
                mExpAmountLevel = expAmountLevelTemp;
            }

            if (gameDataJson.ContainsKey("FireAddDmgLevel"))
            {
                int fireAddDmgLevelTemp = 0;
                int.TryParse(gameDataJson["FireAddDmgLevel"].ToString(), out fireAddDmgLevelTemp);
                mFireAddDmgLevel = fireAddDmgLevelTemp;
            }

            if (gameDataJson.ContainsKey("WaterAddDmgLevel"))
            {
                int waterAddDmgLevelTemp = 0;
                int.TryParse(gameDataJson["WaterAddDmgLevel"].ToString(), out waterAddDmgLevelTemp);
                mWaterAddDmgLevel = waterAddDmgLevelTemp;
            }

            if (gameDataJson.ContainsKey("GrassAddDmgLevel"))
            {
                int grassAddDmgLevelTemp = 0;
                int.TryParse(gameDataJson["GrassAddDmgLevel"].ToString(), out grassAddDmgLevelTemp);
                mGrassAddDmgLevel = grassAddDmgLevelTemp;
            }

            mReworkAtkNHp = false;

            if (gameDataJson.ContainsKey("ReworkAtkNHp"))
            {
                bool reworkAtkNHpTemp = false;

                bool.TryParse(gameDataJson["ReworkAtkNHp"].ToString(), out reworkAtkNHpTemp);

                mReworkAtkNHp = reworkAtkNHpTemp;
            }

            if (mReworkAtkNHp == false)
            {
                mReworkAtkNHp = true;

                mAtkLevel = Mathf.RoundToInt(mAtkLevel / 10f);
                mHpLevel = Mathf.RoundToInt(mHpLevel / 10f);

                SetIsChangedData(true);
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mAtkLevel.RandomizeCryptoKey();
            mHpLevel.RandomizeCryptoKey();
            mCriProbLevel.RandomizeCryptoKey();
            mCriDmgLevel.RandomizeCryptoKey();
            mSuperCriProbLevel.RandomizeCryptoKey();
            mSuperCriDmgLevel.RandomizeCryptoKey();
            mPowerAtkLevel.RandomizeCryptoKey();
            mPowerHpLevel.RandomizeCryptoKey();
            mAmplifyAtkLevel.RandomizeCryptoKey();
            mAmplifyHpLevel.RandomizeCryptoKey();
            mGoldAmountLevel.RandomizeCryptoKey();
            mExpAmountLevel.RandomizeCryptoKey();
            mFireAddDmgLevel.RandomizeCryptoKey();
            mWaterAddDmgLevel.RandomizeCryptoKey();
            mGrassAddDmgLevel.RandomizeCryptoKey();
        }

        public bool IsMaxLevel(StatType type)
        {
            int maxLevel = DataUtil.GetGoldTrainMaxLevel(type);

            int level = GetTrainLevel(type);

            return level == maxLevel;
        }

        public bool IsSatisfiedRequireType(StatType type)
        {
            var goldTrainData = Lance.GameData.GoldTrainData.TryGet(type);
            if (goldTrainData != null)
            {
                int level = GetTrainLevel(goldTrainData.requireType);

                return level >= goldTrainData.requireLevel;
            }

            return false;
        }
    }
}