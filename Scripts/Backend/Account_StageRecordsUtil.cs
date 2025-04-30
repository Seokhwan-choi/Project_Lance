using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    public partial class Account
    {
        public RewardResult GetSkipBattleRewards()
        {
            RewardResult reward = new RewardResult();

            // 현재 스테이지를 기준으로 빠른 전투 보상을 결정
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
                        int monsterKillCalcMinute = DataUtil.GetSkipBattleMonsterKillCount(StageRecords.GetBestTotalStage());
                        int timeForMinute = Lance.GameData.SkipBattleCommonData.maxTimeForMinute;
                        int monsterKillCount = monsterKillCalcMinute * timeForMinute;

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
