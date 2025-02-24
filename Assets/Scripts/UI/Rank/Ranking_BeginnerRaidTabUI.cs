using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Ranking_BeginnerRaidTabUI : RankingTabUI
    {
        public override void OnEnter()
        {
            base.OnEnter();

            Lance.GameManager.CheckQuest(QuestType.ConfirmRaidRank, 1);
        }
    }
}