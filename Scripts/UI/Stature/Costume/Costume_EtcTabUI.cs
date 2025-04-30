using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Costume_EtcTabUI : CostumeTabUI
    {
        public override void Init(CostumeTabUIManager parent, CostumeTab tab)
        {
            base.Init(parent, tab);

            RefreshCostume();
        }

        public override void RefreshCostume()
        {
            RefreshCostumeItemUIs(Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Etc));
        }
    }
}