using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Lance
{
    public class EventInfo
    {
        public string Id;
        public NewEventQuest EventQuest;
        public NewEventPass EventPass;
        public NewEventShop EventShop;

        ObscuredString mId;
        NewEventQuest mEventQuest = new();
        NewEventPass mEventPass = new();
        NewEventShop mEventShop = new();
        public void Init(string id, NewEventQuest quest, NewEventPass pass, NewEventShop shop)
        {
            mId = id;
            mEventQuest = quest;
            mEventPass = pass;
            mEventShop = shop;

            mEventPass?.UpdateRewardValue(mEventShop?.GetStackedCurrency() ?? 0);
        }

        public void ReadyToSave()
        {
            Id = mId;

            mEventQuest.ReadyToSave();
            EventQuest = mEventQuest;

            mEventPass?.ReadyToSave();
            EventPass = mEventPass;

            mEventShop?.ReadyToSave();
            EventShop = mEventShop;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mEventQuest?.RandomizeKey();
            mEventPass?.RandomizeKey();
            mEventShop?.RandomizeKey();
        }

        public bool UpdateEventQuest()
        {
            return IsActiveEvent() && (mEventQuest?.Update() ?? false);
        }

        public string GetId()
        {
            return mId;
        }

        public EventType GetEventType()
        {
            var eventData = Lance.GameData.EventData.TryGet(mId);

            return eventData.eventType;
        }

        public int CalcEventQuestPassDay()
        {
            return mEventQuest?.CalcPassDay() ?? 0;
        }

        public int GetTotalPayments()
        {
            return mEventPass?.GetTotalPayments() ?? 0;
        }

        public QuestInfo GetQuestInfo(string questId)
        {
            return mEventQuest?.GetQuestInfo(questId);
        }

        public bool IsActiveEvent()
        {
            var data = Lance.GameData.EventData.TryGet(mId);
            if (data == null)
                return false;

            if (data.active == false)
                return false;

            if (data.startDate > 0 && data.endDate > 0)
                return TimeUtil.IsActiveDateNum(data.startDate, data.endDate);
            else
                return true;
        }

        public IEnumerable<QuestInfo> GetQuestInfoByType(QuestType type)
        {
            return mEventQuest?.GetQuestInfoByType(type) ?? null;
        }

        public bool AnyCanReceiveQuestReward()
        {
            return IsActiveEvent() && (mEventQuest?.AnyCanReceiveReward() ?? false);
        }

        public bool AnyCanReceivePassReward()
        {
            return IsActiveEvent() && (mEventPass?.AnyCanReceiveReward() ?? false);
        }

        public bool AnyCanReceiveReward()
        {
            return IsActiveEvent() && (AnyCanReceiveQuestReward() || AnyCanReceivePassReward());
        }

        public bool AnyCanReceiveQuestReward(int startRange, int endRange)
        {
            return IsActiveEvent() && (mEventQuest?.AnyCanReceiveReward(startRange, endRange) ?? false);
        }

        public bool AnyCanReceiveQuestReward(int openDay)
        {
            return IsActiveEvent() && (mEventQuest?.AnyCanReceiveReward(openDay) ?? false);
        }

        public (string rewardId, int count) ReceiveQuestReward(string id)
        {
            return mEventQuest?.ReceiveReward(id) ?? (string.Empty, 0);
        }

        public List<(string questId, string rewardId, int count)> ReceiveAllQuestReward()
        {
            return mEventQuest?.ReceiveAllQuestReward() ?? null;
        }

        public IEnumerable<QuestInfo> GetQuestInfos()
        {
            return mEventQuest?.GetQuestInfos() ?? null;
        }

        public void UpdatePassRewardValue()
        {
            int eventCurrency = mEventShop?.GetStackedCurrency() ?? 0;

            mEventPass?.UpdateRewardValue(eventCurrency);
        }

        public void AddEventCurrency(int currency)
        {
            mEventShop?.AddEventCurrency(currency);

            UpdatePassRewardValue();
        }

        public (List<int> free, List<int> pay) ReceiveAllPassReward()
        {
            return mEventPass.ReceiveAllReward();
        }

        public bool IsEnoughEventCurrency(int price)
        {
            return mEventShop?.IsEnoughEventCurrency(price) ?? false;
        }

        public bool IsPurchasedPass()
        {
            return mEventPass?.IsPurchased() ?? false;
        }

        public bool CanReceivePassReward(int step, bool isFree)
        {
            return IsActiveEvent() && (mEventPass?.CanReceiveReward(step, isFree) ?? false);
        }

        public bool IsEnoughPurchaseCount(string id, int count)
        {
            return mEventShop?.IsEnoughPurchaseCount(id, count) ?? false;
        }

        public bool PurchaseEventItem(string id, int count)
        {
            return mEventShop?.Purchase(id, count) ?? false;
        }

        public int GetQuestClearCount()
        {
            return mEventQuest?.GeQuestClearCount() ?? 0;
        }

        public int GetCurrency()
        {
            return mEventShop?.GetCurrency() ?? 0;
        }

        public int GetRemainPuchaseCount(string id)
        {
            return mEventShop?.GetRemainPurchaseCount(id) ?? 0;
        }

        public bool IsSatisfiedPassValue(int step)
        {
            return mEventPass?.IsSatisfiedValue(step) ?? false;
        }

        public bool ReceivePassReward(int step, bool isFree)
        {
            return mEventPass?.ReceiveReward(step, isFree) ?? false;
        }

        public bool IsAlreadyPassRewardReceived(int step, bool isFree)
        {
            return mEventPass?.IsAlreadyReceived(step, isFree) ?? false;
        }

        public bool IsAlreadyPurchasedPass()
        {
            return mEventPass?.IsAlreadyPurchased() ?? false;
        }

        public double GetPassRewardValue()
        {
            return mEventPass?.GetRewardValue() ?? 0;
        }

        public bool PurchasePass()
        {
            return mEventPass?.Purchase() ?? false;
        }
    }
}