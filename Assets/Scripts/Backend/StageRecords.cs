using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BackEnd;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class StageRecords : AccountBase
    {
        ObscuredInt mCurrentTotalStage;
        ObscuredInt mBestTotalStage;

        DailyCounter mSkipBattleCounter = new();

        public StageDifficulty GetCurDifficulty()
        {
            return GetDifficulty(mCurrentTotalStage);
        }

        public int GetCurChapter()
        {
            return GetChapter(mCurrentTotalStage);
        }

        public int GetCurStage()
        {
            return GetStage(mCurrentTotalStage);
        }

        public int GetCurTotalStage()
        {
            return mCurrentTotalStage;
        }

        public StageDifficulty GetBestDifficulty()
        {
            return GetDifficulty(mBestTotalStage);
        }

        public int GetBestChapter()
        {
            return GetChapter(mBestTotalStage);
        }

        public int GetBestStage()
        {
            return GetStage(mBestTotalStage);
        }

        public int GetBestTotalStage()
        {
            return mBestTotalStage;
        }

        StageDifficulty GetDifficulty(int totalStage)
        {
            var result = StageRecordsUtil.SplitTotalStage(totalStage);

            return result.diff;
        }

        int GetChapter(int totalStage)
        {
            var result = StageRecordsUtil.SplitTotalStage(totalStage);

            return result.chapter;
        }

        int GetStage(int totalStage)
        {
            var result = StageRecordsUtil.SplitTotalStage(totalStage);

            return result.stage;
        }

        public StageData GetCurrentStageData()
        {
            var result = StageRecordsUtil.SplitTotalStage(mCurrentTotalStage);

            return DataUtil.GetStageData(result.diff, result.chapter, result.stage);
        }

        public bool CanChangeStage(StageDifficulty diff, int chapter, int stage)
        {
            int totalStage = StageRecordsUtil.CalcTotalStage(diff, chapter, stage);
            if (totalStage > mBestTotalStage)
                return false;

            return true;
        }

        public bool ChangeStage(StageDifficulty diff, int chapter, int stage)
        {
            if (CanChangeStage(diff, chapter, stage) == false)
                return false;

            int changeTotalStage = StageRecordsUtil.CalcTotalStage(diff, chapter, stage);

            mCurrentTotalStage = changeTotalStage;

            SetIsChangedData(true);

            return true;
        }

        public void NextStage()
        {
            mCurrentTotalStage++;

            UpdateNewBestRecord();

            mCurrentTotalStage = Mathf.Min(mCurrentTotalStage, StageRecordsUtil.GetMaxTotalStage());

            SetIsChangedData(true);
        }

        public void SetBestStage(int stage)
        {
            mBestTotalStage = stage;
        }

        public bool UpdateRecord(int totalStage)
        {
            var result = StageRecordsUtil.SplitTotalStage(totalStage);

            // �������� �����Ͱ� �����ϴ��� Ȯ���Ѵ�.
            var data = DataUtil.GetStageData(result.diff, result.chapter, result.stage);
            if (data == null)
                return false;

            mCurrentTotalStage = totalStage;

            UpdateNewBestRecord();

            SetIsChangedData(true);

            return true;
        }

        void UpdateNewBestRecord()
        {
            if (mCurrentTotalStage > mBestTotalStage)
                mBestTotalStage = mCurrentTotalStage;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mCurrentTotalStage.RandomizeCryptoKey();
            mBestTotalStage.RandomizeCryptoKey();
            mSkipBattleCounter?.RandomizeKey();
        }

        public override string GetTableName()
        {
            return "StageRecords";
        }

        public override bool CanUpdateRankScore()
        {
            return mCurrentTotalStage > 0;
        }

        protected override void InitializeData()
        {
            UpdateRecord(1);
        }

        public override Param GetParam()
        {
            var param = new Param();
            param.Add("curTotalStage", (int)mCurrentTotalStage);
            param.Add("bestTotalStage", (int)mBestTotalStage);

            if (mSkipBattleCounter != null)
            {
                mSkipBattleCounter.ReadyToSave();
                param.Add("skipBattleCounter", mSkipBattleCounter);
            }

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int curTotalStageTemp = 0;
            int.TryParse(gameDataJson["curTotalStage"].ToString(), out curTotalStageTemp);
            mCurrentTotalStage = curTotalStageTemp;

            int bestTotalStageTemp = 0;
            int.TryParse(gameDataJson["bestTotalStage"].ToString(), out bestTotalStageTemp);
            mBestTotalStage = bestTotalStageTemp;

            if (gameDataJson.ContainsKey("skipBattleCounter"))
            {
                mSkipBattleCounter.SetServerDataToLocal(gameDataJson["skipBattleCounter"]);
            }
        }

        public bool SkipBattle()
        {
            int maxCount = Lance.GameData.SkipBattleCommonData.dailyMaxCount;

            if (mSkipBattleCounter?.IsMaxCount(maxCount) ?? false)
                return false;

            SetIsChangedData(true);

            return mSkipBattleCounter?.StackCount(maxCount) ?? false;
        }

        public int GetSkipBattleRemainCount()
        {
            int maxCount = Lance.GameData.SkipBattleCommonData.dailyMaxCount;

            return mSkipBattleCounter?.GetRemainCount(maxCount) ?? 0;
        }

        public int GetSkipBattleStackedCount()
        {
            return mSkipBattleCounter?.GetStackedCount() ?? 0;
        }

        void Compare()
        {
            //mCurrent_Difficulty = (int)diff;
            //mCurrent_Chapter = chapter;
            //mCurrent_Stage = stage;

            //// 1. ���̵��� ���Ѵ�
            //if (mBest_Difficulty <= mCurrent_Difficulty)
            //{
            //    // �ְ� ��Ϻ��� ���� ���̵��� ���ٸ� �ְ� ��� ����
            //    if (mBest_Difficulty < mCurrent_Difficulty)
            //    {
            //        UpdateNewBestRecord();
            //    }
            //    // ���̵��� ���ٸ�  
            //    else
            //    {
            //        // 2. é�͸� ���Ѵ�. 
            //        if (mBest_Chapter <= mCurrent_Chapter)
            //        {
            //            // �ְ� ��Ϻ��� ���� é�Ͱ� ���ٸ� �ְ� ��� ����
            //            if (mBest_Chapter < mCurrent_Chapter)
            //            {
            //                UpdateNewBestRecord();
            //            }
            //            // é�Ͱ� ���ٸ�
            //            else
            //            {
            //                // 3. ���������� ���Ѵ�.
            //                if (mBest_Stage <= mCurrent_Stage)
            //                {
            //                    // �ְ� ��Ϻ��� ���� ���������� ���ٸ� �ְ� ��� ����
            //                    if (mBest_Chapter < mCurrent_Stage)
            //                    {
            //                        UpdateNewBestRecord();
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }
    }
}