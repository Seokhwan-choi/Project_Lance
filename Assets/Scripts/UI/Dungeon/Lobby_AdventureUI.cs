using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class Lobby_AdventureUI : LobbyTabUI
    {
        AdventureTabUIManager mTabUIManager;
        public override void Init(LobbyTab tab)
        {
            base.Init(tab);

            mTabUIManager = new AdventureTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public override void RefreshContentsLockUI()
        {
            mTabUIManager.RefreshContentsLockUI();
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }
    }
}