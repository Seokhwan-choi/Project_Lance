using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lance
{
    public enum ShopTab
    {
        Currency,
        Package,
        Normal,
        Mileage,

        Count,
    }

    class ShopTabUIManager
    {
        ShopTab mCurTab;
        TabNavBarUIManager<ShopTab> mNavBarUI;

        GameObject mGameObject;
        List<ShopTabUI> mShopTabUIList;
        public void Init(GameObject go)
        {
            GameObject navBar = go.FindGameObject("Shop_NavBar");

            mNavBarUI = new TabNavBarUIManager<ShopTab>();
            mNavBarUI.Init(navBar, OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(ShopTab.Currency);

            mGameObject = go.FindGameObject("TabUIList");
            mShopTabUIList = new List<ShopTabUI>();
            InitShopTab<Shop_CurrencyTabUI>(ShopTab.Currency);
            InitShopTab<Shop_PackageTabUI>(ShopTab.Package);
            InitShopTab<Shop_NormalTabUI>(ShopTab.Normal);
            InitShopTab<Shop_MileageTabUI>(ShopTab.Mileage);

            RefrehRedDots();
        }

        public void RefrehRedDots()
        {
            for(int i = 0; i < (int)ShopTab.Count; ++i)
            {
                ShopTab tab = (ShopTab)i;

                mNavBarUI.SetActiveRedDot(tab, RedDotUtil.IsAcitveRedDotByShopTab(tab));
            }

            foreach(var tab in mShopTabUIList)
            {
                tab.Refresh();
                tab.RefreshRedDots();
            }
        }

        public void InitFinished()
        {
            foreach(var tab in mShopTabUIList)
            {
                tab.SetVisible(false);
            }

            ShowTab(ShopTab.Currency);
        }

        public void OnUpdate()
        {
            // 현재 탭만 
            foreach (ShopTabUI tab in mShopTabUIList)
            {
                if (mCurTab == tab.Tab)
                {
                    tab.OnUpdate();
                }
            }
        }

        public void InitShopTab<T>(ShopTab tab) where T : ShopTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 ShopTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            mShopTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : ShopTabUI
        {
            return mShopTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Refresh()
        {
            var curTab = mShopTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mShopTabUIList)
            {
                tab.Localize();
            }
        }

        int OnChangeTabButton(ShopTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out ShopTab result))
            {
                ChangeTab(result);
            }
        }

        public bool ChangeTab(ShopTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(ShopTab tab)
        {
            ShopTabUI showTab = mShopTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisible(true);
        }

        void HideTab(ShopTab tab)
        {
            ShopTabUI hideTab = mShopTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class ShopTabUI : MonoBehaviour
    {
        protected ShopTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public ShopTab Tab => mTab;
        public virtual void Init(ShopTab tab) 
        {
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }

        public virtual void Localize() { }
        public virtual void Refresh() { }
        public virtual void RefreshRedDots() { }
        public virtual void OnUpdate() { }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}