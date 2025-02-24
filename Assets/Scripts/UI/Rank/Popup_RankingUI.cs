using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Popup_RankingUI : PopupBase
    {
        RankingTabUIManager mTabUIManager;
        public void Init(RankingTab tab = RankingTab.Stage)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Ranking"));

            mTabUIManager = new RankingTabUIManager();
            mTabUIManager.Init(gameObject, tab);
        }
    }
}