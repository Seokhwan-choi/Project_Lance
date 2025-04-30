using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum ChuseokTab
    {
        Quest,
        Pass,
        Shop,

        Count,
    }

    class ChuseokTabUIManager
    {
        readonly float[] FrameMovePos = new float[] { 125, 352, 579 };
        Image mImageButtonFrame;

        string mEventId;
        ChuseokTab mCurTab;
        List<ChuseokTabItemUI> mTabItemUIList;
        List<ChuseokTabUI> mTabUIList;

        GameObject mGameObject;
        public void Init(GameObject go, string eventId)
        {
            mEventId = eventId;

            mGameObject = go.FindGameObject("Contents");

            var navBar = go.FindGameObject("Navbar");

            mImageButtonFrame = navBar.FindComponent<Image>("Image_Frame");
            mTabItemUIList = new List<ChuseokTabItemUI>();
            for (int i = 0; i < (int)ChuseokTab.Count; ++i)
            {
                ChuseokTab tab = (ChuseokTab)i;

                var itemObj = navBar.FindGameObject($"{tab}");

                var itemUI = itemObj.GetOrAddComponent<ChuseokTabItemUI>();
                itemUI.Init(tab, OnChangeTabButton);

                mTabItemUIList.Add(itemUI);
            }

            mTabUIList = new List<ChuseokTabUI>();
            InitChuseokTab<ChuseokEvent_QuestTabUI>(eventId, ChuseokTab.Quest);
            InitChuseokTab<ChuseokEvent_PassTabUI>(eventId, ChuseokTab.Pass);
            InitChuseokTab<ChuseokEvent_ShopTabUI>(eventId, ChuseokTab.Shop);

            Refresh();

            ShowTab(ChuseokTab.Quest);
        }

        public void InitChuseokTab<T>(string eventId, ChuseokTab tab) where T : ChuseokTabUI
        {
            GameObject tabObj = mGameObject.FindGameObject($"{tab}");

            Debug.Assert(tabObj != null, $"{tab}의 ChuseokTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, eventId, tab);

            tabUI.SetVisible(tab == ChuseokTab.Quest);

            mTabUIList.Add(tabUI);
        }

        public void Refresh()
        {
            var curTab = mTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)ChuseokTab.Count; ++i)
            {
                mTabUIList[i].RefreshRedDots();
            }

            foreach (var item in mTabItemUIList)
            {
                item.SetActiveRedDot(AnyCanReceiveEventReward(item.Tab));
                item.SetActiveTextDay(item.Tab == mCurTab);
            }

            bool AnyCanReceiveEventReward(ChuseokTab tab)
            {
                if (tab == ChuseokTab.Quest)
                {
                    return Lance.Account.Event.AnyCanReceiveQuestReward(mEventId);
                }
                else if (tab == ChuseokTab.Pass)
                {
                    return Lance.Account.Event.AnyCanReceivePassReward(mEventId);
                }
                else
                {
                    return false;
                }
            }
        }

        void OnChangeTabButton(ChuseokTab tab)
        {
            ChangeTab(tab);
        }

        public bool ChangeTab(ChuseokTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            MoveToButtonFrame();

            foreach (var item in mTabItemUIList)
            {
                item.SetActiveTextDay(item.Tab == mCurTab);
            }

            return true;
        }

        void MoveToButtonFrame()
        {
            float endValue = FrameMovePos[(int)mCurTab];

            mImageButtonFrame.rectTransform.DOAnchorPosX(endValue, 0.25f).SetAutoKill(false);
        }

        void ShowTab(ChuseokTab tab)
        {
            ChuseokTabUI showTab = mTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(ChuseokTab tab)
        {
            ChuseokTabUI hideTab = mTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class ChuseokTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        protected ChuseokTabUIManager mParent;
        protected ChuseokTab mTab;
        protected string mEventId;
        public string EventId => mEventId;
        public virtual void Init(ChuseokTabUIManager parent, string eventId, ChuseokTab tab)
        {
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();

            mParent = parent;
            mEventId = eventId;
            mTab = tab;
        }
        public virtual void OnEnter()
        {
            Refresh();
        }
        public virtual void OnLeave() { }
        public virtual void Refresh() { }
        public virtual void RefreshRedDots() { }
        public void SetVisible(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }

    class ChuseokTabItemUI : MonoBehaviour
    {
        ChuseokTab mTab;
        TextMeshProUGUI mTextName;
        GameObject mRedDotObj;
        public ChuseokTab Tab => mTab;
        public void Init(ChuseokTab tab, Action<ChuseokTab> onButton)
        {
            mTab = tab;

            mTextName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            mRedDotObj = gameObject.FindGameObject("RedDot");

            var button = gameObject.GetComponent<Button>();
            button.SetButtonAction(() => onButton.Invoke(tab));
        }

        public void SetActiveTextDay(bool isActive)
        {
            mTextName.color = isActive ? Color.white : Color.gray;
        }

        public void SetActiveRedDot(bool isActive)
        {
            mRedDotObj.SetActive(isActive);
        }
    }
}
