using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;
using System.Linq;


namespace Lance
{
    class JoustingRankingTierItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        JoustingInfo_TierTableTabUI mParent;
        Image mImageTier;
        Image mImageMyReward;
        TextMeshProUGUI mTextTierName;
        TextMeshProUGUI mTextTierScore;
        public void Init()
        {
            mParent = gameObject.GetComponentInParent<JoustingInfo_TierTableTabUI>();
            mImageTier = gameObject.FindComponent<Image>("Image_Tier");
            mImageMyReward = gameObject.FindComponent<Image>("Image_MyTier");
            mTextTierName = gameObject.FindComponent<TextMeshProUGUI>("Text_TierName");
            mTextTierScore = gameObject.FindComponent<TextMeshProUGUI>("Text_TierScore");
        }

        public void OnUpdateItem(int index)
        {
            if (mParent == null)
                mParent = gameObject.GetComponentInParent<JoustingInfo_TierTableTabUI>();

            if (mParent != null)
            {
                var data = mParent.GetTierData(index);
                if (data != null)
                {
                    mImageTier.sprite = Lance.Atlas.GetUISprite($"Icon_Joust_Tier_{data.tier}");
                    mTextTierScore.text = $"{data.rankScore}";
                    mTextTierName.text = StringTableUtil.GetName($"{data.tier}");
                    mImageMyReward.gameObject.SetActive(data.tier == mParent.MyTier);
                }
            }
            
        }
    }
}