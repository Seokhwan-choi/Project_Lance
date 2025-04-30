using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System.Linq;
using BackEnd;

namespace Lance
{
    public class StackedSpawnInfo
    {
        public int Type;
        public int StackedSpawnCount;
        public List<int> ReceivedReward;
        public List<string> FirstSpawned;
        public DailyCounter DailyCounter;
        
        ObscuredInt mType;
        ObscuredInt mStackedSpawnCount;
        List<ObscuredInt> mReceivedReward = new();
        List<ObscuredString> mFirstSpawned = new();
        DailyCounter mDailyCounter;
        SpawnData mData;
        public void Init(ItemType type)
        {
            mType = (int)type;
            mReceivedReward = new List<ObscuredInt>();
            mFirstSpawned = new List<ObscuredString>();
            mDailyCounter = new DailyCounter();
            mData = Lance.GameData.SpawnData.TryGet(GetSpawnType());
        }

        public ItemType GetSpawnType()
        {
            return (ItemType)(int)mType;
        }

        public ObscuredInt GetStackedSpawnCount()
        {
            return mStackedSpawnCount;
        }

        public void StackSpawnCount(int spawnCount)
        {
            mStackedSpawnCount += spawnCount;
        }

        public void RandomizeKey()
        {
            mType.RandomizeCryptoKey();

            foreach(var received in mReceivedReward)
            {
                received.RandomizeCryptoKey();
            }

            foreach(var firstSpawned in mFirstSpawned)
            {
                firstSpawned.RandomizeCryptoKey();
            }
            
            mStackedSpawnCount.RandomizeCryptoKey();
            mDailyCounter.RandomizeKey();
        }

        public void ReadyToSave()
        {
            Type = mType;
            StackedSpawnCount = mStackedSpawnCount;
            ReceivedReward = mReceivedReward.Select(x => (int)x).ToList();
            FirstSpawned = mFirstSpawned.Select(x => (string)x).ToList();
            mDailyCounter.ReadyToSave();
            DailyCounter = mDailyCounter;
        }

        public void SetServerDataToLocal(JsonData jsonData)
        {
            int typeTemp = 0;
            int.TryParse(jsonData["Type"].ToString(), out typeTemp);
            mType = typeTemp;

            int stackTemp = 0;
            int.TryParse(jsonData["StackedSpawnCount"].ToString(), out stackTemp);
            mStackedSpawnCount = stackTemp;

            if (jsonData["ReceivedReward"].Count > 0)
            {
                for(int i = 0; i < jsonData["ReceivedReward"].Count; ++i)
                {
                    int receiveRewardTemp = 0;

                    int.TryParse(jsonData["ReceivedReward"][i].ToString(), out receiveRewardTemp);

                    mReceivedReward.Add(receiveRewardTemp);
                }
            }

            if (jsonData["FirstSpawned"].Count > 0)
            {
                for (int i = 0; i < jsonData["FirstSpawned"].Count; ++i)
                {
                    mFirstSpawned.Add(jsonData["FirstSpawned"][i].ToString());
                }
            }

            mDailyCounter.SetServerDataToLocal(jsonData["DailyCounter"]);
        }

        public bool IsReceievedReward(int level)
        {
            if (mReceivedReward.Contains(level))
            {
                SpawnRewardData rewardData = DataUtil.GetSpawnRewardData(GetSpawnType(), level);
                if (rewardData == null || rewardData.reward.IsValid() == false)
                    return false;

                return true;
            }
            else
            {
                return false;
            }
            
        }

        public bool IsFreeSpawn(string id)
        {
            var spawnPriceData = DataUtil.GetSpawnPriceData(id);
            if (spawnPriceData == null)
                return false;

            if (spawnPriceData.firstSpawnFree == false)
                return false;

            return mFirstSpawned.Contains(id) == false;
        }

        public void AddFreeSpawned(string id)
        {
            if (IsFreeSpawn(id))
            {
                mFirstSpawned.Add(id);
            }
        }

        public bool AnyRangeCanReceiveReward(int pivotLevel, bool isLeft)
        {
            foreach (SpawnGradeProbData data in DataUtil.GetSpawnGradeProbDatas((ItemType)(int)mType))
            {
                if (isLeft)
                {
                    if (data.level < pivotLevel)
                    {
                        if (CanReceiveReward(data.level))
                            return true;
                    }
                }
                else
                {
                    if (data.level > pivotLevel)
                    {
                        if (CanReceiveReward(data.level))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool AnyCanReceiveReward()
        {
            foreach(SpawnGradeProbData data in DataUtil.GetSpawnGradeProbDatas((ItemType)(int)mType))
            {
                if (CanReceiveReward(data.level))
                    return true;
            }

            return false;
        }

        public bool CanReceiveReward(int level)
        {
            ItemType itemType = GetSpawnType();

            SpawnRewardData rewardData = DataUtil.GetSpawnRewardData(itemType, level);
            if (rewardData == null || rewardData.reward.IsValid() == false)
                return false;

            SpawnGradeProbData gradeProbData = DataUtil.GetSpawnGradeProbDataByLevel(mData.probId, level);
            if (gradeProbData == null)
                return false;

            if (DataUtil.GetSpawnTotalRequireCount(mData.probId, level) > mStackedSpawnCount)
                return false;

            if (mReceivedReward.Contains(level))
            {
                if (rewardData.repeatReward.IsValid())
                {
                    if (CalcCanReceiveRepeatRewardCount(level) <= 0)
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public int GetRepeatStackedCount(int level)
        {
            ItemType itemType = (ItemType)(int)mType;

            SpawnRewardData rewardData = DataUtil.GetSpawnRewardData(itemType, level);
            if (rewardData == null)
                return 0;

            SpawnGradeProbData gradeProbData = DataUtil.GetSpawnGradeProbDataByLevel(mData.probId, level);
            if (gradeProbData == null)
                return 0;

            // 반복 보상을 받을 수 있는지 확인한다.
            int receivedCount = Mathf.Max(0, mReceivedReward.Count(x => x == rewardData.level) - 1);
            int repeatStackedCount = mStackedSpawnCount - DataUtil.GetSpawnTotalRequireCount(mData.probId, level);

            // 여태 반복 보상을 받은 횟수 차감
            return repeatStackedCount - (rewardData.repeatRewardRequire * receivedCount);
        }

        int CalcCanReceiveRepeatRewardCount(int level)
        {
            ItemType itemType = (ItemType)(int)mType;

            SpawnRewardData rewardData = DataUtil.GetSpawnRewardData(itemType, level);
            if (rewardData == null)
                return 0;

            int remainRepeatStackedCount = GetRepeatStackedCount(level);

            // 보상을 받을 수 있는 횟수 계산
            return (int)((float)remainRepeatStackedCount / (float)rewardData.repeatRewardRequire);
        }

        public RewardResult ReceiveReward(int level)
        {
            RewardResult result = new RewardResult();
            ItemType itemType = (ItemType)(int)mType;

            SpawnRewardData spawnRewardData = DataUtil.GetSpawnRewardData(itemType, level);
            if (spawnRewardData == null)
                return result;

            if (mReceivedReward.Contains(level))
            {
                // 받은적이 있는데 또 받으려고 한다면
                // 반복 보상을 받는 것인지 확인한다.
                if (spawnRewardData.repeatReward.IsValid() == false)
                    return result;

                int repeatRewardCount = CalcCanReceiveRepeatRewardCount(level);
                if (repeatRewardCount <= 0)
                    return result;

                RewardData reward = Lance.GameData.RewardData.TryGet(spawnRewardData.repeatReward);
                if (reward == null)
                    return result;

                for (int i = 0; i < repeatRewardCount; ++i)
                {
                    mReceivedReward.Add(level);

                    result = result.AddReward(reward);
                }

                return result;
            }
            else
            {
                SpawnGradeProbData gradeProbData = DataUtil.GetSpawnGradeProbDataByLevel(mData.probId, level);
                if (gradeProbData.requireStack > mStackedSpawnCount)
                    return result;

                if (spawnRewardData.reward.IsValid() == false)
                    return result;

                RewardData reward = Lance.GameData.RewardData.TryGet(spawnRewardData.reward);
                if (reward == null)
                    return result;

                mReceivedReward.Add(level);

                result = result.AddReward(reward);

                return result;
            }
        }

        public bool IsEnoughWathAdCount()
        {
            int maxCount = GetDailyWatchAdMaxCount();

            return mDailyCounter.IsMaxCount(maxCount) == false;
        }

        public int GetDailyWatchAdRemainCount()
        {
            int maxCount = GetDailyWatchAdMaxCount();

            return mDailyCounter.GetRemainCount(maxCount);
        }

        public int GetDailyWatchAdMaxCount()
        {
            ItemType type = (ItemType)(int)mType;

            SpawnPriceData data = DataUtil.GetSpawnPriceData(type, SpawnType.Ad);

            return data?.dailyLimitCount ?? 0;
        }

        public bool WatchAd(int count = 1)
        {
            if (IsEnoughWathAdCount() == false)
                return false;

            mDailyCounter.StackCount(GetDailyWatchAdMaxCount(), count);

            return true;
        }

        public void ResetRewardList()
        {
            if (mData.type != ItemType.Skill)
                mReceivedReward.Clear();
        }
    }

    public class SpawnInfo : AccountBase
    {
        ObscuredBool mResetReceivedRewardList;
        Dictionary<ObscuredInt, StackedSpawnInfo> mStackedSpawnInfo;
        public SpawnInfo()
        {
            mStackedSpawnInfo = new Dictionary<ObscuredInt, StackedSpawnInfo>();

            // 장비 (무기, 갑옷, 장갑, 신발) + 스킬 + 유물
            for (int i = 0; i < (int)ItemType.Count; ++i)
            {
                ItemType type = (ItemType)i;
                int index = i;

                if (type.IsSpawn() == false)
                    continue;

                var info = new StackedSpawnInfo();
                info.Init(type);

                mStackedSpawnInfo.Add(index, info);
            }
        }

        public bool IsFreeSpawn(ItemType itemType, string id)
        {
            var info = GetInfo(itemType);

            return info?.IsFreeSpawn(id) ?? false;
        }

        public void AddFreeSpawned(ItemType itemType, string id)
        {
            var info = GetInfo(itemType);

            info?.AddFreeSpawned(id);

            SetIsChangedData(true);
        }

        public int GetTotalStackedSpawnCount()
        {
            int total = 0;

            foreach(var info in mStackedSpawnInfo.Values)
            {
                total += info.GetStackedSpawnCount(); ;
            }

            return total;
        }

        public int GetEquipmentTotalStackedSpawnCount()
        {
            int total = 0;

            foreach (var info in mStackedSpawnInfo.Values)
            {
                if (info.GetSpawnType().IsEquipment())
                    total += info.GetStackedSpawnCount();
            }

            return total;
        }

        public int GetArtifactStackedSpawnCount()
        {
            int total = 0;

            foreach (var info in mStackedSpawnInfo.Values)
            {
                if (info.GetSpawnType().IsArtifact())
                    total += info.GetStackedSpawnCount();
            }

            return total;
        }

        public int GetEquipmentStackedSpawnCount(ItemType itemType)
        {
            int total = 0;

            foreach (var info in mStackedSpawnInfo.Values)
            {
                var infoType = info.GetSpawnType();

                if (infoType.IsEquipment() && itemType == infoType)
                    return info.GetStackedSpawnCount();
            }

            return total;
        }

        public int GetAccessoryStackedSpawnCount(ItemType itemType)
        {
            int total = 0;

            foreach (var info in mStackedSpawnInfo.Values)
            {
                var infoType = info.GetSpawnType();

                if (infoType.IsAccessory() && itemType == infoType)
                    return info.GetStackedSpawnCount();
            }

            return total;
        }

        public int GetSkillTotalStackedSpawnCount()
        {
            int total = 0;

            foreach (var info in mStackedSpawnInfo.Values)
            {
                if (info.GetSpawnType().IsSkill())
                    total += info.GetStackedSpawnCount();
            }

            return total;
        }

        public StackedSpawnInfo GetInfo(ItemType type)
        {
            return mStackedSpawnInfo.TryGet((int)type);
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var info in mStackedSpawnInfo)
            {
                info.Key.RandomizeCryptoKey();
                info.Value.RandomizeKey();
            }

            mResetReceivedRewardList.RandomizeCryptoKey();
        }

        public override string GetTableName()
        {
            return "SpawnInfo";
        }

        public override Param GetParam()
        {
            var param = new Param();

            for (int i = 0; i < (int)ItemType.Count; ++i)
            {
                ItemType type = (ItemType)i;
                int index = i;

                StackedSpawnInfo info = mStackedSpawnInfo.TryGet(index);
                if (info != null)
                {
                    info.ReadyToSave();

                    param.Add($"{type}_info", info);
                }
            }

            param.Add("ResetReceivedRewardList", (bool)mResetReceivedRewardList);

            return param;
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            mResetReceivedRewardList = true;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            for (int i = 0; i < (int)ItemType.Count; ++i)
            {
                ItemType type = (ItemType)i;
                int index = i;

                StackedSpawnInfo info = mStackedSpawnInfo.TryGet(index);
                if (info != null)
                {
                    if (gameDataJson.ContainsKey($"{type}_info"))
                    {
                        info.SetServerDataToLocal(gameDataJson[$"{type}_info"]);
                    }
                }
            }

            mResetReceivedRewardList = false;

            if (gameDataJson.ContainsKey("ResetReceivedRewardList"))
            {
                bool resetReceivedRewardListTemp = false;

                bool.TryParse(gameDataJson["ResetReceivedRewardList"].ToString(), out resetReceivedRewardListTemp);

                mResetReceivedRewardList = resetReceivedRewardListTemp;
            }

            if (mResetReceivedRewardList == false)
            {
                mResetReceivedRewardList = true;

                foreach(var info in mStackedSpawnInfo.Values)
                {
                    info.ResetRewardList();
                }

                SetIsChangedData(true);
            }
        }
    }
    
}