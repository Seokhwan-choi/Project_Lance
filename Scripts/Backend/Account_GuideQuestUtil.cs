using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public bool CanReceiveGuideQuestReward()
        {
            return GuideQuest.CanReceiveReward();
        }

        public RewardResult ReceiveGuideQuestReward()
        {
            RewardResult reward = new RewardResult();

            (string rewardId, int count) result = GuideQuest.ReceiveReward();
            if (result.rewardId.IsValid())
            {
                var rewardData = Lance.GameData.RewardData.TryGet(result.rewardId);
                if (rewardData != null)
                {
                    for (int i = 0; i < result.count; ++i)
                    {
                        reward = reward.AddReward(rewardData);
                    }
                }
            }

            GiveReward(reward);

            return reward;
        }
    }
}