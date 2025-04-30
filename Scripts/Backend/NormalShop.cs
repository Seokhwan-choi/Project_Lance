using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System;

namespace Lance
{
    public class NormalShop : AccountBase
    {
        Dictionary<string, NormalItemInst> mNormalItemInsts;
        protected override void InitializeData()
        {
            mNormalItemInsts = new Dictionary<string, NormalItemInst>();

            foreach (NormalShopData data in Lance.GameData.NormalShopData.Values)
            {
                var inst = new NormalItemInst();

                inst.Init(data.id, 0, new DailyCounter());

                mNormalItemInsts.Add(data.id, inst);
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var inst in mNormalItemInsts.Values)
            {
                inst.RandomizeKey();
            }
        }

        public override string GetTableName()
        {
            return "NormalShop";
        }
        public override Param GetParam()
        {
            foreach (var normalItem in mNormalItemInsts.Values)
            {
                normalItem.ReadyToSave();
            }

            var param = new Param();

            param.Add("NormalItems", mNormalItemInsts);

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mNormalItemInsts = new Dictionary<string, NormalItemInst>();

            var normalitems = gameDataJson["NormalItems"];

            for (int i = 0; i < normalitems.Count; ++i)
            {
                var normalItem = normalitems[i];

                string id = normalItem["Id"].ToString();

                var data = Lance.GameData.NormalShopData.TryGet(id);
                if (data == null)
                    continue;

                int purchaseCountTemp = 0;

                int.TryParse(normalItem["PurchasedCount"].ToString(), out purchaseCountTemp);

                DailyCounter dailyCounter = new DailyCounter();

                dailyCounter.SetServerDataToLocal(normalItem["DailyCounter"]);

                var NormalItemInst = new NormalItemInst();

                NormalItemInst.Init(id, purchaseCountTemp, dailyCounter);

                mNormalItemInsts.Add(id, NormalItemInst);
            }

            foreach (NormalShopData data in Lance.GameData.NormalShopData.Values)
            {
                if (mNormalItemInsts.ContainsKey(data.id) == false)
                {
                    var inst = new NormalItemInst();

                    inst.Init(data.id, 0, new DailyCounter());

                    mNormalItemInsts.Add(data.id, inst);
                }
            }
        }

        public NormalItemInst GetInst(string id)
        {
            return mNormalItemInsts.TryGet(id);
        }

        public int GetTotalPurchasedCount()
        {
            int totalPurchasedCount = 0;

            foreach(var inst in mNormalItemInsts.Values)
            {
                totalPurchasedCount += inst.GetPurchasedCount();
            }

            return totalPurchasedCount;
        }

        public int GetRemainPurchaseCount(string id)
        {
            var inst = GetInst(id);
            if (inst == null)
                return 0;

            return inst.GetRemainPurchaseCount();
        }

        public bool IsEnoughPurchaseCount(string id)
        {
            var inst = GetInst(id);
            if (inst == null)
                return false;

            return inst.IsEnoughPurchaseCount();
        }
    }

    public class NormalItemInst
    {
        public string Id;
        public int PurchasedCount;
        public DailyCounter DailyCounter;

        ObscuredString mId;
        ObscuredInt mPurchasedCount;
        DailyCounter mDailyCounter;

        public void Init(string id, int purchasedCount, DailyCounter dailyCounter)
        {
            mId = id;
            mPurchasedCount = purchasedCount;
            mDailyCounter = dailyCounter;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mPurchasedCount.RandomizeCryptoKey();
            mDailyCounter.RandomizeKey();
        }

        public void ReadyToSave()
        {
            Id = mId;
            PurchasedCount = mPurchasedCount;
            mDailyCounter.ReadyToSave();
            DailyCounter = mDailyCounter;
        }

        public int GetPurchasedCount()
        {
            return mPurchasedCount;
        }

        public bool IsEnoughPurchaseCount()
        {
            var data = Lance.GameData.NormalShopData.TryGet(mId);
            if (data == null)
                return false;

            if (data.purchaseDailyCount <= 0)
                return true;

            return mDailyCounter.IsMaxCount(data.purchaseDailyCount) == false;
        }

        public bool Purchase(int count = 1)
        {
            var data = Lance.GameData.NormalShopData.TryGet(mId);
            if (data == null)
                return false;

            if (IsEnoughPurchaseCount() == false)
                return false;

            mDailyCounter.StackCount(data.purchaseDailyCount, count);

            mPurchasedCount += count;

            return true;
        }

        public int GetRemainPurchaseCount()
        {
            var data = Lance.GameData.NormalShopData.TryGet(mId);
            if (data == null)
                return 0;

            return mDailyCounter.GetRemainCount(data.purchaseDailyCount);
        }
    }
}