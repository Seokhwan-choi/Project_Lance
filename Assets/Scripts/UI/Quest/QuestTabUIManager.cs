using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    enum QuestTab
    {
        Daily,
        Weekly,
        Repeat,

        Count
    }

    class QuestTabUIManager
    {
        QuestTab mCurTab;
        TabNavBarUIManager<QuestTab> mNavBarUI;

        Button mButtonAllReceive;
        GameObject mAllReceiveRedDotObj;
        GameObject mGameObject;
        List<QuestTabUI> mQuestTabUIList;
        public IEnumerator InitAsync(GameObject go)
        {
            mGameObject = go.FindGameObject("TabList");

            mNavBarUI = new TabNavBarUIManager<QuestTab>();
            mNavBarUI.Init(go.FindGameObject("NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(QuestTab.Daily);

            mButtonAllReceive = go.FindComponent<Button>("Button_AllReceive");
            mButtonAllReceive.SetButtonAction(ReceiveAllReward);

            mAllReceiveRedDotObj = mButtonAllReceive.gameObject.FindGameObject("RedDot");

            mQuestTabUIList = new List<QuestTabUI>();
            InitQuestTab<Quest_DailyTabUI>(QuestTab.Daily);

            yield return null;

            InitQuestTab<Quest_WeeklyTabUI>(QuestTab.Weekly);

            yield return null;

            InitQuestTab<Quest_RepeatTabUI>(QuestTab.Repeat);

            yield return null;

            Refresh();

            ShowTab(QuestTab.Daily);
        }

        public void InitQuestTab<T>(QuestTab tab) where T : QuestTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 QuestTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mQuestTabUIList.Add(tabUI);
        }

        public void Refresh()
        {
            var curTab = mQuestTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void ReceiveAllReward()
        {
            var curTab = mQuestTabUIList[(int)mCurTab];

            curTab.ReceiveAllReward();
        }

        public void OnReceiveReward()
        {
            mButtonAllReceive.SetActiveFrame(Lance.Account.AnyQuestCanReceiveReward(mCurTab.ChangeToQuestType()));

            RefreshRedDots();
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)QuestTab.Count; ++i)
            {
                QuestTab tab = (QuestTab)i;

                QuestUpdateType updateType = tab.ChangeToQuestType();

                mNavBarUI.SetActiveRedDot(tab, Lance.Account.AnyQuestCanReceiveReward(updateType));
            }

            mAllReceiveRedDotObj.SetActive(Lance.Account.AnyQuestCanReceiveReward(mCurTab.ChangeToQuestType()));
        }

        int OnChangeTabButton(QuestTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(QuestTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(QuestTab tab)
        {
            QuestTabUI showTab = mQuestTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(QuestTab tab)
        {
            QuestTabUI hideTab = mQuestTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    class QuestTabUI : MonoBehaviour
    {
        protected QuestTabUIManager mParent;
        protected QuestTab mTab;
        protected AchieveQuestUI mAchieveQuestUI;
        protected List<QuestItemUI> mQuestItemList;

        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        public virtual void Init(QuestTabUIManager parent, QuestTab tab) 
        {
            mParent = parent;
            mTab = tab;

            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        public virtual void OnEnter() 
        {
            Refresh();
        }
        public virtual void OnLeave() { }
        public virtual void Refresh() 
        {
            mAchieveQuestUI?.Refresh();

            foreach(var questItemUI in mQuestItemList)
            {
                questItemUI.Refresh();
            }

            OnReceiveReward();
        }
        public virtual void ReceiveAllReward() 
        {
            Lance.GameManager.ReceiveAllQuestReward(mTab.ChangeToQuestType(), () =>
            {
                if (mAchieveQuestUI != null)
                {
                    mAchieveQuestUI.RefreshProgress();
                    mAchieveQuestUI.PlayAllReceiveMotion();
                }

                foreach (var questItemUI in mQuestItemList)
                {
                    bool prev = questItemUI.IsReceived;

                    questItemUI.Refresh();

                    bool cur = questItemUI.IsReceived;

                    if (prev != cur)
                        questItemUI.PlayReceiveMotion();
                }

                OnReceiveReward();
            });
        }
        protected void InitAchieveQuestUI(QuestUpdateType updateType)
        {
            var achieveQuestUIObj = gameObject.FindGameObject("AchieveQuest");

            mAchieveQuestUI = achieveQuestUIObj.GetOrAddComponent<AchieveQuestUI>();
            mAchieveQuestUI.Init(updateType, OnReceiveReward);
        }

        protected void InitQuestItemUIs(QuestUpdateType updateType)
        {
            mQuestItemList = new List<QuestItemUI>();

            var questItemListObj = gameObject.FindGameObject("QuestItemList");
            questItemListObj.AllChildObjectOff();

            foreach (QuestInfo questInfo in Lance.Account.GetQuestInfos(updateType))
            {
                if (questInfo.GetQuestType().IsQuestClearType())
                    continue;

                var questItemUIObj = Util.InstantiateUI("QuestItemUI", questItemListObj.transform);

                QuestItemUI questItemUI = questItemUIObj.GetOrAddComponent<QuestItemUI>();

                questItemUI.Init(questInfo.GetId(), OnReceiveReward);

                mQuestItemList.Add(questItemUI);
            }
        }

        public void OnReceiveReward()
        {
            mAchieveQuestUI?.Refresh();

            mParent.OnReceiveReward();
        }

        public void SetVisible(bool isVisible)
        {
            mCanvas.enabled = isVisible;
            mGraphicRaycaster.enabled = isVisible;
        }
    }

    static class QuestTabExt
    {
        public static QuestUpdateType ChangeToQuestType(this QuestTab tab)
        {
            if (tab == QuestTab.Daily)
                return QuestUpdateType.Daily;
            else if (tab == QuestTab.Weekly)
                return QuestUpdateType.Weekly;
            else
                return QuestUpdateType.Repeat;
        }
    }
}