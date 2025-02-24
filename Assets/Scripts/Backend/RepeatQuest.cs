using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;

namespace Lance
{
    public class RepeatQuest : Quest
    {
        public RepeatQuest()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();
        }

        public override void Update()
        {
            // 반복 퀘스트는 한번 생기면 끝이다.
            if (mQuestInfos.Count < 0)
            {
                CreateNewQuests();
            }
            else
            {
                // 반복퀘스트를 추가하고 싶다면 새로운 구현이 필요하다
            }
        }

        protected override void CreateNewQuests()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();

            foreach (QuestData data in DataUtil.GetQuestDatas(QuestUpdateType.Repeat))
            {
                // 이미 추가된적이 있는 퀘스트 타입이라면 무시하자
                IEnumerable<QuestInfo> infos = GetQuestInfos(data.type);
                if (infos != null && infos.Count() > 0)
                    continue;

                AddQuestInfo(data);
            }

            SetIsChangedData(true);
        }

        public override string GetTableName()
        {
            return "RepeatQuest";
        }
    }
}