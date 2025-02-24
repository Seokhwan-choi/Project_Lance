using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    public class DailyOpenEventQuest : NewEventQuest
    {
        public override (string rewardId, int count) ReceiveReward(string id)
        {
            QuestInfo questInfo = GetQuestInfo(id);
            if (questInfo == null)
                return (string.Empty, 0);

            int openDay = questInfo.GetOpenDay();

            if (openDay > CalcPassDay())
                return (string.Empty, 0);

            if (questInfo.CanReceiveReward() == false)
                return (string.Empty, 0);

            var result = questInfo.ReceiveReward();
            if (result.rewardId.IsValid())
            {
                if (questInfo.GetQuestType().IsQuestClearType() == false)
                    mQuestClearCount += 1;

                int clearCount = GetClearCountByOpenDay(openDay);

                foreach (var info in GetQuestInfoByType(QuestType.EventquestClearByOpenDay))
                {
                    if (info.GetOpenDay() == openDay)
                    {
                        info.AttainRequireCount(clearCount);
                    }
                }

                return result;
            }

            return (string.Empty, 0);
        }

        public override bool AnyCanReceiveReward()
        {
            foreach(var questInfo in GetQuestInfos())
            {
                if (questInfo.GetOpenDay() > CalcPassDay())
                    continue;

                if (questInfo.CanReceiveReward() == false)
                    continue;

                return true;
            }

            return false;
        }

        public int GetClearCountByOpenDay(int openDay)
        {
            int count = 0;

            foreach(var questInfo in GetQuestInfos())
            {
                if (questInfo.GetQuestType().IsQuestClearType())
                    continue;

                if (questInfo.GetOpenDay() != openDay)
                    continue;

                if (questInfo.GetIsReceived() == false)
                    continue;

                count++;
            }

            return count;
        }
    }
}