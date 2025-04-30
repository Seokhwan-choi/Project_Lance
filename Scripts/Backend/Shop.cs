using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;
using System.Linq;
using System;

namespace Lance
{
    public class Shop : AccountBase
    {
        public DailyCounter FreeGemDailyCounter;

        DailyCounter mFreeGemDailyCounter = new();

        Dictionary<string, GemShopInst> mGemShopInsts = new();
        public override string GetTableName()
        {
            return "Shop";
        }

        public override Param GetParam()
        {
            var param = new Param();

            mFreeGemDailyCounter.ReadyToSave();
            FreeGemDailyCounter = mFreeGemDailyCounter;

            foreach(var gemShopInst in mGemShopInsts.Values)
            {
                gemShopInst.ReadyToSave();
            }

            param.Add("FreeGemDailyCounter", FreeGemDailyCounter);
            param.Add("GemShopInsts", mGemShopInsts);

            return param;
        }

        protected override void InitializeData()
        {
            foreach(var gemShopData in Lance.GameData.GemShopData.Values)
            {
                if (gemShopData.productId.IsValid())
                {
                    var gemShopInst = new GemShopInst();

                    gemShopInst.Init(gemShopData.id, 0, 0);

                    mGemShopInsts.Add(gemShopData.id, gemShopInst);
                }
            }
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            if (gameDataJson.ContainsKey("FreeGemDailyCounter"))
            {
                var jsonData = gameDataJson["FreeGemDailyCounter"];

                mFreeGemDailyCounter = new DailyCounter();
                mFreeGemDailyCounter.SetServerDataToLocal(jsonData);
            }

            if (gameDataJson.ContainsKey("GemShopInsts"))
            {
                var gemShopInstDatas = gameDataJson["GemShopInsts"];

                for(int i = 0; i < gemShopInstDatas.Count; ++i)
                {
                    var gemShopInstData = gemShopInstDatas[i];

                    string id = gemShopInstData["Id"].ToString();

                    var data = Lance.GameData.GemShopData.TryGet(id);
                    if (data == null)
                        continue;

                    int stackedPurchaseCountTemp = 0;

                    int.TryParse(gemShopInstData["StackedPurchaseCount"].ToString(), out stackedPurchaseCountTemp);

                    int bonusCountTemp = 0;

                    int.TryParse(gemShopInstData["BonusCount"].ToString(), out bonusCountTemp);

                    var inst = new GemShopInst();

                    inst.Init(id, bonusCountTemp, stackedPurchaseCountTemp);

                    mGemShopInsts.Add(id, inst);
                }
            }
            else
            {
                InitializeData();
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mFreeGemDailyCounter.RandomizeKey();
        }

        public bool StackWatchAdFreeGemCount(int maxCount)
        {
            if (mFreeGemDailyCounter.StackCount(maxCount))
            {
                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public bool IsEnoughFreeGemWatchAdCount(int maxCount)
        {
            return mFreeGemDailyCounter.IsMaxCount(maxCount) == false;
        }

        public int GetFreeGemWatchAdRemainCount(int maxCount)
        {
            return mFreeGemDailyCounter.GetRemainCount(maxCount);
        }

        public bool PurchaseGem(string id)
        {
            var gemShopInst = mGemShopInsts.TryGet(id);
            if (gemShopInst == null)
                return false;

            if (gemShopInst.IsEnoughBonusCount())
            {
                gemShopInst.StackBonusCount();
            }

            gemShopInst.Purchase();

            SetIsChangedData(true);

            return true;
        }

        public bool IsEnoughBonusCount(string id)
        {
            var gemShopInst = mGemShopInsts.TryGet(id);
            if (gemShopInst == null)
                return false;

            return gemShopInst.IsEnoughBonusCount();
        }

        public int GetRemainBonusCount(string id)
        {
            var gemShopInst = mGemShopInsts.TryGet(id);
            if (gemShopInst == null)
                return 0;

            return gemShopInst.GetRemainBonusCount();
        }

        public int CalcTotalPayments()
        {
            int totalPayments = 0;

            foreach(var gemShopInst in mGemShopInsts.Values)
            {
                totalPayments += gemShopInst.GetTotalPayments();
            }

            return totalPayments;
        }
    }

    public class GemShopInst
    {
        public string Id;
        public int BonusCount;
        public int StackedPurchaseCount;

        ObscuredString mId;
        ObscuredInt mBonusCount;
        ObscuredInt mStackedPurchaseCount;
        public void Init(string id, int bonusCount, int stackedPurchaseCount)
        {
            mId = id;
            mBonusCount = bonusCount;
            mStackedPurchaseCount = stackedPurchaseCount;
        }

        public GemShopData GetData()
        {
            return Lance.GameData.GemShopData.TryGet(mId);
        }

        public void ReadyToSave()
        {
            Id = mId;
            BonusCount = mBonusCount;
            StackedPurchaseCount = mStackedPurchaseCount;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mBonusCount.RandomizeCryptoKey();
            mStackedPurchaseCount.RandomizeCryptoKey();
        }

        public int GetRemainBonusCount()
        {
            var data = GetData();

            return data.bonusCount - mBonusCount;
        }

        public bool IsEnoughBonusCount()
        {
            return GetRemainBonusCount() > 0;
        }

        public bool StackBonusCount()
        {
            if (IsEnoughBonusCount())
            {
                mBonusCount++;

                return true;
            }

            return false;
        }

        public void Purchase()
        {
            // 구매 카운트 증가
            mStackedPurchaseCount += 1;
        }

        public string GetId()
        {
            return mId;
        }

        public int GetTotalPayments()
        {
            var data = GetData();

            return data.price * mStackedPurchaseCount;
        }

        public int GetStackedPurchaseCount()
        {
            return mStackedPurchaseCount;
        }
    }
}