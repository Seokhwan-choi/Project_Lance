using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Lance
{
    enum FeedCount
    {
        One,
        Ten,
        Hundred,

        Count
    }

    class PetFeedCountUIManager
    {
        readonly float[] FrameMovePos = new float[] { -88, 0, 88 };

        Image mImageButtonFrame;

        FeedCount mFeedCount;
        List<FeedCountItemUI> mFeedCountItemList;
        public void Init(GameObject parent, Action<int> onChangeFeedCount)
        {
            mFeedCountItemList = new List<FeedCountItemUI>();

            var list = parent.FindGameObject("FeedCountList");

            mImageButtonFrame = list.FindComponent<Image>("Image_Frame");

            for (int i = 0; i < (int)FeedCount.Count; ++i)
            {
                FeedCount feedCount = (FeedCount)i;

                var itemObj = list.FindGameObject($"FeedCount_{feedCount}");

                var itemUI = itemObj.GetOrAddComponent<FeedCountItemUI>();
                itemUI.Init(feedCount, () =>
                {
                    if (mFeedCount != feedCount)
                    {
                        mFeedCount = feedCount;

                        MoveToButtonFrame();
                    }

                    int feedCountInt = feedCount.ChangeToInt();

                    onChangeFeedCount?.Invoke(feedCountInt);

                    RefreshCountItems();
                });

                mFeedCountItemList.Add(itemUI);
            }

            RefreshCountItems();
        }

        void MoveToButtonFrame()
        {
            float endValue = FrameMovePos[(int)mFeedCount];

            mImageButtonFrame.rectTransform.DOAnchorPosX(endValue, 0.25f)
                .SetAutoKill(false);
        }

        void RefreshCountItems()
        {
            foreach (var item in mFeedCountItemList)
            {
                item.SetActiveTextCount(mFeedCount == item.FeedCount);
            }
        }
    }

    class FeedCountItemUI : MonoBehaviour
    {
        FeedCount mFeedCount;
        TextMeshProUGUI mTextCount;
        public FeedCount FeedCount => mFeedCount;
        public void Init(FeedCount feedCount, Action onButton)
        {
            mFeedCount = feedCount;

            mTextCount = gameObject.FindComponent<TextMeshProUGUI>("Text_Count");
            mTextCount.text = $"x{feedCount.ChangeToInt()}";

            var button = gameObject.GetComponent<Button>();
            button.SetButtonAction(() => onButton.Invoke());
        }

        public void SetActiveTextCount(bool isActive)
        {
            mTextCount.color = isActive ? Color.white : Color.gray;
        }
    }

    static class FeedCountExt
    {
        static int[] FeedCount = new int[] { 1, 10, 100 };
        public static int ChangeToInt(this FeedCount trainCount)
        {
            return FeedCount[(int)trainCount];
        }
    }
}