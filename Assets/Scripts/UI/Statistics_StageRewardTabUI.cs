using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Statistics_StageRewardTabUI : StatisticsTabUI
    {
        TextMeshProUGUI mTextKillCountValue;
        RectTransform mRewardListRectTm;
        RewardResult mStackedReward;
        int mStackedKillCount;

        List<ItemSlotUI> mStackedRewardSlotUIList;
        public override void Init(StatisticsTab tab)
        {
            base.Init(tab);

            mRewardListRectTm = gameObject.FindComponent<RectTransform>("StatisticsRewardList");

            mRewardListRectTm.AllChildObjectOff();

            mStackedRewardSlotUIList = new List<ItemSlotUI>();

            mTextKillCountValue = gameObject.FindComponent<TextMeshProUGUI>("Text_KillCountValue");
        }

        public override void StackReward(RewardResult reward)
        {
            mStackedKillCount++;

            RefreshKillCountText();

            mStackedReward = mStackedReward.AddReward(reward);

            RefreshStackedReward();
        }

        void RefreshStackedReward()
        {
            if (mCanvas.enabled == false || mGraphicRaycaster.enabled == false)
                return;

            if (mStackedReward.IsEmpty())
            {
                foreach (var rewardSlotUI in mStackedRewardSlotUIList)
                {
                    Lance.ObjectPool.ReleaseUI(rewardSlotUI.gameObject);
                }

                mStackedRewardSlotUIList.Clear();
            }
            else
            {
                if (mStackedRewardSlotUIList.Count > 0)
                {
                    var splitReward = mStackedReward.Split();

                    for (int i = 0; i < splitReward.Count; ++i)
                    {
                        if (mStackedRewardSlotUIList.Count > i)
                        {
                            var itemSlotUI = mStackedRewardSlotUIList[i];

                            itemSlotUI.Init(splitReward[i]);
                        }
                        else
                        {
                            var itemSlotObj = Lance.ObjectPool.AcquireUI("StatisticsRewardSlotUI", mRewardListRectTm);

                            var itemSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
                            itemSlotUI.Init(splitReward[i]);

                            mStackedRewardSlotUIList.Add(itemSlotUI);
                        }
                    }
                }
                else
                {
                    mStackedRewardSlotUIList = ItemSlotUIUtil.CreateItemSlotUIList(mRewardListRectTm, mStackedReward, "StatisticsRewardSlotUI");
                }
            }
        }

        void RefreshKillCountText()
        {
            mTextKillCountValue.text = $"{mStackedKillCount}";
        }

        public override void ResetInfo()
        {
            base.ResetInfo();

            mStackedKillCount = 0;

            RefreshKillCountText();

            mStackedReward = new RewardResult();

            RefreshStackedReward();
        }
    }
}