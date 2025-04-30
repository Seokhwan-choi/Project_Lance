using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    enum NewEventTab
    {
        NewbieSupportBuffEvent,
        NewbieEvent,
        TrainSupportEvent,
        PaybackEvent,
        DailyPaybackEvent,
        //SnakeNewYear2025Event,

        Count
    }

    class NewEventTabUIManager
    {
        NewEventTab mCurTab;
        TabNavBarUIManager<NewEventTab> mNavBarUI;
        List<NewEventTabUI> mEventTabUIList;

        GameObject mGameObject;
        public IEnumerator DelayedInit(GameObject go)
        {
            mGameObject = go.FindGameObject("TabList");

            mNavBarUI = new TabNavBarUIManager<NewEventTab>();
            mNavBarUI.Init(go.FindGameObject("Navbar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(NewEventTab.NewbieSupportBuffEvent);
            //mNavBarUI.SetActiveTab(NewEventTab.SnakeNewYear2025Event, true);

            mEventTabUIList = new List<NewEventTabUI>();

            InitNewEventTab<NewEvent_NewbieSupportBuffTabUI>(NewEventTab.NewbieSupportBuffEvent);

            yield return new WaitForSeconds(0.1f);

            InitNewEventTab<NewEvent_NewbieTabUI>(NewEventTab.NewbieEvent);

            yield return new WaitForSeconds(0.5f);

            InitNewEventTab<NewEvent_TrainSupportTabUI>(NewEventTab.TrainSupportEvent);

            yield return new WaitForSeconds(0.1f);

            InitNewEventTab<NewEvent_PaybackTabUI>(NewEventTab.PaybackEvent);

            yield return new WaitForSeconds(0.1f);

            InitNewEventTab<NewEvent_DailyPaybackTabUI>(NewEventTab.DailyPaybackEvent);

            //yield return new WaitForSeconds(0.1f);

            //InitNewEventTab<NewEvent_SnakeNewYearTabUI>(NewEventTab.SnakeNewYear2025Event);

            //var eventType = NewEventTab.SnakeNewYear2025Event.ChangeToEventType();
            //var eventData = DataUtil.GetEventData(eventType);
            //if (eventData != null)
            //{
            //    if (eventData.active)
            //    {
            //        if (eventData.startDate > 0 && eventData.endDate > 0)
            //        {
            //            if (TimeUtil.IsActiveDateNum(eventData.startDate, eventData.endDate))
            //            {
            //                mNavBarUI.SetActiveTab(NewEventTab.SnakeNewYear2025Event, true);
            //            }
            //        }
            //    }
            //}

            //yield return new WaitForSeconds(0.33f);

            //InitNewEventTab<NewEvent_SnakeNewYearTabUI>(NewEventTab.SnakeNewYear2025Event);

            Refresh();

            ShowTab(NewEventTab.NewbieSupportBuffEvent);
        }

        public void InitNewEventTab<T>(NewEventTab tab) where T : NewEventTabUI
        {
            GameObject tabObj = Util.InstantiateUI($"{tab}TabUI", mGameObject.transform);

            Debug.Assert(tabObj != null, $"{tab}의 NewEventTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(this, tab);

            tabUI.SetVisible(tab == NewEventTab.NewbieSupportBuffEvent);

            mEventTabUIList.Add(tabUI);
        }

        public void Refresh()
        {
            var curTab = mEventTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)NewEventTab.Count; ++i)
            {
                NewEventTab tab = (NewEventTab)i;

                EventType type = tab.ChangeToEventType();

                mNavBarUI.SetActiveRedDot(tab, Lance.Account.AnyCanReceiveNewEventReward(type));

                mEventTabUIList[i].RefreshRedDots();
            }
        }

        int OnChangeTabButton(NewEventTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public bool ChangeTab(NewEventTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(NewEventTab tab)
        {
            NewEventTabUI showTab = mEventTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();
        }

        void HideTab(NewEventTab tab)
        {
            NewEventTabUI hideTab = mEventTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }
    }

    static class NewEventTabExt
    {
        public static EventType ChangeToEventType(this NewEventTab tab)
        {
            if (tab == NewEventTab.NewbieEvent)
                return EventType.Newbie;
            else if (tab == NewEventTab.TrainSupportEvent)
                return EventType.TrainSupport;
            else if (tab == NewEventTab.PaybackEvent)
                return EventType.Payback;
            else if (tab == NewEventTab.DailyPaybackEvent)
                return EventType.DailyPayback;
            //else if (tab == NewEventTab.SnakeNewYear2025Event)
            //    return EventType.SnakeNewYear2025;
            else
                return EventType.NewbieSupportBuff;
                //return EventType.Halloween2024;
            //else if (tab == NewEventTab.Chuseok2024Event)
            //    return EventType.Chuseok2024;
            //else
            //    return EventType.Summer202406;
        }
    }
}