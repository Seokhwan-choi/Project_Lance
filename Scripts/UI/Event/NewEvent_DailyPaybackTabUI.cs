using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;
using System.Linq;

namespace Lance
{
    class NewEvent_DailyPaybackTabUI : NewEventTabUI
    {
        QuestData[] mQuestDatas;
        DynamicVScrollView mScrollView;
        public override void Init(NewEventTabUIManager parent, NewEventTab tab)
        {
            base.Init(parent, tab);

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");

            RefreshQuestDatas();
        }

        public override void Refresh()
        {
            mScrollView.refresh();
        }

        public override void RefreshRedDots()
        {
            mScrollView.refresh();
        }

        void OnChangeDayAction()
        {
            RefreshQuestDatas();
        }

        void RefreshQuestDatas()
        {
            mQuestDatas = DataUtil.GetEventQuestDatas(mEventId).ToArray();

            mScrollView.totalItemCount = mQuestDatas.Length;

            mScrollView.refresh();
        }

        public override QuestData GetQuestData(int index)
        {
            return mQuestDatas[index];
        }
    }
}