using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        // 이벤트 퀘스트 확인
        public RewardResult ReceiveNewEventQuestReward(string eventId, string id)
        {
            RewardResult reward = new RewardResult();

            (string rewardId, int count) result = Lance.Account.Event.ReceiveQuestReward(eventId, id);
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

        public (List<(string questId, string rewardId, int count)>, RewardResult totalReward, int totalCount) ReceiveAllNewEventQuestReward(string eventId)
        {
            int totalRewardCount = 0;
            RewardResult totalReward = new RewardResult();

            List<(string questId, string rewardId, int count)> results = Lance.Account.Event.ReceiveAllQuestReward(eventId);
            if (results.Count > 0)
            {
                totalRewardCount += results.Count;
                foreach (var result in results)
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(result.rewardId);
                    if (rewardData != null)
                    {
                        for (int i = 0; i < result.count; ++i)
                        {
                            totalReward = totalReward.AddReward(rewardData);
                        }
                    }
                }

                GiveReward(totalReward);
            }

            return (results, totalReward, totalRewardCount);
        }

        public bool AnyCanReceiveNewEventReward(EventType eventType)
        {
            return Lance.Account.Event.AnyCanReceiveReward(eventType);
        }

        public bool AnyCanReceiveNewEventReward()
        {
            return Lance.Account.Event.AnyCanReceiveReward();
        }
    }
}