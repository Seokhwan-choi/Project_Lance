using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System.Linq;

namespace Lance
{
    public class NewEventShop
    {
        public string EventId;
        public int EventCurrency;
        public int StackedEventCurrency;
        public Dictionary<string, NewEventItemInst> EventItemInsts;

        ObscuredString mEventId;
        ObscuredInt mEventCurrency;
        ObscuredInt mStackedEventCurrency;
        Dictionary<string, NewEventItemInst> mEventItemInsts = new();

        public void InitializeData(string eventId)
        {
            mEventId = eventId;

            foreach (EventShopData data in Lance.GameData.EventShopData.Values.Where(x => x.eventId == eventId))
            {
                var inst = new NewEventItemInst();

                inst.Init(data.id, 0);

                mEventItemInsts.Add(data.id, inst);
            }
        }

        public void ReadyToSave()
        {
            foreach (var eventCurrency in mEventItemInsts.Values)
            {
                eventCurrency.ReadyToSave();
            }

            EventId = mEventId;
            EventCurrency = mEventCurrency;
            StackedEventCurrency = mStackedEventCurrency;
            EventItemInsts = mEventItemInsts;
        }

        public void SetServerDataToLocal(JsonData gameDataJson)
        {
            mEventItemInsts = new Dictionary<string, NewEventItemInst>();

            mEventId = gameDataJson["EventId"].ToString();

            var eventItems = gameDataJson["EventItemInsts"];

            for (int i = 0; i < eventItems.Count; ++i)
            {
                var eventItem = eventItems[i];

                string id = eventItem["Id"].ToString();

                var data = Lance.GameData.EventShopData.TryGet(id);
                if (data == null)
                    continue;

                int purchaseCountTemp = 0;

                int.TryParse(eventItem["PurchasedCount"].ToString(), out purchaseCountTemp);

                var eventItemInst = new NewEventItemInst();

                eventItemInst.Init(id, purchaseCountTemp);

                mEventItemInsts.Add(id, eventItemInst);
            }

            foreach (EventShopData data in Lance.GameData.EventShopData.Values.Where(x => x.eventId == mEventId))
            {
                if (mEventItemInsts.ContainsKey(data.id) == false)
                {
                    var inst = new NewEventItemInst();

                    inst.Init(data.id, 0);

                    mEventItemInsts.Add(data.id, inst);
                }
            }

            int eventCurrencyTemp = 0;
            int.TryParse(gameDataJson["EventCurrency"].ToString(), out eventCurrencyTemp);
            mEventCurrency = eventCurrencyTemp;

            int stackedEventCurrencyTemp = 0;
            int.TryParse(gameDataJson["StackedEventCurrency"].ToString(), out stackedEventCurrencyTemp);
            mStackedEventCurrency = stackedEventCurrencyTemp;
        }

        public void RandomizeKey()
        {
            foreach (var eventItem in mEventItemInsts.Values)
            {
                eventItem.RandomizeKey();
            }

            mEventId.RandomizeCryptoKey();
            mEventCurrency.RandomizeCryptoKey();
            mStackedEventCurrency.RandomizeCryptoKey();
        }

        public int GetCurrency()
        {
            return mEventCurrency;
        }

        public int GetStackedCurrency()
        {
            return mStackedEventCurrency;
        }

        public NewEventItemInst GetInst(string id)
        {
            return mEventItemInsts.TryGet(id);
        }

        public bool Purchase(string id, int count)
        {
            var data = Lance.GameData.EventShopData.TryGet(id);
            if (data == null)
                return false;

            var inst = GetInst(id);
            if (inst == null)
                return false;

            if (CanPurchase(id, count) == false)
                return false;

            int totalPrice = data.price * count;

            if (UseEventCurrency(totalPrice))
            {
                inst.Purchase(count);

                return true;
            }

            return false;
        }

        public int GetRemainPurchaseCount(string id)
        {
            var inst = GetInst(id);
            if (inst == null)
                return 0;

            return inst.GetRemainPurchaseCount();
        }

        public bool CanPurchase(string id, int count)
        {
            var inst = mEventItemInsts.TryGet(id);
            if (inst == null)
                return false;

            var data = Lance.GameData.EventShopData.TryGet(id);
            if (data == null || count <= 0)
                return false;

            int totalPrice = data.price * count;

            return inst.IsEnoughPurchaseCount(count) &&
                IsEnoughEventCurrency(totalPrice);
        }

        public bool UseEventCurrency(int eventCurrency)
        {
            if (IsEnoughEventCurrency(eventCurrency) == false)
                return false;

            mEventCurrency -= eventCurrency;

            return true;
        }

        public void AddEventCurrency(int eventCurrency)
        {
            if (eventCurrency <= 0)
                return;

            mEventCurrency += eventCurrency;
            mStackedEventCurrency += eventCurrency;
        }

        public bool IsEnoughEventCurrency(int eventCurrency)
        {
            return mEventCurrency >= eventCurrency;
        }

        public bool IsEnoughPurchaseCount(string id, int count)
        {
            var inst = GetInst(id);
            if (inst == null)
                return false;

            return inst.IsEnoughPurchaseCount(count);
        }
    }

    public class NewEventItemInst
    {
        public string Id;
        public int PurchasedCount;

        ObscuredString mId;
        ObscuredInt mPurchasedCount;
        public void Init(string id, int purchasedCount)
        {
            mId = id;
            mPurchasedCount = purchasedCount;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mPurchasedCount.RandomizeCryptoKey();
        }

        public void ReadyToSave()
        {
            Id = mId;
            PurchasedCount = mPurchasedCount;
        }

        public bool IsEnoughPurchaseCount(int count)
        {
            var data = Lance.GameData.EventShopData.TryGet(mId);
            if (data == null)
                return false;

            if (data.purchaseCount <= 0)
                return true;

            return GetRemainPurchaseCount() >= count;
        }

        public bool Purchase(int count)
        {
            var data = Lance.GameData.EventShopData.TryGet(mId);
            if (data == null)
                return false;

            if (IsEnoughPurchaseCount(count) == false)
                return false;

            mPurchasedCount += count;

            return true;
        }

        public int GetRemainPurchaseCount()
        {
            var data = Lance.GameData.EventShopData.TryGet(mId);
            if (data == null)
                return 0;

            return data.purchaseCount - mPurchasedCount;
        }
    }
}