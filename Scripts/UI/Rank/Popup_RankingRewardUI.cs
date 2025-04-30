using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_RankingRewardUI : PopupBase
    {
        public void Init(RankingTab tab)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get($"Title_{tab}RankingReward"));

            int myRank = 0;

            var rankInfo = Lance.Account.Leaderboard.GetLeaderboardInfo(tab.ChangeToTableName());

            var myRankItem = rankInfo.MyUserLeaderboardItem;
            if (myRankItem != null)
            {
                myRank = myRankItem.rank.ToIntSafe();
            }

            var rewardListObj = gameObject.FindGameObject("RewardList");

            rewardListObj.AllChildObjectOff();

            var rewardDatas = tab == RankingTab.AdvancedRaid ? Lance.GameData.AdvancedRankingRewardData : Lance.GameData.BeginnerRankingRewardData;

            foreach (var rewardData in rewardDatas)
            {
                var rewardItemObj = Util.InstantiateUI("RankingRewardItemUI", rewardListObj.transform);

                var rewardItemUI = rewardItemObj.GetOrAddComponent<RankingRewardItemUI>();

                rewardItemUI.Init(rewardData.rankMin, rewardData.rankMax, rewardData.reward, myRank);
            }
        }
    }

    class RankingRewardItemUI : MonoBehaviour
    {
        public void Init(int rankMin, int rankMax, string reward, int myRank)
        {
            var textRankRange = gameObject.FindComponent<TextMeshProUGUI>("Text_RankRange");

            string rankText = string.Empty;
            bool isMyReward = false;
            if (rankMin > 0 && rankMax > 0)
            {
                StringParam param = new StringParam("rankMin", rankMin);
                param.AddParam("rankMax", rankMax);

                rankText = StringTableUtil.Get("UIString_RankRange", param);

                isMyReward = rankMin > myRank && rankMax <= myRank;
            }
            else if (rankMax > 0)
            {
                StringParam param = new StringParam("rank", rankMax);

                rankText = StringTableUtil.Get("UIString_Rank", param);

                isMyReward = rankMax == myRank;
            }
            else
            {
                rankText = StringTableUtil.Get("UIString_RankParticipation");

                isMyReward = true;
            }

            textRankRange.text = rankText;

            var textRewardAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_RewardAmount");

            var rewardData = Lance.GameData.RewardData.TryGet(reward);

            // To Do : 지금은 무조건 잼 보상만 있어서 간단하게 이렇게 해두자
            // 다른 보상주면 당연히 바뀌어야겠지?
            textRewardAmount.text = $"{rewardData.gem}";

            var imageMyReward = gameObject.FindComponent<Image>("Image_MyReward");

            imageMyReward.gameObject.SetActive(isMyReward);
        }
    }
}