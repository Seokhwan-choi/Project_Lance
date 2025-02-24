using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace Lance
{
    class CostumeShopTabUIManager
    {
        TextMeshProUGUI mTextMyCostumeUpgrade;
        CostumeTab mCurTab;
        CostumeTabNavBarUI mNavBarUI;
        GameObject mGameObject;
        List<CostumeShopTabUI> mCostumeTabUIList;
        public void Init(StatureCostume_CostumeShopTabUI parent)
        {
            mTextMyCostumeUpgrade = parent.gameObject.FindComponent<TextMeshProUGUI>("Text_MyCostumeUpgrade");

            var navBarObj = parent.gameObject.FindGameObject("NavBar");

            mNavBarUI = navBarObj.GetOrAddComponent<CostumeTabNavBarUI>();
            mNavBarUI.Init(ChangeTab);
            mNavBarUI.SetActiveRedDots(false);

            mGameObject = parent.gameObject.FindGameObject("TabUIList");

            mCostumeTabUIList = new List<CostumeShopTabUI>();
            InitCostumeTab<CostumeShop_BodyTabUI>(CostumeTab.Body);
            InitCostumeTab<CostumeShop_WeaponTabUI>(CostumeTab.Weapon);
            InitCostumeTab<CostumeShop_EtcTabUI>(CostumeTab.Etc);

            ShowTab(CostumeTab.Body);
        }

        public void RefreshTab(CostumeTab tab)
        {
            var tabUI = mCostumeTabUIList[(int)tab];

            tabUI.Refresh();
        }

        public void InitCostumeTab<T>(CostumeTab tab) where T : CostumeShopTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 CostumeShopTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mCostumeTabUIList.Add(tabUI);
        }

        public void OnEnter()
        {
            var curTab = mCostumeTabUIList[(int)mCurTab];

            curTab.OnEnter();

            RefreshMyCostumeUpgrade();
        }

        public void Localize()
        {
            foreach (var tab in mCostumeTabUIList)
            {
                tab.Localize();
            }
        }

        public void Refresh()
        {
            var curTab = mCostumeTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshMyCostumeUpgrade()
        {
            mTextMyCostumeUpgrade.text = Lance.Account.Currency.GetCostumeUpgrade().ToAlphaString();
        }

        public void ChangeTab(CostumeTab tab)
        {
            if (mCurTab == tab)
                return;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);
        }

        void ShowTab(CostumeTab tab)
        {
            CostumeShopTabUI showTab = mCostumeTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(CostumeTab tab)
        {
            CostumeShopTabUI hideTab = mCostumeTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class CostumeShopTabUI : MonoBehaviour
    {
        protected string mSelectedCostume;
        protected CostumeShopTabUIManager mParent;
        protected CostumeTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        protected List<CostumeShopItemUI> mCostumeShopItemUIs;
        public CostumeShopTabUIManager Parent => mParent;
        public virtual void Init(CostumeShopTabUIManager parent, CostumeTab tab)
        {
            mParent = parent;
            mTab = tab;
            mCostumeShopItemUIs = new List<CostumeShopItemUI>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
            mCanvas = GetComponent<Canvas>();

            var costumeItemList = gameObject.FindGameObject("Contents");

            costumeItemList.AllChildObjectOff();

            foreach(var data in DataUtil.GetCostumeShopDatas(tab.ChangeToCostumeType()))
            {
                var costumeItemObj = Util.InstantiateUI("CostumeShopItemUI", costumeItemList.transform);

                var costumeShopItemUI = costumeItemObj.GetOrAddComponent<CostumeShopItemUI>();

                costumeShopItemUI.Init(this, data.id);

                mCostumeShopItemUIs.Add(costumeShopItemUI);
            }
        }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }

        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() 
        {
            foreach(var itemUI in mCostumeShopItemUIs)
            {
                itemUI.Localize();
            }
        }
        public virtual void Refresh() { }
        public virtual void RefreshCostume() { }
        public virtual void OnUpdate() { }
    }
}