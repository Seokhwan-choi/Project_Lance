using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum SnakeNewYearTab
    {
        Quest,
        Pass,
        Shop,

        Count,
    }

    class SnakeNewYearTabUIManager
    {
        readonly float[] FrameMovePos = new float[] { 125, 352, 579 };
        Image mImageButtonFrame;

        string mEventId;
        SnakeNewYearTab mCurTab;
        List<SnakeNewYearTabItemUI> mTabItemUIList;
        List<SnakeNewYearTabUI> mTabUIList;

        GameObject mGameObject;
        public void Init(GameObject go, string eventId)
        {
            mEventId = eventId;

            bool isKorean = Lance.LocalSave.LangCode == LangCode.KR;
            var imageBanner = go.FindComponent<Image>("Image_Title");
            imageBanner.sprite = Lance.Atlas.GetUISprite(isKorean ? "Title_Event_SnakeNewYear2025" : "Title_Event_SnakeNewYear2025_Eng");

            mGameObject = go.FindGameObject("Contents");

            var navBar = go.FindGameObject("Navbar");

            mImageButtonFrame = navBar.FindComponent<Image>("Image_Frame");
            mTabItemUIList = new List<SnakeNewYearTabItemUI>();
            for (int i = 0; i < (int)SnakeNewYearTab.Count; ++i)
            {
                SnakeNewYearTab tab = (SnakeNewYearTab)i;

                var itemObj = navBar.FindGameObject($"{tab}");

                var itemUI = itemObj.GetOrAddComponent<SnakeNewYearTabItemUI>();
                itemUI.Init(tab, OnChangeTabButton);

                mTabItemUIList.Add(itemUI);
            }

            mTabUIList = new List<SnakeNewYearTabUI>();
            InitSnakeNewYearTab<SnakeNewYearEvent_QuestTabUI>(eventId, SnakeNewYearTab.Quest);
            InitSnakeNewYearTab<SnakeNewYearEvent_PassTabUI>(eventId, SnakeNewYearTab.Pass);
            InitSnakeNewYearTab<SnakeNewYearEvent_ShopTabUI>(eventId, SnakeNewYearTab.Shop);

            Refresh();

            ShowTab(SnakeNewYearTab.Quest);
        }

        public void InitSnakeNewYearTab<T>(string eventId, SnakeNewYearTab tab) where T : SnakeNewYearTabUI
        {
            GameObject tabObj = mGameObject.FindGameObject($"{tab}");

            Debug.Assert(tabObj != null, $"{tab}의 SnakeNewYearTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, eventId, tab);

            tabUI.SetVisible(tab == SnakeNewYearTab.Quest);

            mTabUIList.Add(tabUI);
        }

        public void Refresh()
        {
            var curTab = mTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)SnakeNewYearTab.Count; ++i)
            {
                mTabUIList[i].RefreshRedDots();
            }

            foreach (var item in mTabItemUIList)
            {
                item.SetActiveRedDot(AnyCanReceiveEventReward(item.Tab));
                item.SetActiveTextDay(item.Tab == mCurTab);
            }

            bool AnyCanReceiveEventReward(SnakeNewYearTab tab)
            {
                if (tab == SnakeNewYearTab.Quest)
                {
                    return Lance.Account.Event.AnyCanReceiveQuestReward(mEventId);
                }
                else if (tab == SnakeNewYearTab.Pass)
                {
                    return Lance.Account.Event.AnyCanReceivePassReward(mEventId);
                }
                else
                {
                    return false;
                }
            }
        }

        void OnChangeTabButton(SnakeNewYearTab tab)
        {
            ChangeTab(tab);
        }

        public bool ChangeTab(SnakeNewYearTab tab)
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

        void ShowTab(SnakeNewYearTab tab)
        {
            SnakeNewYearTabUI showTab = mTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(SnakeNewYearTab tab)
        {
            SnakeNewYearTabUI hideTab = mTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class SnakeNewYearTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        protected SnakeNewYearTabUIManager mParent;
        protected SnakeNewYearTab mTab;
        protected string mEventId;
        public string EventId => mEventId;
        public virtual void Init(SnakeNewYearTabUIManager parent, string eventId, SnakeNewYearTab tab)
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

    class SnakeNewYearTabItemUI : MonoBehaviour
    {
        SnakeNewYearTab mTab;
        TextMeshProUGUI mTextName;
        GameObject mRedDotObj;
        public SnakeNewYearTab Tab => mTab;
        public void Init(SnakeNewYearTab tab, Action<SnakeNewYearTab> onButton)
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
