using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Lobby_SpawnUI : LobbyTabUI
    {
        SpawnTabUIManager mTabUIManager;
        public override void Init(LobbyTab tab)
        {
            base.Init(tab);

            mTabUIManager = new SpawnTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public void ChangeTab(SpawnTab tab)
        {
            mTabUIManager.ChangeTab(tab);
            mTabUIManager.RefreshTabFrame(tab);
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public override void OnUpdate()
        {
            mTabUIManager.OnUpdate();
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