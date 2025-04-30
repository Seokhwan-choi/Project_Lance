using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System;

namespace Lance
{
    public class JoustShop : AccountBase
    {
        Dictionary<string, JoustItemInst> mJoustItemInsts;
        protected override void InitializeData()
        {
            mJoustItemInsts = new Dictionary<string, JoustItemInst>();

            foreach (JoustShopData data in Lance.GameData.JoustShopData.Values)
            {
                var inst = new JoustItemInst();

                inst.Init(data.id, 0, new DailyCounter());

                mJoustItemInsts.Add(data.id, inst);
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var inst in mJoustItemInsts.Values)
            {
                inst.RandomizeKey();
            }
        }

        public override string GetTableName()
        {
            return "JoustShop";
        }
        public override Param GetParam()
        {
            foreach (var JoustItem in mJoustItemInsts.Values)
            {
                JoustItem.ReadyToSave();
            }

            var param = new Param();

            param.Add("JoustItems", mJoustItemInsts);

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mJoustItemInsts = new Dictionary<string, JoustItemInst>();

            var Joustitems = gameDataJson["JoustItems"];

            for (int i = 0; i < Joustitems.Count; ++i)
            {
                var JoustItem = Joustitems[i];

                string id = JoustItem["Id"].ToString();

                var data = Lance.GameData.JoustShopData.TryGet(id);
                if (data == null)
                    continue;

                int purchaseCountTemp = 0;

                int.TryParse(JoustItem["PurchasedCount"].ToString(), out purchaseCountTemp);

                DailyCounter dailyCounter = new DailyCounter();

                dailyCounter.SetServerDataToLocal(JoustItem["DailyCounter"]);

                var JoustItemInst = new JoustItemInst();

                JoustItemInst.Init(id, purchaseCountTemp, dailyCounter);

                mJoustItemInsts.Add(id, JoustItemInst);
            }

            foreach (JoustShopData data in Lance.GameData.JoustShopData.Values)
            {
                if (mJoustItemInsts.ContainsKey(data.id) == false)
                {
                    var inst = new JoustItemInst();

                    inst.Init(data.id, 0, new DailyCounter());

                    mJoustItemInsts.Add(data.id, inst);
                }
            }
        }

        public JoustItemInst GetInst(string id)
        {
            return mJoustItemInsts.TryGet(id);
        }

        public int GetTotalPurchasedCount()
        {
            int totalPurchasedCount = 0;

            foreach (var inst in mJoustItemInsts.Values)
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

    public class JoustItemInst
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
            var data = Lance.GameData.JoustShopData.TryGet(mId);
            if (data == null)
                return false;

            if (data.purchaseDailyCount <= 0)
                return true;

            return mDailyCounter.IsMaxCount(data.purchaseDailyCount) == false;
        }

        public bool Purchase(int count = 1)
        {
            var data = Lance.GameData.JoustShopData.TryGet(mId);
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
            var data = Lance.GameData.JoustShopData.TryGet(mId);
            if (data == null)
                return 0;

            return mDailyCounter.GetRemainCount(data.purchaseDailyCount);
        }
    }
}