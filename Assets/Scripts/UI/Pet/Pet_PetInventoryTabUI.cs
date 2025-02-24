using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Pet_PetInventoryTabUI : PetTabUI
    {
        PetInventoryTabUIManager mTabUIManager;
        public override void Init(PetTab tab)
        {
            base.Init(tab);

            mTabUIManager = new PetInventoryTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Refresh();
        }

        public override void Refresh()
        {
            base.Refresh();

            mTabUIManager.Refresh();
        }

        public override void RefreshRedDots()
        {
            base.RefreshRedDots();

            mTabUIManager.RefreshRedDots();
        }
    }
}