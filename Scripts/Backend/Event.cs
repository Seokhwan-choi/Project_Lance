using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using LitJson;

namespace Lance
{
    public class Event : AccountBase
    {
        Dictionary<string, EventInfo> mEventInfos = new();
        protected override void InitializeData()
        {
            base.InitializeData();

            foreach (EventData data in Lance.GameData.EventData.Values)
            {
                if (data.startDate > 0 && data.endDate > 0)
                {
                    if (TimeUtil.IsActiveDateNum(data.startDate, data.endDate) == false)
                        continue;

                    if (data.active == false)
                        continue;
                }
                
                var eventInfo = new EventInfo();

                NewEventQuest eventQuest = CreateEventQuest(data);
                eventQuest.InitializeData(data.id);

                NewEventPass eventPass = data.pass ? new NewEventPass() : null;
                eventPass?.InitializeData(data.id);

                NewEventShop eventShop = data.shop ? new NewEventShop() : null;
                eventShop?.InitializeData(data.id);

                eventInfo.Init(data.id, eventQuest, eventPass, eventShop);

                mEventInfos.Add(data.id, eventInfo);
            }
        }

        public int CalcTotalPayments()
        {
            int totalPayments = 0;

            foreach(var eventInfo in mEventInfos.Values)
            {
                totalPayments += eventInfo.GetTotalPayments();
            }

            return totalPayments;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            var eventInfosData = gameDataJson["EventInfos"];

            for (int i = 0; i < eventInfosData.Count; i++)
            {
                var eventInfoData = eventInfosData[i];

                string id = eventInfoData["Id"].ToString();

                var eventData = Lance.GameData.EventData.TryGet(id);
                if (eventData == null)
                    continue;

                NewEventQuest eventQuest = CreateEventQuest(eventData);

                if (eventInfoData.ContainsKey("EventQuest"))
                {
                    var eventQuestData = eventInfoData["EventQuest"];

                    eventQuest.SetServerDataToLocal(eventQuestData);
                }

                NewEventPass eventPass = eventData.pass ? new NewEventPass() : null;

                if (eventInfoData.ContainsKey("EventPass"))
                {
                    var eventPassData = eventInfoData["EventPass"];

                    eventPass?.SetServerDataToLocal(eventPassData);
                }

                NewEventShop eventShop = eventData.shop ? new NewEventShop() : null;

                if (eventInfoData.ContainsKey("EventShop"))
                {
                    var eventShopData = eventInfoData["EventShop"];

                    eventShop?.SetServerDataToLocal(eventShopData);
                }

                var eventInfo = new EventInfo();

                eventInfo.Init(id, eventQuest, eventPass, eventShop);

                mEventInfos.Add(id, eventInfo);
            }

            foreach (EventData data in Lance.GameData.EventData.Values)
            {
                if (mEventInfos.ContainsKey(data.id))
                    continue;

                if (data.startDate > 0 && data.endDate > 0)
                {
                    int nowDateNum = TimeUtil.NowDateNum();

                    if (data.startDate > nowDateNum || nowDateNum > data.endDate)
                        continue;

                    if (data.active == false)
                        continue;
                }

                var eventInfo = new EventInfo();

                NewEventQuest eventQuest = CreateEventQuest(data);
                eventQuest.InitializeData(data.id);

                NewEventPass eventPass = data.pass ? new NewEventPass() : null;
                eventPass?.InitializeData(data.id);

                NewEventShop eventShop = data.shop ? new NewEventShop() : null;
                eventShop?.InitializeData(data.id);

                eventInfo.Init(data.id, eventQuest, eventPass, eventShop);

                mEventInfos.Add(data.id, eventInfo);
            }
        }

        public void UpdateEventQuest()
        {
            foreach(var info in mEventInfos.Values)
            {
                if (info.UpdateEventQuest())
                {
                    SetIsChangedData(true);
                }
            }
        }

        public int GetCurrency(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.GetCurrency() ?? 0;
        }

        public (List<int> free, List<int> pay) ReceiveAllPassReward(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);
            if (eventInfo == null)
                return (null, null);

            if (eventInfo.IsActiveEvent() == false)
                return (null, null);

            return eventInfo?.ReceiveAllPassReward() ?? (null, null);
        }

        public bool IsPurchasedPass(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.IsPurchasedPass() ?? false;
        }

        public bool CanReceivePassReward(string eventId, int step, bool isFree)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.CanReceivePassReward(step, isFree) ?? false;
        }

        public IEnumerable<QuestInfo> GetQuestInfos(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.GetQuestInfos();
        }

        internal bool IsSatisfiedValue(string eventId, int step)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.IsSatisfiedPassValue(step) ?? false;
        }

        public bool ReceivePassReward(string eventId, int step, bool isFree)
        {
            var eventInfo = GetEventInfo(eventId);
            if (eventInfo == null)
                return false;

            if (eventInfo.IsActiveEvent() == false)
                return false;

            return eventInfo?.ReceivePassReward(step, isFree) ?? false;
        }
    
        public bool IsAlreadyPassRewardReceived(string eventId, int step, bool isFree)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.IsAlreadyPassRewardReceived(step, isFree) ?? false;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var eventInfo in mEventInfos.Values)
            {
                eventInfo.RandomizeKey();
            }
        }

        public override Param GetParam()
        {
            Dictionary<string, EventInfo> saveEventInfos = new Dictionary<string, EventInfo>();

            foreach (var info in mEventInfos)
            {
                info.Value.ReadyToSave();

                saveEventInfos.Add(info.Key, info.Value);
            }

            Param param = new Param();

            param.Add("EventInfos", saveEventInfos);

            return param;
        }

        public QuestInfo GetQuestInfo(string eventId, string id)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.GetQuestInfo(id);
        }

        public int GetQuestClearCount(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.GetQuestClearCount() ?? 0;
        }

        public override string GetTableName()
        {
            return "Event";
        }

        public EventInfo GetEventInfo(string eventId)
        {
            return mEventInfos.TryGet(eventId);
        }

        public int CalcEventQuestPassDay(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.CalcEventQuestPassDay() ?? 0;
        }

        public IEnumerable<QuestInfo> GetQuestInfoByType(QuestType type)
        {
            List<QuestInfo> questInfos = new List<QuestInfo>();

            foreach(var eventInfo in mEventInfos.Values)
            {
                var eventQuestInfos = eventInfo.GetQuestInfoByType(type);

                if (eventQuestInfos != null && eventQuestInfos.Count() > 0)
                    questInfos.AddRange(eventQuestInfos);
            }

            return questInfos;
        }

        public bool AnyCanReceiveReward()
        {
            foreach(var eventInfo in mEventInfos.Values)
            {
                if (eventInfo.AnyCanReceiveReward())
                    return true;
            }

            return false;
        }

        public int GetRemainPurchaseCount(string eventId, string id)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.GetRemainPuchaseCount(id) ?? 0;
        }

        public bool PurchasePass(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.PurchasePass() ?? false;
        }

        public bool AnyCanReceiveQuestReward(string eventId, int openDay)
        {
            foreach (var eventInfo in mEventInfos.Values)
            {
                if (eventInfo.GetId() == eventId)
                {
                    return eventInfo.AnyCanReceiveQuestReward(openDay);
                }
            }

            return false;
        }

        public bool AnyCanReceiveQuestReward(string eventId, int startRange, int endRange)
        {
            foreach (var eventInfo in mEventInfos.Values)
            {
                if (eventInfo.GetId() == eventId)
                {
                    return eventInfo.AnyCanReceiveQuestReward(startRange, endRange);
                }
            }

            return false;
        }

        public bool AnyCanReceiveQuestReward(string eventId)
        {
            foreach (var eventInfo in mEventInfos.Values)
            {
                if (eventInfo.GetId() == eventId)
                {
                    return eventInfo.AnyCanReceiveQuestReward();
                }
            }

            return false;
        }

        public bool AnyCanReceivePassReward(string eventId)
        {
            foreach (var eventInfo in mEventInfos.Values)
            {
                if (eventInfo.GetId() == eventId)
                {
                    return eventInfo.AnyCanReceivePassReward();
                }
            }

            return false;
        }

        public bool AnyCanReceiveReward(EventType eventType)
        {
            foreach (var eventInfo in mEventInfos.Values)
            {
                if (eventInfo.GetEventType() == eventType)
                {
                    return eventInfo.AnyCanReceiveReward();
                }
            }

            return false;
        }

        public (string rewardId, int count) ReceiveQuestReward(string eventId, string id)
        {
            var eventInfo = GetEventInfo(eventId);
            if (eventInfo == null)
                return (string.Empty, 0);

            if (eventInfo.IsActiveEvent() == false)
                return (string.Empty, 0);

            var result = eventInfo.ReceiveQuestReward(id);
            if (result.rewardId.IsValid())
                SetIsChangedData(true);

            return result;
        }

        public bool IsAlreadyPurchasedPass(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.IsAlreadyPurchasedPass() ?? false;
        }

        public List<(string questId, string rewardId, int count)> ReceiveAllQuestReward(string eventId)
        {
            var eventInfo = GetEventInfo(eventId);
            if (eventInfo == null)
                return new List<(string, string, int)>();

            if (eventInfo.IsActiveEvent() == false)
                return new List<(string, string, int)>();

            var result = eventInfo.ReceiveAllQuestReward();
            if (result != null)
                SetIsChangedData(true);

            return result;
        }

        public void UpdatePassRewardValue(string eventId)
        {
            EventInfo eventInfo = GetEventInfo(eventId);
            if (eventInfo == null)
                return;

            if (eventInfo.IsActiveEvent() == false)
                return;

            eventInfo.UpdatePassRewardValue();

            SetIsChangedData(true);
        }

        public void AddEventCurrency(string eventId, int currency)
        {
            EventInfo eventInfo = GetEventInfo(eventId);
            if (eventInfo == null)
                return;

            if (eventInfo.IsActiveEvent() == false)
                return;

            eventInfo.AddEventCurrency(currency);

            SetIsChangedData(true);
        }

        public double GetRewardValue(string eventId)
        {
            EventInfo eventInfo = GetEventInfo(eventId);

            return eventInfo?.GetPassRewardValue() ?? 0;
        }

        NewEventQuest CreateEventQuest(EventData data)
        {
            switch(data.quest)
            {
                case EventQuestUpdateType.DayOpen:
                    return new DailyOpenEventQuest();
                case EventQuestUpdateType.Normal:
                    
                //case EventQuestUpdateType.Daily:
                //case EventQuestUpdateType.None:
                default:
                    return new NormalEventQuest();
            }
        }

        public bool IsEnoughEventCurrency(string eventId, int price)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.IsEnoughEventCurrency(price) ?? false;
        } 

        public bool IsEnoughPurchaseCount(string eventId, string id, int count)
        {
            var eventInfo = GetEventInfo(eventId);

            return eventInfo?.IsEnoughPurchaseCount(id, count) ?? false;
        }

        public bool PurchaseEventItem(string eventId, string id, int count)
        {
            var eventInfo = GetEventInfo(eventId);
            if (eventInfo == null)
                return false;

            if (eventInfo.IsActiveEvent() == false)
                return false;

            return eventInfo?.PurchaseEventItem(id, count) ?? false;
        }
    }
}