using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class PetInventory_FireTabUI : PetInventoryTabUI
    {
        public override void Init(PetInventoryTab tab)
        {
            base.Init(tab);

            InitItemUIList(Lance.GameData.PetEquipmentData.Values.Where(x => x.type == ElementalType.Fire));
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