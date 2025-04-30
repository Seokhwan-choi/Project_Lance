using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Popup_JoustingInfoUI : PopupBase
    {
        JoustingInfoTabUIManager mTabUIManager;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_JoustingInfo"));

            mTabUIManager = new JoustingInfoTabUIManager();
            mTabUIManager.Init(gameObject);
        }
    }
}