using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class JoustRankInfo : AccountBase
    {
        ObscuredInt mRankScore;          // ·©Å· ½ºÄÚ¾î
        //ObscuredString mBodyCostume;     // Âø¿ëÁßÀÎ ÄÚ½ºÆ¬
        ObscuredString mExtraInfo;       // ÄÚ½ºÆ¬&ÇÁ·¹ÀÓ
        DailyCounter mDailyWatchAdCount; 
        public override string GetTableName()
        {
            return "JoustRankInfo";
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            mExtraInfo = string.Empty;
            mDailyWatchAdCount = new ();
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int rankScoreTemp = 0;

            int.TryParse(gameDataJson["RankScore"].ToString(), out rankScoreTemp);

            mRankScore = rankScoreTemp;

            mExtraInfo = string.Empty;

            if (gameDataJson.ContainsKey("BodyCostume"))
            {
                mExtraInfo = gameDataJson["BodyCostume"].ToString();
            }

            mDailyWatchAdCount = new();

            if (gameDataJson.ContainsKey("DailyWatchAdCount"))
            {
                mDailyWatchAdCount.SetServerDataToLocal(gameDataJson["DailyWatchAdCount"]);
            }
        }

        public override Param GetParam()
        {
            Param param = new Param();

            param.Add("RankScore", (int)mRankScore);
            param.Add("BodyCostume", mExtraInfo.ToString());

            mDailyWatchAdCount.ReadyToSave();
            param.Add("DailyWatchAdCount", mDailyWatchAdCount);

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mRankScore.RandomizeCryptoKey();
            mExtraInfo?.RandomizeCryptoKey();
            mDailyWatchAdCount.RandomizeKey();
        }

        public override bool CanUpdateRankScore()
        {
            return mRankScore > 0;
        }

        public void SetRankScore(int score)
        {
            mRankScore = score;

            SetIsChangedData(true);
        }

        public void AddRankScore(int score)
        {
            mRankScore += score;

            SetIsChangedData(true);
        }

        public int GetRankScore()
        {
            return mRankScore;
        }

        public void SetExtraInfo(string extraInfo)
        {
            mExtraInfo = extraInfo;

            SetIsChangedData(true);
        }

        public bool StackWatchAdCount()
        {
            return mDailyWatchAdCount.StackCount(Lance.GameData.JoustingCommonData.dailyWatchAdCount);
        }

        public bool IsEnoughWatchAdCount()
        {
            return GetRemainWatchAdCount() > 0;
        }

        public int GetRemainWatchAdCount()
        {
            return mDailyWatchAdCount.GetRemainCount(Lance.GameData.JoustingCommonData.dailyWatchAdCount);
        }
    }
}