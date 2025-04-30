using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mosframe;
using DG.Tweening;
using DG;


namespace Lance
{
    class NewEventQuestItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        bool mInit;
        QuestData mData;
        Slider mSliderProgress;
        TextMeshProUGUI mTextProgress;
        NewEventTabUI mParent;
        Button mButtonReceive;
        GameObject mRedDotObj;
        GameObject mReceivedObj;
        List<ItemSlotUI> mRewardSlotList;
        public string Id => mData?.id ?? string.Empty;
        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<NewEventTabUI>(true);
            
            // 퀘스트 진행도
            mSliderProgress = gameObject.FindComponent<Slider>("Slider_Progress");
            mTextProgress = gameObject.FindComponent<TextMeshProUGUI>("Text_Progress");

            mReceivedObj = gameObject.FindGameObject("Received");
            mRedDotObj = gameObject.FindGameObject("RedDot");

            // 퀘스트 보상 버튼
            mButtonReceive = gameObject.FindComponent<Button>("Button_Receive");

            // 퀘스트 보상 목록
            mRewardSlotList = new List<ItemSlotUI>();
            var rewardList = gameObject.FindGameObject("RewardList");
            for(int i = 0; i < 5; ++i)
            {
                var rewardSlotObj = rewardList.FindGameObject($"RewardSlot_{i + 1}");

                var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();
                rewardSlotUI.Init();

                rewardSlotUI.gameObject.SetActive(false);

                mRewardSlotList.Add(rewardSlotUI);
            }
        }

        public void SetData(QuestData data)
        {
            mData = data;

            Refresh();
        }

        public void Refresh()
        {
            if (mData != null)
            {
                // 퀘스트 이름
                StringParam param = new StringParam("openDay", mData.openDay);

                if (mData.type == QuestType.LevelUpCentralEssence)
                {
                    param.AddParam("count", mData.requireCount + 1);
                }
                else
                {
                    if (mData.type == QuestType.ClearStage)
                    {
                        (StageDifficulty diff, int chapter, int stage) result = StageRecordsUtil.SplitTotalStage(mData.requireCount);

                        string stage = StageRecordsUtil.ChangeStageInfoToString(result, true);

                        param.AddParam("stage", stage);
                    }

                    param.AddParam("count", mData.requireCount);
                }

                TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
                textName.text = StringTableUtil.Get($"QuestName_{mData.type}", param);

                var rewardData = Lance.GameData.RewardData.TryGet(mData.rewardId);
                if (rewardData != null)
                {
                    RewardResult reward = new RewardResult();

                    reward = reward.AddReward(rewardData);

                    List<ItemInfo> itemInfos = reward.Split();

                    for (int i = 0; i < mRewardSlotList.Count; ++i)
                    {
                        var rewardSlotUI = mRewardSlotList[i];

                        if (i < itemInfos.Count)
                        {
                            rewardSlotUI.gameObject.SetActive(true);
                            rewardSlotUI.Refresh(itemInfos[i]);
                        }
                        else
                        {
                            rewardSlotUI.gameObject.SetActive(false);
                        }
                    }
                }

                QuestInfo questInfo = Lance.Account.Event.GetQuestInfo(mData.eventId, mData.id);

                int curCount = questInfo.GetStackedCount();
                int maxCount = questInfo.GetMaxRequireCount();

                mSliderProgress.value = (float)curCount / (float)maxCount;
                mTextProgress.text = $"{UIUtil.GetColorString("52FF00", curCount)}/{UIUtil.GetColorString("FFFFFF", maxCount)}";

                bool isReceive = questInfo.GetIsReceived();
                bool canReceive = questInfo.CanReceiveReward();

                // 보상 받았을 때 가리기
                mReceivedObj.SetActive(isReceive);

                // 버튼
                mButtonReceive.SetActiveFrame(canReceive);

                // 레드닷
                mRedDotObj.SetActive(canReceive);
            }
        }

        public void OnUpdateItem(int index)
        {
            SetData(mParent.GetQuestData(index));
        }

        public void PlayReceiveMotion()
        {
            var anim = gameObject.FindComponent<Animation>("QuestItemUI");

            anim.Play();
        }

        public void OnReceiveButton()
        {
            if (mData != null)
            {
                if ( Lance.GameManager.ReceiveNewEventQuestReward(mData.eventId, mData.id) )
                {
                    string questId = mData.id;

                    var popup = Lance.PopupManager.GetPopup<Popup_NewEventUI>();
                    popup?.Refresh();
                    popup?.RefreshRedDots();

                    mParent.PlayQuestItemReceiveMotion(questId);

                    SoundPlayer.PlayUIButtonTouchSound();
                }
            }
        }
    }
}