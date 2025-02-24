using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;
using System;
using System.Linq;

namespace Lance
{
    public class DemonicRealm : AccountBase
    {
        Dictionary<StageType, DemonicRealmInfo> mDemonicRealmInfos;
        public DemonicRealm()
        {
            mDemonicRealmInfos = new Dictionary<StageType, DemonicRealmInfo>();
        }

        public bool CanChangeStep(StageType type, int step)
        {
            var info = mDemonicRealmInfos.TryGet(type);

            return info?.CanChangeStep(step) ?? false;
        }

        public int GetRemainWatchAdCount(StageType type)
        {
            var info = mDemonicRealmInfos.TryGet(type);

            return info?.GetRemainWatchAdCount() ?? 0;
        }

        public bool IsEnoughWatchAdCount(StageType type)
        {
            var info = mDemonicRealmInfos.TryGet(type);

            return info?.IsEnoughWatchAdCount() ?? false;
        }

        public void StackClearCount(StageType type)
        {
            var info = mDemonicRealmInfos.TryGet(type);

            info?.StackClearCount();

            SetIsChangedData(true);
        }

        public void StackWatchAdCount(StageType type, int watchAdCount = 1)
        {
            var info = mDemonicRealmInfos.TryGet(type);

            if (info?.IsEnoughWatchAdCount() ?? false)
            {
                info?.StackWatchAdCount(watchAdCount);

                SetIsChangedData(true);
            }
        }

        public void NextStep(StageType type)
        {
            var info = mDemonicRealmInfos.TryGet(type);

            info?.NextStep();
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var info in mDemonicRealmInfos.Values)
            {
                info.RandomizeKey();
            }
        }

        public int GetTotalStackedClearCount()
        {
            int totalClearCount = 0;

            foreach (var DemonicRealmInfo in mDemonicRealmInfos.Values)
            {
                totalClearCount += DemonicRealmInfo.GetStackedClearCount();
            }

            return totalClearCount;
        }

        public int GetStackedClearCount(StageType stageType)
        {
            var DemonicRealmInfo = mDemonicRealmInfos.TryGet(stageType);

            return DemonicRealmInfo?.GetStackedClearCount() ?? 0;
        }

        public int GetBestStep(StageType stageType)
        {
            var DemonicRealmInfo = mDemonicRealmInfos.TryGet(stageType);

            return DemonicRealmInfo?.GetBestStep() ?? 1;
        }

        public void SetBestStep(StageType stageType, int step)
        {
            var DemonicRealmInfo = mDemonicRealmInfos.TryGet(stageType);

            DemonicRealmInfo?.SetBestStep(step);
        }

        public override string GetTableName()
        {
            return "DemonicRealm";
        }

        public override Param GetParam()
        {
            Param param = new Param();

            foreach (var info in mDemonicRealmInfos.Values)
            {
                info.ReadyToSave();
            }

            param.Add("DemonicRealmInfos", mDemonicRealmInfos);

            return param;
        }

        protected override void InitializeData()
        {
            foreach (var data in Lance.GameData.DemonicRealmData.Values)
            {
                if (mDemonicRealmInfos.ContainsKey(data.type) == false)
                {
                    var info = new DemonicRealmInfo();

                    info.Init(data.type, 1, 0, new DailyCounter());

                    mDemonicRealmInfos.Add(data.type, info);
                }
            }
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            for (int i = 0; i < gameDataJson["DemonicRealmInfos"].Count; ++i)
            {
                var DemonicRealmInfoData = gameDataJson["DemonicRealmInfos"][i];

                if (Enum.TryParse(DemonicRealmInfoData["StageType"].ToString(), out StageType stageType))
                {
                    var data = Lance.GameData.DemonicRealmData.TryGet(stageType);
                    if (data == null)
                        continue;

                    int bestStepTemp = 0;

                    int.TryParse(DemonicRealmInfoData["BestStep"].ToString(), out bestStepTemp);

                    int stackedClearCountTemp = 0;

                    int.TryParse(DemonicRealmInfoData["StackedClearCount"].ToString(), out stackedClearCountTemp);

                    var dailyCounter = new DailyCounter();

                    dailyCounter.SetServerDataToLocal(DemonicRealmInfoData["WatchAdDailyCounter"]);

                    var info = new DemonicRealmInfo();

                    info.Init(stageType, bestStepTemp, stackedClearCountTemp, dailyCounter);

                    mDemonicRealmInfos.Add(stageType, info);
                }
            }
        }
    }

    public class DemonicRealmInfo
    {
        public StageType StageType;
        public int BestStep;
        public int StackedClearCount;
        public DailyCounter WatchAdDailyCounter;

        ObscuredInt mStageType;
        ObscuredInt mBestStep;
        ObscuredInt mStackedClearCount;
        DailyCounter mWatchAdDailyCounter;
        DemonicRealmData Data => Lance.GameData.DemonicRealmData.TryGet((StageType)(int)mStageType);
        public void Init(StageType stageType, int bestStep, int stackedClearCount, DailyCounter watchAdDailyCounter)
        {
            mStageType = (int)stageType;
            mBestStep = bestStep;
            mStackedClearCount = stackedClearCount;
            mWatchAdDailyCounter = watchAdDailyCounter;
        }

        public void NextStep()
        {
            SetBestStep(mBestStep + 1);
        }

        public void SetBestStep(int bestStep)
        {
            bestStep = Mathf.Min(bestStep, Data.maxStep + 1);

            mBestStep = bestStep;
        }

        public bool CanChangeStep(int step)
        {
            return step >= 1 && mBestStep <= step;
        }

        public int GetBestStep()
        {
            return mBestStep;
        }

        public bool IsEnoughWatchAdCount()
        {
            if (Data == null)
                return false;

            int maxCount = Data.dailyWatchAdCount;

            return mWatchAdDailyCounter.IsMaxCount(maxCount) == false;
        }

        public int GetStackedClearCount()
        {
            return mStackedClearCount;
        }

        public void StackClearCount()
        {
            mStackedClearCount++;
        }

        public bool StackWatchAdCount(int stackCount = 1)
        {
            if (Data == null)
                return false;

            int maxCount = Data.dailyWatchAdCount;

            return mWatchAdDailyCounter.StackCount(maxCount, stackCount);
        }

        public int GetRemainWatchAdCount()
        {
            if (Data == null)
                return 0;

            int maxCount = Data.dailyWatchAdCount;

            return mWatchAdDailyCounter.GetRemainCount(maxCount);
        }

        public void RandomizeKey()
        {
            mStageType.RandomizeCryptoKey();
            mBestStep.RandomizeCryptoKey();
            mStackedClearCount.RandomizeCryptoKey();
            mWatchAdDailyCounter.RandomizeKey();
        }

        public void ReadyToSave()
        {
            StageType = (StageType)(int)mStageType;
            BestStep = mBestStep;
            StackedClearCount = mStackedClearCount;

            mWatchAdDailyCounter.ReadyToSave();
            WatchAdDailyCounter = mWatchAdDailyCounter;
        }
    }
}