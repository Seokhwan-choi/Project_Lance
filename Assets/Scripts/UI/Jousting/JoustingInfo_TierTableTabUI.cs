using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;
using System.Linq;

namespace Lance
{
    class JoustingInfo_TierTableTabUI : JoustingInfoTabUI
    {
        JoustingTierData[] mDatas;
        int mMyScore;
        JoustingTier mMyTier;
        DynamicVScrollView mScrollView;
        public JoustingTier MyTier => mMyTier;
        public override void Init(JoustingInfoTabUIManager parent, JoustingInfoTab tab)
        {
            base.Init(parent, tab);

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            mScrollView.totalItemCount = 0;

            mDatas = Lance.GameData.JoustingTierData.OrderByDescending(x => x.rankScore).ToArray();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            StartCoroutine(DelayedToScrollMyIndex());
        }

        IEnumerator DelayedToScrollMyIndex()
        {
            var rankInfo = Lance.Account.Leaderboard.GetLeaderboardInfo("JoustRankInfo");

            var myRankItem = rankInfo.MyUserLeaderboardItem;
            if (myRankItem != null)
            {
                mMyScore = myRankItem.score.ToIntSafe();
                mMyTier = DataUtil.GetJoustingTier(mMyScore);
            }

            mScrollView.totalItemCount = Lance.GameData.JoustingTierData.Count;

            yield return null;

            //mScrollView.scrollByItemIndex(Lance.GameData.JoustingTierData.Count - 1);

            //var myTier = DataUtil.GetJoustingTier(mMyScore);

            //if (myTier != JoustingTier.None)
            //    mScrollView.scrollByItemIndex((int)myTier);
            //else
            //    mScrollView.scrollByItemIndex(0);
        }

        public JoustingTierData GetTierData(int index)
        {
            return mDatas[index];
        }
    }
}