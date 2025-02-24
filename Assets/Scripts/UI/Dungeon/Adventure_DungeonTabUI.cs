using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Lance
{
    class Adventure_DungeonTabUI : AdventureTabUI
    {
        List<DungeonItemUI> mDungeonItemList;
        public override void Init(AdventureTab tab)
        {
            base.Init(tab);

            mDungeonItemList = new List<DungeonItemUI>();

            var dungeonItemListObj = gameObject.FindGameObject("DungeonItemList");

            dungeonItemListObj.AllChildObjectOff();

            foreach (var data in Lance.GameData.DungeonData.Values)
            {
                var dungeonItemObj = Util.InstantiateUI("DungeonItemUI", dungeonItemListObj.transform);

                var dungeonItemUI = dungeonItemObj.GetOrAddComponent<DungeonItemUI>();

                dungeonItemUI.Init(data);

                mDungeonItemList.Add(dungeonItemUI);
            }

            //var buttonInfo = gameObject.FindComponent<Button>("Button_Info");
            //buttonInfo.SetButtonAction(() =>
            //{
            //    var popup = Lance.PopupManager.CreatePopup<Popup_DescUI>();

            //    popup.Init(StringTableUtil.Get("TabName_Dungeon"), StringTableUtil.GetDesc("Dungeon"));
            //});
        }

        public override void OnEnter()
        {
            Refresh();

            RefreshContentsLockUI();
        }

        public override void RefreshContentsLockUI()
        {
            foreach (var dungeonItemUI in mDungeonItemList)
            {
                dungeonItemUI.RefreshContentsLockUI();
            }
        }

        public override void Localize()
        {
            foreach (var item in mDungeonItemList)
            {
                item.Localize();
            }
        }

        public override void Refresh()
        {
            Lance.Account.Currency.UpdateFreeTicket();

            foreach (var item in mDungeonItemList)
            {
                item.Refresh();
            }
        }
    }
}