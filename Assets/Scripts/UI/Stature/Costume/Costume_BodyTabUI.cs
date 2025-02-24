using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Costume_BodyTabUI : CostumeTabUI
    {
        public override void Init(CostumeTabUIManager parent, CostumeTab tab)
        {
            base.Init(parent, tab);

            RefreshCostumeItemUIs(Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Body));
        }

        public override void RefreshCostume()
        {
            OnSelectCostume(Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Body));
        }
    }
}