using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Inventory_AccessoryTabUI : InventoryTabUI
    {
        AccessoryInventoryTabUIManager mTabUIManager;
        public override void Init(InventoryTab tab)
        {
            base.Init(tab);

            mTabUIManager = new AccessoryInventoryTabUIManager();
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
    }
}