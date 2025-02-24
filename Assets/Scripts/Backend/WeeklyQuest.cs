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
            // �ְ� ����Ʈ
            int thisWeekStartDateNum = TimeUtil.ThisWeekStartDateNum();

            // �̹��� �������� �������� �� ���۳�¥�� ��
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

            // �ְ� ����Ʈ �Ϸ� ����Ʈ�� �ϴ� ��Ͽ��� �������ش�.
            foreach (QuestData data in Lance.GameData.WeeklyQuestData.Values.Where(x => x.type != QuestType.WeeklyquestClear))
            {
                // �̹� �߰������� �ִ� ����Ʈ Ÿ���̶�� ��������
                IEnumerable<QuestInfo> infos = GetQuestInfos(data.type);
                if (infos != null && infos.Count() > 0)
                    continue;

                AddQuestInfo(data);

                // �ִ밹������ ����Ʈ�� ��� ä�� �־�����
                if (mQuestInfos.Count >= Lance.GameData.QuestCommonData.weeklyQuestMaxCount)
                {
                    // ����Ʈ ��� �Ϸ� ����Ʈ�� �־�����
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