using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class RankingNavBarUI
    {
        RankingTabUIManager mParent;
        public void Init(RankingTabUIManager parent, GameObject go)
        {
            mParent = parent;

            for (int i = 0; i < (int)RankingTab.Count; ++i)
            {
                RankingTab tab = (RankingTab)i;

                string tabName = $"{tab}";

                Button buttonNav = go.FindComponent<Button>(tabName);

                buttonNav.SetButtonAction(() => OnButtonAction(tab));
            }
        }

        public void OnButtonAction(RankingTab tab)
        {
            mParent.ChangeTab(tab);
        }
    }
}