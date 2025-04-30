using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Lance
{
    class Adventure_DemonicRealmTabUI : AdventureTabUI
    {
        List<DemonicRealmItemUI> mDemonicRealmItemList;
        public override void Init(AdventureTab tab)
        {
            base.Init(tab);

            mDemonicRealmItemList = new List<DemonicRealmItemUI>();

            var demonicRealmItemListObj = gameObject.FindGameObject("DemonicRealmItemList");

            demonicRealmItemListObj.AllChildObjectOff();

            foreach (var data in Lance.GameData.DemonicRealmData.Values)
            {
                var demonicRealmItemObj = Util.InstantiateUI("DemonicRealmItemUI", demonicRealmItemListObj.transform);

                var demonicRealmItemUI = demonicRealmItemObj.GetOrAddComponent<DemonicRealmItemUI>();

                demonicRealmItemUI.Init(data);

                mDemonicRealmItemList.Add(demonicRealmItemUI);
            }
        }

        public override void OnEnter()
        {
            Refresh();

            RefreshContentsLockUI();
        }

        public override void RefreshContentsLockUI()
        {
            foreach (var demonicRealmItemUI in mDemonicRealmItemList)
            {
                demonicRealmItemUI.RefreshContentsLockUI();
            }
        }

        public override void Localize()
        {
            foreach(var itemUI in mDemonicRealmItemList)
            {
                itemUI.Localize();
            }
        }

        public override void Refresh()
        {
            if (ContentsLockUtil.IsLockContents(ContentsLockType.DemonicRealm) == false)
                Lance.Account.Currency.UpdateFreeDemonicRealmStone();

            foreach (var item in mDemonicRealmItemList)
            {
                item.Refresh();
            }
        }
    }
}