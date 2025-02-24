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
    public class BountyQuest : AccountBase
    {
        ObscuredInt mLastUpdateDateNum;
        ObscuredInt mQuestClearCount;
        Dictionary<string, BountyQuestInfo> mQuestInfos;
        protected override void InitializeData()
        {
            CreateNewQuests();
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mQuestInfos = new Dictionary<string, BountyQuestInfo>();

            var questJsonDatas = gameDataJson["BountyQuests"];

            for (int i = 0; i < questJsonDatas.Count; i++)
            {
                var questJsonData = questJsonDatas[i];

                string id = questJsonData["Id"].ToString();

                BountyQuestData questData = Lance.GameData.BountyQuestData.TryGet(id);
                if (questData == null)
                    continue;

                int requireCountTemp = 0;

                int.TryParse(questJsonData["RequireCount"].ToString(), out requireCountTemp);

                bool isReceivedTemp = true;

                bool.TryParse(questJsonData["IsReceived"].ToString(), out isReceivedTemp);

                string reward = questJsonData["Reward"].ToString();

                BountyQuestInfo info = new BountyQuestInfo();

                info.Init(id, requireCountTemp, reward, isReceivedTemp);

                mQuestInfos.Add(id, info);
            }

            if (gameDataJson.ContainsKey("LastUpdateDateNum"))
            {
                int lastUpdateDateNumTemp = 0;

                int.TryParse(gameDataJson["LastUpdateDateNum"].ToString(), out lastUpdateDateNumTemp);

                if (lastUpdateDateNumTemp >= 20680101)
                    lastUpdateDateNumTemp = TimeUtil.NowDateNum() - 1;

                mLastUpdateDateNum = lastUpdateDateNumTemp;
            }

            if (gameDataJson.ContainsKey("QuestClearCount"))
            {
                int questClearCountTemp = 0;

                int.TryParse(gameDataJson["QuestClearCount"].ToString(), out questClearCountTemp);

                mQuestClearCount = questClearCountTemp;
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

            param.Add("BountyQuests", mQuestInfos);
            param.Add("QuestClearCount", (int)mQuestClearCount);
            param.Add("LastUpdateDateNum", (int)mLastUpdateDateNum);

            return param;
        }

        public override string GetTableName()
        {
            return "BountyQuest";
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var info in GetQuestInfos())
            {
                info.RandomizeKey();
            }
        }

        public BountyQuestInfo GetQuestInfo(string id)
        {
            return mQuestInfos.TryGet(id);
        }

        public int GetQuestClearCount()
        {
            return mQuestClearCount;
        }

        public IEnumerable<BountyQuestInfo> GetQuestInfos()
        {
            return mQuestInfos.Values;
        }

        public (string rewardId, int count) ReceiveReward(string id)
        {
            BountyQuestInfo questInfo = GetQuestInfo(id);
            if (questInfo == null)
                return (string.Empty, 0);

            if (questInfo.CanReceiveReward() == false)
                return (string.Empty, 0);

            var result = questInfo.ReceiveReward();
            if (result.rewardId.IsValid())
            {
                SetIsChangedData(true);

                mQuestClearCount += 1;

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

        public void Update()
        {
            if (mLastUpdateDateNum < TimeUtil.NowDateNum() ||
                mQuestInfos.Count != Lance.GameData.QuestCommonData.bountyQeustMaxCount)
            {
                CreateNewQuests();
            }
        }

        public void CreateNewQuests()
        {
            mQuestInfos = new Dictionary<string, BountyQuestInfo>();

            // 일일 퀘스트 완료 퀘스트는 일단 목록에서 제외해준다.
            foreach (BountyQuestData data in Util.Shuffle(Lance.GameData.BountyQuestData.Values))
            {
                BountyQuestInfo info = new BountyQuestInfo();

                string reward = DataUtil.GetBountyQuestReward();

                info.Init(data.id, 0, reward, false);

                mQuestInfos.Add(data.id, info);

                // 최대갯수까지 퀘스트를 모두 채워 넣었으면
                if (mQuestInfos.Count >= Lance.GameData.QuestCommonData.bountyQeustMaxCount)
                {
                    break;
                }
            }

            mLastUpdateDateNum = TimeUtil.NowDateNum();

            SetIsChangedData(true);
        }
    }

    public class BountyQuestInfo
    {
        public string Id;
        public int RequireCount;
        public bool IsReceived;
        public string Reward;

        ObscuredString mId;
        ObscuredInt mRequireCount;
        ObscuredBool mIsReceived;
        ObscuredString mReward;
        BountyQuestData mData;
        public void Init(string id, int requireCount, string reward, bool isReceived)
        {
            mId = id;
            mRequireCount = requireCount;
            mIsReceived = isReceived;
            mReward = reward;

            mData = Lance.GameData.BountyQuestData.TryGet(id);
        }

        public int GetMaxRequireCount()
        {
            return mData.killCount;
        }

        public MonsterType GetMonsterType()
        {
            return mData.monsterType;
        }

        public string GetMonsterId()
        {
            return mData.monsterId;
        }

        public string GetId()
        {
            return mId;
        }

        public string GetReward()
        {
            return mReward;
        }

        public int GetStackedCount()
        {
            return mRequireCount;
        }

        public (string rewardId, int count) ReceiveReward()
        {
            if (CanReceiveReward() == false)
                return (string.Empty, 0);

            mIsReceived = true;

            return (mReward, 1);
        }

        public bool GetIsReceived()
        {
            return mIsReceived;
        }

        public bool IsSatisfied()
        {
            return mRequireCount >= GetMaxRequireCount();
        }

        public bool CanReceiveReward()
        {
            return mIsReceived == false && IsSatisfied();
        }

        public void StackRequireCount(int count = 1)
        {
            mRequireCount += count;
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
            Reward = mReward;
        }

        public void RandomizeKey()
        {
            mId?.RandomizeCryptoKey();
            mRequireCount.RandomizeCryptoKey();
            mIsReceived.RandomizeCryptoKey();
            mReward?.RandomizeCryptoKey();
        }
    }
}