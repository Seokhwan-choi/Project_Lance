using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class QuestItemUI : MonoBehaviour
    {
        Action mOnReceiveReward;
        string mId;
        bool mIsReceived;
        bool mIsReceivedBonus;
        Slider mSliderProgress;
        TextMeshProUGUI mTextProgress;

        Button mButtonReceive;
        Button mButtonBonusReceive;
        GameObject mRedDotObj;
        GameObject mReceivedObj;
        Image mImageModal;
        Image mImageCheck;
        ItemSlotUI mRewardSlotUI;
        public bool IsReceived => mIsReceived;
        public void Init(string id, Action onReceiveReward)
        {
            mId = id;
            mOnReceiveReward = onReceiveReward;

            var questInfo = Lance.Account.GetQuestInfo(id);

            // ����Ʈ �̸�
            StringParam param = new StringParam("count", $"{questInfo.GetMaxRequireCount()}");
            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.Get($"QuestName_{questInfo.GetQuestType()}", param);

            // ����Ʈ ���൵
            mSliderProgress = gameObject.FindComponent<Slider>("Slider_Progress");
            mTextProgress = gameObject.FindComponent<TextMeshProUGUI>("Text_Progress");

            // ����Ʈ ���� ��ư
            mButtonReceive = gameObject.FindComponent<Button>("Button_Receive");
            mButtonReceive.SetButtonAction(OnReceiveButton);

            // ����Ʈ ���ʽ� ���� ��ư
            mButtonBonusReceive = gameObject.FindComponent<Button>("Button_BonusReceive");
            mButtonBonusReceive.SetButtonAction(OnReceiveBuonusButton);

            mRedDotObj = gameObject.FindGameObject("RedDot");
            mReceivedObj = gameObject.FindGameObject("Received");
            mImageModal = mReceivedObj.FindComponent<Image>("Image_Modal");
            mImageCheck = mReceivedObj.FindComponent<Image>("Image_Check");

            Refresh();
        }

        void OnReceiveButton()
        {
            Lance.GameManager.ReceiveQuestReward(mId, OnFinish);

            void OnFinish()
            {
                PlayReceiveMotion();

                mOnReceiveReward?.Invoke();
            }
        }

        void OnReceiveBuonusButton()
        {
            Lance.GameManager.ReceiveQuestBonusReward(mId, OnFinish);

            void OnFinish()
            {
                PlayReceiveMotion();

                mOnReceiveReward?.Invoke();
            }
        }

        public void Refresh()
        {
            QuestInfo questInfo = Lance.Account.GetQuestInfo(mId);

            int curCount = questInfo.GetStackedCount();
            int maxCount = questInfo.GetMaxRequireCount();

            mSliderProgress.value = (float)curCount / (float)maxCount;
            mTextProgress.text = $"{UIUtil.GetColorString("52FF00", curCount)}/{UIUtil.GetColorString("FFFFFF",maxCount)}";

            mIsReceived = questInfo.GetIsReceived();
            mIsReceivedBonus = questInfo.GetIsReceivedBonus();
            bool haveBonus = questInfo.GetBonusReward().IsValid();
            bool canReceive = questInfo.CanReceiveReward();
            bool canReceiveBonus = questInfo.CanReceiveBonusReward();

            // ���� �޾��� �� ������
            mReceivedObj.SetActive(mIsReceived && (haveBonus ? mIsReceivedBonus : true));

            // ��ư
            mButtonReceive.gameObject.SetActive(haveBonus ? !mIsReceived : true);
            mButtonReceive.SetActiveFrame(canReceive);

            // ���ʽ� ��ư
            mButtonBonusReceive.gameObject.SetActive(haveBonus ? mIsReceived : false);

            if (haveBonus)
            {
                if (mIsReceived)
                {
                    RefreshRewardSlotUI(questInfo.GetBonusReward());
                }
                else
                {
                    RefreshRewardSlotUI(questInfo.GetReward());
                }
            }
            else
            {
                RefreshRewardSlotUI(questInfo.GetReward());
            }

            // �����
            mRedDotObj.SetActive(canReceive);
        }

        void RefreshRewardSlotUI(string reward)
        {
            var rewardData = Lance.GameData.RewardData.TryGet(reward);
            if (rewardData != null)
            {
                // ����Ʈ ���� ����
                var itemSlotUIObj = gameObject.FindGameObject("QuestSlotUI");
                mRewardSlotUI = itemSlotUIObj.GetOrAddComponent<ItemSlotUI>();
                mRewardSlotUI.Init(new ItemInfo(rewardData));
            }
        }

        public void PlayReceiveMotion()
        {
            Refresh();

            UIUtil.PlayStampMotion(mImageModal, mImageCheck);
        }
    }
}