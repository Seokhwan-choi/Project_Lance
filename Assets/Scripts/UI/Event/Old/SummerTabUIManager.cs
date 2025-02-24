using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum SummerTab
    {
        Quest,
        Pass,
        Shop,

        Count,
    }

    class SummerTabUIManager
    {
        readonly float[] FrameMovePos = new float[] { 125, 352, 579 };
        Image mImageButtonFrame;

        string mEventId;
        SummerTab mCurTab;
        List<SummerTabItemUI> mTabItemUIList;
        List<SummerTabUI> mTabUIList;

        GameObject mGameObject;
        public void Init(GameObject go, string eventId)
        {
            mEventId = eventId;

            mGameObject = go.FindGameObject("Contents");

            var navBar = go.FindGameObject("Navbar");

            mImageButtonFrame = navBar.FindComponent<Image>("Image_Frame");
            mTabItemUIList = new List<SummerTabItemUI>();
            for (int i = 0; i < (int)SummerTab.Count; ++i)
            {
                SummerTab tab = (SummerTab)i;

                var itemObj = navBar.FindGameObject($"{tab}");

                var itemUI = itemObj.GetOrAddComponent<SummerTabItemUI>();
                itemUI.Init(tab, OnChangeTabButton);

                mTabItemUIList.Add(itemUI);
            }

            mTabUIList = new List<SummerTabUI>();
            InitSummerTab<SummerEvent_QuestTabUI>(eventId, SummerTab.Quest);
            InitSummerTab<SummerEvent_PassTabUI>(eventId, SummerTab.Pass);
            InitSummerTab<SummerEvent_ShopTabUI>(eventId, SummerTab.Shop);

            Refresh();

            ShowTab(SummerTab.Quest);
        }

        public void InitSummerTab<T>(string eventId, SummerTab tab) where T : SummerTabUI
        {
            GameObject tabObj = mGameObject.FindGameObject($"{tab}");

            Debug.Assert(tabObj != null, $"{tab}의 SummerTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, eventId, tab);

            tabUI.SetVisible(tab == SummerTab.Quest);

            mTabUIList.Add(tabUI);
        }

        public void Refresh()
        {
            var curTab = mTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)SummerTab.Count; ++i)
            {
                mTabUIList[i].RefreshRedDots();
            }

            foreach (var item in mTabItemUIList)
            {
                item.SetActiveRedDot(AnyCanReceiveEventReward(item.Tab));
                item.SetActiveTextDay(item.Tab == mCurTab);
            }

            bool AnyCanReceiveEventReward(SummerTab tab)
            {
                if (tab == SummerTab.Quest)
                {
                    return Lance.Account.Event.AnyCanReceiveQuestReward(mEventId);
                }
                else if (tab == SummerTab.Pass)
                {
                    return Lance.Account.Event.AnyCanReceivePassReward(mEventId);
                }
                else
                {
                    return false;
                }
            }
        }

        void OnChangeTabButton(SummerTab tab)
        {
            ChangeTab(tab);
        }

        public bool ChangeTab(SummerTab tab)
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

        void ShowTab(SummerTab tab)
        {
            SummerTabUI showTab = mTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(SummerTab tab)
        {
            SummerTabUI hideTab = mTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class SummerTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        protected SummerTabUIManager mParent;
        protected SummerTab mTab;
        protected string mEventId;
        public string EventId => mEventId;
        public virtual void Init(SummerTabUIManager parent, string eventId, SummerTab tab)
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

    class SummerTabItemUI : MonoBehaviour
    {
        SummerTab mTab;
        TextMeshProUGUI mTextName;
        GameObject mRedDotObj;
        public SummerTab Tab => mTab;
        public void Init(SummerTab tab, Action<SummerTab> onButton)
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
