using UnityEngine;
using TMPro;

namespace Lance
{
    class StatureCostume_CostumeShopTabUI : StatureCostumeTabUI
    {
        CostumeShopTabUIManager mTabUIManager;
        public override void Init(StatureCostumeTabUIManager parent, StatureCostumeTab tab)
        {
            base.Init(parent, tab);

            mTabUIManager = new CostumeShopTabUIManager();
            mTabUIManager.Init(this);
        }

        public override void Refresh()
        {
            base.Refresh();

            mTabUIManager.Refresh();
        }

        public override void Localize()
        {
            base.Localize();

            mTabUIManager.Localize();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            mTabUIManager.OnEnter();
        }
    }
}


