using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class PostNavBarUI
    {
        PostTabUIManager mParent;
        public void Init(PostTabUIManager parent, GameObject go)
        {
            mParent = parent;

            for (int i = 0; i < (int)PostTab.Count; ++i)
            {
                PostTab tab = (PostTab)i;

                string tabName = $"{tab}";

                Button buttonNav = go.FindComponent<Button>(tabName);

                buttonNav.SetButtonAction(() => OnButtonAction(tab));
            }
        }

        public void OnButtonAction(PostTab tab)
        {
            mParent.ChangeTab(tab);
        }
    }
}


