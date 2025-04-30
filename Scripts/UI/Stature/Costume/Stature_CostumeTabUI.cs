using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

namespace Lance
{
    class Stature_CostumeTabUI : StatureTabUI
    {
        StatureCostumeTabUIManager mTabUIManager;
        
        public override void Init(StatureTab tab)
        {
            base.Init(tab);

            mTabUIManager = new StatureCostumeTabUIManager();
            mTabUIManager.Init(this);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            mTabUIManager.OnEnter();
        }

        public override void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void RefreshRedDots()
        {
            base.RefreshRedDots();

            mTabUIManager.RefreshRedDots();
        }
    }
}