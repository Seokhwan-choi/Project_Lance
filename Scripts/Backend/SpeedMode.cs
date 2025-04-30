using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    public class SpeedMode : AccountBase
    {
        ObscuredInt mStackedActiveCount;
        ObscuredFloat mDurationTime;   // 버프가 지속시간 종료시간
        ObscuredBool mIsActive;
        ObscuredBool mFirstActive;

        public bool InSpeedMode()
        {
            return mIsActive && mDurationTime > 0;
        }
        
        public bool GetFirstActive()
        {
            return mFirstActive;
        }

        public float GetDuration()
        {
            return mDurationTime;
        }

        public bool GetActive()
        {
            return mIsActive;
        }

        public float GetDurationTime()
        {
            return mDurationTime;
        }

        public void OnUpdate(float time, bool purchasedRemovedAD)
        {
            if (mIsActive)
            {
                if (mDurationTime > 0f && !purchasedRemovedAD)
                {
                    mDurationTime -= time;
                    // 스피드모드 끗
                    if (mDurationTime <= 0f)
                    {
                        mDurationTime = 0f;

                        mIsActive = false;

                        Lance.GameManager.OnChangeSpeedMode();
                    }

                    SetIsChangedData(true);
                }
            }
        }

        public void ToggleActive()
        {
            if (mDurationTime > 0)
            {
                mIsActive = !mIsActive;
            }
            else
            {
                mIsActive = false;
            }

            SetIsChangedData(true);
        }

        public bool ActiveSpeedMode()
        {
            if (mDurationTime > 0)
                return false;

            mDurationTime = Lance.GameData.CommonData.speedModeDuration;

            mStackedActiveCount++;

            mIsActive = true;
            mFirstActive = true;

            SetIsChangedData(true);

            return true;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mStackedActiveCount.RandomizeCryptoKey();
            mDurationTime.RandomizeCryptoKey();
            mIsActive.RandomizeCryptoKey();
            mFirstActive.RandomizeCryptoKey();
        }

        public override string GetTableName()
        {
            return "SpeedMode";
        }

        public override Param GetParam()
        {
            Param param = new Param();

            param.Add("StackedActiveCount", (int)mStackedActiveCount);
            param.Add("Duration", (float)mDurationTime);
            param.Add("IsActive", (bool)mIsActive);
            param.Add("FirstActive", (bool)mFirstActive);

            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int stackedActiveCountTemp = 0;

            int.TryParse(gameDataJson["StackedActiveCount"].ToString(), out stackedActiveCountTemp);

            mStackedActiveCount = stackedActiveCountTemp;

            float durationTimeTemp = 0;

            float.TryParse(gameDataJson["Duration"].ToString(), out durationTimeTemp);

            mDurationTime = durationTimeTemp;

            bool isActiveTemp = false;

            bool.TryParse(gameDataJson["IsActive"].ToString(), out isActiveTemp);

            mIsActive = isActiveTemp;

            if (gameDataJson.ContainsKey("FirstActive"))
            {
                bool firstActiveTemp = false;

                bool.TryParse(gameDataJson["FirstActive"].ToString(), out firstActiveTemp);

                mFirstActive = firstActiveTemp;
            }
        }
    }
}