using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum TrainCount
    {
        One,
        Ten,
        Hundred,

        Count
    }

    class GoldTrainCountUIManager
    {
        readonly float[] FrameMovePos = new float[] { -150, 0, 150 };

        Image mImageButtonFrame;

        TrainCount mTrainCount;
        Stature_GoldTrainTabUI mParent;
        List<GoldTrainCountItemUI> mGoldTrainCountItemList;
        public int GoldTrainCount => mTrainCount.ChangeToInt();
        public void Init(Stature_GoldTrainTabUI parent)
        {
            mParent = parent;
            mGoldTrainCountItemList = new List<GoldTrainCountItemUI>();

            var list = parent.gameObject.FindGameObject("GoldTrainCountList");

            mImageButtonFrame = list.FindComponent<Image>("Image_Frame");

            for (int i = 0; i < (int)TrainCount.Count; ++i)
            {
                TrainCount trainCount = (TrainCount)i;

                var itemObj = list.FindGameObject($"TrainCount_{trainCount}");

                var itemUI = itemObj.GetOrAddComponent<GoldTrainCountItemUI>();
                itemUI.Init(trainCount, OnChangeGoldTrainCount);

                mGoldTrainCountItemList.Add(itemUI);
            }

            RefreshCountItems();
        }

        void MoveToButtonFrame()
        {
            float endValue = FrameMovePos[(int)mTrainCount];

            mImageButtonFrame.rectTransform.DOAnchorPosX(endValue, 0.25f)
                .SetAutoKill(false);
        }

        void OnChangeGoldTrainCount(TrainCount trainCount)
        {
            if (mTrainCount != trainCount)
            {
                mTrainCount = trainCount;

                MoveToButtonFrame();
            }

            mParent.OnChangeGoldTrainCount();

            RefreshCountItems();
        }

        void RefreshCountItems()
        {
            foreach(var item in mGoldTrainCountItemList)
            {
                item.SetActiveTextCount(mTrainCount == item.TrainCount);
            }
        }
    }

    class GoldTrainCountItemUI : MonoBehaviour
    {
        TrainCount mTrainCount;
        TextMeshProUGUI mTextCount;
        public TrainCount TrainCount => mTrainCount;
        public void Init(TrainCount trainCount, Action<TrainCount> onButton)
        {
            mTrainCount = trainCount;

            mTextCount = gameObject.FindComponent<TextMeshProUGUI>("Text_Count");
            mTextCount.text = $"x{trainCount.ChangeToInt()}";

            var button = gameObject.GetComponent<Button>();
            button.SetButtonAction(() => onButton.Invoke(trainCount));
        }

        public void SetActiveTextCount(bool isActive)
        {
            mTextCount.color = isActive ? Color.white : Color.gray;
        }
    }

    static class TrainCountExt
    {
        static int[] TrainCount = new int[] { 1, 10, 100 };
        public static int ChangeToInt(this TrainCount trainCount)
        {
            return TrainCount[(int)trainCount];
        }
    }
}