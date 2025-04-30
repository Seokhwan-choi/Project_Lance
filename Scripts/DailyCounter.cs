using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;

namespace Lance
{
    public class DailyCounter
    {
        public int LastUpdateDateNum;
        public int StackedCount;

        ObscuredInt mLastUpdateDateNum;
        ObscuredInt mStackedCount;

        public DailyCounter()
        {
            mLastUpdateDateNum = TimeUtil.NowDateNum();
            mStackedCount = 0;
        }

        public void ReadyToSave()
        {
            LastUpdateDateNum = mLastUpdateDateNum;
            StackedCount = mStackedCount;
        }

        public void SetServerDataToLocal(JsonData data)
        {
            int dateNumTemp = 0;
            int.TryParse(data["LastUpdateDateNum"].ToString(), out dateNumTemp);

            if (dateNumTemp >= 20680101)
                dateNumTemp = TimeUtil.NowDateNum() - 1;

            mLastUpdateDateNum = dateNumTemp;

            int stackedCountTemp = 0;
            int.TryParse(data["StackedCount"].ToString(), out stackedCountTemp);
            mStackedCount = stackedCountTemp;

            if (mLastUpdateDateNum == 0)
            {
                mLastUpdateDateNum = TimeUtil.NowDateNum();
            }
        }

        public void RandomizeKey()
        {
            mLastUpdateDateNum.RandomizeCryptoKey();
            mStackedCount.RandomizeCryptoKey();
        }

        public bool IsMaxCount(int maxCount)
        {
            Update();

            return mStackedCount == maxCount;
        }

        public bool StackCount(int maxCount, int count = 1)
        {
            if (IsMaxCount(maxCount))
                return false;

            mStackedCount += count;

            return true;
        }

        public int GetRemainCount(int maxCount)
        {
            Update();

            return Mathf.Max(0, maxCount - mStackedCount);
        }

        public int GetStackedCount()
        {
            return mStackedCount;
        }

        void Update()
        {
            int nowDateNum = TimeUtil.NowDateNum();

            int dateNumGap = mLastUpdateDateNum - nowDateNum;
            if (dateNumGap > 2 * 10000)
            {
                Refresh();
            }

            if (mLastUpdateDateNum < nowDateNum)
            {
                Refresh();
            }

            void Refresh()
            {
                mLastUpdateDateNum = nowDateNum;

                mStackedCount = 0;
            }
        }
    }
}