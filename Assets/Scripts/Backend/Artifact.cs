using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;

namespace Lance
{
    public class Artifact : AccountBase
    {
        ObscuredInt mTryUpgradeCount;
        ObscuredInt mUpgradeSuccessCount;
        Dictionary<string, ArtifactInst> mArtifacts;

        public Artifact()
        {
            mArtifacts = new Dictionary<string, ArtifactInst>();
        }

        public bool CanLevelUp(string id)
        {
            var artifactInst = mArtifacts.TryGet(id);

            return artifactInst?.CanLevelUp() ?? false;
        }

        public bool LevelUp(string id)
        {
            if (CanLevelUp(id) == false)
                return false;

            mTryUpgradeCount += 1;

            ArtifactInst artifactInst = mArtifacts.TryGet(id);
            if (artifactInst.LevelUp())
            {
                mUpgradeSuccessCount += 1;

                SetIsChangedData(true);

                return true;
            }
            else
            {
                return false;
            }
        }

        public int DismantleAll()
        {
            int totalDismantleCount = 0;

            if (IsAllArtifactMaxLevel() == false)
                return totalDismantleCount;

            foreach (var artifactInst in mArtifacts.Values)
            {
                if (artifactInst.IsMaxLevel())
                {
                    int count = artifactInst.GetCount();

                    artifactInst.UseCount(count);

                    totalDismantleCount += count;
                }
            }

            if (totalDismantleCount > 0)
                SetIsChangedData(true);

            return totalDismantleCount;
        }

        public int Dismantle(string id)
        {
            ArtifactInst artifactInst = mArtifacts.TryGet(id);
            if (artifactInst != null)
            {
                // 팔려고 하는 유물은 최고레벨이고
                if (artifactInst.IsMaxLevel())
                {
                    // 다른 유물은 모두 최고레벨이면 분해할 수 있음
                    if (IsAllArtifactMaxLevel())
                    {
                        int count = artifactInst.GetCount();

                        artifactInst.UseCount(count);

                        SetIsChangedData(true);

                        return count;
                    }
                }
            }

            return 0;
        }

        public int GetFailedCount(string id)
        {
            ArtifactInst artifactInst = mArtifacts.TryGet(id);
            if (artifactInst != null)
            {
                return artifactInst.GetFailedCount();
            }
            else
            {
                return 0;
            }
        }

        public int SellAll()
        {
            int totalSellCount = 0;

            if (IsAllArtifactMaxLevel())
                return totalSellCount;

            foreach(var artifactInst in mArtifacts.Values)
            {
                if (artifactInst.IsMaxLevel())
                {
                    int count = artifactInst.GetCount();

                    artifactInst.UseCount(count);

                    totalSellCount += count;
                }
            }

            if (totalSellCount > 0)
                SetIsChangedData(true);

            return totalSellCount;
        }

        public int Sell(string id)
        {
            ArtifactInst artifactInst = mArtifacts.TryGet(id);
            if (artifactInst != null)
            {
                // 팔려고 하는 유물은 최고레벨이지만
                if (artifactInst.IsMaxLevel())
                {
                    // 다른 유물은 모두 최고레벨이 아니여야 팔 수 있음
                    if (IsAllArtifactMaxLevel() == false)
                    {
                        int count = artifactInst.GetCount();

                        artifactInst.UseCount(count);

                        SetIsChangedData(true);

                        return count;
                    }
                }
            }

            return 0;
        }

        public void AddArtifact(string id, int count)
        {
            if (mArtifacts.ContainsKey(id))
            {
                var artifactInst = mArtifacts.TryGet(id);

                artifactInst.AddCount(count);
            }
            else
            {
                var inst = new ArtifactInst();

                inst.Init(id, 1, count, 0);

                mArtifacts.Add(id, inst);
            }

            SetIsChangedData(true);
        }

#if UNITY_EDITOR
        public void AddArtifact(string id, int level, int count)
        {
            if (mArtifacts.ContainsKey(id))
            {
                var artifactInst = mArtifacts.TryGet(id);

                artifactInst.SetLevel(level);

                artifactInst.AddCount(count);
            }
            else
            {
                var inst = new ArtifactInst();

                inst.Init(id, level, count, 0);

                mArtifacts.Add(id, inst);
            }

            SetIsChangedData(true);
        }
#endif

        public int GetCount(string id)
        {
            var inst = mArtifacts.TryGet(id);

            return inst?.GetCount() ?? 0;
        }

        public bool IsEnoughCount(string id, int count)
        {
            var inst = mArtifacts.TryGet(id);

            return inst?.IsEnoughCount(count) ?? false;
        }

        public float GetUpgradeProb(string id)
        {
            ArtifactInst inst = mArtifacts.TryGet(id);
            if (inst != null)
            {
                return DataUtil.GetArtifactUpgradeProb(false, inst.GetLevel());
            }

            return 0;
        }

        public int GetLevel(string id)
        {
            ArtifactInst inst = mArtifacts.TryGet(id);

            return inst?.GetLevel() ?? 0;
        }

        public bool IsMaxLevel(string id)
        {
            ArtifactInst inst = mArtifacts.TryGet(id);

            return inst?.IsMaxLevel() ?? false;
        }

        public double GatherStatValues(StatType type)
        {
            double totalStatValue = 0;

            foreach (ArtifactInst artifact in mArtifacts.Values)
            {
                if (artifact.GetStatType() == type)
                    totalStatValue += artifact.GetStatValue();
            }

            return totalStatValue;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var artifact in mArtifacts.Values)
            {
                artifact.RandomizeKey();
            }
        }

        public override string GetTableName()
        {
            return "Artifact";
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var artifactDatas = gameDataJson["Artifacts"];

            for(int i = 0; i < artifactDatas.Count; ++i)
            {
                var artifactJsonData = artifactDatas[i];

                string id = artifactJsonData["Id"].ToString();

                var data = Lance.GameData.ArtifactData.TryGet(id);
                if (data == null)
                    continue;

                int levelTemp = 0;

                int.TryParse(artifactJsonData["Level"].ToString(), out levelTemp);

                int countTemp = 0;

                int.TryParse(artifactJsonData["Count"].ToString(), out countTemp);

                int failedCountTemp = 0;

                if (artifactJsonData.ContainsKey("FailedCount"))
                {
                    int.TryParse(artifactJsonData["FailedCount"].ToString(), out failedCountTemp);
                }

                if (levelTemp > 0)
                {
                    var artifactInst = new ArtifactInst();

                    artifactInst.Init(id, levelTemp, countTemp, failedCountTemp);

                    mArtifacts.Add(id, artifactInst);
                }
            }

            int tryCountTemp = 0;

            int.TryParse(gameDataJson["TryUpgradeCount"].ToString(), out tryCountTemp);

            mTryUpgradeCount = tryCountTemp;

            int successCountTemp = 0;

            int.TryParse(gameDataJson["UpgradeSuccessCount"].ToString(), out successCountTemp);

            mUpgradeSuccessCount = successCountTemp;
        }

        public bool AnyCanTryLevelUp()
        {
            foreach(var artifact in mArtifacts.Values)
            {
                if (artifact.CanLevelUp())
                    return true;
            }

            return false;
        }

        public bool AnyCanSell()
        {
            if (IsAllArtifactMaxLevel())
                return false;

            foreach (ArtifactInst artifact in mArtifacts.Values)
            {
                if (artifact.IsMaxLevel() && artifact.GetCount() > 0)
                    return true;
            }

            return false;
        }

        public bool AnyCanDismantle()
        {
            foreach(ArtifactInst artifact in mArtifacts.Values)
            {
                if (artifact.CanDismantle())
                    return true;
            }

            return false;
        }

        public override Param GetParam()
        {
            Param param = new Param();

            foreach(var artifact in mArtifacts.Values)
            {
                artifact.ReadyToSave();
            }


            param.Add("Artifacts", mArtifacts.Values);
            param.Add("TryUpgradeCount", (int)mTryUpgradeCount);
            param.Add("UpgradeSuccessCount", (int)mUpgradeSuccessCount);

            return param;
        }

        public int GetTryUpgradeCount()
        {
            return mTryUpgradeCount;
        }

        public bool CanDismantle(string id)
        {
            var artifact = mArtifacts.TryGet(id);
            if (artifact == null)
                return false;

            return IsAllArtifactMaxLevel() && artifact.CanDismantle();
        }

        public bool CanSell(string id)
        {
            var artifact = mArtifacts.TryGet(id);
            if (artifact == null)
                return false;

            if (IsAllArtifactMaxLevel())
                return false;

            return artifact.IsMaxLevel() && artifact.GetCount() > 0;
        }

        public bool IsAllArtifactMaxLevel()
        {
            if (mArtifacts.Count <= 0)
                return false;

            foreach(var artifact in mArtifacts.Values)
            {
                if (artifact.IsMaxLevel() == false)
                    return false;
            }

            return true;
        }
    }

    public class ArtifactInst
    {
        public string Id;
        public int Level;
        public int Count;
        public int FailedCount;

        ObscuredString mId;
        ObscuredInt mLevel;
        ObscuredInt mCount;
        ObscuredInt mFailedCount;
        ArtifactData mData;

        public void Init(string id, int level, int count, int failedCount)
        {
            mId = id;
            mLevel = level;
            mCount = count;
            mFailedCount = failedCount;
            mData = DataUtil.GetArtifactData(id);
        }

        public string GetId()
        {
            return mId;
        }

        public int GetLevel()
        {
            return mLevel;
        }

        public int GetCount()
        {
            return mCount;
        }

        public int GetFailedCount()
        {
            return mFailedCount;
        }

        public bool IsEnoughCount(int count)
        {
            return mCount >= count;
        }

        public void AddCount(int count)
        {
            if (count <= 0)
                return;

            mCount += count;
        }

        public bool UseCount(int count)
        {
            if (count <= 0)
                return false;

            if (IsEnoughCount(count) == false)
                return false;

            mCount -= count;

            return true;
        }

        public bool CanLevelUp()
        {
            ArtifactLevelUpData levelUpData = DataUtil.GetArtifactLevelUpData(DataUtil.IsAncientArtifact(mId), mLevel);
            if (levelUpData == null)
                return false;

            int requireCount = levelUpData.requireCount;
            if (IsEnoughCount(requireCount) == false)
                return false;

            return true;
        }

        public bool LevelUp()
        {
            if (CanLevelUp() == false)
                return false;

            var levelUpData = DataUtil.GetArtifactLevelUpData(DataUtil.IsAncientArtifact(mId), mLevel);
            int requireCount = levelUpData?.requireCount ?? int.MaxValue;

            if (UseCount(requireCount) == false)
                return false;

            float failedBonusProb = mFailedCount * levelUpData.prob * Lance.GameData.ArtifactCommonData.failedBonusProbValue;

            float totalProb = levelUpData.prob + failedBonusProb;

            // 확률 적용
            if (Util.Dice(totalProb))
            {
                mLevel += 1;

                mFailedCount = 0;

                return true;
            }
            else
            {
                mFailedCount++;

                return false;
            }
        }

        public float GetFailedBonusProb()
        {
            var levelUpData = DataUtil.GetArtifactLevelUpData(DataUtil.IsAncientArtifact(mId), mLevel);
            if (levelUpData == null)
                return 0f;

            return mFailedCount * levelUpData.prob * Lance.GameData.ArtifactCommonData.failedBonusProbValue;
        }

        public StatType GetStatType()
        {
            return mData.statType;
        }

        public double GetStatValue()
        {
            return mData.baseValue + (mLevel * mData.levelUpValue);
        }

        public void ReadyToSave()
        {
            Id = mId;
            Level = mLevel;
            Count = mCount;
            FailedCount = mFailedCount;
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();
            mCount.RandomizeCryptoKey();
            mFailedCount.RandomizeCryptoKey();
        }

        public bool IsMaxLevel()
        {
            int maxLevel = DataUtil.GetArtifactMaxLevel(DataUtil.IsAncientArtifact(mId));

            return mLevel == maxLevel;
        }

        public bool CanDismantle()
        {
            if (IsMaxLevel() == false)
                return false;

            if (GetCount() <= 0)
                return false;

            return true;
        }
#if UNITY_EDITOR
        public void SetLevel(int level)
        {
            mLevel = level;
        }
#endif
    }
}