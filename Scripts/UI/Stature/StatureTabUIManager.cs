using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lance
{
    public enum StatureTab
    {
        GoldTrain,
        Ability,
        LimitBreak,
        Artifact,
        Costume,

        Count,
    }

    class StatureTabUIManager
    {
        StatureTab mCurTab;
        TabNavBarUIManager<StatureTab> mNavBarUI;

        GameObject mGameObject;
        List<StatureTabUI> mStatureTabUIList;
        public StatureTab CurTab => mCurTab;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<StatureTab>();
            mNavBarUI.Init(go.FindGameObject("Stature_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(StatureTab.GoldTrain);

            mStatureTabUIList = new List<StatureTabUI>();
            InitStatureTab<Stature_GoldTrainTabUI>(StatureTab.GoldTrain);
            InitStatureTab<Stature_AbilityTabUI>(StatureTab.Ability);
            InitStatureTab<Stature_LimitBreakTabUI>(StatureTab.LimitBreak);
            InitStatureTab<Stature_ArtifactTabUI>(StatureTab.Artifact);
            InitStatureTab<Stature_CostumeTabUI>(StatureTab.Costume);

            ShowTab(StatureTab.GoldTrain);
        }

        public void RefreshContentsLockUI()
        {
            foreach(var tabButtonUI in mNavBarUI.GetTabButtonUIList())
            {
                StatureTab tab = (StatureTab)tabButtonUI.Tab;

                tabButtonUI.SetLockButton(ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()));
            }

            foreach(var tab in mStatureTabUIList)
            {
                tab.RefreshContentsLockUI();
            }
        }

        public void RefreshTab(StatureTab tab)
        {
            var tabUI = mStatureTabUIList[(int)tab];

            tabUI.Refresh();
        }

        public void RefreshTabFrame(StatureTab tab)
        {
            mNavBarUI.RefreshActiveFrame(tab);
        }

        public void OnUpdate()
        {
            // 현재 탭만 
            foreach (StatureTabUI tab in mStatureTabUIList)
            {
                if (mCurTab == tab.Tab)
                {
                    tab.OnUpdate();
                }
            }
        }

        public void InitStatureTab<T>(StatureTab tab) where T : StatureTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 StatureTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mStatureTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : StatureTabUI
        {
            return mStatureTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void OnEnter()
        {
            var curTab = mStatureTabUIList[(int)mCurTab];

            curTab.OnEnter();
        }

        public void Refresh()
        {
            var curTab = mStatureTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshAll()
        {
            foreach(var tabUI in mStatureTabUIList)
            {
                tabUI.Refresh();
            }
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mStatureTabUIList)
            {
                tab.Localize();
            }
        }

        int OnChangeTabButton(StatureTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(StatureTab tab)
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

        void ShowTab(StatureTab tab)
        {
            StatureTabUI showTab = mStatureTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(StatureTab tab)
        {
            StatureTabUI hideTab = mStatureTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            for(int i = 0; i < (int)StatureTab.Count; ++i)
            {
                mStatureTabUIList[i].RefreshRedDots();

                StatureTab tab = (StatureTab)i;

                mNavBarUI.SetActiveRedDot(tab, 
                    RedDotUtil.IsAcitveRedDotByStatureTab(tab) &&
                    ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()) == false);
            }
        }
    }

    class StatureTabUI : MonoBehaviour
    {
        protected StatureTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public StatureTab Tab => mTab;
        public virtual void Init(StatureTab tab) 
        {
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void Localize() 
        {
            var uiReaders = gameObject.GetComponentsInChildren<StringTableUIReader>();
            foreach (var uiReader in uiReaders)
            {
                uiReader.Localize();
            }
        }
        public virtual void OnUpdate() { }
        public virtual void RefreshRedDots() { }
        public virtual void RefreshContentsLockUI() { }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}
