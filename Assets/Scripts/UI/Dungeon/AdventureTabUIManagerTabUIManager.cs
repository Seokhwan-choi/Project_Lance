using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using BackEnd;

namespace Lance
{
    public enum AdventureTab
    {
        Dungeon,
        DemonicRealm,

        Count,
    }

    class AdventureTabUIManager
    {
        AdventureTab mCurTab;
        TabNavBarUIManager<AdventureTab> mNavBarUI;

        GameObject mGameObject;
        List<AdventureTabUI> mAdventureTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<AdventureTab>();
            mNavBarUI.Init(go.FindGameObject("Advanture_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(AdventureTab.Dungeon);

            mAdventureTabUIList = new List<AdventureTabUI>();
            InitAdventureTab<Adventure_DungeonTabUI>(AdventureTab.Dungeon);
            InitAdventureTab<Adventure_DemonicRealmTabUI>(AdventureTab.DemonicRealm);

            ShowTab(AdventureTab.Dungeon);
        }

        public void RefreshContentsLockUI()
        {
            foreach (var tabButtonUI in mNavBarUI.GetTabButtonUIList())
            {
                AdventureTab tab = (AdventureTab)tabButtonUI.Tab;

                if (tab == AdventureTab.Dungeon)
                {
                    tabButtonUI.SetLockButton(ContentsLockUtil.IsLockContents(ContentsLockType.DungeonTab));
                }
                else
                {
                    tabButtonUI.SetLockButton(ContentsLockUtil.IsLockContents(ContentsLockType.DemonicRealm));
                }
            }

            foreach (var tabUI in mAdventureTabUIList)
            {
                tabUI.RefreshContentsLockUI();
            }
        }

        public void RefreshRedDots()
        {
            foreach(var tab in mAdventureTabUIList)
            {
                tab.RefreshRedDots();
            }
        }

        public void InitAdventureTab<T>(AdventureTab tab) where T : AdventureTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 AdventureTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mAdventureTabUIList.Add(tabUI);
        }

        public AdventureTabUI GetTab(AdventureTab tab)
        {
            return mAdventureTabUIList[(int)tab];
        }

        public T GetTab<T>() where T : AdventureTabUI
        {
            return mAdventureTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Refresh()
        {
            var curTab = mAdventureTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mAdventureTabUIList)
            {
                tab.Localize();
            }
        }

        int OnChangeTabButton(AdventureTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out AdventureTab result))
            {
                ChangeTab(result);
            }
        }

        public void RefreshActiveTab()
        {
            mNavBarUI.RefreshActiveFrame(mCurTab);
        }

        public bool ChangeTab(AdventureTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(AdventureTab tab)
        {
            AdventureTabUI showTab = mAdventureTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisible(true);
        }

        void HideTab(AdventureTab tab)
        {
            AdventureTabUI hideTab = mAdventureTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class AdventureTabUI : MonoBehaviour
    {
        protected AdventureTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public AdventureTab Tab => mTab;
        public virtual void Init(AdventureTab tab)
        {
            mTab = tab;
            mCanvas = GetComponentInChildren<Canvas>();
            mGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() { }
        public virtual void Refresh() { }
        public virtual void OnUpdate() { }
        public virtual void RefreshContentsLockUI() { }
        public virtual void RefreshRedDots() { }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}