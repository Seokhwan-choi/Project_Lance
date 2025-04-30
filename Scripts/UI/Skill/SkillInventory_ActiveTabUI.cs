using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class SkillInventory_ActiveTabUI : SkillInventoryTabUI
    {
        public override void Init(SkillInventoryTab tab, SkillInventoryTabUIManager parent)
        {
            base.Init(tab, parent);

            InitItemUIList(Lance.GameData.ActiveSkillData.Values);
        }
    }
}