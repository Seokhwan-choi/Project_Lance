using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;
using System.Linq;

namespace Lance
{
    class NewEvent_NewbieTabUI : NewEventTabUI
    {
        QuestData[] mQuestDatas;
        DynamicVScrollView mScrollView;
        NewEvent_NewbieDayUIManager mNewbieDayUIManager;
        public override void Init(NewEventTabUIManager parent, NewEventTab tab)
        {
            base.Init(parent, tab);

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");

            mNewbieDayUIManager = new NewEvent_NewbieDayUIManager();
            mNewbieDayUIManager.Init(mEventId, gameObject, OnChangeDayAction);

            RefreshQuestDatas();
        }

        public override void Refresh()
        {
            mScrollView.refresh();
        }

        public override void RefreshRedDots()
        {
            mScrollView.refresh();

            mNewbieDayUIManager.RefreshNewbieDayItems();
        }

        void OnChangeDayAction()
        {
            RefreshQuestDatas();
        }

        void RefreshQuestDatas()
        {
            mQuestDatas = DataUtil.GetEventQuestDatas(mEventId).Where(x => x.openDay == (int)mNewbieDayUIManager.SelectedDay + 1).ToArray();

            mScrollView.totalItemCount = mQuestDatas.Length;

            mScrollView.refresh();
        }

        public override QuestData GetQuestData(int index)
        {
            return mQuestDatas[index];
        }
    }
}