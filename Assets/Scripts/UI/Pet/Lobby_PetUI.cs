using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class Lobby_PetUI : LobbyTabUI
    {
        PetTabUIManager mTabUIManager;
        public override void Init(LobbyTab tab)
        {
            base.Init(tab);

            mTabUIManager = new PetTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void Refresh()
        {
            base.Refresh();

            mTabUIManager.Refresh();
        }

        public override void OnEnter()
        {
            Refresh();

        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }

        public RectTransform GetBestPetItemUI()
        {
            return mTabUIManager.GetBestPetItemUI();
        }
    }
}