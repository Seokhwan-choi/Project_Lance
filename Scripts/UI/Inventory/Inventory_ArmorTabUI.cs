using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Inventory_ArmorTabUI : InventoryTabUI
    {
        public override void Init(InventoryTab tab)
        {
            base.Init(tab);

            InitItemUIList(Lance.GameData.ArmorData.Values);
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Refresh()
        {
            //mInfoUI.Refresh();

            foreach (var itemUI in mItemUIList)
            {
                itemUI.Refresh();
            }
        }
    }
}