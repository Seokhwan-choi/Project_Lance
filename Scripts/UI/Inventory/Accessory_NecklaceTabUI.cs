using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Accessory_NecklaceTabUI : AccessoryInventoryTabUI
    {
        public override void Init(AccessoryInventoryTab tab)
        {
            base.Init(tab);

            InitItemUIList(Lance.GameData.NecklaceData.Values);
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
        }
    }
}