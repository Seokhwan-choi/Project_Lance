using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System.Linq;
using BackEnd;
using System;

namespace Lance
{
    public class Quest : AccountBase
    {
        protected ObscuredInt mLastUpdateDateNum;
        protected ObscuredInt mQuestClearCount;
        protected ObscuredInt mQuestBonusCount;
        protected Dictionary<string, QuestInfo> mQuestInfos;
        public virtual void Update() { }
        protected virtual void CreateNewQuests() { }
        protected override void InitializeData()
        {
            CreateNewQuests();
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var questJsonDatas = gameDataJson["Quests"];

            for (int i = 0; i < questJsonDatas.Count; i++)
            {
                var questJsonData = questJsonDatas[i];

                string id = questJsonData["Id"].ToString();

                var questData = DataUtil.GetQuestData(id);
                if (questData == null)
                    continue;

                int requireCountTemp = 0;

                int.TryParse(questJsonData["RequireCount"].ToString(), out requireCountTemp);

                bool isReceivedTemp = true;

                bool.TryParse(questJsonData["IsReceived"].ToString(), out isReceivedTemp);

                bool isReceivedBonusTemp = false;

                if (questJsonData.ContainsKey("IsReceivedBonus"))
                {
                    bool.TryParse(questJsonData["IsReceivedBonus"].ToString(), out isReceivedBonusTemp);
                }

                QuestInfo info = new QuestInfo();

                info.Init(id, requireCountTemp, isReceivedTemp, isReceivedBonusTemp);

                mQuestInfos.Add(id, info);
            }

            if (gameDataJson.ContainsKey("LastUpdateDateNum"))
            {
                int lastUpdateDateNumTemp = 0;

                int.TryParse(gameDataJson["LastUpdateDateNum"].ToString(), out lastUpdateDateNumTemp);

                mLastUpdateDateNum = lastUpdateDateNumTemp;
            }

            if (gameDataJson.ContainsKey("QuestClearCount"))
            {
                int questClearCountTemp = 0;

                int.TryParse(gameDataJson["QuestClearCount"].ToString(), out questClearCountTemp);

                mQuestClearCount = questClearCountTemp;
            }

            if (gameDataJson.ContainsKey("QuestBonusCount"))
            {
                int questBonusCountTemp = 0;

                int.TryParse(gameDataJson["QuestBonusCount"].ToString(), out questBonusCountTemp);

                mQuestBonusCount = questBonusCountTemp;
            }

            Update();
        }

        public override Param GetParam()
        {
            foreach (var info in GetQuestInfos())
            {
                info.ReadyToSave();
            }

            Param param = new Param();

            param.Add("Quests", mQuestInfos);
            param.Add("QuestClearCount", (int)mQuestClearCount);
            param.Add("QuestBonusCount", (int)mQuestBonusCount);
            param.Add("LastUpdateDateNum", (int)mLastUpdateDateNum);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var info in GetQuestInfos())
            {
                info.RandomizeKey();
            }
        }

        public QuestInfo GetQuestInfo(string id)
        {
            return mQuestInfos.TryGet(id);
        }

        public int GetQuestClearCount()
        {
            return mQuestClearCount;
        }

        public IEnumerable<QuestInfo> GetQuestInfos()
        {
            return mQuestInfos.Values;
        }

        public IEnumerable<QuestInfo> GetQuestInfos(QuestType questType)
        {
            return mQuestInfos.Values.Where(x => x.GetQuestType() == questType);
        }

        public List<(string questId, string rewardId, int count)> AllReceiveReward()
        {
            List<(string, string, int)> receivedRewards = new List<(string, string, int)>();

            foreach(var questInfo in mQuestInfos.Values)
            {
                if (questInfo.CanReceiveReward() == false)
                    continue;

                (string rewardId, int count) result = questInfo.ReceiveReward();
                if (result.rewardId.IsValid())
                {
                    if (questInfo.GetQuestType().IsQuestClearType() == false)
                        mQuestClearCount += 1;

                    receivedRewards.Add((questInfo.GetId(), result.rewardId, result.count));
                }
            }

            if (receivedRewards.Count > 0)
            {
                SetIsChangedData(true);
            }

            return receivedRewards;
        }

        public (string rewardId, int count) ReceiveReward(string id)
        {
            QuestInfo questInfo = GetQuestInfo(id);
            if (questInfo == null)
                return (string.Empty, 0);

            if (questInfo.CanReceiveReward() == false)
                return (string.Empty, 0);

            var result = questInfo.ReceiveReward();
            if (result.rewardId.IsValid())
            {
                SetIsChangedData(true);

                if (questInfo.GetQuestType().IsQuestClearType() == false)
                    mQuestClearCount += 1;

                return result;
            }

            return (string.Empty, 0);
        }

        public (string rewardId, int count) ReceiveBonusReward(string id)
        {
            QuestInfo questInfo = GetQuestInfo(id);
            if (questInfo == null)
                return (string.Empty, 0);

            if (questInfo.CanReceiveBonusReward() == false)
                return (string.Empty, 0);

            var result = questInfo.ReceiveBonusReward();
            if (result.rewardId.IsValid())
            {
                SetIsChangedData(true);

                mQuestBonusCount += 1;

                return result;
            }

            return (string.Empty, 0);
        }

        public bool AnyCanReceiveReward()
        {
            foreach (var info in GetQuestInfos())
            {
                if (info.CanReceiveReward())
                    return true;
            }

            return false;
        }

        public IEnumerable<QuestInfo> GetQuestInfoByType(QuestType type)
        {
            return GetQuestInfos().Where(x => x.GetQuestType() == type);
        }

        protected void AddQuestInfo(QuestData data)
        {
            QuestInfo info = new QuestInfo();

            info.Init(data.id, 0, false, false);

            mQuestInfos.Add(data.id, info);
        }
    }

    public class QuestInfo
    {
        public string Id;
        //public int LastUpdateDateNum;
        public int RequireCount;
        public bool IsReceived;
        public bool IsReceivedBonus;

        ObscuredString mId;
        //ObscuredInt mLastUpdateDateNum;
        ObscuredInt mRequireCount;
        ObscuredBool mIsReceived;
        ObscuredBool mIsReceivedBonus;
        QuestData mData;
        public void Init(string id, int requireCount, bool isReceived, bool isReceivedBonus)
        {
            mId = id;
            mRequireCount = requireCount;
            mIsReceived = isReceived;
            mIsReceivedBonus = isReceivedBonus;
            mData = DataUtil.GetQuestData(mId);
        }

        public int GetMaxRequireCount()
        {
            return mData.requireCount;
        }

        public string GetReward()
        {
            return mData.rewardId;
        }

        public string GetBonusReward()
        {
            return mData.adBonusReward;
        }

        public QuestCheckType GetQuestCheckType()
        {
            return mData.checkType;
        }

        public string GetId()
        {
            return mId;
        }

        public int GetStackedCount()
        {
            return mRequireCount;
        }

        public (string rewardId, int count) ReceiveReward()
        {
            if (CanReceiveReward() == false)
                return (string.Empty, 0);

            if (mData.updateType != QuestUpdateType.Repeat)
            {
                mIsReceived = true;

                return (mData.rewardId, 1);
            }
            else
            {
                int receiveCount = (int)((float)mRequireCount / (float)GetMaxRequireCount());

                mRequireCount -= (GetMaxRequireCount() * receiveCount);

                return (mData.rewardId, receiveCount);
            }
        }

        public (string rewardId, int count) ReceiveBonusReward()
        {
            if (CanReceiveBonusReward() == false)
                return (string.Empty, 0);

            mIsReceivedBonus = true;

            return (mData.adBonusReward, 1);
        }

        public bool GetIsReceived()
        {
            return mIsReceived;
        }

        public bool GetIsReceivedBonus()
        {
            return mIsReceivedBonus;
        }

        public QuestType GetQuestType()
        {
            return mData?.type ?? QuestType.None;
        }

        public int GetOpenDay()
        {
            return mData?.openDay ?? 0;
        }

        public bool IsSatisfied()
        {
            return mRequireCount >= GetMaxRequireCount();
        }

        public bool IsSatisfiedBonus()
        {
            return mIsReceived && mData.adBonusReward.IsValid();
        }

        public bool CanReceiveReward()
        {
            return mIsReceived == false && IsSatisfied();
        }

        public bool CanReceiveBonusReward()
        {
            return mIsReceived && mIsReceivedBonus == false && mData.adBonusReward.IsValid();
        }

        public void StackRequireCount(int count = 1)
        {
            mRequireCount += count;
            if (mData.updateType != QuestUpdateType.Repeat)
                mRequireCount = Math.Min(mRequireCount, GetMaxRequireCount());
        }

        public void AttainRequireCount(int count)
        {
            mRequireCount = count;
            mRequireCount = Math.Min(mRequireCount, GetMaxRequireCount());
        }

        public void ReadyToSave()
        {
            Id = mId;
            RequireCount = mRequireCount;
            IsReceived = mIsReceived;
            IsReceivedBonus = mIsReceivedBonus;
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mRequireCount.RandomizeCryptoKey();
            mIsReceived.RandomizeCryptoKey();
            mIsReceivedBonus.RandomizeCryptoKey();
        }
    }
}