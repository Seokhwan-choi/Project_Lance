using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Lance
{
    class Quest_DailyTabUI : QuestTabUI
    {
        public override void Init(QuestTabUIManager parent, QuestTab tab)
        {
            base.Init(parent, tab);

            var textUpdateTime = gameObject.FindComponent<TextMeshProUGUI>("Text_UpdateTime");
            textUpdateTime.text = StringTableUtil.GetDesc("DailyQuest");

            InitAchieveQuestUI(QuestUpdateType.Daily);
            InitQuestItemUIs(QuestUpdateType.Daily);
        }
    }
}