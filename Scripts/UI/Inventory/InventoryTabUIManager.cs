using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using BackEnd;

namespace Lance
{
    public enum InventoryTab
    {
        Weapon,
        Armor,
        Gloves,
        Shoes,
        Accessory,

        Count,
    }

    class InventoryTabUIManager
    {
        InventoryTab mCurTab;
        TabNavBarUIManager<InventoryTab> mNavBarUI;

        GameObject mGameObject;
        List<InventoryTabUI> mInventoryTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<InventoryTab>();
            mNavBarUI.Init(go.FindGameObject("Inventory_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(InventoryTab.Weapon);

            mInventoryTabUIList = new List<InventoryTabUI>();
            InitInventoryTab<Inventory_WeaponTabUI>(InventoryTab.Weapon);
            InitInventoryTab<Inventory_ArmorTabUI>(InventoryTab.Armor);
            InitInventoryTab<Inventory_GlovesTabUI>(InventoryTab.Gloves);
            InitInventoryTab<Inventory_ShoesTabUI>(InventoryTab.Shoes);
            InitInventoryTab<Inventory_AccessoryTabUI>(InventoryTab.Accessory);

            ShowTab(InventoryTab.Weapon);
        }

        public void RefreshContentsLockUI()
        {
            foreach(var tabButtonUI in mNavBarUI.GetTabButtonUIList())
            {
                InventoryTab tab = (InventoryTab)tabButtonUI.Tab;

                tabButtonUI.SetLockButton(ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()));
            }
        }

        public void InitInventoryTab<T>(InventoryTab tab) where T : InventoryTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 InventoryTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mInventoryTabUIList.Add(tabUI);
        }

        public InventoryTabUI GetTab(InventoryTab tab)
        {
            return mInventoryTabUIList[(int)tab];
        }

        public T GetTab<T>() where T : InventoryTabUI
        {
            return mInventoryTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public Inventory_EquipmentItemUI GetBestEquipmentItemUI()
        {
            var curTab = mInventoryTabUIList[(int)mCurTab];

            return curTab.GetBestEquipmentItemUI();
        }

        public void Refresh()
        {
            var curTab = mInventoryTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mInventoryTabUIList)
            {
                tab.Localize();
            }
        }

        int OnChangeTabButton(InventoryTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out InventoryTab result))
            {
                ChangeTab(result);
            }
        }

        public void RefreshActiveTab()
        {
            mNavBarUI.RefreshActiveFrame(mCurTab);
        }

        public bool ChangeTab(InventoryTab tab)
        {
            if (ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()))
            {
                ContentsLockUtil.ShowContentsLockMessage(tab.ChangeToContentsLockType());

                SoundPlayer.PlayErrorSound();

                return false;
            }

            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(InventoryTab tab)
        {
            InventoryTabUI showTab = mInventoryTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisible(true);
        }

        void HideTab(InventoryTab tab)
        {
            InventoryTabUI hideTab = mInventoryTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            for(int i = 0; i < (int)InventoryTab.Count; ++i)
            {
                InventoryTab tab = (InventoryTab)i;

                mNavBarUI.SetActiveRedDot(tab, 
                    RedDotUtil.IsAcitveRedDotByInventoryTab(tab) && 
                    ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()) == false);
            }

            foreach(var tab in mInventoryTabUIList)
            {
                tab.Refresh();
            }
        }
    }

    class InventoryTabUI : MonoBehaviour
    {
        protected InventoryTab mTab;
        protected List<Inventory_EquipmentItemUI> mItemUIList;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public InventoryTab Tab => mTab;
        public virtual void Init(InventoryTab tab) 
        {
            mTab = tab;

            var buttonRecomandEquip = gameObject.FindComponent<Button>("Recomand_Equip");
            buttonRecomandEquip.SetButtonAction(OnRecomandEquipButton);
            var buttonAllCombine = gameObject.FindComponent<Button>("AllCombine");
            buttonAllCombine.SetButtonAction(OnAllCombineButton);

            var guideTag = buttonAllCombine.GetOrAddComponent<GuideActionTag>();
            guideTag.Tag = mTab.ChangeToGuideActionTagAllCombine();

            mCanvas = GetComponentInChildren<Canvas>();
            mGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
        }

        public virtual void Localize() 
        {
            foreach(var itemUI in mItemUIList)
            {
                itemUI.Localize();
            }
        }

        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void OnUpdate() { }

        protected void InitItemUIList(IEnumerable<EquipmentData> datas)
        {
            mItemUIList = new List<Inventory_EquipmentItemUI>();

            GameObject listParent = gameObject.FindGameObject("EquipmentItemList");

            listParent.AllChildObjectOff();

            foreach (EquipmentData data in datas)
            {
                GameObject itemObj = Util.InstantiateUI("EquipmentItemUI", listParent.transform);

                Inventory_EquipmentItemUI itemUI = itemObj.GetOrAddComponent<Inventory_EquipmentItemUI>();

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

        public Inventory_EquipmentItemUI GetBestEquipmentItemUI()
        {
            double baseStat = 0;
            Inventory_EquipmentItemUI bestItem = null;

            for (int i = 0; i < mItemUIList.Count; ++i)
            {
                var itemUI = mItemUIList[i];

                if (Lance.Account.HaveEquipment(itemUI.Id) == false)
                    continue;

                var data = DataUtil.GetEquipmentData(itemUI.Id);
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
            var popup = Lance.PopupManager.CreatePopup<Popup_EquipmentInfoUI>();
            popup.Init(this, id);
            popup.SetOnCloseAction(() =>
            {
                RefreshItemSelelected(string.Empty);

                var data = DataUtil.GetEquipmentData(id);
                if (data != null)
                {
                    if (data.type == ItemType.Weapon)
                    {
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.UpgradeWeapon);
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EquipWeapon);
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.CombineWeapon);
                    }
                    else if (data.type == ItemType.Armor)
                    {
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EquipArmor);
                    }
                    else if (data.type == ItemType.Gloves)
                    {
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EquipGloves);
                    }
                    else
                    {
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EquipShoes);
                    }
                }
                
            });

            RefreshItemSelelected(id);
        }

        void OnRecomandEquipButton()
        {
            var results = Lance.Account.EquipRecomandEquipments();
            if (results != null && results.Count > 0)
            {
                SoundPlayer.PlayEquipmentEquip();

                Refresh();

                RewardResult rewardResult = new RewardResult();

                foreach (var result in results)
                {
                    if (result.IsValid())
                    {
                        var data = DataUtil.GetEquipmentData(result);

                        rewardResult = rewardResult.AddReward(new MultiReward(data.type, data.id, 0));
                    }
                }

                UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_EquipResult"));

                Lance.GameManager.UpdatePlayerStat();
            }
        }

        void OnAllCombineButton()
        {
            var itemType = mTab.ChangeToItemType();

            List<(string id, int count)> results = Lance.Account.CombineAllEquipment(itemType);
            if (results.Count > 0)
            {
                SoundPlayer.PlayEquipmentCombine();

                int totalCount = 0;

                RewardResult rewardResult = new RewardResult();

                foreach (var result in results.OrderBy(x => DataUtil.GetEquipmentData(x.id).baseValue))
                {
                    totalCount += result.count;

                    rewardResult = rewardResult.AddReward(new MultiReward(itemType, result.id, result.count));
                }

                UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_CombineResult"));

                Lance.GameManager.CheckQuest(QuestType.CombineEquipment, totalCount);

                if (itemType == ItemType.Weapon)
                {
                    Lance.GameManager.CheckQuest(QuestType.CombineWeapon, totalCount);
                }

                Param param = new Param();
                param.Add("result", results);

                Lance.BackEnd.InsertLog("CombineAllEquipment", param, 7);

                Refresh();
            }
            else
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughEquipmentCombineRequireCount");
            }

            results = null;
        }
    }


    static class InventoryTabExt
    {
        public static ItemType ChangeToItemType(this InventoryTab tab)
        {
            switch (tab)
            {
                case InventoryTab.Weapon:
                    return ItemType.Weapon;
                case InventoryTab.Armor:
                    return ItemType.Armor;
                case InventoryTab.Gloves:
                    return ItemType.Gloves;
                case InventoryTab.Shoes:
                default:
                    return ItemType.Shoes;
            }
        }

        public static GuideActionType ChangeToGuideActionTagAllCombine(this InventoryTab tab)
        {
            switch (tab)
            {
                case InventoryTab.Weapon:
                    return GuideActionType.Highlight_AllCombineWeaponButton;
                case InventoryTab.Armor:
                    return GuideActionType.Highlight_AllCombineArmorButton;
                case InventoryTab.Gloves:
                    return GuideActionType.Highlight_AllCombineGlovesButton;
                case InventoryTab.Shoes:
                default:
                    return GuideActionType.Highlight_AllCombineShoesButton;
            }
        }
    }
}
