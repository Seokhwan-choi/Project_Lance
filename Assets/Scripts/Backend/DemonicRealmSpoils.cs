using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using System;

namespace Lance
{
    public class DemonicRealmSpoils : AccountBase
    {
        ObscuredInt mFriendShipStackedExp;
        ObscuredInt mFriendShipExp;
        ObscuredInt mFriendShipLevel;
        DailyCounter mFriendShipVisitCounter;
        Dictionary<string, DemonicRealmSpoilsItemInst> mDemonicRealmSpoilsItemInsts;
        protected override void InitializeData()
        {
            mFriendShipStackedExp = 0;
            mFriendShipExp = 0;
            mFriendShipLevel = 0;
            mFriendShipVisitCounter = new DailyCounter();
            mDemonicRealmSpoilsItemInsts = new Dictionary<string, DemonicRealmSpoilsItemInst>();

            foreach (DemonicRealmSpoilsData data in Lance.GameData.DemonicRealmSpoilsData.Values)
            {
                var inst = new DemonicRealmSpoilsItemInst();

                inst.Init(data.id, 0, new DailyCounter());

                mDemonicRealmSpoilsItemInsts.Add(data.id, inst);
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mFriendShipStackedExp.RandomizeCryptoKey();
            mFriendShipExp.RandomizeCryptoKey();
            mFriendShipLevel.RandomizeCryptoKey();
            mFriendShipVisitCounter.RandomizeKey();

            foreach (var inst in mDemonicRealmSpoilsItemInsts.Values)
            {
                inst.RandomizeKey();
            }
        }

        public override string GetTableName()
        {
            return "DemonicRealmSpoils";
        }
        public override Param GetParam()
        {
            foreach (var demonicRealmSpoilsItem in mDemonicRealmSpoilsItemInsts.Values)
            {
                demonicRealmSpoilsItem.ReadyToSave();
            }

            var param = new Param();

            mFriendShipVisitCounter.ReadyToSave();

            param.Add("FriendShipStackedExp", (int)mFriendShipStackedExp);
            param.Add("FriendShipExp", (int)mFriendShipExp);
            param.Add("FriendShipLevel", (int)mFriendShipLevel);
            param.Add("FriendShipVisitCounter", mFriendShipVisitCounter);
            param.Add("DemonicRealmSpoilsItems", mDemonicRealmSpoilsItemInsts);

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int friendShipStackedExpTemp = 0;
            int.TryParse(gameDataJson["FriendShipStackedExp"].ToString(), out friendShipStackedExpTemp);
            mFriendShipStackedExp = friendShipStackedExpTemp;

            int friendShipExpTemp = 0;
            int.TryParse(gameDataJson["FriendShipExp"].ToString(), out friendShipExpTemp);
            mFriendShipExp = friendShipExpTemp;

            int friendShipLevelTemp = 0;
            int.TryParse(gameDataJson["FriendShipLevel"].ToString(), out friendShipLevelTemp);
            mFriendShipLevel = friendShipLevelTemp;

            DailyCounter friendShipVisitCounter = new DailyCounter();
            friendShipVisitCounter.SetServerDataToLocal(gameDataJson["FriendShipVisitCounter"]);
            mFriendShipVisitCounter = friendShipVisitCounter;

            mDemonicRealmSpoilsItemInsts = new Dictionary<string, DemonicRealmSpoilsItemInst>();

            var demonicRealmSpoilsitems = gameDataJson["DemonicRealmSpoilsItems"];
            for (int i = 0; i < demonicRealmSpoilsitems.Count; ++i)
            {
                var demonicRealmSpoilsItem = demonicRealmSpoilsitems[i];

                string id = demonicRealmSpoilsItem["Id"].ToString();

                var data = Lance.GameData.DemonicRealmSpoilsData.TryGet(id);
                if (data == null)
                    continue;

                int purchaseCountTemp = 0;

                int.TryParse(demonicRealmSpoilsItem["PurchasedCount"].ToString(), out purchaseCountTemp);

                DailyCounter dailyCounter = new DailyCounter();

                dailyCounter.SetServerDataToLocal(demonicRealmSpoilsItem["DailyCounter"]);

                var DemonicRealmSpoilsItemInst = new DemonicRealmSpoilsItemInst();

                DemonicRealmSpoilsItemInst.Init(id, purchaseCountTemp, dailyCounter);

                mDemonicRealmSpoilsItemInsts.Add(id, DemonicRealmSpoilsItemInst);
            }

            foreach (DemonicRealmSpoilsData data in Lance.GameData.DemonicRealmSpoilsData.Values)
            {
                if (mDemonicRealmSpoilsItemInsts.ContainsKey(data.id) == false)
                {
                    var inst = new DemonicRealmSpoilsItemInst();

                    inst.Init(data.id, 0, new DailyCounter());

                    mDemonicRealmSpoilsItemInsts.Add(data.id, inst);
                }
            }

            if (mFriendShipStackedExp > 0)
            {
                NormalizeFriendShipLevel();
            }
        }

        public DemonicRealmSpoilsItemInst GetInst(string id)
        {
            return mDemonicRealmSpoilsItemInsts.TryGet(id);
        }

        public int GetTotalPurchasedCount()
        {
            int totalPurchasedCount = 0;

            foreach(var inst in mDemonicRealmSpoilsItemInsts.Values)
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

        public int GetRequireFirendShipLevel(string id)
        {
            var inst = GetInst(id);
            if (inst == null)
                return 0;

            return inst.GetRequireFriendShipLevel();
        }

        public bool IsEnoughPurchaseCount(string id, int count)
        {
            var inst = GetInst(id);
            if (inst == null)
                return false;

            return inst.IsEnoughPurchaseCount(count);
        }

        public bool Visit()
        {
            if (mFriendShipVisitCounter.StackCount(1))
            {
                int visitExp = DataUtil.GetFriendShipVisitExp(mFriendShipLevel);

                StackFriendShipExp(visitExp);

                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public bool Purchase(string id, int count)
        {
            var inst = GetInst(id);
            if (inst == null)
                return false;

            if (inst.GetRequireFriendShipLevel() > mFriendShipLevel)
                return false;

            int friendShipExp = inst.GetFriendShipExp();

            if (inst.Purchase(count))
            {
                int totalFriendShipExp = friendShipExp * count;

                StackFriendShipExp(totalFriendShipExp);

                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public int StackFriendShipExp(int friendShipExp)
        {
            int levelUpCount = 0;

            if (friendShipExp > 0)
            {
                mFriendShipExp += friendShipExp;
                mFriendShipStackedExp += friendShipExp;

                SetIsChangedData(true);

                // 레벨업을 할 수 있으면 한번에 하자
                while (CanLevelUp())
                {
                    if (LevelUp())
                        levelUpCount++;
                }
            }

            return levelUpCount;
        }

        void NormalizeFriendShipLevel()
        { 
            mFriendShipLevel = 0;
            mFriendShipExp = mFriendShipStackedExp;

            SetIsChangedData(true);

            // 레벨업을 할 수 있으면 한번에 하자
            while (CanLevelUp())
            {
                LevelUp();
            }
        }

        public bool LevelUp()
        {
            int nextLevel = mFriendShipLevel + 1;

            var levelUpData = DataUtil.GetFriendShipLevelUpData(nextLevel);
            if (levelUpData == null)
                return false;

            if (levelUpData.requireExp > mFriendShipExp)
                return false;

            mFriendShipExp -= levelUpData.requireExp;

            mFriendShipLevel += 1;

            SetIsChangedData(true);

            return true;
        }

        public bool CanLevelUp()
        {
            int nextLevel = mFriendShipLevel + 1;

            var levelUpData = DataUtil.GetFriendShipLevelUpData(nextLevel);

            return levelUpData != null && levelUpData.requireExp <= mFriendShipExp;
        }

        public int GetFriendShipLevel()
        {
            return mFriendShipLevel;
        }

        public int GetStackedFriendShipExp()
        {
            return mFriendShipStackedExp;
        }

        public int GetFriendShipExp()
        {
            return mFriendShipExp;
        }

        public int GetRequireFriendShipLevelUpExp()
        {
            int nextLevel = mFriendShipLevel + 1;

            var levelUpData = DataUtil.GetFriendShipLevelUpData(nextLevel);

            return levelUpData?.requireExp ?? 0;
        }
    }

    public class DemonicRealmSpoilsItemInst
    {
        public string Id;
        public int PurchasedCount;
        public DailyCounter DailyCounter;

        ObscuredString mId;
        ObscuredInt mPurchasedCount;
        DailyCounter mDailyCounter;
        DemonicRealmSpoilsData mData;
        public void Init(string id, int purchasedCount, DailyCounter dailyCounter)
        {
            mId = id;
            mPurchasedCount = purchasedCount;
            mDailyCounter = dailyCounter;

            mData = Lance.GameData.DemonicRealmSpoilsData.TryGet(mId);
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

        public int GetDailyPurchaseCount()
        {
            return mData.purchaseDailyCount;
        }

        public int GetRequireFriendShipLevel()
        {
            return mData.requireFriendShipLevel;
        }

        public int GetFriendShipExp()
        {
            return mData.friendShipExp;
        }

        public bool IsEnoughPurchaseCount(int count)
        {
            if (mData.purchaseDailyCount > 0)
            {
                int remainCount = mDailyCounter.GetRemainCount(mData.purchaseDailyCount);
                if (remainCount < count)
                    return false;
            }

            if (mData.purchaseLimitCount > 0)
            {
                return mData.purchaseLimitCount > mPurchasedCount;
            }
            else
            {
                return true;
            }
        }

        public bool Purchase(int count = 1)
        {
            if (IsEnoughPurchaseCount(count) == false)
                return false;

            mDailyCounter.StackCount(mData.purchaseDailyCount, count);

            mPurchasedCount += count;

            return true;
        }

        public int GetRemainPurchaseCount()
        {
            return mDailyCounter.GetRemainCount(mData.purchaseDailyCount);
        }
    }
}