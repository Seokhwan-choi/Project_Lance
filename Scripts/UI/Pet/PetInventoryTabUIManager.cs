using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using BackEnd;

namespace Lance
{
    public enum PetInventoryTab
    {
        Fire,
        Water,
        Grass,

        Count,
    }

    class PetInventoryTabUIManager
    {
        PetInventoryTab mCurTab;
        TabNavBarUIManager<PetInventoryTab> mNavBarUI;

        GameObject mGameObject;
        List<PetInventoryTabUI> mPetInventoryTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<PetInventoryTab>();
            mNavBarUI.Init(go.FindGameObject("PetInventory_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(PetInventoryTab.Fire);

            mPetInventoryTabUIList = new List<PetInventoryTabUI>();
            InitPetInventoryTab<PetInventory_FireTabUI>(PetInventoryTab.Fire);
            InitPetInventoryTab<PetInventory_WaterTabUI>(PetInventoryTab.Water);
            InitPetInventoryTab<PetInventory_GrassTabUI>(PetInventoryTab.Grass);

            ShowTab(PetInventoryTab.Fire);
        }

        public void InitPetInventoryTab<T>(PetInventoryTab tab) where T : PetInventoryTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 PetInventoryTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mPetInventoryTabUIList.Add(tabUI);
        }

        public void Localize()
        {
            mNavBarUI.Localize();
        }

        public void Refresh()
        {
            var curTab = mPetInventoryTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        int OnChangeTabButton(PetInventoryTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out PetInventoryTab result))
            {
                ChangeTab(result);
            }
        }

        public void RefreshActiveTab()
        {
            mNavBarUI.RefreshActiveFrame(mCurTab);
        }

        public bool ChangeTab(PetInventoryTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(PetInventoryTab tab)
        {
            PetInventoryTabUI showTab = mPetInventoryTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisible(true);
        }

        void HideTab(PetInventoryTab tab)
        {
            PetInventoryTabUI hideTab = mPetInventoryTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)PetInventoryTab.Count; ++i)
            {
                PetInventoryTab tab = (PetInventoryTab)i;

                mNavBarUI.SetActiveRedDot(tab, RedDotUtil.IsAcitveRedDotByPetInventoryTab(tab));
            }

            foreach (var tab in mPetInventoryTabUIList)
            {
                tab.Refresh();
            }
        }
    }

    class PetInventoryTabUI : MonoBehaviour
    {
        protected PetInventoryTab mTab;
        protected List<PetInventory_EquipmentItemUI> mItemUIList;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public PetInventoryTab Tab => mTab;
        public virtual void Init(PetInventoryTab tab)
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
        public virtual void Refresh() { }
        public virtual void OnUpdate() { }

        protected void InitItemUIList(IEnumerable<PetEquipmentData> datas)
        {
            mItemUIList = new List<PetInventory_EquipmentItemUI>();

            GameObject listParent = gameObject.FindGameObject("EquipmentItemList");

            listParent.AllChildObjectOff();

            foreach (PetEquipmentData data in datas)
            {
                GameObject itemObj = Util.InstantiateUI("EquipmentItemUI", listParent.transform);

                PetInventory_EquipmentItemUI itemUI = itemObj.GetOrAddComponent<PetInventory_EquipmentItemUI>();

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

        public PetInventory_EquipmentItemUI GetBestEquipmentItemUI()
        {
            double baseStat = 0;
            PetInventory_EquipmentItemUI bestItem = null;

            for (int i = 0; i < mItemUIList.Count; ++i)
            {
                var itemUI = mItemUIList[i];

                if (Lance.Account.HavePetEquipment(itemUI.Id) == false)
                    continue;

                var data = Lance.GameData.PetEquipmentData.TryGet(itemUI.Id);
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
            var popup = Lance.PopupManager.CreatePopup<Popup_PetEquipmentInfoUI>();
            popup.Init(this, id);

            popup.SetOnCloseAction(() =>
            {
                RefreshItemSelelected(string.Empty);
            });

            RefreshItemSelelected(id);
        }

        void OnRecomandEquipButton()
        {
            var results = Lance.Account.EquipRecomandPetEquipments();
            if (results != null && results.Count > 0)
            {
                SoundPlayer.PlayEquipmentEquip();

                Refresh();

                RewardResult rewardResult = new RewardResult();

                foreach (var result in results)
                {
                    if (result.IsValid())
                    {
                        var data = Lance.GameData.PetEquipmentData.TryGet(result);

                        rewardResult = rewardResult.AddReward(new MultiReward(ItemType.PetEquipment, data.id, 0));
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
            var type = mTab.ChangeToElementalType();

            List<(string id, int count)> results = Lance.Account.CombineAllPetEquipment(type);
            if (results.Count > 0)
            {
                SoundPlayer.PlayEquipmentCombine();

                int totalCount = 0;

                RewardResult rewardResult = new RewardResult();

                foreach (var result in results.OrderBy(x => Lance.GameData.PetEquipmentData.TryGet(x.id).baseValue))
                {
                    totalCount += result.count;

                    rewardResult = rewardResult.AddReward(new MultiReward(ItemType.PetEquipment, result.id, result.count));
                }

                UIUtil.ShowRewardPopup(rewardResult, StringTableUtil.Get("Title_CombineResult"));

                Param param = new Param();
                param.Add("result", results);
                param.Add("elementalType", $"{type}");

                Lance.BackEnd.InsertLog("CombineAllPetEquipment", param, 7);

                Refresh();
            }
            else
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPetEquipmentCombineRequireCount");
            }

            results = null;
        }
    }


    static class PetInventoryTabExt
    {
        public static ElementalType ChangeToElementalType(this PetInventoryTab tab)
        {
            switch (tab)
            {
                case PetInventoryTab.Fire:
                    return ElementalType.Fire;
                case PetInventoryTab.Water:
                    return ElementalType.Water;
                case PetInventoryTab.Grass:
                default:
                    return ElementalType.Grass;
            }
        }

        //public static GuideActionType ChangeToGuideActionTagAllCombine(this PetInventoryTab tab)
        //{
        //    switch (tab)
        //    {
        //        case PetInventoryTab.Weapon:
        //            return GuideActionType.Highlight_AllCombineWeaponButton;
        //        case PetInventoryTab.Armor:
        //            return GuideActionType.Highlight_AllCombineArmorButton;
        //        case PetInventoryTab.Gloves:
        //            return GuideActionType.Highlight_AllCombineGlovesButton;
        //        case PetInventoryTab.Shoes:
        //        default:
        //            return GuideActionType.Highlight_AllCombineShoesButton;
        //    }
        //}
    }
}
