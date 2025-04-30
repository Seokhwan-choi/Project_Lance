using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;


namespace Lance
{
    public class PackageShop : AccountBase
    {
        ObscuredBool mRetroactiveGrowhthPackage;
        Dictionary<ObscuredString, PackageInst> mPackageInsts;

        protected override void InitializeData()
        {
            mRetroactiveGrowhthPackage = true;
            mPackageInsts = new Dictionary<ObscuredString, PackageInst>();

            foreach (var data in Lance.GameData.PackageShopData.Where(x => x.step == 1))
            {
                if (data.active == false)
                    continue;

                if (data.startDate > 0 && data.endDate > 0)
                {
                    if (TimeUtil.IsActiveDateNum(data.startDate, data.endDate) == false)
                        continue;
                }

                var packageInst = new PackageInst();

                packageInst.Init(data.id, 1, 0, 0, 0, 0, 0);

                mPackageInsts.Add(data.id, packageInst);
            }
        }

        public override string GetTableName()
        {
            return "PackageShop";
        }

        public override Param GetParam()
        {
            foreach (var inst in mPackageInsts.Values)
            {
                inst.ReadyToSave();
            }

            var param = new Param();

            param.Add("RetroactiveGrowhthPackage", (bool)mRetroactiveGrowhthPackage);
            param.Add("PackageInsts", mPackageInsts);

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mPackageInsts = new Dictionary<ObscuredString, PackageInst>();

            var packageInsts = gameDataJson["PackageInsts"];

            for (int i = 0; i < packageInsts.Count; ++i)
            {
                var packageInstData = packageInsts[i];

                string id = packageInstData["Id"].ToString();

                int stepTemp = 0;

                int.TryParse(packageInstData["Step"].ToString(), out stepTemp);

                var data = DataUtil.GetPackageShopData(id, stepTemp);
                if (data == null)
                    continue;

                int stackedPurchaseCountTemp = 0;

                int.TryParse(packageInstData["StackedPurchaseCount"].ToString(), out stackedPurchaseCountTemp);

                int purchaseCountTemp = 0;

                int.TryParse(packageInstData["PurchaseCount"].ToString(), out purchaseCountTemp);

                int lastUpdateDateNumTemp = 0;

                int.TryParse(packageInstData["LastUpdateDateNum"].ToString(), out lastUpdateDateNumTemp);

                int monthlyFeeLastUpdateNumTemp = 0;

                if (packageInstData.ContainsKey("MonthlyFeeLastUpdateNum"))
                {
                    int.TryParse(packageInstData["MonthlyFeeLastUpdateNum"].ToString(), out monthlyFeeLastUpdateNumTemp);
                }

                int monthlyFeeDailyRewardLastUpdateDateNumTemp = 0;

                if (packageInstData.ContainsKey("MonthlyFeeDailyRewardLastUpdateDateNum"))
                {
                    int.TryParse(packageInstData["MonthlyFeeDailyRewardLastUpdateDateNum"].ToString(), out monthlyFeeDailyRewardLastUpdateDateNumTemp);
                }

                var inst = new PackageInst();

                inst.Init(id, stepTemp, stackedPurchaseCountTemp, purchaseCountTemp, lastUpdateDateNumTemp,
                    monthlyFeeLastUpdateNumTemp, monthlyFeeDailyRewardLastUpdateDateNumTemp);

                mPackageInsts.Add(id, inst);
            }

            foreach (var data in Lance.GameData.PackageShopData.Where(x => x.step == 1))
            {
                if (mPackageInsts.ContainsKey(data.id) == false)
                {
                    if (data.active == false)
                        continue;

                    if (data.startDate > 0 && data.endDate > 0 )
                    {
                        if (TimeUtil.IsActiveDateNum(data.startDate, data.endDate) == false)
                            continue;
                    }

                    var packageInst = new PackageInst();

                    packageInst.Init(data.id, 1, 0, 0, 0, 0, 0);

                    mPackageInsts.Add(data.id, packageInst);
                }
            }

            foreach(PackageInst packageInst in mPackageInsts.Values)
            {
                string id = packageInst.GetId();
                if (id == "Package_PetGrowhth")
                {
                    int step = packageInst.GetStep();
                    int stackedPurchaseCount = packageInst.GetStackedPurchaseCount();

                    if (step == 3)
                    {
                        if (step == stackedPurchaseCount)
                        {
                            packageInst.NextStep();

                            SetIsChangedData(true);
                        }
                    }
                }
            }

            if (gameDataJson.ContainsKey("RetroactiveGrowhthPackage"))
            {
                bool retroactiveGrowhthPackageTemp = false;

                bool.TryParse(gameDataJson["RetroactiveGrowhthPackage"].ToString(), out retroactiveGrowhthPackageTemp);

                mRetroactiveGrowhthPackage = retroactiveGrowhthPackageTemp;
            }

            if (mRetroactiveGrowhthPackage == false)
            {
                mRetroactiveGrowhthPackage = true;

                SetIsChangedData(true);

                // 성장 패키지를 구매한적이 있다면 지급해주자
                PackageInst package = mPackageInsts.TryGet("Package_Growhth");
                if (package != null)
                {
                    Param param = new Param();

                    for(int i = 0; i < package.GetStackedPurchaseCount(); ++i)
                    {
                        int step = i + 1;

                        var packageData = DataUtil.GetPackageShopData(package.GetId(), step);
                        if (packageData != null)
                        {
                            RewardResult rewardResult = new RewardResult();

                            var rewardData = Lance.GameData.RewardData.TryGet(packageData.reward);
                            if (rewardData != null)
                            {
                                rewardResult = rewardResult.AddReward(rewardData);
                            }

                            Lance.Account.GiveReward(rewardResult);

                            param.Add($"step_{step}", step);
                        }
                    }

                    param.Add("nowDateNum", TimeUtil.NowDateNum());

                    Lance.BackEnd.InsertLog("RetroactiveGrowhthPackage", param, 7);
                }
            }

#if UNITY_EDITOR
            //InitializeData();
#endif
        }

        public bool IsPurchasedMonthlyFee(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return false;

            return inst.IsPurchasedMonthlyFee();
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mRetroactiveGrowhthPackage.RandomizeCryptoKey();

            foreach (var inst in mPackageInsts)
            {
                inst.Key.RandomizeCryptoKey();
                inst.Value.RandomizeKey();
            }
        }

        public bool Purchase(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst.IsEnoughPurchaseCount() == false)
                return false;

            return inst.Purchase();
        }

        public int GetRemainPurchaseCount(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return 0;

            return inst.GetRemainPurchaseCount();
        }

        public bool AnyCanReceiveMonthlyFeeDailyReward()
        {
            foreach(var packageInst in mPackageInsts.Values)
            {
                if (packageInst.CanReceiveMonthlyFeeDailyReward())
                    return true;
            }

            return false;
        }

        public bool CanReceiveMonthlyFeeDailyReward(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return false;

            return inst.CanReceiveMonthlyFeeDailyReward();
        }

        // 보상은 밖에서주자
        public bool ReceiveMonthlyFeeDailyReward(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return false;

            return inst.CheckMonthlyFeeDailyReward();
        }

        public int GetMonthlyFeeEndDateNum(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return 0;

            return inst.GetMonthlyFeeEndDateNum();
        }

        public bool IsEnoughPurchaseCount(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return false;

            return inst.IsEnoughPurchaseCount();
        }

        public int GetStep(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return 1;

            return inst.GetStep();
        }

        public IEnumerable<PackageShopData> GetInStockDatas(PackageCategory category)
        {
            string[] ids = Lance.GameData.PackageShopData
                .Where(x => x.category == category)
                .Select(x => x.id).Distinct().ToArray();

            foreach(var id in ids)
            {
                var inst = mPackageInsts.TryGet(id);
                if (inst != null)
                {
                    if (inst.IsActive() == false)
                        continue;

                    if (inst.IsSoldOut() == false)
                        yield return inst.GetData();
                }
            }
        }

        public bool AnyCanPurchaseFreePackage()
        {
            foreach(PackageInst package in mPackageInsts.Values)
            {
                var data = package.GetData();
                if (data.productId.IsValid() == false && package.IsEnoughPurchaseCount())
                    return true;
            }

            return false;
        }

        public bool CanPurchaseFreePackage(PackageCategory category)
        {
            foreach (PackageInst package in mPackageInsts.Values)
            {
                var data = package.GetData();
                if (data.category != category)
                    continue;

                if (data.productId.IsValid() == false && package.IsEnoughPurchaseCount())
                    return true;
            }

            return false;
        }

        public PackageShopData GetData(string id)
        {
            var inst = mPackageInsts.TryGet(id);
            if (inst == null)
                return null;

            return inst.GetData();
        }

        public bool IsPurchasedRemoveAD()
        {
            foreach(var data in Lance.GameData.PackageShopData)
            {
                if (data.type == PackageType.RemoveAD)
                {
                    var inst = mPackageInsts.TryGet(data.id);
                    if (inst == null)
                        continue;

                    if (inst.GetRemainPurchaseCount() <= 0)
                        return true;
                }
            }

            return false;
        }

        public int CalcTotalPayments()
        {
            int totalPayments = 0;

            foreach(PackageInst packageInst in mPackageInsts.Values)
            {
                totalPayments += packageInst.GetTotalPayments();
            }

            return totalPayments;
        }
    }

    public class PackageInst
    {
        public string Id;
        public int Step;
        public int StackedPurchaseCount;
        public int PurchaseCount;
        public int LastUpdateDateNum;

        public int MonthlyFeeLastUpdateNum;
        public int MonthlyFeeDailyRewardLastUpdateDateNum;

        ObscuredString mId;
        ObscuredInt mStep;
        ObscuredInt mStackedPurchaseCount;
        ObscuredInt mPurchaseCount;
        ObscuredInt mLastUpdateDateNum;

        ObscuredInt mMonthlyFeeEndDateNum;
        ObscuredInt mMonthlyFeeDailyRewardLastUpdateDateNum;

        PackageShopData mData;

        public void Init(string id, int step, int stackedPurchaseCount, int purchaseCount, int lastUpdateDateNum, 
            int monthlyFeeEndDateNum, int monthlyFeeDailyRewardLastUpdateDateNum)
        {
            mId = id;
            mStep = step;
            mStackedPurchaseCount = stackedPurchaseCount;
            mPurchaseCount = purchaseCount;
            mLastUpdateDateNum = lastUpdateDateNum;

            mMonthlyFeeEndDateNum = monthlyFeeEndDateNum;
            mMonthlyFeeDailyRewardLastUpdateDateNum = monthlyFeeDailyRewardLastUpdateDateNum;

            mData = DataUtil.GetPackageShopData(mId, mStep);

            Update();
        }

        public PackageShopData GetData()
        {
            return mData;
        }

        public void ReadyToSave()
        {
            Id = mId;
            Step = mStep;
            StackedPurchaseCount = mStackedPurchaseCount;
            PurchaseCount = mPurchaseCount;
            LastUpdateDateNum = mLastUpdateDateNum;

            MonthlyFeeLastUpdateNum = mMonthlyFeeEndDateNum;
            MonthlyFeeDailyRewardLastUpdateDateNum = mMonthlyFeeDailyRewardLastUpdateDateNum;
        }

        public void RandomizeKey()
        {
            mId.RandomizeCryptoKey();
            mStep.RandomizeCryptoKey();
            mStackedPurchaseCount.RandomizeCryptoKey();
            mPurchaseCount.RandomizeCryptoKey();
            mLastUpdateDateNum.RandomizeCryptoKey();

            mMonthlyFeeEndDateNum.RandomizeCryptoKey();
            mMonthlyFeeDailyRewardLastUpdateDateNum.RandomizeCryptoKey();
        }

        public bool Purchase()
        {
            // 구매 가능한 횟수가 충분한지 확인하자
            if (IsEnoughPurchaseCount() == false)
                return false;

            // 구매 카운트 증가
            mPurchaseCount += 1;
            mStackedPurchaseCount += 1;

            if (mData.type == PackageType.StepReward)
            {
                NextStep();
            }
            else if (mData.type == PackageType.MonthlyFee)
            {
                var endDate = TimeUtil.EndDateNum(Lance.GameData.ShopCommonData.monthlyFeeDurationDay);

                mMonthlyFeeEndDateNum = endDate;
            }

            return true;
        }

        public bool CheckMonthlyFeeDailyReward()
        {
            if (mData.type != PackageType.MonthlyFee)
                return false;

            int now = TimeUtil.NowDateNum();

            if (mMonthlyFeeEndDateNum <= now)
                return false;

            int rankNow = TimeUtil.RankNowDateNum();

            if (mMonthlyFeeDailyRewardLastUpdateDateNum > rankNow)
                return false;

            mMonthlyFeeDailyRewardLastUpdateDateNum = rankNow;

            return true;
        }

        public int GetMonthlyFeeEndDateNum()
        {
            return mMonthlyFeeEndDateNum;
        }

        public bool IsSoldOut()
        {
            return mData.resetType == PackageResetType.None && IsEnoughPurchaseCount() == false;
        }

        public bool IsActive()
        {
            if (mData.active == false)
                return false;

            if (mData.startDate > 0 && mData.endDate > 0)
            {
                if (TimeUtil.IsActiveDateNum(mData.startDate, mData.endDate) == false)
                    return false;
            }

            return true;
        }

        public int GetRemainPurchaseCount()
        {
            return mData.purchaseLimitCount - mPurchaseCount;
        }

        public bool IsEnoughPurchaseCount()
        {
            Update();

            return GetRemainPurchaseCount() > 0;
        }

        void Update()
        {
            if (mData.resetType == PackageResetType.Daily)
            {
                int nowDateNum = TimeUtil.NowDateNum();
                if (mLastUpdateDateNum < nowDateNum || mLastUpdateDateNum >= 20680101)
                {
                    mLastUpdateDateNum = nowDateNum;

                    mPurchaseCount = 0;
                }
            }
            else if (mData.resetType == PackageResetType.Weekly)
            {
                int thisWeekStartDateNum = TimeUtil.ThisWeekStartDateNum();

                // 이번주 월요일을 기준으로 주 시작날짜와 비교
                if (mLastUpdateDateNum < thisWeekStartDateNum || mLastUpdateDateNum >= 20680101) 
                {
                    mLastUpdateDateNum = thisWeekStartDateNum;

                    mPurchaseCount = 0;
                }
            }
            else if (mData.resetType == PackageResetType.Monthly)
            {
                int nowDateNum = TimeUtil.NowDateNum();
                var lastDateNumSplit = TimeUtil.SplitDateNum(mLastUpdateDateNum);
                var nowDateNumSplit = TimeUtil.SplitDateNum(nowDateNum);

                if (lastDateNumSplit.year < nowDateNumSplit.year || 
                    (lastDateNumSplit.year <= nowDateNumSplit.year && lastDateNumSplit.month < nowDateNumSplit.month))
                {
                    mLastUpdateDateNum = nowDateNum;

                    mPurchaseCount = 0;
                }
            }
            else if (mData.resetType == PackageResetType.MonthlyFee)
            {
                // 월정액 상품을 구매할 수 있는지 확인할 것
                int nowDateNum = TimeUtil.NowDateNum();
                
                if (mMonthlyFeeEndDateNum <= nowDateNum)
                {
                    mLastUpdateDateNum = nowDateNum;

                    mPurchaseCount = 0;
                }
            }
        }

        public string GetId()
        {
            return mId;
        }

        public int GetStep()
        {
            return mStep;
        }

        public int GetTotalPayments()
        {
            if (mData.type == PackageType.StepReward)
            {
                int totalPayments = 0;

                for (int i = 0; i < mStackedPurchaseCount; ++i)
                {
                    int step = i + 1;

                    var stepData = DataUtil.GetPackageShopData(mId, step);
                    if (stepData != null)
                        totalPayments += mData.price;
                }

                return totalPayments;
            }
            else
            {
                if (mData.resetType == PackageResetType.None)
                {
                    return mData.price * mPurchaseCount;
                }
                else
                {
                    return mData.price * mStackedPurchaseCount;
                }
            }
        }

        public int GetStackedPurchaseCount()
        {
            return mStackedPurchaseCount;
        }

        public void NextStep()
        {
            int maxStep = DataUtil.GetPackageShopDataMaxStep(mId);

            mStep += 1;
            if (mStep <= maxStep)
                mPurchaseCount = 0;

            mStep = Math.Min(maxStep, mStep);
        }

        public bool CanReceiveMonthlyFeeDailyReward()
        {
            if (mData.type != PackageType.MonthlyFee)
                return false;

            Update();

            if (mPurchaseCount <= 0)
                return false;

            int now = TimeUtil.NowDateNum();

            if (mMonthlyFeeEndDateNum <= now)
                return false;

            int rankNow = TimeUtil.RankNowDateNum();

            return mMonthlyFeeDailyRewardLastUpdateDateNum < rankNow;
        }

        public bool IsPurchasedMonthlyFee()
        {
            if (mData.type != PackageType.MonthlyFee)
                return false;

            Update();

            var now = TimeUtil.NowDateNum();

            return mPurchaseCount > 0 && mMonthlyFeeEndDateNum > now;
        }
    }
}