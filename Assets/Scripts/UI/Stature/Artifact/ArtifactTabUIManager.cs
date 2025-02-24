using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lance
{
    public enum ArtifactTab
    {
        Artifact,
        AncientArtifact,
        Excalibur,

        Count,
    }

    class ArtifactTabUIManager
    {
        ArtifactTab mCurTab;
        TabNavBarUIManager<ArtifactTab> mNavBarUI;

        GameObject mGameObject;
        List<ArtifactTabUI> mArtifactTabUIList;
        public ArtifactTab CurTab => mCurTab;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<ArtifactTab>();
            mNavBarUI.Init(go.FindGameObject("Artifact_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(ArtifactTab.Artifact);

            mArtifactTabUIList = new List<ArtifactTabUI>();
            InitArtifactTab<Artifact_ArtifactTabUI>(ArtifactTab.Artifact);
            InitArtifactTab<Artifact_AncientArtifactTabUI>(ArtifactTab.AncientArtifact);
            InitArtifactTab<Artifact_ExcaliburTabUI>(ArtifactTab.Excalibur);

            ShowTab(ArtifactTab.Artifact);
        }

        public void RefreshTabFrame(ArtifactTab tab)
        {
            mNavBarUI.RefreshActiveFrame(tab);
        }

        public void InitArtifactTab<T>(ArtifactTab tab) where T : ArtifactTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 ArtifactTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mArtifactTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : ArtifactTabUI
        {
            return mArtifactTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void OnEnter()
        {
            var curTab = mArtifactTabUIList[(int)mCurTab];

            curTab.OnEnter();
        }

        public void OnLeave()
        {
            var curTab = mArtifactTabUIList[(int)mCurTab];

            curTab.OnLeave();
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach (var tab in mArtifactTabUIList)
            {
                tab.Localize();
            }
        }

        public void Refresh()
        {
            var curTab = mArtifactTabUIList[(int)mCurTab];

            curTab.Refresh();

            RefreshRedDots();
            RefreshContentsLockUI();
        }

        int OnChangeTabButton(ArtifactTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(ArtifactTab tab)
        {
            if (tab == ArtifactTab.AncientArtifact)
            {
                if (Lance.Account.Artifact.IsAllArtifactMaxLevel() == false)
                {
                    // 유물이 모두 만랩이어야함
                    UIUtil.ShowSystemErrorMessage("RequireAllArtifactMaxLevel");

                    SoundPlayer.PlayErrorSound();

                    return false;
                }
            }

            if (tab == ArtifactTab.Excalibur)
            {
                if (Lance.Account.AncientArtifact.IsAllArtifactMaxLevel() == false)
                {
                    // 유물이 모두 만랩이어야함
                    UIUtil.ShowSystemErrorMessage("RequireAllArtifactMaxLevel");

                    SoundPlayer.PlayErrorSound();

                    return false;
                }
            }

            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(ArtifactTab tab)
        {
            ArtifactTabUI showTab = mArtifactTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(ArtifactTab tab)
        {
            ArtifactTabUI hideTab = mArtifactTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)ArtifactTab.Count; ++i)
            {
                mArtifactTabUIList[i].RefreshRedDots();

                ArtifactTab tab = (ArtifactTab)i;

                if (tab == ArtifactTab.Artifact)
                {
                    mNavBarUI.SetActiveRedDot(tab,
                    RedDotUtil.IsAcitveRedDotByArtifactTab(tab) &&
                    ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()) == false);
                }
                else if (tab == ArtifactTab.AncientArtifact)
                {
                    mNavBarUI.SetActiveRedDot(tab, 
                        RedDotUtil.IsAcitveRedDotByArtifactTab(tab) &&
                        Lance.Account.Artifact.IsAllArtifactMaxLevel());
                }
                else
                {
                    mNavBarUI.SetActiveRedDot(tab,
                        RedDotUtil.IsAcitveRedDotByArtifactTab(tab) &&
                        Lance.Account.AncientArtifact.IsAllArtifactMaxLevel());
                }
            }
        }

        public void RefreshContentsLockUI()
        {
            foreach (var tabButtonUI in mNavBarUI.GetTabButtonUIList())
            {
                ArtifactTab tab = (ArtifactTab)tabButtonUI.Tab;

                if (tab == ArtifactTab.Artifact)
                    tabButtonUI.SetLockButton(ContentsLockUtil.IsLockContents(tab.ChangeToContentsLockType()));
                else if (tab == ArtifactTab.AncientArtifact)
                    tabButtonUI.SetLockButton(Lance.Account.Artifact.IsAllArtifactMaxLevel() == false);
                else
                    tabButtonUI.SetLockButton(Lance.Account.AncientArtifact.IsAllArtifactMaxLevel() == false);
            }
        }
    }

    class ArtifactTabUI : MonoBehaviour
    {
        protected ArtifactTabUIManager mParnet;
        protected ArtifactTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;

        public ArtifactTabUIManager Parnet => mParnet;
        public virtual void Init(ArtifactTabUIManager parent, ArtifactTab tab)
        {
            mParnet = parent;
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() { }
        public virtual void Refresh() { }
        public virtual void RefreshRedDots() { }
        public virtual void RefreshContentsLockUI() { }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}
