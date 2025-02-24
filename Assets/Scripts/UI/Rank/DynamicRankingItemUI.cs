using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;
using BackEnd.Leaderboard;

namespace Lance
{
    class DynamicRankingItemUI : RankingItemUI, IDynamicScrollViewItem
    {
        public new void Init()
        {
            base.Init();
        }

        public void OnUpdateItem(int index)
        {
            if (mParent != null)
            {
                UserLeaderboardItem rankItem = mParent.GetUserLeaderboardItem(index);

                if (rankItem != null)
                {
                    UserLeaderboardItem myRankItem = mParent.GetMyUserLeaderboardItem();

                    bool isMyRank = rankItem.nickname == (myRankItem?.nickname ?? string.Empty);

                    OnUpdate(rankItem, isMyRank);
                }
            }
        }
    }
}