using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Lance
{
    class AchieveEventQuestUI : MonoBehaviour
    {
        string mEventId;
        Slider mSliderProgress;
        List<AchieveEventQuestSlotUI> mQuestSlotUIList;
        public void Init(string eventId)
        {
            mEventId = eventId;
            mQuestSlotUIList = new List<AchieveEventQuestSlotUI>();
            mSliderProgress = gameObject.FindComponent<Slider>("Slider_Achieve");

            int index = 0;
            foreach (QuestData data in DataUtil.GetEventQuestDatas(eventId).Where(x => x.type.IsQuestClearType()))
            {
                var achieveQuestSlotObj = gameObject.FindGameObject($"AchieveQuestSlot_{index + 1}");

                AchieveEventQuestSlotUI achieveQuestSlotUI = achieveQuestSlotObj.GetOrAddComponent<AchieveEventQuestSlotUI>();

                achieveQuestSlotUI.Init(eventId, data.id);

                mQuestSlotUIList.Add(achieveQuestSlotUI);

                index++;
            }

            Refresh();
        }

        public void Refresh()
        {
            RefreshProgress();
            RefreshSlots();
        }

        public void RefreshProgress()
        {
            float curClearCount = Lance.Account.Event.GetQuestClearCount(mEventId);
            float maxClearCount = DataUtil.GetEventQuestDatas(mEventId).Where(x => x.type == QuestType.EventquestClear).Max(x => x.requireCount);

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
            foreach (var slot in mQuestSlotUIList)
            {
                bool prevReceived = slot.IsReceived;

                slot.Refresh();

                bool curReceived = slot.IsReceived;

                if (prevReceived != curReceived)
                    slot.PlayReceiveMotion();
            }
        }
    }

    class AchieveEventQuestSlotUI : MonoBehaviour
    {
        string mEventId;
        string mId;
        bool mIsReceived;
        Image mImageFrame;
        TextMeshProUGUI mTextAchieveNum;
        Image mImageModal;
        Image mImageCheck;
        GameObject mRedDotObj;
        public bool IsReceived => mIsReceived;
        public void Init(string eventId, string id)
        {
            mEventId = eventId;
            mId = id;

            QuestData questData = Lance.GameData.EventQuestData.TryGet(id);
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
        }

        void OnReceiveButton()
        {
            if (Lance.GameManager.ReceiveNewEventQuestReward(mEventId, mId))
            {
                var popup = Lance.PopupManager.GetPopup<Popup_NewEventUI>();

                popup?.Refresh();

                popup?.RefreshRedDots();

                UIUtil.PlayStampMotion(mImageModal, mImageCheck);
            }
        }

        public void Refresh()
        {
            QuestInfo questInfo = Lance.Account.Event.GetQuestInfo(mEventId, mId);
            if (questInfo != null)
            {
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
        }

        public void PlayReceiveMotion()
        {
            Refresh();

            UIUtil.PlayStampMotion(mImageModal, mImageCheck);
        }
    }
}