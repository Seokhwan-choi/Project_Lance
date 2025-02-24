using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;
using System.Linq;

namespace Lance
{
    class NewEvent_TrainSupportTabUI : NewEventTabUI
    {
        QuestData[] mQuestDatas;
        DynamicVScrollView mScrollView;
        NewEvent_TrainSupportNavUIManager mTrainSupportNavUIManager;
        public override void Init(NewEventTabUIManager parent, NewEventTab tab)
        {
            base.Init(parent, tab);

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");

            mTrainSupportNavUIManager = new NewEvent_TrainSupportNavUIManager();
            mTrainSupportNavUIManager.Init(mEventId, gameObject, OnChangeTabAction);

            RefreshQuestDatas();
        }

        public override void Refresh()
        {
            mScrollView.refresh();
        }

        public override void RefreshRedDots()
        {
            mScrollView.refresh();

            mTrainSupportNavUIManager.RefreshTrainSupportTabItems();
        }

        void OnChangeTabAction()
        {
            RefreshQuestDatas();
        }

        void RefreshQuestDatas()
        {
            var result = DataUtil.SplitTrainSupportRange(mTrainSupportNavUIManager.SelectedTab); 

            mQuestDatas = DataUtil.GetEventQuestDatas(mEventId).Where(x => x.requireCount >= result.startRange && x.requireCount <= result.endRange).ToArray();

            mScrollView.totalItemCount = mQuestDatas.Length;

            mScrollView.refresh();
        }

        public override QuestData GetQuestData(int index)
        {
            return mQuestDatas[index];
        }
    }
}