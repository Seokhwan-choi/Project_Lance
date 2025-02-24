using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class NewEventQuest
    {
        public string EventId;
        public int LastUpdateDateNum;
        public int QuestClearCount;
        public Dictionary<string, QuestInfo> QuestInfos;

        protected ObscuredString mEventId;
        protected ObscuredInt mLastUpdateDateNum;
        protected ObscuredInt mQuestClearCount;
        protected Dictionary<string, QuestInfo> mQuestInfos = new();

        public void InitializeData(string eventId)
        {
            mEventId = eventId;

            CreateNewQuests();
        }

        public void SetServerDataToLocal(JsonData eventQuestData)
        {
            mEventId = eventQuestData["EventId"].ToString();

            int lastUpdateDateNumTemp = 0;
            int.TryParse(eventQuestData["LastUpdateDateNum"].ToString(), out lastUpdateDateNumTemp);
            mLastUpdateDateNum = lastUpdateDateNumTemp;

            int questClearCountTemp = 0;
            int.TryParse(eventQuestData["QuestClearCount"].ToString(), out questClearCountTemp);
            mQuestClearCount = questClearCountTemp;

            if (eventQuestData.ContainsKey("QuestInfos"))
            {
                var questInfoJsonDatas = eventQuestData["QuestInfos"];

                for (int i = 0; i < questInfoJsonDatas.Count; i++)
                {
                    var questInfoJsonData = questInfoJsonDatas[i];

                    string id = questInfoJsonData["Id"].ToString();

                    var questData = DataUtil.GetQuestData(id);
                    if (questData == null)
                        continue;

                    int requireCountTemp = 0;

                    int.TryParse(questInfoJsonData["RequireCount"].ToString(), out requireCountTemp);

                    bool isReceivedTemp = true;

                    bool.TryParse(questInfoJsonData["IsReceived"].ToString(), out isReceivedTemp);

                    QuestInfo info = new QuestInfo();

                    info.Init(id, requireCountTemp, isReceivedTemp, false);

                    if (questData.checkType == QuestCheckType.Attain)
                    {
                        info.AttainRequireCount(Lance.Account.GetQuestTypeValue(questData.type));
                    }

                    mQuestInfos.Add(id, info);
                }
            }

            Update();
        }

        public void RandomizeKey()
        {
            mEventId.RandomizeCryptoKey();
            mLastUpdateDateNum.RandomizeCryptoKey();
            mQuestClearCount.RandomizeCryptoKey();

            foreach (var info in GetQuestInfos())
            {
                info.RandomizeKey();
            }
        }

        public void ReadyToSave()
        {
            EventId = mEventId;
            LastUpdateDateNum = mLastUpdateDateNum;
            QuestClearCount = mQuestClearCount;
            foreach(var questInfo in mQuestInfos.Values)
            {
                questInfo.ReadyToSave();
            }

            QuestInfos = mQuestInfos;
        }

        public int CalcPassDay()
        {
            int nowDateNum = TimeUtil.NowDateNum();

            int day = nowDateNum - mLastUpdateDateNum;

            day = Mathf.Max(day, 0);

            return day + 1;
        }

        public IEnumerable<QuestInfo> GetQuestInfos()
        {
            return mQuestInfos.Values;
        }

        public QuestInfo GetQuestInfo(string id)
        {
            return mQuestInfos.TryGet(id);
        }

        public bool Update() 
        {
            var eventData = Lance.GameData.EventData.TryGet(mEventId);
            if (eventData.quest == EventQuestUpdateType.Daily)
            {
                if (mLastUpdateDateNum < TimeUtil.NowDateNum())
                {
                    CreateNewQuests();

                    return true;
                }
            }

            var questDatas = DataUtil.GetEventQuestDatas(mEventId);
            
            if (mQuestInfos.Count != questDatas.Count())
            {
                foreach (QuestData data in questDatas)
                {
                    if (mQuestInfos.ContainsKey(data.id) == false)
                        AddQuestInfo(data);
                }
            }

            return false;
        }
        public virtual (string rewardId, int count) ReceiveReward(string id)
        {
            QuestInfo questInfo = GetQuestInfo(id);
            if (questInfo == null)
                return (string.Empty, 0);

            if (questInfo.CanReceiveReward() == false)
                return (string.Empty, 0);

            var result = questInfo.ReceiveReward();
            if (result.rewardId.IsValid())
            {
                if (questInfo.GetQuestType().IsQuestClearType() == false)
                    mQuestClearCount += 1;

                AttainQuestClearCount();

                return result;
            }

            return (string.Empty, 0);
        }

        public int GeQuestClearCount()
        {
            return mQuestClearCount;
        }

        public List<(string questId, string rewardId, int count)> ReceiveAllQuestReward()
        {
            List<(string, string, int)> receivedRewards = new List<(string, string, int)>();

            foreach (var questInfo in mQuestInfos.Values)
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

            AttainQuestClearCount();

            return receivedRewards;
        }

        void AttainQuestClearCount()
        {
            foreach(var info in GetQuestInfos())
            {
                if (info.GetQuestType() == QuestType.EventquestClear)
                {
                    info.AttainRequireCount(mQuestClearCount);
                }
            }
        }

        public virtual bool AnyCanReceiveReward()
        {
            foreach (var info in GetQuestInfos())
            {
                if (info.CanReceiveReward())
                    return true;
            }

            return false;
        }

        public virtual bool AnyCanReceiveReward(int startRange, int endRange)
        {
            foreach (var info in GetQuestInfos())
            {
                if (info.GetMaxRequireCount() >= startRange &&
                    info.GetMaxRequireCount() <= endRange)
                {
                    if (info.CanReceiveReward())
                        return true;
                }
            }

            return false;
        }

        public virtual bool AnyCanReceiveReward(int openDay)
        {
            foreach (var info in GetQuestInfos())
            {
                if (info.GetOpenDay() == openDay)
                {
                    if (info.CanReceiveReward())
                        return true;
                }
            }

            return false;
        }

        public IEnumerable<QuestInfo> GetQuestInfoByType(QuestType type)
        {
            return GetQuestInfos().Where(x => x.GetQuestType() == type);
        }

        protected virtual void CreateNewQuests()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();

            foreach (QuestData data in DataUtil.GetEventQuestDatas(mEventId))
            {
                AddQuestInfo(data);
            }

            mQuestClearCount = 0;

            mLastUpdateDateNum = TimeUtil.NowDateNum();
        }

        protected void AddQuestInfo(QuestData data)
        {
            QuestInfo info = new QuestInfo();

            info.Init(data.id, 0, false, false);

            mQuestInfos.Add(data.id, info);
        }
    }
}