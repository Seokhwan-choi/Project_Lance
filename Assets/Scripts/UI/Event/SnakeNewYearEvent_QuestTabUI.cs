using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Lance
{
    class SnakeNewYearEvent_QuestTabUI : SnakeNewYearTabUI
    {
        Button mButtonAllReceive;
        GameObject mAllReceiveRedDotObj;

        AchieveEventQuestUI mAchieveQuestUI;
        List<NewEventQuestItemUI> mQuestItemList;
        public override void Init(SnakeNewYearTabUIManager parent, string eventId, SnakeNewYearTab tab)
        {
            base.Init(parent, eventId, tab);

            var textUpdateTime = gameObject.FindComponent<TextMeshProUGUI>("Text_UpdateTime");
            textUpdateTime.text = StringTableUtil.GetDesc("DailyQuest");

            mButtonAllReceive = gameObject.FindComponent<Button>("Button_AllReceive");
            mButtonAllReceive.SetButtonAction(ReceiveAllReward);

            mAllReceiveRedDotObj = mButtonAllReceive.gameObject.FindGameObject("RedDot");

            var achieveQuestUIObj = gameObject.FindGameObject("AchieveQuest");

            mAchieveQuestUI = achieveQuestUIObj.GetOrAddComponent<AchieveEventQuestUI>();
            mAchieveQuestUI.Init(eventId);

            mQuestItemList = new List<NewEventQuestItemUI>();

            var questItemListObj = gameObject.FindGameObject("QuestItemList");
            questItemListObj.AllChildObjectOff();

            var questInfos = Lance.Account.Event.GetQuestInfos(mEventId);
            if (questInfos != null && questInfos.Count() > 0)
            {
                foreach (QuestInfo questInfo in questInfos)
                {
                    if (questInfo.GetQuestType().IsQuestClearType())
                        continue;

                    var questItemUIObj = Util.InstantiateUI("EventQuestItemUI", questItemListObj.transform);

                    NewEventQuestItemUI questItemUI = questItemUIObj.GetOrAddComponent<NewEventQuestItemUI>();

                    questItemUI.Init();
                    questItemUI.SetData(DataUtil.GetQuestData(questInfo.GetId()));

                    mQuestItemList.Add(questItemUI);
                }
            }
        }

        public override void RefreshRedDots()
        {
            mAllReceiveRedDotObj.SetActive(Lance.Account.Event.AnyCanReceiveQuestReward(mEventId));
        }

        public override void OnEnter()
        {
            Refresh();
        }
        public override void Refresh()
        {
            mAchieveQuestUI?.Refresh();

            foreach (var questItemUI in mQuestItemList)
            {
                questItemUI.Refresh();
            }

            mButtonAllReceive.SetActiveFrame(Lance.Account.Event.AnyCanReceiveQuestReward(mEventId));
        }

        void ReceiveAllReward()
        {
            var result = Lance.GameManager.ReceiveAllNewEventQuestReward(mEventId);
            if (result.totalReward.IsEmpty() == false)
            {
                if (mAchieveQuestUI != null)
                {
                    mAchieveQuestUI.RefreshProgress();
                    mAchieveQuestUI.PlayAllReceiveMotion();
                }

                var popup = Lance.PopupManager.GetPopup<Popup_NewEventUI>();
                popup?.Refresh();
                popup?.RefreshRedDots();

                foreach (var item in result.Item1)
                {
                    var questItem = GetQuestItemUI(item.questId);
                    if (questItem != null)
                    {
                        questItem.PlayReceiveMotion();
                    }
                }
            }
        }

        NewEventQuestItemUI GetQuestItemUI(string id)
        {
            return mQuestItemList.Where(x => x.Id == id).FirstOrDefault();
        }
    }
}