using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;
using BackEnd.Leaderboard;

namespace Lance
{
    class DynamicJoustingRankingItemUI : JoustingRankingItemUI, IDynamicScrollViewItem
    {
        public new void Init()
        {
            base.Init();
        }

        public void OnUpdateItem(int index)
        {
            if (mParent != null)
            {
                UserLeaderboardItem rankItem = mParent.GetRankItem(index);

                if (rankItem != null)
                {
                    UserLeaderboardItem myRankItem = mParent.GetMyRankItem();

                    bool isMyRank = rankItem.nickname == (myRankItem?.nickname ?? string.Empty);

                    OnUpdate(rankItem, isMyRank);
                }
            }
        }
    }
}


