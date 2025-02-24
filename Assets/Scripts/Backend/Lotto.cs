using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using BackEnd;
using System.Linq;


namespace Lance
{
    public class Lotto : AccountBase
    {
        ObscuredInt mLastUpdateDateNum;   // ���������� ������Ʈ�� DateNum
        ObscuredInt mStackedCount;        // ���� ���� ���� Ƚ��
        ObscuredInt mDailyCount;          // ���� ���� ���� Ƚ��
        ObscuredInt mCoolTime;            // ������ ��Ÿ���� ����
        ObscuredInt[] mRewardIndexs;      // ���� ���� �ε��� ����

        public void Update() 
        {
            if (mLastUpdateDateNum < TimeUtil.NowDateNum() || mLastUpdateDateNum >= 20680101)
            {
                mDailyCount = 0;

                mLastUpdateDateNum = TimeUtil.NowDateNum();

                CreateNewRewards();
            }
        }

        // ���� ���ŵ� ��, ���� �̸� ����� ���� ������
        void CreateNewRewards()
        {
            mRewardIndexs = null;

            List<ObscuredInt> rewardIndexList = new List<ObscuredInt>();

            for(int i = 0; i < Lance.GameData.LottoCommonData.dailyMaxCount; ++i)
            {
                var randRewardIndex = Util.RandomChoose(Lance.GameData.LottoRewardData.Values.Select(x => x.prob).ToArray());

                rewardIndexList.Add(randRewardIndex);
            }

            mRewardIndexs = rewardIndexList.ToArray();

            SetIsChangedData(true);
        }

        public string DrawLotto()
        {
            // ���� ��밡���� Ƚ���� ������� Ȯ��
            if (mDailyCount > Lance.GameData.LottoCommonData.dailyMaxCount)
                return string.Empty;

            // ��Ÿ�� ������ Ȯ��
            if (mCoolTime > TimeUtil.Now)
                return string.Empty;

            if (mRewardIndexs.Length <= mDailyCount || mDailyCount < 0)
                return string.Empty;

            int rewardIndex = mRewardIndexs[mDailyCount];

            var rewardData = Lance.GameData.LottoRewardData.TryGet(rewardIndex);

            mDailyCount++;
            mStackedCount++;
            mCoolTime = TimeUtil.Now + Lance.GameData.LottoCommonData.coolTimeForMinute * TimeUtil.SecondsInMinute;

            SetIsChangedData(true);

            return rewardData.reward;
        }

        public int GetRemainDailyCount()
        {
            return Lance.GameData.LottoCommonData.dailyMaxCount - mDailyCount;
        }

        public int GetRemainCoolTime()
        {
            return mCoolTime - TimeUtil.Now;
        }

        public bool InCoolTime()
        {
            return mCoolTime > TimeUtil.Now;
        }

        public string GetNowReward()
        {
            var rewardIndex = GetNowRewardIndex();

            var rewardData = Lance.GameData.LottoRewardData.TryGet(rewardIndex);

            return rewardData?.reward ?? string.Empty;
        }

        public int GetNowRewardIndex()
        {
            if (mRewardIndexs.Length <= mDailyCount || mDailyCount < 0)
                return -1;

            return mRewardIndexs[mDailyCount];
        }

        protected override void InitializeData()
        {
            CreateNewRewards();
        }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            int lastUpdateDateNumTemp = 0;

            int.TryParse(gameDataJson["LastUpdateDateNum"].ToString(), out lastUpdateDateNumTemp);

            mLastUpdateDateNum = lastUpdateDateNumTemp;

            int stackedCountTemp = 0;

            int.TryParse(gameDataJson["StackedCount"].ToString(), out stackedCountTemp);

            mStackedCount = stackedCountTemp;

            int coolTimeTemp = 0;

            int.TryParse(gameDataJson["CoolTime"].ToString(), out coolTimeTemp);

            mCoolTime = coolTimeTemp;

            if (mLastUpdateDateNum < TimeUtil.NowDateNum() || mLastUpdateDateNum >= 20680101)
            {
                mDailyCount = 0;

                mLastUpdateDateNum = TimeUtil.NowDateNum();

                CreateNewRewards();
            }
            else
            {
                int dailyCountTemp = 0;

                int.TryParse(gameDataJson["DailyCount"].ToString(), out dailyCountTemp);

                mDailyCount = dailyCountTemp;

                if (gameDataJson.ContainsKey("RewardIndexs"))
                {
                    List<ObscuredInt> rewardIndexList = new List<ObscuredInt>();

                    foreach (var rewardIndexJsonData in gameDataJson["RewardIndexs"])
                    {
                        int rewardIndexTemp;

                        int.TryParse(rewardIndexJsonData.ToString(), out rewardIndexTemp);

                        rewardIndexList.Add(rewardIndexTemp);
                    }

                    mRewardIndexs = rewardIndexList.ToArray();

                    if (mRewardIndexs.Length != Lance.GameData.LottoCommonData.dailyMaxCount)
                    {
                        CreateNewRewards();
                    }
                }
                else
                {
                    CreateNewRewards();
                }
            }
        }

        public override string GetTableName()
        {
            return "Lotto";
        }

        public override Param GetParam()
        {
            Param param = new Param();
            param.Add("LastUpdateDateNum", (int)mLastUpdateDateNum);
            param.Add("StackedCount", (int)mStackedCount);
            param.Add("DailyCount", (int)mDailyCount);
            param.Add("CoolTime", (int)mCoolTime);
            
            if (mRewardIndexs != null)
                param.Add("RewardIndexs", mRewardIndexs.Select(x => (int)x));

            return param;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mLastUpdateDateNum.RandomizeCryptoKey();
            mStackedCount.RandomizeCryptoKey();
            mDailyCount.RandomizeCryptoKey();
            mCoolTime.RandomizeCryptoKey();

            if (mRewardIndexs != null)
            {
                foreach(var rewardIndex in mRewardIndexs)
                {
                    rewardIndex.RandomizeCryptoKey();
                }
            }
        }

        public bool CanDrawLotto()
        {
            return GetRemainDailyCount() > 0 && InCoolTime() == false;
        }
    }
}