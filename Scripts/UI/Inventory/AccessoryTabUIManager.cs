using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using BackEnd;

namespace Lance
{
    public enum AccessoryInventoryTab
    {
        Necklace,
        Earring,
        Ring,

        Count,
    }

    class AccessoryInventoryTabUIManager
    {
        AccessoryInventoryTab mCurTab;
        TabNavBarUIManager<AccessoryInventoryTab> mNavBarUI;

        GameObject mGameObject;
        List<AccessoryInventoryTabUI> mAccessoryInventoryTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<AccessoryInventoryTab>();
            mNavBarUI.Init(go.FindGameObject("Accessory_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(AccessoryInventoryTab.Necklace);

            mAccessoryInventoryTabUIList = new List<AccessoryInventoryTabUI>();
            InitAccessoryInventoryTab<Accessory_NecklaceTabUI>(AccessoryInventoryTab.Necklace);
            InitAccessoryInventoryTab<Accessory_EarringTabUI>(AccessoryInventoryTab.Earring);
            InitAccessoryInventoryTab<Accessory_RingTabUI>(AccessoryInventoryTab.Ring);

            ShowTab(AccessoryInventoryTab.Necklace);
        }

        public void InitAccessoryInventoryTab<T>(AccessoryInventoryTab tab) where T : AccessoryInventoryTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 AccessoryInventoryTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mAccessoryInventoryTabUIList.Add(tabUI);
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mAccessoryInventoryTabUIList)
            {
                tab.Localize();
            }
        }

        public void Refresh()
        {
            var curTab = mAccessoryInventoryTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        int OnChangeTabButton(AccessoryInventoryTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out AccessoryInventoryTab result))
            {
                ChangeTab(result);
            }
        }

        public void RefreshActiveTab()
        {
            mNavBarUI.RefreshActiveFrame(mCurTab);
        }

        public bool ChangeTab(AccessoryInventoryTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(AccessoryInventoryTab tab)
        {
            AccessoryInventoryTabUI showTab = mAccessoryInventoryTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisible(true);
        }

        void HideTab(AccessoryInventoryTab tab)
        {
            AccessoryInventoryTabUI hideTab = mAccessoryInventoryTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)AccessoryInventoryTab.Count; ++i)
            {
                AccessoryInventoryTab tab = (AccessoryInventoryTab)i;

                mNavBarUI.SetActiveRedDot(tab, RedDotUtil.IsAcitveRedDotByAccessoryTab(tab));
            }

            foreach (var tab in mAccessoryInventoryTabUIList)
            {
                tab.Refresh();
            }
        }
    }

    class AccessoryInventoryTabUI : MonoBehaviour
    {
        protected AccessoryInventoryTab mTab;
        protected List<Inventory_AccessoryItemUI> mItemUIList;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public AccessoryInventoryTab Tab => mTab;
        public virtual void Init(AccessoryInventoryTab tab)
        {
            mTab = tab;

            var buttonRecomandEquip = gameObject.FindComponent<Button>("Recomand_Equip");
            buttonRecomandEquip.SetButtonAction(OnRecomandEquipButton);

            var buttonAllCombine = gameObject.FindComponent<Button>("AllCombine");
            buttonAllCombine.SetButtonAction(OnAllCombineButton);

            mCanvas = GetComponentInChildren<Canvas>();
            mGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() { }
        public virtual void Refresh() { }
        public virtual void OnUpdate() { }

        protected void InitItemUIList(IEnumerable<AccessoryData> datas)
        {
            mItemUIList = new List<Inventory_AccessoryItemUI>();

            GameObject listParent = gameObject.FindGameObject("AccessoryItemList");

            listParent.AllChildObjectOff();

            foreach (AccessoryData data in datas)
            {
                GameObject itemObj = Util.InstantiateUI("AccessoryItemUI", listParent.transform);

                Inventory_AccessoryItemUI itemUI = itemObj.GetOrAddComponent<Inventory_AccessoryItemUI>();

                itemUI.Init();
                itemUI.SetId(data.id);
                itemUI.SetButtonAction(OnSelectedItemUI);

                mItemUIList.Add(itemUI);
            }

            RefreshItemSelelected(string.Empty);
        }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }

        public Inventory_AccessoryItemUI GetBestEquipmentItemUI()
        {
            double baseStat = 0;
            Inventory_AccessoryItemUI bestItem = null;

            for (int i = 0; i < mItemUIList.Count; ++i)
            {
                var itemUI = mItemUIList[i];

                if (Lance.Account.HaveAccessory(itemUI.Id) == false)
                    continue;

                var data = DataUtil.GetAccessoryData(itemUI.Id);
                if (data == null)
                    continue;

                if (data.baseValue > baseStat)
                {
                    baseStat = data.baseValue;
                    bestItem = itemUI;
                }
            }

            return bestItem;
        }

        public void RefreshItemSelelected(string id)
        {
            foreach (var itemUI in mItemUIList)
            {
                itemUI.SetSelected(itemUI.Id == id);
            }
        }

        void OnSelectedItemUI(string id)
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_AccessoryInfoUI>();
            popup.Init(this, id);

            popup.SetOnCloseAction(() =>
            {
                RefreshItemSelelected(string.Empty);
            });

            RefreshItemSelelected(id);
        }

        void OnRecomandEquipButton()
        {
            var results = Lance.Account.EquipRecomandAccessorys();
            if (results != null && results.Count > 0)
            {
                SoundPlayer.PlayEquipmentEquip();

                Refresh();

                RewardResult rewardResult = new RewardResult();

                foreach (var result in results)
                {
                    if (result.IsValid())
                    {
                        var data = DataUtil.GetAccessoryData(result);

                        rewardResult = rewardResult.AddReward(new MultiReward(data.type, data.id, 0));
                    }
                }

                if (rewardResult.IsEmpty() == false)
                {
                    UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_EquipResult"));

                    Lance.GameManager.UpdatePlayerStat();
                }
            }
        }

        void OnAllCombineButton()
        {
            var type = mTab.ChangeToItemType();

            List<(string id, int count)> results = Lance.Account.CombineAllAccessory(type);
            if (results.Count > 0)
            {
                SoundPlayer.PlayEquipmentCombine();

                int totalCount = 0;

                RewardResult rewardResult = new RewardResult();

                foreach (var result in results.OrderBy(x => DataUtil.GetAccessoryData(x.id).baseValue))
                {
                    totalCount += result.count;

                    rewardResult = rewardResult.AddReward(new MultiReward(type, result.id, result.count));
                }

                UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_CombineResult"));

                Param param = new Param();
                param.Add("result", results);
                param.Add("itemType", $"{type}");

                Lance.BackEnd.InsertLog("CombineAllAccessory", param, 7);

                Refresh();
            }
            else
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughAccessoryCombineRequireCount");
            }

            results = null;
        }
    }


    static class AccessoryInventoryTabExt
    {
        public static ItemType ChangeToItemType(this AccessoryInventoryTab tab)
        {
            switch (tab)
            {
                case AccessoryInventoryTab.Necklace:
                    return ItemType.Necklace;
                case AccessoryInventoryTab.Earring:
                    return ItemType.Earring;
                case AccessoryInventoryTab.Ring:
                default:
                    return ItemType.Ring;
            }
        }
    }
}
