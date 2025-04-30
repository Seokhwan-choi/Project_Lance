using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Lobby_GuideQuestUI : MonoBehaviour
    {
        GameObject mActiveObj;
        ItemSlotUI mRewardSlotUI;
        TextMeshProUGUI mTextGuideStep;
        TextMeshProUGUI mTextQuestDesc;
        public void Init()
        {
            var contentsObj = gameObject.FindGameObject("Contents");
            mActiveObj = gameObject.FindGameObject("Active");

            var button = contentsObj.GetComponent<Button>();
            button.SetButtonAction(OnButtonAction);

            mTextGuideStep = gameObject.FindComponent<TextMeshProUGUI>("Text_GuideStep");
            mTextQuestDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_QuestDesc");

            var rewardSlotObj = gameObject.FindGameObject("RewardSlotUI");
            mRewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();
            mRewardSlotUI.Init();

            Refresh();
        }

        void OnButtonAction()
        {
            QuestInfo questInfo = Lance.Account.GuideQuest.GetCurrentQuest();
            if (questInfo != null)
            {
                if (questInfo.CanReceiveReward())
                {
                    Lance.GameManager.ReceiveGuideQuestReward(Refresh);
                }
                else
                {
                    // 가이드 액션 시작
                    Lance.Lobby.StartGuideAction(isAuto:false);
                }
            }
        }

        public void Localize()
        {
            Refresh();
        }

        public void Refresh()
        {
            QuestInfo questInfo = Lance.Account.GuideQuest.GetCurrentQuest();
            if (questInfo != null && questInfo.GetIsReceived() == false)
            {
                if (gameObject.activeSelf == false)
                    SetActive(Lance.GameManager.StageManager.StageData.type.IsNormal() && true);

                StringParam param = null;

                var questType = questInfo.GetQuestType();

                if (questType == QuestType.ClearGoldDungeon ||
                    questType == QuestType.ClearStoneDungeon ||
                    questType == QuestType.ClearPetDungeon ||
                    questType == QuestType.ClearReforgeDungeon ||
                    questType == QuestType.ClearGrowthDungeon ||
                    questType == QuestType.ClearAncientDungeon)
                {
                    param = new StringParam("step", questInfo.GetMaxRequireCount());
                }
                else if (questType == QuestType.ClearStage)
                {
                    (StageDifficulty diff, int chapter, int stage) result = StageRecordsUtil.SplitTotalStage(questInfo.GetMaxRequireCount());

                    string stage = StageRecordsUtil.ChangeStageInfoToString(result, true);

                    param = new StringParam("stage", stage);
                }
                else if (questType == QuestType.LimitBreak)
                {
                    param = new StringParam("limitBreak", questInfo.GetMaxRequireCount());
                }

                // 가이드 단계
                StringParam stepParam = new StringParam("step", Lance.Account.GuideQuest.GetCurrentStep());

                mTextGuideStep.text = $"[ {StringTableUtil.Get("UIString_GuideStep", stepParam)} ]";

                // 퀘스트 설명
                string desc = StringTableUtil.Get($"GuideQuestName_{questInfo.GetQuestType()}", param);

                string stackedCount = UIUtil.GetColorString("36CD57", questInfo.GetStackedCount());

                string count = $"({stackedCount}/{questInfo.GetMaxRequireCount()})";

                mTextQuestDesc.text = $"{desc} {count}";

                // 보상 관련 초기화
                var rewardId = Lance.Account.GuideQuest.GetCurrentReward();
                if (rewardId.IsValid())
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(rewardId);
                    if (rewardData != null)
                    {
                        mRewardSlotUI.Refresh(new ItemInfo(rewardData));
                    }
                }
            }
            else
            {
                SetActive(false);
            }

            RefreshActive();
        }

        public void RefreshActive()
        {
            QuestInfo questInfo = Lance.Account.GuideQuest.GetCurrentQuest();
            bool canReceiveReward = questInfo?.CanReceiveReward() ?? false;
            if (mActiveObj.activeSelf != canReceiveReward)
                mActiveObj.SetActive(canReceiveReward);
        }

        public void OnStartStage(StageData stageData)
        {
            QuestInfo questInfo = Lance.Account.GuideQuest.GetCurrentQuest();

            SetActive(questInfo != null && questInfo.GetIsReceived() == false && stageData.type.IsNormal());
        }

        void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}