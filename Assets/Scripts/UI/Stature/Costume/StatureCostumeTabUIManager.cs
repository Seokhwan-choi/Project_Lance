using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lance
{
    public enum StatureCostumeTab
    {
        Costume,
        CostumeShop,

        Count,
    }

    class StatureCostumeTabUIManager
    {
        Stature_CostumeTabUI mParent;
        StatureCostumeTab mCurTab;
        TabNavBarUIManager<StatureCostumeTab> mNavBarUI;
        GameObject mGameObject;
        List<StatureCostumeTabUI> mStatureCostumeTabUIList;
        public void Init(Stature_CostumeTabUI parent)
        {
            mParent = parent;

            var navBarObj = parent.gameObject.FindGameObject("Costume_NavBar");

            mNavBarUI = new TabNavBarUIManager<StatureCostumeTab>();
            mNavBarUI.Init(navBarObj, OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(StatureCostumeTab.Costume);

            mGameObject = parent.gameObject.FindGameObject("TabUIList");

            mStatureCostumeTabUIList = new List<StatureCostumeTabUI>();
            InitStatureCostumeTab<StatureCostume_CostumeTabUI>(StatureCostumeTab.Costume);
            InitStatureCostumeTab<StatureCostume_CostumeShopTabUI>(StatureCostumeTab.CostumeShop);

            StatureCostumeTabUI showTab = mStatureCostumeTabUIList[(int)StatureCostumeTab.Costume];

            showTab.SetVisible(true);
        }

        public void RefreshTab(StatureCostumeTab tab)
        {
            var tabUI = mStatureCostumeTabUIList[(int)tab];

            tabUI.Refresh();
        }

        public void InitStatureCostumeTab<T>(StatureCostumeTab tab) where T : StatureCostumeTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 StatureCostumeTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mStatureCostumeTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : StatureCostumeTabUI
        {
            return mStatureCostumeTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void OnEnter()
        {
            var curTab = mStatureCostumeTabUIList[(int)mCurTab];

            curTab.OnEnter();
        }

        //public void AllTabRefreshCostume()
        //{
        //    foreach (var tab in mStatureCostumeTabUIList)
        //    {
        //        tab.RefreshCostume();
        //    }
        //}

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mStatureCostumeTabUIList)
            {
                tab.Localize();
            }
        }

        public void Refresh()
        {
            var curTab = mStatureCostumeTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        int OnChangeTabButton(StatureCostumeTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(StatureCostumeTab tab)
        {
            if (mCurTab == tab)
                return;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);
        }

        void ShowTab(StatureCostumeTab tab)
        {
            StatureCostumeTabUI showTab = mStatureCostumeTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(StatureCostumeTab tab)
        {
            StatureCostumeTabUI hideTab = mStatureCostumeTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            foreach (var tab in mStatureCostumeTabUIList)
            {
                tab.RefreshRedDots();
            }

            mNavBarUI.SetActiveRedDot(StatureCostumeTab.Costume, 
                RedDotUtil.IsActiveRedDotByCostumeTab(CostumeTab.Body) ||
                RedDotUtil.IsActiveRedDotByCostumeTab(CostumeTab.Weapon) ||
                RedDotUtil.IsActiveRedDotByCostumeTab(CostumeTab.Etc));
        }
    }

    class StatureCostumeTabUI : MonoBehaviour
    {
        protected StatureCostumeTabUIManager mParent;
        protected StatureCostumeTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public StatureCostumeTab Tab => mTab;
        public virtual void Init(StatureCostumeTabUIManager parent, StatureCostumeTab tab)
        {
            mParent = parent;
            mTab = tab;

            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() { }
        
        public virtual void Refresh() { }
        public virtual void RefreshRedDots() { }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}
