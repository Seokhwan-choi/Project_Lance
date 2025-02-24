using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System;

namespace Lance
{
    public class MileageShop : AccountBase
    {
        Dictionary<string, MileageItemInst> mMileageItemInsts;
        ObscuredInt mMileage;

        protected override void InitializeData()
        {
            mMileageItemInsts = new Dictionary<string, MileageItemInst>();

            foreach(MileageShopData data in Lance.GameData.MileageShopData.Values)
            {
                var inst = new MileageItemInst();

                inst.Init(data.id, 0, new DailyCounter());

                mMileageItemInsts.Add(data.id, inst);
            }
        }

        public override string GetTableName()
        {
            return "MileageShop";
        }
        public override Param GetParam()
        {
            foreach(var mileageItem in mMileageItemInsts.Values)
            {
                mileageItem.ReadyToSave();
            }

            var param = new Param();

            param.Add("MileageItems", mMileageItemInsts);
            param.Add("Mileage", (int)mMileage);

            return param;
        }

        public int GetMileage()
        {
            return mMileage;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mMileageItemInsts = new Dictionary<string, MileageItemInst>();

            var mileageitems = gameDataJson["MileageItems"];

            for (int i = 0; i < mileageitems.Count; ++i)
            {
                var mileageItem = mileageitems[i];

                string id = mileageItem["Id"].ToString();

                var data = Lance.GameData.MileageShopData.TryGet(id);
                if (data == null)
                    continue;

                int purchaseCountTemp = 0;

                int.TryParse(mileageItem["PurchasedCount"].ToString(), out purchaseCountTemp);

                DailyCounter dailyCounter = new DailyCounter();

                dailyCounter.SetServerDataToLocal(mileageItem["DailyCounter"]);

                var mileageItemInst = new MileageItemInst();

                mileageItemInst.Init(id, purchaseCountTemp, dailyCounter);

                mMileageItemInsts.Add(id, mileageItemInst);
            }

            foreach (MileageShopData data in Lance.GameData.MileageShopData.Values)
            {
                if (mMileageItemInsts.ContainsKey(data.id) == false)
                {
                    var inst = new MileageItemInst();

                    inst.Init(data.id, 0, new DailyCounter());

                    mMileageItemInsts.Add(data.id, inst);
                }
            }

            int mileageTemp = 0;
            int.TryParse(gameDataJson["Mileage"].ToString(), out mileageTemp);
            mMileage = mileageTemp;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var inst in mMileageItemInsts.Values)
            {
                inst.RandomizeKey();
            }

            mMileage.RandomizeCryptoKey();
        }

        public MileageItemInst GetInst(string id)
        {
            return mMileageItemInsts.TryGet(id);
        }

        public bool Purchase(string id, int count)
        {
            var data = Lance.GameData.MileageShopData.TryGet(id);
            if (data == null)
                return false;

            var inst = GetInst(id);
            if (inst == null)
                return false;

            if (CanPurchase(id, count) == false)
                return false;

            int totalPrice = data.price * count;

            if (UseMileage(totalPrice))
            {
                inst.Purchase(count);

                SetIsChangedData(true);

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
            var inst = mMileageItemInsts.TryGet(id);
            if (inst == null)
                return false;

            var data = Lance.GameData.MileageShopData.TryGet(id);
            if (data == null)
                return false;

            int totalPrice = data.price * count;

            return inst.IsEnoughPurchaseCount(count) &&
                IsEnoughMileage(totalPrice);
        }

        public bool UseMileage(int mileage)
        {
            if (IsEnoughMileage(mileage) == false)
                return false;

            mMileage -= mileage;

            SetIsChangedData(true);

            return true;
        }

        public void AddMileage(int mileage)
        {
            if (mileage <= 0)
                return;

            mMileage += mileage;

            SetIsChangedData(true);
        }

        public bool IsEnoughMileage(int mileage)
        {
            return mMileage >= mileage;
        }

        public bool IsEnoughPurchaseCount(string id, int count)
        {
            var inst = GetInst(id);
            if (inst == null)
                return false;

            return inst.IsEnoughPurchaseCount(count);
        }
    }

    public class MileageItemInst
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

        public bool IsEnoughPurchaseCount(int count)
        {
            var data = Lance.GameData.MileageShopData.TryGet(mId);
            if (data == null)
                return false;

            if (data.purchaseDailyCount <= 0)
                return true;

            return mDailyCounter.GetRemainCount(data.purchaseDailyCount) >= count;
        }

        public bool Purchase(int count)
        {
            var data = Lance.GameData.MileageShopData.TryGet(mId);
            if (data == null)
                return false;

            if (IsEnoughPurchaseCount(count) == false)
                return false;

            mDailyCounter.StackCount(data.purchaseDailyCount, count);

            mPurchasedCount += count;

            return true;
        }

        public int GetRemainPurchaseCount()
        {
            var data = Lance.GameData.MileageShopData.TryGet(mId);
            if (data == null)
                return 0;

            return mDailyCounter.GetRemainCount(data.purchaseDailyCount);
        }
    }
}