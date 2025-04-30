using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;

namespace Lance
{
    class RaidDungeonRewardItemUI : MonoBehaviour,IDynamicScrollViewItem
    {
        bool mInit;
        Image mImageRank;
        TextMeshProUGUI mTextDamageRange;
        List<ItemSlotUI> mRewardSlotUIList;
        Popup_RaidDungeonRewardInfoUI mParent;

        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<Popup_RaidDungeonRewardInfoUI>();
            mImageRank = gameObject.FindComponent<Image>("Image_Rank");
            mTextDamageRange = gameObject.FindComponent<TextMeshProUGUI>("Text_DamageRange");

            mRewardSlotUIList = new List<ItemSlotUI>();

            for(int i = 0; i < 2; ++i)
            {
                int rewardSlotIndex = i + 1;

                var rewardSlotObj = gameObject.FindGameObject($"RewardSlot_{rewardSlotIndex}");
                var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();

                mRewardSlotUIList.Add(rewardSlotUI);
            }
        }

        public void OnUpdateItem(int index)
        {
            if (mParent == null)
                mParent = gameObject.GetComponentInParent<Popup_RaidDungeonRewardInfoUI>();

            if (mParent != null)
            {
                var data = mParent.GetRaidRewardData(index);
                if (data != null)
                {
                    mImageRank.sprite = Lance.Atlas.GetIconGrade(data.rankGrade);

                    StringParam param = new StringParam("minValue", data.minValue.ToAlphaString());
                    param.AddParam("maxValue", data.maxValue > 0 ? data.maxValue.ToAlphaString() : "");

                    mTextDamageRange.text = StringTableUtil.Get("UIString_Range", param);

                    var rewardData = Lance.GameData.RewardData.TryGet(data.reward);
                    if (rewardData != null)
                    {
                        var itemInfos = ItemInfoUtil.CreateItemInfos(rewardData);
                        if (itemInfos != null)
                        {
                            for(int i = 0; i < mRewardSlotUIList.Count; ++i)
                            {
                                if (itemInfos.Count <= i)
                                {
                                    mRewardSlotUIList[i].gameObject.SetActive(false);
                                }
                                else
                                {
                                    mRewardSlotUIList[i].gameObject.SetActive(true);
                                    mRewardSlotUIList[i].Init(itemInfos[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mRewardSlotUIList.Count; ++i)
                        {
                            mRewardSlotUIList[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

}
