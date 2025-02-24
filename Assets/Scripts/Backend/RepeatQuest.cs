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
            // �ݺ� ����Ʈ�� �ѹ� ����� ���̴�.
            if (mQuestInfos.Count < 0)
            {
                CreateNewQuests();
            }
            else
            {
                // �ݺ�����Ʈ�� �߰��ϰ� �ʹٸ� ���ο� ������ �ʿ��ϴ�
            }
        }

        protected override void CreateNewQuests()
        {
            mQuestInfos = new Dictionary<string, QuestInfo>();

            foreach (QuestData data in DataUtil.GetQuestDatas(QuestUpdateType.Repeat))
            {
                // �̹� �߰������� �ִ� ����Ʈ Ÿ���̶�� ��������
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