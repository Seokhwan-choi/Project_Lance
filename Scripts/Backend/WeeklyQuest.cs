using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using BackEnd;
using LitJson;

namespace Lance
{
    public class WeeklyQuest : Quest
    {
        public WeeklyQuest()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();
        }
        public override string GetTableName()
        {
            return "WeeklyQuest";
        }

        //protected override void SetServerDataToLocal(JsonData gameDataJson)
        //{
        //    base.SetServerDataToLocal(gameDataJson);

        //    CreateNewQuests();
        //}

        public override void Update()
        {
            // 주간 퀘스트
            int thisWeekStartDateNum = TimeUtil.ThisWeekStartDateNum();

            // 이번주 월요일을 기준으로 주 시작날짜와 비교
            if (mLastUpdateDateNum < TimeUtil.ThisWeekStartDateNum() || mLastUpdateDateNum >= 20680101)
            {
                CreateNewQuests();
            }
        }

        public void CreateNew()
        {

        }

        protected override void CreateNewQuests()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();

            // 주간 퀘스트 완료 퀘스트는 일단 목록에서 제외해준다.
            foreach (QuestData data in Lance.GameData.WeeklyQuestData.Values.Where(x => x.type != QuestType.WeeklyquestClear))
            {
                // 이미 추가된적이 있는 퀘스트 타입이라면 무시하자
                IEnumerable<QuestInfo> infos = GetQuestInfos(data.type);
                if (infos != null && infos.Count() > 0)
                    continue;

                AddQuestInfo(data);

                // 최대갯수까지 퀘스트를 모두 채워 넣었으면
                if (mQuestInfos.Count >= Lance.GameData.QuestCommonData.weeklyQuestMaxCount)
                {
                    // 퀘스트 모두 완료 퀘스트를 넣어주자
                    foreach (QuestData clearData in DataUtil.GetQuestDatas(QuestUpdateType.Weekly, QuestType.WeeklyquestClear))
                    {
                        AddQuestInfo(clearData);
                    }

                    break;
                }
            }

            mQuestClearCount = 0;

            mLastUpdateDateNum = TimeUtil.ThisWeekStartDateNum();

            SetIsChangedData(true);
        }
    }
}