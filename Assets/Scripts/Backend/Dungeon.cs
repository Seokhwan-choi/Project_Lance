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
    public class Dungeon : AccountBase
    {
        ObscuredDouble mRaidBossBestScore;
        ObscuredDouble[] mRaidBossBestScores;
        Dictionary<StageType, DungeonInfo> mDungeonInfos;

        public Dungeon()
        {
            mRaidBossBestScores = Enumerable.Repeat<ObscuredDouble>(0, (int)ElementalType.Count).ToArray();
            mDungeonInfos = new Dictionary<StageType, DungeonInfo>();
        }

        public bool CanChangeStep(StageType type, int step)
        {
            var info = mDungeonInfos.TryGet(type);

            return info?.CanChangeStep(step) ?? false;
        }

        public int GetRemainWatchAdCount(StageType type)
        {
            var info = mDungeonInfos.TryGet(type);

            return info?.GetRemainWatchAdCount() ?? 0;
        }

        public bool IsEnoughWatchAdCount(StageType type)
        {
            var info = mDungeonInfos.TryGet(type);

            return info?.IsEnoughWatchAdCount() ?? false;
        }

        public void UpdateRaidBossDamage(ElementalType type, double damage)
        {
            if (mRaidBossBestScore < damage)
            {
                SetRaidBossDamage(damage);
            }

            int typeInx = (int)type;

            if (mRaidBossBestScores.Length > typeInx && typeInx >= 0)
            {
                if (mRaidBossBestScores[typeInx] < damage)
                {
                    mRaidBossBestScores[typeInx] = damage;

                    SetIsChangedData(true);
                }
            }
        }

        public void SetRaidBossDamage(double damage)
        {
            mRaidBossBestScore = damage;

            SetIsChangedData(true);
        }


        public double GetRaidBossBestDamage(ElementalType type)
        {
            int typeInx = (int)type;

            if (mRaidBossBestScores.Length <= typeInx)
                return 0;

            return mRaidBossBestScores[typeInx];
        }

        public void StackClearCount(StageType type)
        {
            var info = mDungeonInfos.TryGet(type);

            info?.StackClearCount();

            SetIsChangedData(true);
        }

        public void StackWatchAdCount(StageType type, int watchAdCount = 1)
        {
            var info = mDungeonInfos.TryGet(type);

            if (info?.IsEnoughWatchAdCount() ?? false)
            {
                info?.StackWatchAdCount(watchAdCount);

                SetIsChangedData(true);
            }
        }

        public void NextStep(StageType type)
        {
            var info = mDungeonInfos.TryGet(type);

            info?.NextStep();
        }

        public override bool CanUpdateRankScore()
        {
            return mRaidBossBestScores.Any(x => x > Lance.GameData.RaidRankCommonData.rankStandardDamage);
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach (var info in mDungeonInfos.Values)
            {
                info.RandomizeKey();
            }
        }

        public int GetTotalStackedClearCount()
        {
            int totalClearCount = 0;

            foreach(var dungeonInfo in mDungeonInfos.Values)
            {
                totalClearCount += dungeonInfo.GetStackedClearCount();
            }

            return totalClearCount;
        }

        public int GetStackedClearCount(StageType stageType)
        {
            var dungeonInfo = mDungeonInfos.TryGet(stageType);

            return dungeonInfo?.GetStackedClearCount() ?? 0;
        }

        public int GetBestStep(StageType stageType)
        {
            var dungeonInfo = mDungeonInfos.TryGet(stageType);

            return dungeonInfo?.GetBestStep() ?? 1;
        }

        public void SetBestStep(StageType stageType, int step)
        {
            var dungeonInfo = mDungeonInfos.TryGet(stageType);

            dungeonInfo?.SetBestStep(step);
        }

        public override string GetTableName()
        {
            return "Dungeon";
        }

        public override Param GetParam()
        {
            Param param = new Param();

            foreach (var info in mDungeonInfos.Values)
            {
                info.ReadyToSave();
            }

            param.Add("DungeonInfos", mDungeonInfos);
            param.Add("BestScore", (double)mRaidBossBestScore);
            param.Add("BestScores", mRaidBossBestScores.Select(x => (double)x).ToArray());

            return param;
        }

        protected override void InitializeData()
        {
            foreach (var data in Lance.GameData.DungeonData.Values)
            {
                if (mDungeonInfos.ContainsKey(data.type) == false)
                {
                    var info = new DungeonInfo();

                    info.Init(data.type, 1, 0, new DailyCounter());

                    mDungeonInfos.Add(data.type, info);
                }
            }
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            double bestScoreTemp = 0;

            double.TryParse(gameDataJson["BestScore"].ToString(), out bestScoreTemp);

            mRaidBossBestScore = bestScoreTemp;

            for(int i = 0; i < gameDataJson["DungeonInfos"].Count; ++i)
            {
                var dungeonInfoData = gameDataJson["DungeonInfos"][i];

                if (Enum.TryParse(dungeonInfoData["StageType"].ToString(), out StageType stageType))
                {
                    var data = Lance.GameData.DungeonData.TryGet(stageType);
                    if (data == null)
                        continue;

                    int bestStepTemp = 0;

                    int.TryParse(dungeonInfoData["BestStep"].ToString(), out bestStepTemp);

                    int stackedClearCountTemp = 0;

                    int.TryParse(dungeonInfoData["StackedClearCount"].ToString(), out stackedClearCountTemp);

                    var dailyCounter = new DailyCounter();

                    dailyCounter.SetServerDataToLocal(dungeonInfoData["WatchAdDailyCounter"]);

                    var info = new DungeonInfo();

                    info.Init(stageType, bestStepTemp, stackedClearCountTemp, dailyCounter);

                    mDungeonInfos.Add(stageType, info);
                }
            }

            if (gameDataJson.ContainsKey("BestScores"))
            {
                for (int i = 0; i < gameDataJson["BestScores"].Count; ++i)
                {
                    if (mRaidBossBestScores.Length <= i)
                        break;

                    double bestScoresTemp = 0;

                    double.TryParse(gameDataJson["BestScores"][i].ToString(), out bestScoresTemp);

                    mRaidBossBestScores[i] = bestScoresTemp;
                }
            }

            if (mDungeonInfos.ContainsKey(StageType.Pet) == false)
            {
                var info = new DungeonInfo();

                info.Init(StageType.Pet, 1, 0, new DailyCounter());

                mDungeonInfos.Add(StageType.Pet, info);
            }

            if (mDungeonInfos.ContainsKey(StageType.Reforge) == false)
            {
                var info = new DungeonInfo();

                info.Init(StageType.Reforge, 1, 0, new DailyCounter());

                mDungeonInfos.Add(StageType.Reforge, info);
            }

            if (mDungeonInfos.ContainsKey(StageType.Growth) == false)
            {
                var info = new DungeonInfo();

                info.Init(StageType.Growth, 1, 0, new DailyCounter());

                mDungeonInfos.Add(StageType.Growth, info);
            }

            if (mDungeonInfos.ContainsKey(StageType.Ancient) == false)
            {
                var info = new DungeonInfo();

                info.Init(StageType.Ancient, 1, 0, new DailyCounter());

                mDungeonInfos.Add(StageType.Ancient, info);
            }
        }
    }

    public class DungeonInfo
    {
        public StageType StageType;
        public int BestStep;
        public int StackedClearCount;
        public DailyCounter WatchAdDailyCounter;
        
        ObscuredInt mStageType;
        ObscuredInt mBestStep;
        ObscuredInt mStackedClearCount;
        DailyCounter mWatchAdDailyCounter;
        DungeonData Data => Lance.GameData.DungeonData.TryGet((StageType)(int)mStageType);
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