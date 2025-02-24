using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class SkillInventory_PassiveTabUI : SkillInventoryTabUI
    {
        public override void Init(SkillInventoryTab tab, SkillInventoryTabUIManager parent)
        {
            base.Init(tab, parent);

            InitItemUIList(Lance.GameData.PassiveSkillData.Values);
        }
    }
}