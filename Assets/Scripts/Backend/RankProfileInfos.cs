using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;


namespace Lance
{
    public class RankProfile_EquipmentInfo
    {
        public string Id;
        public int Level;
        public int ReforgeStep;

        ObscuredString mId;
        ObscuredInt mLevel;
        ObscuredInt mReforgeStep;

        public void Init(string id, int level, int reforgeStep)
        {
            mId = id;
            mLevel = level;
            mReforgeStep = reforgeStep;
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            ReforgeStep = mReforgeStep;
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mReforgeStep.RandomizeCryptoKey();
        }

        public string GetId()
        {
            return mId;
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public int GetReforgeStep()
        {
            return mReforgeStep;
        }
    }

    public class RankProfile_AccessoryInfo
    {
        public string Id;
        public int Level;
        public int ReforgeStep;

        ObscuredString mId;
        ObscuredInt mLevel;
        ObscuredInt mReforgeStep;

        public void Init(string id, int level, int reforgeStep)
        {
            mId = id;
            mLevel = level;
            mReforgeStep = reforgeStep;
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            ReforgeStep = mReforgeStep;
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mReforgeStep.RandomizeCryptoKey();
        }

        public string GetId()
        {
            return mId;
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public int GetReforgeStep()
        {
            return mReforgeStep;
        }
    }

    public class RankProfile_PetInfo
    {
        public string Id;
        public int Level;
        public int Step;

        ObscuredString mId;
        ObscuredInt mLevel;
        ObscuredInt mStep;

        public void Init(string id, int level, int step)
        {
            mId = id;
            mLevel = level;
            mStep = step;
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            Step = mStep;
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mStep.RandomizeCryptoKey();
        }

        public string GetId()
        {
            return mId;
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public int GetStep()
        {
            return mStep;
        }
    }

    public class RankProfile_StatInfo
    {
        public int StatType;
        public double StatValue;

        ObscuredInt mStatType;
        ObscuredDouble mStatValue;

        public void Init(int statType, double statValue)
        {
            mStatType = statType;
            mStatValue = statValue;
        }

        public void Init(StatType statType, double statValue)
        {
            mStatType = (int)statType;
            mStatValue = statValue;
        }

        public void ReadyToSave()
        {
            StatType = mStatType;
            StatValue = mStatValue;
        }

        public void RandomizeKey()
        {
            mStatType.RandomizeCryptoKey();
            mStatValue.RandomizeCryptoKey();
        }

        public StatType GetStatType()
        {
            return (StatType)((int)mStatType);
        }

        public double GetStatValue()
        {
            return mStatValue;
        }
    }
}