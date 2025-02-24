using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Inventory_GlovesTabUI : InventoryTabUI
    {
        public override void Init(InventoryTab tab)
        {
            base.Init(tab);

            InitItemUIList(Lance.GameData.GlovesData.Values);
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Refresh()
        {
            foreach (var itemUI in mItemUIList)
            {
                itemUI.Refresh();
            }

            //mInfoUI.Refresh();
        }
    }
}