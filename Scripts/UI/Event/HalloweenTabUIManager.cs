using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum HalloweenTab
    {
        Quest,
        Pass,
        Shop,

        Count,
    }

    class HalloweenTabUIManager
    {
        readonly float[] FrameMovePos = new float[] { 125, 352, 579 };
        Image mImageButtonFrame;

        string mEventId;
        HalloweenTab mCurTab;
        List<HalloweenTabItemUI> mTabItemUIList;
        List<HalloweenTabUI> mTabUIList;

        GameObject mGameObject;
        public void Init(GameObject go, string eventId)
        {
            mEventId = eventId;

            mGameObject = go.FindGameObject("Contents");

            var navBar = go.FindGameObject("Navbar");

            mImageButtonFrame = navBar.FindComponent<Image>("Image_Frame");
            mTabItemUIList = new List<HalloweenTabItemUI>();
            for (int i = 0; i < (int)HalloweenTab.Count; ++i)
            {
                HalloweenTab tab = (HalloweenTab)i;

                var itemObj = navBar.FindGameObject($"{tab}");

                var itemUI = itemObj.GetOrAddComponent<HalloweenTabItemUI>();
                itemUI.Init(tab, OnChangeTabButton);

                mTabItemUIList.Add(itemUI);
            }

            mTabUIList = new List<HalloweenTabUI>();
            InitHalloweenTab<HalloweenEvent_QuestTabUI>(eventId, HalloweenTab.Quest);
            InitHalloweenTab<HalloweenEvent_PassTabUI>(eventId, HalloweenTab.Pass);
            InitHalloweenTab<HalloweenEvent_ShopTabUI>(eventId, HalloweenTab.Shop);

            Refresh();

            ShowTab(HalloweenTab.Quest);
        }

        public void InitHalloweenTab<T>(string eventId, HalloweenTab tab) where T : HalloweenTabUI
        {
            GameObject tabObj = mGameObject.FindGameObject($"{tab}");

            Debug.Assert(tabObj != null, $"{tab}의 HalloweenTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, eventId, tab);

            tabUI.SetVisible(tab == HalloweenTab.Quest);

            mTabUIList.Add(tabUI);
        }

        public void Refresh()
        {
            var curTab = mTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)HalloweenTab.Count; ++i)
            {
                mTabUIList[i].RefreshRedDots();
            }

            foreach (var item in mTabItemUIList)
            {
                item.SetActiveRedDot(AnyCanReceiveEventReward(item.Tab));
                item.SetActiveTextDay(item.Tab == mCurTab);
            }

            bool AnyCanReceiveEventReward(HalloweenTab tab)
            {
                if (tab == HalloweenTab.Quest)
                {
                    return Lance.Account.Event.AnyCanReceiveQuestReward(mEventId);
                }
                else if (tab == HalloweenTab.Pass)
                {
                    return Lance.Account.Event.AnyCanReceivePassReward(mEventId);
                }
                else
                {
                    return false;
                }
            }
        }

        void OnChangeTabButton(HalloweenTab tab)
        {
            ChangeTab(tab);
        }

        public bool ChangeTab(HalloweenTab tab)
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

        void ShowTab(HalloweenTab tab)
        {
            HalloweenTabUI showTab = mTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(HalloweenTab tab)
        {
            HalloweenTabUI hideTab = mTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class HalloweenTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        protected HalloweenTabUIManager mParent;
        protected HalloweenTab mTab;
        protected string mEventId;
        public string EventId => mEventId;
        public virtual void Init(HalloweenTabUIManager parent, string eventId, HalloweenTab tab)
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

    class HalloweenTabItemUI : MonoBehaviour
    {
        HalloweenTab mTab;
        TextMeshProUGUI mTextName;
        GameObject mRedDotObj;
        public HalloweenTab Tab => mTab;
        public void Init(HalloweenTab tab, Action<HalloweenTab> onButton)
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
