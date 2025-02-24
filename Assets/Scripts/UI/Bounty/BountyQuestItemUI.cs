using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class BountyQuestItemUI : MonoBehaviour
    {
        string mId;
        bool mIsReceived;
        Slider mSliderProgress;
        TextMeshProUGUI mTextProgress;

        Button mButtonReceive;
        Button mButtonMove;
        GameObject mRedDotObj;
        GameObject mReceivedObj;
        Image mImageModal;
        Image mImageCheck;
        public void Init(string id, Action onMove)
        {
            mId = id;

            BountyQuestInfo questInfo = Lance.Account.GetBountyQuestInfo(id);
            if (questInfo != null)
            {
                BountyQuestData data = Lance.GameData.BountyQuestData.TryGet(id);
                if (data != null)
                {
                    var imageWantedText = gameObject.FindComponent<Image>("Image_Title");
                    imageWantedText.sprite = Lance.Atlas.GetUISprite(data.monsterType == MonsterType.boss ? "Text_Wanted_Boss" : "Text_Wanted_Monster");

                    // 보스 초상화
                    var portraitParent = gameObject.FindGameObject("Portrait");
                    foreach (Image portrait in portraitParent.GetComponentsInChildren<Image>(true))
                    {
                        if (portrait.name == data.uiSprite)
                        {
                            portrait.gameObject.SetActive(true);
                            break;
                        }
                    }

                    // 설명
                    var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");

                    StringParam requireCountParam = new StringParam("requireCount", data.killCount);

                    string descIdx = data.monsterType == MonsterType.boss ? "BountyQuest_KillBoss" : "BountyQuest_KillMonster";

                    textName.text = StringTableUtil.GetDesc(descIdx, requireCountParam);
                }

                // 보상
                if (questInfo.GetReward().IsValid())
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(questInfo.GetReward());
                    if (rewardData != null)
                    {
                        // 퀘스트 보상 슬롯
                        var itemSlotUIObj = gameObject.FindGameObject("QuestSlotUI");
                        var itemSlotUI = itemSlotUIObj.GetOrAddComponent<ItemSlotUI>();
                        itemSlotUI.Init(new ItemInfo(rewardData));
                    }
                }
            }

            // 퀘스트 진행도
            mSliderProgress = gameObject.FindComponent<Slider>("Slider_Progress");
            mTextProgress = gameObject.FindComponent<TextMeshProUGUI>("Text_Progress");

            // 퀘스트 보상 버튼
            mButtonReceive = gameObject.FindComponent<Button>("Button_Receive");
            mButtonReceive.SetButtonAction(OnReceiveButton);

            // 빠른 이동버튼
            mButtonMove = gameObject.FindComponent<Button>("Button_Move");
            mButtonMove.SetButtonAction(() => OnMoveButton(onMove));

            mRedDotObj = gameObject.FindGameObject("RedDot");
            mReceivedObj = gameObject.FindGameObject("Received");
            mImageModal = mReceivedObj.FindComponent<Image>("Image_Modal");
            mImageCheck = mReceivedObj.FindComponent<Image>("Image_Check");

            Refresh();
        }

        void OnReceiveButton()
        {
            Lance.GameManager.ReceiveBountyQuestReward(mId, PlayReceiveMotion);
        }

        void OnMoveButton(Action onMove)
        {
            Lance.GameManager.MoveToBountyQuestTarget(mId, onMove);
        }

        public void Refresh()
        {
            BountyQuestInfo questInfo = Lance.Account.GetBountyQuestInfo(mId);

            int curCount = questInfo.GetStackedCount();
            int maxCount = questInfo.GetMaxRequireCount();

            mSliderProgress.value = (float)curCount / (float)maxCount;
            mTextProgress.text = $"{UIUtil.GetColorString("52FF00", curCount)}/{UIUtil.GetColorString("FFFFFF", maxCount)}";

            mIsReceived = questInfo.GetIsReceived();
            bool canReceive = questInfo.CanReceiveReward();

            // 보상 받았을 때 가리기
            mReceivedObj.SetActive(mIsReceived);

            // 버튼
            if ( canReceive )
            {
                mButtonReceive.gameObject.SetActive(true);
                mButtonMove.gameObject.SetActive(false);

                // 레드닷
                mRedDotObj.SetActive(true);
            }
            else
            {
                mButtonReceive.gameObject.SetActive(false);
                mButtonMove.gameObject.SetActive(true);

                // 레드닷
                mRedDotObj.SetActive(false);
            }
        }

        public void PlayReceiveMotion()
        {
            Refresh();

            UIUtil.PlayStampMotion(mImageModal, mImageCheck);
        }
    }
}