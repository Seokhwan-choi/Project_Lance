using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;

namespace Lance
{
    public class AncientArtifact : AccountBase
    {
        ObscuredInt mTryUpgradeCount;
        ObscuredInt mUpgradeSuccessCount;
        Dictionary<string, ArtifactInst> mArtifacts;

        public AncientArtifact()
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

        public bool CanDismantle(string id)
        {
            var artifact = mArtifacts.TryGet(id);
            if (artifact == null)
                return false;

            return artifact.CanDismantle();
        }

        public int DismantleAll()
        {
            int totalDismantleCount = 0;

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

        public int Dismantle(string id)
        {
            ArtifactInst artifactInst = mArtifacts.TryGet(id);
            if (artifactInst != null)
            {
                // 팔려고 하는 유물은 최고레벨이고
                if (artifactInst.IsMaxLevel())
                {
                    int count = artifactInst.GetCount();

                    artifactInst.UseCount(count);

                    SetIsChangedData(true);

                    return count;
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
                return DataUtil.GetArtifactUpgradeProb(true, inst.GetLevel());
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

            foreach (var artifact in mArtifacts.Values)
            {
                artifact.RandomizeKey();
            }
        }

        public override string GetTableName()
        {
            return "AncientArtifact";
        }

        public bool IsAllArtifactMaxLevel()
        {
            if (mArtifacts.Count <= 0)
                return false;

            foreach (var artifact in mArtifacts.Values)
            {
                if (artifact.IsMaxLevel() == false)
                    return false;
            }

            return true;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var artifactDatas = gameDataJson["AncientArtifacts"];

            for (int i = 0; i < artifactDatas.Count; ++i)
            {
                var artifactJsonData = artifactDatas[i];

                string id = artifactJsonData["Id"].ToString();

                var data = Lance.GameData.AncientArtifactData.TryGet(id);
                if (data == null)
                    continue;

                int levelTemp = 0;

                int.TryParse(artifactJsonData["Level"].ToString(), out levelTemp);

                int countTemp = 0;

                int.TryParse(artifactJsonData["Count"].ToString(), out countTemp);

                int failedCountTemp = 0;

                if(artifactJsonData.ContainsKey("FailedCount"))
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
            foreach (var artifact in mArtifacts.Values)
            {
                if (artifact.CanLevelUp())
                    return true;
            }

            return false;
        }

        public override Param GetParam()
        {
            Param param = new Param();

            foreach (var artifact in mArtifacts.Values)
            {
                artifact.ReadyToSave();
            }

            param.Add("AncientArtifacts", mArtifacts.Values);
            param.Add("TryUpgradeCount", (int)mTryUpgradeCount);
            param.Add("UpgradeSuccessCount", (int)mUpgradeSuccessCount);

            return param;
        }

        public int GetTryUpgradeCount()
        {
            return mTryUpgradeCount;
        }

        public bool AnyCanDismantle()
        {
            foreach (ArtifactInst artifact in mArtifacts.Values)
            {
                if (artifact.CanDismantle())
                    return true;
            }

            return false;
        }
    }
}