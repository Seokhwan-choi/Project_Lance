using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Quest_RepeatTabUI : QuestTabUI
    {
        public override void Init(QuestTabUIManager parent, QuestTab tab)
        {
            base.Init(parent, tab);

            InitQuestItemUIs(QuestUpdateType.Repeat);
        }
    }
}