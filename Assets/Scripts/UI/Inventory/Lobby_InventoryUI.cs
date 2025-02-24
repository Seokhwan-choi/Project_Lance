using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Lobby_InventoryUI : LobbyTabUI
    {
        InventoryTabUIManager mTabUIManager;
        public override void Init(LobbyTab tab)
        {
            base.Init(tab);

            mTabUIManager = new InventoryTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void OnEnter()
        {
            mTabUIManager.Refresh();
        }

        public override void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public void ChangeTab(InventoryTab tab)
        {
            mTabUIManager.ChangeTab(tab);
            mTabUIManager.RefreshActiveTab();
        }

        public InventoryTabUI GetTab(InventoryTab tab)
        {
            return mTabUIManager.GetTab(tab);
        }

        public Inventory_EquipmentItemUI GetBestEquipmentItemUI()
        {
            return mTabUIManager.GetBestEquipmentItemUI();
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