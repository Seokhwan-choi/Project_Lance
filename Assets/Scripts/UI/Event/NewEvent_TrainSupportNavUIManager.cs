using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum TrainSupportTab
    {
        Beginner,
        Intermediate,
        Advanced,

        Count
    }

    class NewEvent_TrainSupportNavUIManager
    {
        readonly float[] FrameMovePos = new float[] { 120, 350, 580 };
        Image mImageButtonFrame;

        string mEventId;

        TrainSupportTab mSelectedTab;
        Action mOnChangeAction;
        List<TrainSupportTabItemUI> mTrainSupportTabItemList;
        public TrainSupportTab SelectedTab => mSelectedTab;
        public void Init(string eventId, GameObject go, Action onChangeAction)
        {
            mEventId = eventId;
            mOnChangeAction = onChangeAction;
            mTrainSupportTabItemList = new List<TrainSupportTabItemUI>();

            var list = go.FindGameObject("TrainList");

            mImageButtonFrame = list.FindComponent<Image>("Image_Frame");

            for (int i = 0; i < (int)TrainSupportTab.Count; ++i)
            {
                TrainSupportTab trainSupportTab = (TrainSupportTab)i;

                var itemObj = list.FindGameObject($"{trainSupportTab}");

                var itemUI = itemObj.GetOrAddComponent<TrainSupportTabItemUI>();
                itemUI.Init(trainSupportTab, OnChangeTrainSupportTab);

                mTrainSupportTabItemList.Add(itemUI);
            }

            RefreshTrainSupportTabItems();
        }

        void MoveToButtonFrame()
        {
            float endValue = FrameMovePos[(int)mSelectedTab];

            mImageButtonFrame.rectTransform.DOAnchorPosX(endValue, 0.25f).SetAutoKill(false);
        }

        void OnChangeTrainSupportTab(TrainSupportTab trainSupportTab)
        {
            if (mSelectedTab != trainSupportTab)
            {
                mSelectedTab = trainSupportTab;

                MoveToButtonFrame();
            }

            mOnChangeAction?.Invoke();

            RefreshTrainSupportTabItems();
        }

        public void RefreshTrainSupportTabItems()
        {
            foreach (var item in mTrainSupportTabItemList)
            {
                var result = DataUtil.SplitTrainSupportRange(item.TrainSupportTab);

                item.SetActiveRedDot(Lance.Account.Event.AnyCanReceiveQuestReward(mEventId, result.startRange, result.endRange));
                item.SetActiveTextDay(mSelectedTab == item.TrainSupportTab);
            }
        }
    }

    class TrainSupportTabItemUI : MonoBehaviour
    {
        TrainSupportTab mTrainSupportTab;
        TextMeshProUGUI mTextName;
        GameObject mRedDotObj;
        public TrainSupportTab TrainSupportTab => mTrainSupportTab;
        public void Init(TrainSupportTab TrainSupportTab, Action<TrainSupportTab> onButton)
        {
            mTrainSupportTab = TrainSupportTab;

            mTextName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            //mTextName.text = StringTableUtil.GetName($"{TrainSupportTab}");

            mRedDotObj = gameObject.FindGameObject("RedDot");

            var button = gameObject.GetComponent<Button>();
            button.SetButtonAction(() => onButton.Invoke(TrainSupportTab));
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