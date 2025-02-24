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

            // ���� ����Ʈ �Ϸ� ����Ʈ�� �ϴ� ��Ͽ��� �������ش�.
            foreach (QuestData data in Lance.GameData.DailyQuestData.Values.Where(x => x.type != QuestType.DailyquestClear))
            {
                // �̹� �߰������� �ִ� ����Ʈ Ÿ���̶�� ��������
                IEnumerable<QuestInfo> infos = GetQuestInfos(data.type);
                if (infos != null && infos.Count() > 0)
                    continue;

                AddQuestInfo(data);

                // �ִ밹������ ����Ʈ�� ��� ä�� �־�����
                if (mQuestInfos.Count >= Lance.GameData.QuestCommonData.dailyQuestMaxCount)
                {
                    // ����Ʈ ��� �Ϸ� ����Ʈ�� �־�����
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


