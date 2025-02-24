using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lance
{
    public enum PetTab
    {
        Pet,
        PetInventory,

        Count,
    }

    class PetTabUIManager
    {
        PetTab mCurTab;
        TabNavBarUIManager<PetTab> mNavBarUI;

        GameObject mGameObject;
        List<PetTabUI> mPetTabUIList;
        public PetTab CurTab => mCurTab;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<PetTab>();
            mNavBarUI.Init(go.FindGameObject("Pet_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(PetTab.Pet);

            mPetTabUIList = new List<PetTabUI>();
            InitPetTab<Pet_PetTabUI>(PetTab.Pet);
            InitPetTab<Pet_PetInventoryTabUI>(PetTab.PetInventory);

            ShowTab(PetTab.Pet);
        }

        public void InitPetTab<T>(PetTab tab) where T : PetTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 PetTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mPetTabUIList.Add(tabUI);
        }

        public void OnEnter()
        {
            var curTab = mPetTabUIList[(int)mCurTab];

            curTab.OnEnter();
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach(var tab in mPetTabUIList)
            {
                tab.Localize();
            }
        }

        public void Refresh()
        {
            var curTab = mPetTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        int OnChangeTabButton(PetTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(PetTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(PetTab tab)
        {
            PetTabUI showTab = mPetTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(PetTab tab)
        {
            PetTabUI hideTab = mPetTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)PetTab.Count; ++i)
            {
                mPetTabUIList[i].RefreshRedDots();

                PetTab tab = (PetTab)i;

                mNavBarUI.SetActiveRedDot(tab, RedDotUtil.IsAcitveRedDotByPetTab(tab));
            }
        }

        public RectTransform GetBestPetItemUI()
        {
            var tabUI = mPetTabUIList[(int)PetTab.Pet] as Pet_PetTabUI;

            return tabUI.GetBestPetItemUI();
        }
    }

    class PetTabUI : MonoBehaviour
    {
        protected PetTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public PetTab Tab => mTab;
        public virtual void Init(PetTab tab)
        {
            mTab = tab;
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Localize() { }
        public virtual void Refresh() { }
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
