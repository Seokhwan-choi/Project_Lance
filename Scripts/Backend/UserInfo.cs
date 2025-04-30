using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;

namespace Lance
{
    public class UserInfo : AccountBase
    {
        ObscuredInt mLastUpdateOnlineTime;
        ObscuredInt mPlayTime;
        ObscuredInt mStackedWatchAdCount;
        ObscuredInt mStackedPayments;
        ObscuredInt mStackedChatCount;
        DailyCounter mDailyChatCounter;
        ObscuredBool mPushAllow;

        List<string> mConfirmedMaintanceInfoList = new();

        ObscuredInt mLastUpdateLoginDateNum;
        ObscuredInt mLoginCount;

        ObscuredBool mIsFinishedRaidBadProcess;

        ObscuredDouble mPowerLevel;
        ObscuredDouble mBestPowerLevel;
        bool mFirstJoustingUpdate;
        bool mResetBestPowerLevel;
        public override string GetTableName() 
        { 
            return "UserInfo"; 
        }
        public override bool CanUpdateRankScore()
        {
            return mBestPowerLevel > 0;
        }
        protected override void InitializeData()
        {
            mResetBestPowerLevel = true;
            mFirstJoustingUpdate = false;
            mIsFinishedRaidBadProcess = true;
            mDailyChatCounter = new DailyCounter();
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int lastUpdateOnlineTimeTemp = 0;

            int.TryParse(gameDataJson["LastUpdateOnlineTime"].ToString(), out lastUpdateOnlineTimeTemp);

            mLastUpdateOnlineTime = lastUpdateOnlineTimeTemp;

            int playTimeTemp = 0;

            int.TryParse(gameDataJson["PlayTime"].ToString(), out playTimeTemp);

            mPlayTime = playTimeTemp;

            int stackedWatchAdCountTemp = 0;

            int.TryParse(gameDataJson["StackedWatchAdCount"].ToString(), out stackedWatchAdCountTemp);

            mStackedWatchAdCount = stackedWatchAdCountTemp;

            int stackedPaymentsTemp = 0;

            int.TryParse(gameDataJson["StackedPayments"].ToString(), out stackedPaymentsTemp);

            mStackedPayments = stackedPaymentsTemp;

            bool pushAllowTemp = false;

            bool.TryParse(gameDataJson["PushAllow"].ToString(), out pushAllowTemp);

            mPushAllow = pushAllowTemp;

            if (gameDataJson.ContainsKey("PowerLevel"))
            {
                double powerLevelTemp = 0;

                double.TryParse(gameDataJson["PowerLevel"].ToString(), out powerLevelTemp);

                mPowerLevel = powerLevelTemp;
            }
            
            if (gameDataJson.ContainsKey("BestPowerLevel"))
            {
                double bestPowerLevelTemp = 0;

                double.TryParse(gameDataJson["BestPowerLevel"].ToString(), out bestPowerLevelTemp);

                mBestPowerLevel = bestPowerLevelTemp;
            }

            if (gameDataJson.ContainsKey("LastUpdateLoginDateNum"))
            {
                int lastUpdateLoginDateNumTemp = 0;

                int.TryParse(gameDataJson["LastUpdateLoginDateNum"].ToString(), out lastUpdateLoginDateNumTemp);

                if (lastUpdateLoginDateNumTemp >= 20680101)
                    lastUpdateLoginDateNumTemp = TimeUtil.NowDateNum() - 1;

                mLastUpdateLoginDateNum = lastUpdateLoginDateNumTemp;
            }

            if (gameDataJson.ContainsKey("LoginCount"))
            {
                int loginCountTemp = 0;

                int.TryParse(gameDataJson["LoginCount"].ToString(), out loginCountTemp);

                mLoginCount = loginCountTemp;
            }

            if (gameDataJson.ContainsKey("ResetBestPowerLevel"))
            {
                bool resetBestPowerLevel = false;

                bool.TryParse(gameDataJson["ResetBestPowerLevel"].ToString(), out resetBestPowerLevel);

                mResetBestPowerLevel = resetBestPowerLevel;
            }

            if (mResetBestPowerLevel == false)
            {
                mBestPowerLevel = 0;

                mResetBestPowerLevel = true;

                SetIsChangedData(true);
            }

            if (gameDataJson.ContainsKey("FirstJoustingUpdate"))
            {
                bool firstJoustingUpdateTemp = false;

                bool.TryParse(gameDataJson["FirstJoustingUpdate"].ToString(), out firstJoustingUpdateTemp);

                mFirstJoustingUpdate = firstJoustingUpdateTemp;
            }


            if (gameDataJson.ContainsKey("StackedChatCount"))
            {
                int stackedChatCountTemp = 0;

                int.TryParse(gameDataJson["StackedChatCount"].ToString(), out stackedChatCountTemp);

                mStackedChatCount = stackedChatCountTemp;
            }

            mDailyChatCounter = new DailyCounter();

            if (gameDataJson.ContainsKey("DailyChatCounter"))
            {
                mDailyChatCounter.SetServerDataToLocal(gameDataJson["DailyChatCounter"]);
            }

            if (gameDataJson.ContainsKey("ConfirmedMaintanceInfoList"))
            {
                foreach(var confirmedMaintanceInfo in gameDataJson["ConfirmedMaintanceInfoList"])
                {
                    string confirmedInfo = confirmedMaintanceInfo.ToString();

                    mConfirmedMaintanceInfoList.Add(confirmedInfo);
                }
            }

            if (gameDataJson.ContainsKey("IsFinishedRaidBadProcess"))
            {
                bool isFinishedRaidBadProcessTemp = false;

                bool.TryParse(gameDataJson["IsFinishedRaidBadProcess"].ToString(), out isFinishedRaidBadProcessTemp);

                mIsFinishedRaidBadProcess = isFinishedRaidBadProcessTemp;
            }
        }

        public override Param GetParam()
        {
            Param param = new Param();

            param.Add("LastUpdateOnlineTime", (int)mLastUpdateOnlineTime);
            param.Add("PlayTime", (int)mPlayTime);
            param.Add("StackedWatchAdCount", (int)mStackedWatchAdCount);
            param.Add("StackedPayments", (int)mStackedPayments);
            param.Add("PushAllow", (bool)mPushAllow);
            param.Add("PowerLevel", (double)mPowerLevel);
            param.Add("BestPowerLevel", (double)mBestPowerLevel);
            param.Add("LastUpdateLoginDateNum", (int)mLastUpdateLoginDateNum);
            param.Add("LoginCount", (int)mLoginCount);
            param.Add("ResetBestPowerLevel", mResetBestPowerLevel);
            param.Add("FirstJoustingUpdate", mFirstJoustingUpdate);
            param.Add("StackedChatCount", (int)mStackedChatCount);
            param.Add("IsFinishedRaidBadProcess", (bool)mIsFinishedRaidBadProcess);

            mDailyChatCounter.ReadyToSave();
            param.Add("DailyChatCounter", mDailyChatCounter);

            if (mConfirmedMaintanceInfoList != null)
                param.Add("ConfirmedMaintanceInfoList", mConfirmedMaintanceInfoList);

            //param.Add("StackedMonsterKillCount", (int)mStackedMonsterKillCount);
            //param.Add("StackedBossKillCount", (int)mStackedBossKillCount);

            return param;
        }

        public bool IsFinishedRaidBadProcess()
        {
            return mIsFinishedRaidBadProcess;
        }

        public void SetIsFinishedRaidBadProcess(bool set)
        {
            mIsFinishedRaidBadProcess = set;

            SetIsChangedData(true);
        }

        public bool IsConfirmedMaintanceInfo(string id)
        {
            return mConfirmedMaintanceInfoList.Contains(id);
        }

        public void ConfirmMaintanceInfo(string id)
        {
            if (IsConfirmedMaintanceInfo(id))
                return;

            mConfirmedMaintanceInfoList.Add(id);

            SetIsChangedData(true);
        }

        public int GetLastUpdateOnlineTime()
        {
            return mLastUpdateOnlineTime;
        }

        public int GetLoginCount()
        {
            return mLoginCount;
        }

        public bool UpdateLoginCount()
        {
            if (mLastUpdateLoginDateNum < TimeUtil.NowDateNum())
            {
                mLastUpdateLoginDateNum = TimeUtil.NowDateNum();

                mLoginCount++;

                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public void UpdateOnlineTime()
        {
            mLastUpdateOnlineTime = TimeUtil.Now;

            SetIsChangedData(true);
        }

        public void StackPlayTime(float playTime)
        {
            mPlayTime += (int)playTime;

            SetIsChangedData(true);
        }

        public int GetStackPlayTime()
        {
            return mPlayTime;
        }

        public void StackWatchAdCount(int count = 1)
        {
            mStackedWatchAdCount += 1;

            SetIsChangedData(true);
        }
        
        public void StackPayments(int payments)
        {
            mStackedPayments += payments;

            SetIsChangedData(true);
        }

        public bool StackChatCount(int stackCount)
        {
            if (mDailyChatCounter.StackCount(Lance.GameData.AchievementCommonData.dailyLimitChatMessageCount, stackCount))
            {
                mStackedChatCount += stackCount;

                SetIsChangedData(true);

                return true;
            }

            return false;
        }

        public int GetStackedPayments()
        {
            return mStackedPayments;
        }

        public int GetStackedChatCount()
        {
            return mStackedChatCount;
        }

        public double GetPlayTime()
        {
            return mPlayTime;
        }

        public int GetStackedWatchAdCount()
        {
            return mStackedWatchAdCount;
        }
        
        public bool GetPushAllow()
        {
            return mPushAllow;
        }

        public void SetPushAllow(bool allow)
        {
            mPushAllow = allow;

            SetIsChangedData(true);
        }

        public bool SetPowerLevel(double powerLevel)
        {
            SetIsChangedData(true);

            mPowerLevel = powerLevel;
            if (mBestPowerLevel < powerLevel)
            {
                mBestPowerLevel = powerLevel;

                return true;
            }
            else
            {
                return false;
            }
        }

        public double GetPowerLevel()
        {
            return mPowerLevel;
        }

        public double GetBestPowerLevel()
        {
            return mBestPowerLevel;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mLastUpdateOnlineTime.RandomizeCryptoKey();
            mPlayTime.RandomizeCryptoKey();
            mStackedWatchAdCount.RandomizeCryptoKey();
            mStackedPayments.RandomizeCryptoKey();
            mPushAllow.RandomizeCryptoKey();
            mPowerLevel.RandomizeCryptoKey();
            mBestPowerLevel.RandomizeCryptoKey();
        }

        public bool RegisterFirstJousting()
        {
            if (mFirstJoustingUpdate)
                return false;

            mFirstJoustingUpdate = true;

            SetIsChangedData(true);

            return true;
        }
    }
}