using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using BackEnd;

namespace Lance
{
    public enum EquipmentOptionInfoTab
    {
        OptionStatProb,
        OptionStatToOwnStat,

        Count,
    }

    class EquipmentOptionInfoTabTabUIManager
    {
        EquipmentOptionInfoTab mCurTab;
        TabNavBarUIManager<EquipmentOptionInfoTab> mNavBarUI;

        GameObject mGameObject;
        List<EquipmentOptionInfoTabUI> mEquipmentOptionInfoTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("Contents");

            mNavBarUI = new TabNavBarUIManager<EquipmentOptionInfoTab>();
            mNavBarUI.Init(go.FindGameObject("NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(EquipmentOptionInfoTab.OptionStatProb);

            mEquipmentOptionInfoTabUIList = new List<EquipmentOptionInfoTabUI>();
            InitEquipmentOptionInfoTab<EquipmentOption_StatProbTabUI>(EquipmentOptionInfoTab.OptionStatProb);
            InitEquipmentOptionInfoTab<EquipmentOption_StatToOwnStatTabUI>(EquipmentOptionInfoTab.OptionStatToOwnStat);

            ShowTab(EquipmentOptionInfoTab.OptionStatProb);
        }

        public void OnUpdate()
        {
            // 현재 탭만 
            foreach (EquipmentOptionInfoTabUI tab in mEquipmentOptionInfoTabUIList)
            {
                if (mCurTab == tab.Tab)
                {
                    tab.OnUpdate();
                }
            }
        }

        public void InitEquipmentOptionInfoTab<T>(EquipmentOptionInfoTab tab) where T : EquipmentOptionInfoTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 EquipmentOptionInfoTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab);

            tabUI.SetVisible(false);

            mEquipmentOptionInfoTabUIList.Add(tabUI);
        }

        public EquipmentOptionInfoTabUI GetTab(EquipmentOptionInfoTab tab)
        {
            return mEquipmentOptionInfoTabUIList[(int)tab];
        }

        public T GetTab<T>() where T : EquipmentOptionInfoTabUI
        {
            return mEquipmentOptionInfoTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Refresh()
        {
            var curTab = mEquipmentOptionInfoTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        int OnChangeTabButton(EquipmentOptionInfoTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out EquipmentOptionInfoTab result))
            {
                ChangeTab(result);
            }
        }

        public void RefreshActiveTab()
        {
            mNavBarUI.RefreshActiveFrame(mCurTab);
        }

        public bool ChangeTab(EquipmentOptionInfoTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(EquipmentOptionInfoTab tab)
        {
            EquipmentOptionInfoTabUI showTab = mEquipmentOptionInfoTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisible(true);
        }

        void HideTab(EquipmentOptionInfoTab tab)
        {
            EquipmentOptionInfoTabUI hideTab = mEquipmentOptionInfoTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class EquipmentOptionInfoTabUI : MonoBehaviour
    {
        protected EquipmentOptionInfoTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public EquipmentOptionInfoTab Tab => mTab;
        public virtual void Init(EquipmentOptionInfoTab tab)
        {
            mTab = tab;

            mCanvas = GetComponentInChildren<Canvas>();
            mGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
        }
        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void OnUpdate() { }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }
}