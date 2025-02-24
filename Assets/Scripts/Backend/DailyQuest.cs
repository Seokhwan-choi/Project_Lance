using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;

namespace Lance
{
    public class DailyQuest : Quest
    {
        public DailyQuest()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();
        }

        public override string GetTableName()
        {
            return "DailyQuest";
        }

        public override void Update()
        {
            if (mLastUpdateDateNum < TimeUtil.NowDateNum() || mLastUpdateDateNum >= 20680101)
            {
                CreateNewQuests();
            }
        }

        public void CreateNew()
        {
            CreateNewQuests();
        }

        protected override void CreateNewQuests()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();

            // 일일 퀘스트 완료 퀘스트는 일단 목록에서 제외해준다.
            foreach (QuestData data in Lance.GameData.DailyQuestData.Values.Where(x => x.type != QuestType.DailyquestClear))
            {
                // 이미 추가된적이 있는 퀘스트 타입이라면 무시하자
                IEnumerable<QuestInfo> infos = GetQuestInfos(data.type);
                if (infos != null && infos.Count() > 0)
                    continue;

                AddQuestInfo(data);

                // 최대갯수까지 퀘스트를 모두 채워 넣었으면
                if (mQuestInfos.Count >= Lance.GameData.QuestCommonData.dailyQuestMaxCount)
                {
                    // 퀘스트 모두 완료 퀘스트를 넣어주자
                    foreach(QuestData clearData in DataUtil.GetQuestDatas(QuestUpdateType.Daily, QuestType.DailyquestClear))
                    {
                        AddQuestInfo(clearData);
                    }

                    break;
                }
            }

            mQuestClearCount = 0;

            mLastUpdateDateNum = TimeUtil.NowDateNum();

            SetIsChangedData(true);
        }
    }
}


