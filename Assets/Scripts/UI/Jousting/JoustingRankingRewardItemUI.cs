using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;


namespace Lance
{
    class JoustingRankingRewardItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        JoustingInfo_RankingRewardTabUI mParent;
        Image mImageMyReward;
        TextMeshProUGUI mTextRankRange;
        ItemSlotUI mRewardSlotGem;
        ItemSlotUI mRewardSlotCoin;
        ItemSlotUI mRewardSlotToken;
        public void Init()
        {
            mParent = gameObject.GetComponentInParent<JoustingInfo_RankingRewardTabUI>();

            mTextRankRange = gameObject.FindComponent<TextMeshProUGUI>("Text_RankRange");
            mImageMyReward = gameObject.FindComponent<Image>("Image_MyReward");

            var rewardSlotGemObj = gameObject.FindGameObject("JoustingRankingRewardSlot_Gem");
            mRewardSlotGem = rewardSlotGemObj.GetOrAddComponent<ItemSlotUI>();

            var rewardSlotCoinObj = gameObject.FindGameObject("JoustingRankingRewardSlot_Coin");
            mRewardSlotCoin = rewardSlotCoinObj.GetOrAddComponent<ItemSlotUI>();

            var rewardSlotTokenObj = gameObject.FindGameObject("JoustingRankingRewardSlot_Token");
            mRewardSlotToken = rewardSlotTokenObj.GetOrAddComponent<ItemSlotUI>();
        }

        public void OnUpdateItem(int index)
        {
            var data = Lance.GameData.JoustingRankingRewardData[index];
            if (data != null)
            {
                if (mParent == null)
                    mParent = gameObject.GetComponentInParent<JoustingInfo_RankingRewardTabUI>();

                string rankText = string.Empty;
                bool isMyReward = false;
                if (data.rankMin > 0 && data.rankMax > 0)
                {
                    StringParam param = new StringParam("rankMin", data.rankMin);
                    param.AddParam("rankMax", data.rankMax);

                    rankText = StringTableUtil.Get("UIString_RankRange", param);

                    isMyReward = data.rankMin > mParent.MyRank && data.rankMax <= mParent.MyRank;
                }
                else if (data.rankMax > 0)
                {
                    StringParam param = new StringParam("rank", data.rankMax);

                    rankText = StringTableUtil.Get("UIString_Rank", param);

                    isMyReward = data.rankMax == mParent.MyRank;
                }
                else
                {
                    rankText = StringTableUtil.Get("UIString_RankParticipation");

                    isMyReward = true;
                }

                mTextRankRange.text = rankText;
                mImageMyReward.gameObject.SetActive(isMyReward);

                var rewardResult = Lance.GameManager.RewardDataChangeToRewardResult(data.reward);

                mRewardSlotGem.Init(new ItemInfo(ItemType.Gem, rewardResult.gem));
                mRewardSlotCoin.Init(new ItemInfo(ItemType.JoustingCoin, rewardResult.joustCoin));
                mRewardSlotToken.Init(new ItemInfo(ItemType.GloryToken, rewardResult.gloryToken));
            }
        }
    }
}
