using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Lance
{
    public enum CostumeTab
    {
        Body,
        Weapon,
        Etc,

        Count,
    }

    class CostumeTabUIManager
    {
        StatureCostume_CostumeTabUI mParent;
        CostumeTab mCurTab;
        CostumeTabNavBarUI mNavBarUI;
        GameObject mGameObject;
        List<CostumeTabUI> mCostumeTabUIList;
        public void Init(StatureCostume_CostumeTabUI parent)
        {
            mParent = parent;

            var navBarObj = parent.gameObject.FindGameObject("CostumeNavBar");

            mNavBarUI = navBarObj.GetOrAddComponent<CostumeTabNavBarUI>();
            mNavBarUI.Init(ChangeTab);

            mGameObject = parent.gameObject.FindGameObject("TabUIList");

            mCostumeTabUIList = new List<CostumeTabUI>();
            InitCostumeTab<Costume_BodyTabUI>(CostumeTab.Body);
            InitCostumeTab<Costume_WeaponTabUI>(CostumeTab.Weapon);
            InitCostumeTab<Costume_EtcTabUI>(CostumeTab.Etc);

            ShowTab(CostumeTab.Body);
        }

        public void RefreshTab(CostumeTab tab)
        {
            var tabUI = mCostumeTabUIList[(int)tab];

            tabUI.Refresh();
        }

        public void InitCostumeTab<T>(CostumeTab tab) where T : CostumeTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}_Tab", true);

            Debug.Assert(tabObj != null, $"{tab}의 CostumeTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mCostumeTabUIList.Add(tabUI);
        }

        public T GetTab<T>() where T : CostumeTabUI
        {
            return mCostumeTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void OnEnter()
        {
            var curTab = mCostumeTabUIList[(int)mCurTab];

            curTab.OnEnter();
        }

        public void AllTabRefreshCostume()
        {
            foreach(var tab in mCostumeTabUIList)
            {
                tab.RefreshCostume();
            }
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
            CostumeTabUI showTab = mCostumeTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(CostumeTab tab)
        {
            CostumeTabUI hideTab = mCostumeTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void OnLeave()
        {
            ChangeTab(CostumeTab.Body);

            mNavBarUI.RefreshActiveFrame(CostumeTab.Body);
        }

        public void RefreshRedDots()
        {
            mNavBarUI.RefreshRedDots();

            foreach(var tab in mCostumeTabUIList)
            {
                tab.RefreshRedDots();
            }
        }

        public void RefreshCostumeUIs(string id)
        {
            var curTab = mCostumeTabUIList[(int)mCurTab];

            curTab.RefreshCostumeItemUIs(id);
        }

        public void OnSelectCostume(string selectedCostume)
        {
            mParent.OnSelectCostume(selectedCostume);
        }
    }

    class CostumeTabUI : MonoBehaviour
    {
        protected string mSelectedCostume;
        protected CostumeTabUIManager mParent;
        protected CostumeTab mTab;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        protected List<CostumeItemUI> mCostumeItemUIs;
        public virtual void Init(CostumeTabUIManager parent, CostumeTab tab)
        {
            mParent = parent;
            mTab = tab;
            mCostumeItemUIs = new List<CostumeItemUI>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
            mCanvas = GetComponent<Canvas>();

            var costumeItemList = gameObject.FindGameObject("CostumeItemList");

            costumeItemList.AllChildObjectOff();

            foreach(var data in DataUtil.GetCostumeDatas(tab.ChangeToCostumeType()))
            {
                var costumeItemObj = Util.InstantiateUI("CostumeItemUI", costumeItemList.transform);

                var costumeItemUI = costumeItemObj.GetOrAddComponent<CostumeItemUI>();

                costumeItemUI.Init(data, OnSelectCostume);

                mCostumeItemUIs.Add(costumeItemUI);
            }
        }

        protected void OnSelectCostume(string id)
        {
            mParent.OnSelectCostume(id);

            RefreshCostumeItemUIs(id);
        }

        public void RefreshCostumeItemUIs(string id)
        {
            mSelectedCostume = id;

            foreach (CostumeItemUI costumeUI in mCostumeItemUIs)
            {
                costumeUI.SetSelect(mSelectedCostume == costumeUI.Id);
                costumeUI.Refresh();
            }

            if (Lance.GameManager != null && Lance.GameManager.IsInit)
            {
                if (mTab.ChangeToCostumeType() == CostumeType.Body)
                {
                    Lance.GameManager.OnSelectBodyCostume(mSelectedCostume);
                }
                else if (mTab.ChangeToCostumeType() == CostumeType.Etc)
                {
                    Lance.GameManager.OnSelectEtcCostume(mSelectedCostume);
                }
                else
                {
                    Lance.GameManager.OnSelectWeaponCostume(mSelectedCostume);
                }
            }
        }

        public virtual void OnEnter() 
        {
            OnSelectCostume(mSelectedCostume);
        }
        public virtual void OnLeave() { }
        public virtual void Localize() 
        {
            foreach (CostumeItemUI costumeUI in mCostumeItemUIs)
            {
                costumeUI.Localize();
            }
        }
        public virtual void Refresh() 
        {
            foreach (CostumeItemUI costumeUI in mCostumeItemUIs)
            {
                costumeUI.Refresh();
            }
        }
        public virtual void RefreshCostume() { }
        public virtual void OnUpdate() { }

        public void RefreshRedDots()
        {
            foreach(var costumeItem in mCostumeItemUIs)
            {
                costumeItem.RefreshRedDot();
            }
        }

        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }

    static class CostumeTabExt
    {
        public static CostumeType ChangeToCostumeType(this CostumeTab tab)
        {
            switch (tab)
            {
                case CostumeTab.Body:
                    return CostumeType.Body;
                case CostumeTab.Etc:
                    return CostumeType.Etc;
                case CostumeTab.Weapon:
                default:
                    return CostumeType.Weapon;
            }
        }
    }
}