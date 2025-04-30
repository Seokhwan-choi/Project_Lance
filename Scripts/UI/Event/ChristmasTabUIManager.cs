using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum ChristmasTab
    {
        Quest,
        Pass,
        Shop,

        Count,
    }

    class ChristmasTabUIManager
    {
        readonly float[] FrameMovePos = new float[] { 125, 352, 579 };
        Image mImageButtonFrame;

        string mEventId;
        ChristmasTab mCurTab;
        List<ChristmasTabItemUI> mTabItemUIList;
        List<ChristmasTabUI> mTabUIList;

        GameObject mGameObject;
        public void Init(GameObject go, string eventId)
        {
            mEventId = eventId;

            bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;
            var imageBanner = go.FindComponent<Image>("Image_Title");
            imageBanner.sprite = Lance.Atlas.GetUISprite(isKorean ? "Title_Event_Christmas2024" : "Title_Event_Christmas2024_Eng");

            mGameObject = go.FindGameObject("Contents");

            var navBar = go.FindGameObject("Navbar");

            mImageButtonFrame = navBar.FindComponent<Image>("Image_Frame");
            mTabItemUIList = new List<ChristmasTabItemUI>();
            for (int i = 0; i < (int)ChristmasTab.Count; ++i)
            {
                ChristmasTab tab = (ChristmasTab)i;

                var itemObj = navBar.FindGameObject($"{tab}");

                var itemUI = itemObj.GetOrAddComponent<ChristmasTabItemUI>();
                itemUI.Init(tab, OnChangeTabButton);

                mTabItemUIList.Add(itemUI);
            }

            mTabUIList = new List<ChristmasTabUI>();
            InitChristmasTab<ChristmasEvent_QuestTabUI>(eventId, ChristmasTab.Quest);
            InitChristmasTab<ChristmasEvent_PassTabUI>(eventId, ChristmasTab.Pass);
            InitChristmasTab<ChristmasEvent_ShopTabUI>(eventId, ChristmasTab.Shop);

            Refresh();

            ShowTab(ChristmasTab.Quest);
        }

        public void InitChristmasTab<T>(string eventId, ChristmasTab tab) where T : ChristmasTabUI
        {
            GameObject tabObj = mGameObject.FindGameObject($"{tab}");

            Debug.Assert(tabObj != null, $"{tab}의 ChristmasTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, eventId, tab);

            tabUI.SetVisible(tab == ChristmasTab.Quest);

            mTabUIList.Add(tabUI);
        }

        public void Refresh()
        {
            var curTab = mTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)ChristmasTab.Count; ++i)
            {
                mTabUIList[i].RefreshRedDots();
            }

            foreach (var item in mTabItemUIList)
            {
                item.SetActiveRedDot(AnyCanReceiveEventReward(item.Tab));
                item.SetActiveTextDay(item.Tab == mCurTab);
            }

            bool AnyCanReceiveEventReward(ChristmasTab tab)
            {
                if (tab == ChristmasTab.Quest)
                {
                    return Lance.Account.Event.AnyCanReceiveQuestReward(mEventId);
                }
                else if (tab == ChristmasTab.Pass)
                {
                    return Lance.Account.Event.AnyCanReceivePassReward(mEventId);
                }
                else
                {
                    return false;
                }
            }
        }

        void OnChangeTabButton(ChristmasTab tab)
        {
            ChangeTab(tab);
        }

        public bool ChangeTab(ChristmasTab tab)
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

        void ShowTab(ChristmasTab tab)
        {
            ChristmasTabUI showTab = mTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(ChristmasTab tab)
        {
            ChristmasTabUI hideTab = mTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class ChristmasTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        protected ChristmasTabUIManager mParent;
        protected ChristmasTab mTab;
        protected string mEventId;
        public string EventId => mEventId;
        public virtual void Init(ChristmasTabUIManager parent, string eventId, ChristmasTab tab)
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

    class ChristmasTabItemUI : MonoBehaviour
    {
        ChristmasTab mTab;
        TextMeshProUGUI mTextName;
        GameObject mRedDotObj;
        public ChristmasTab Tab => mTab;
        public void Init(ChristmasTab tab, Action<ChristmasTab> onButton)
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
