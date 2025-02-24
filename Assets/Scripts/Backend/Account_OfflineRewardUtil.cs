using System;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public RewardResult GetOfflineReward(int offlineTime)
        {
            RewardResult reward = new RewardResult();
            // 현재 선택되어있는 스테이지를 기준으로 오프라인 보상을 결정하자.
            StageDifficulty diff = StageRecords.GetCurDifficulty();
            int chapter = StageRecords.GetCurChapter();
            int stage = StageRecords.GetCurStage();

            StageData stageData = DataUtil.GetStageData(diff, chapter, stage);
            if (stageData != null)
            {
                if (stageData.monsterDropReward.IsValid())
                {
                    MonsterRewardData rewardData = DataUtil.GetMonsterRewardData(stageData.type, stageData.monsterDropReward);
                    if (rewardData != null)
                    {
                        float monsterKillCalcMinute = Lance.GameData.OfflineRewardCommonData.monsterKillCalcMinute;
                        int offlineTimeForMinute = Mathf.RoundToInt(offlineTime / TimeUtil.SecondsInMinute);
                        int monsterKillCount = Mathf.RoundToInt(offlineTimeForMinute * monsterKillCalcMinute);

                        monsterKillCount = Math.Max(1, monsterKillCount);
                        for (int i = 0; i < monsterKillCount; ++i)
                        {
                            var monsterReward = DataUtil.GetMonsterReward(diff, stageData.type, stageData.chapter, stageData.stage, rewardData);

                            reward = reward.AddReward(monsterReward);
                        }

                        reward.exp = reward.exp * (1 + Lance.Account.GatherStat(StatType.ExpAmount));
                        reward.gold = reward.gold * (1 + Lance.Account.GatherStat(StatType.GoldAmount));

                        if (reward.equipments != null)
                            reward.equipments = reward.equipments.GatherReward();

                        if (reward.accessorys != null)
                            reward.accessorys = reward.accessorys.GatherReward();

                        if (reward.skills != null)
                            reward.skills = reward.skills.GatherReward();

                        if (reward.artifacts != null)
                            reward.artifacts = reward.artifacts.GatherReward();

                        if (reward.eventCurrencys != null)
                            reward.eventCurrencys = reward.eventCurrencys.GatherReward();
                    }
                }
            }

            return reward;
        }
    }
}