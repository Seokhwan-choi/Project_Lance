using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;

namespace Lance
{
    class JoustingInfo_RankingRewardTabUI : JoustingInfoTabUI
    {
        int mMyRank;
        DynamicVScrollView mScrollView;
        public int MyRank => mMyRank;
        public override void Init(JoustingInfoTabUIManager parent, JoustingInfoTab tab)
        {
            base.Init(parent, tab);

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            mScrollView.totalItemCount = 0;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            StartCoroutine(DelayedToScrollMyIndex());
        }

        IEnumerator DelayedToScrollMyIndex()
        {
            mScrollView.totalItemCount = Lance.GameData.JoustingRankingRewardData.Count;

            yield return null;

            var rankInfo = Lance.Account.Leaderboard.GetLeaderboardInfo("JoustRankInfo");

            var myRankItem = rankInfo.MyUserLeaderboardItem;
            if (myRankItem != null)
            {
                mMyRank = myRankItem.score.ToIntSafe();
            }

            mScrollView.scrollByItemIndex(0);
        }
    }
}