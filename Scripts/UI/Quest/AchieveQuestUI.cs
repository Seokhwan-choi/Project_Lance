using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

namespace Lance
{
    class AchieveQuestUI : MonoBehaviour
    {
        QuestUpdateType mUpdateType;
        Slider mSliderProgress;
        List<AchieveQuestSlotUI> mQuestSlotUIList;
        public void Init(QuestUpdateType updateType, Action onReceiveAction)
        {
            mUpdateType = updateType;
            mQuestSlotUIList = new List<AchieveQuestSlotUI>();
            mSliderProgress = gameObject.FindComponent<Slider>("Slider_Achieve");
            
            int index = 0;
            foreach(QuestData data in GetQuestDatas())
            {
                var achieveQuestSlotObj = gameObject.FindGameObject($"AchieveQuestSlot_{index + 1}");

                AchieveQuestSlotUI achieveQuestSlotUI = achieveQuestSlotObj.GetOrAddComponent<AchieveQuestSlotUI>();

                achieveQuestSlotUI.Init(data.id, onReceiveAction);

                mQuestSlotUIList.Add(achieveQuestSlotUI);

                index++;
            }

            Refresh();
        }

        IEnumerable<QuestData> GetQuestDatas()
        {
            QuestType questType = GetQuestType();

            return DataUtil.GetQuestDatas(mUpdateType, questType).OrderBy(x => x.requireCount);

            QuestType GetQuestType()
            {
                if (mUpdateType == QuestUpdateType.Daily)
                    return QuestType.DailyquestClear;
                else
                    return QuestType.WeeklyquestClear;
            }
        }

        public void Refresh()
        {
            RefreshProgress();
            RefreshSlots();
        }

        public void RefreshProgress()
        {
            float curClearCount = Lance.Account.GetQuestClearCount(mUpdateType);
            float maxClearCount = GetQuestDatas().Max(x => x.requireCount);

            mSliderProgress.value = curClearCount / maxClearCount;
        }

        public void RefreshSlots()
        {
            foreach (var slot in mQuestSlotUIList)
            {
                slot.Refresh();
            }
        }

        public void PlayAllReceiveMotion()
        {
            foreach(var slot in mQuestSlotUIList)
            {
                bool prevReceived = slot.IsReceived;

                slot.Refresh();

                bool curReceived = slot.IsReceived;

                if (prevReceived != curReceived)
                    slot.PlayReceiveMotion();
            }
        }
    }

    class AchieveQuestSlotUI : MonoBehaviour
    {
        string mId;
        bool mIsReceived;
        Image mImageFrame;
        TextMeshProUGUI mTextAchieveNum;
        Image mImageModal;
        Image mImageCheck;
        GameObject mRedDotObj; 
        Action mOnReceiveAction;
        public bool IsReceived => mIsReceived;
        public void Init(string id, Action onReceiveAction)
        {
            mId = id;
            mOnReceiveAction = onReceiveAction;
            QuestData questData = DataUtil.GetQuestData(id);
            if (questData != null)
            {
                var rewardData = Lance.GameData.RewardData.TryGet(questData.rewardId);
                if (rewardData != null)
                {
                    var itemSlotUIObj = gameObject.FindGameObject("QuestSlotUI");
                    var itemSlotUI = itemSlotUIObj.GetOrAddComponent<ItemSlotUI>();
                    itemSlotUI.Init(new ItemInfo(rewardData));
                }
            }

            var buttonReceive = gameObject.FindComponent<Button>("AchieveQuestSlotUI");
            buttonReceive.SetButtonAction(OnReceiveButton);

            mRedDotObj = gameObject.FindGameObject("RedDot");
            mImageFrame = gameObject.FindComponent<Image>("AchieveNum");
            mTextAchieveNum = gameObject.FindComponent<TextMeshProUGUI>("Text_AchieveNum");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
            mImageCheck = gameObject.FindComponent<Image>("Image_Check");
            mOnReceiveAction = onReceiveAction;
        }

        void OnReceiveButton()
        {
            Lance.GameManager.ReceiveQuestReward(mId, () =>
            {
                PlayReceiveMotion();

                mOnReceiveAction?.Invoke();
            });
        }

        public void Refresh()
        {
            QuestInfo questInfo = Lance.Account.GetQuestInfo(mId);

            bool isSatisfied = questInfo.IsSatisfied();
            mIsReceived = questInfo.GetIsReceived();
            bool canReceive = questInfo.CanReceiveReward();

            string prefix = isSatisfied ? "Active" : "Inactive";
            string color = isSatisfied ? "2A2220" : "E9CFAA";

            mImageFrame.sprite = Lance.Atlas.GetUISprite($"Scrollbar_Frame_Quest_Number_{prefix}");
            mTextAchieveNum.SetColor(color);
            mTextAchieveNum.text = $"{questInfo.GetMaxRequireCount()}";
            mRedDotObj.SetActive(canReceive);

            mImageModal.gameObject.SetActive(mIsReceived || canReceive == false);
            mImageCheck.gameObject.SetActive(mIsReceived);
        }

        public void PlayReceiveMotion()
        {
            Refresh();

            UIUtil.PlayStampMotion(mImageModal, mImageCheck);
        }
    }
}