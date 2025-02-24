using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;


namespace Lance
{
    class CollectionItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        bool mInit;
        CollectionData mData;
        TextMeshProUGUI mTextRewardName;
        TextMeshProUGUI mTextRewardValue;
        GameObject mReceivedObj;
        Button mButtonReceive;
        GameObject mRedDotObj;
        List<CollectionSlotUI> mCollectionSlotUIList;
        Popup_CollectionUI mParent;

        public string Id => mData?.id ?? string.Empty;
        public void Init()
        {
            if (mInit)
                return;

            mParent = gameObject.GetComponentInParent<Popup_CollectionUI>();

            mCollectionSlotUIList = new List<CollectionSlotUI>();

            for(int i = 0; i < 5; ++i)
            {
                int index = i;
                var slotObj = gameObject.FindGameObject($"CollectionSlotUI_{index + 1}");
                var slotUI = slotObj.GetOrAddComponent<CollectionSlotUI>();
                slotUI.Init();

                mCollectionSlotUIList.Add(slotUI);
            }

            mTextRewardName = gameObject.FindComponent<TextMeshProUGUI>("Text_CollectionRewardName");
            mTextRewardValue = gameObject.FindComponent<TextMeshProUGUI>("Text_CollectionRewardValue");
            mReceivedObj = gameObject.FindGameObject("Received");
            mRedDotObj = gameObject.FindGameObject("RedDot");
            mButtonReceive = gameObject.FindComponent<Button>("Button_Receive");

            mInit = true;
        }

        public void OnUpdateItem(int index)
        {
            mData = mParent?.GetCollectionData(index);

            Refresh();
        }

        public void Refresh()
        {
            if (mData != null)
            {
                for (int i = 0; i < mData.requireId.Length; ++i)
                {
                    int slotIndex = i;
                    string requireId = mData.requireId[slotIndex];
                    int requireLevel = mData.requireLevel[slotIndex];

                    if (mCollectionSlotUIList.Count <= slotIndex)
                        continue;

                    var slotUI = mCollectionSlotUIList[slotIndex];
                    if (requireId.IsValid())
                    {
                        slotUI.SetActive(true);
                        slotUI.Refresh(mData.itemType, requireId, requireLevel);
                    }
                    else
                    {
                        slotUI.SetActive(false);
                    }
                }

                var statData = Lance.GameData.CollectionStatData.TryGet(mData.rewardStat);
                if (statData != null)
                {
                    bool isPercentType = statData.valueType.IsPercentType();

                    mTextRewardName.text = StringTableUtil.GetName($"{statData.valueType}");
                    mTextRewardValue.text = isPercentType ? $"{statData.statValue * 100f:F2}%" : statData.statValue.ToAlphaString();
                }

                bool isAlreadyCollect = Lance.Account.Collection.IsAlreadyCollect(mData.id);
                bool isSatisfiedCollection = Lance.Account.IsSatisfiedCollection(mData.id);

                // 보상 받았을 때 가리기
                mReceivedObj.SetActive(isAlreadyCollect);

                // 버튼
                mButtonReceive.SetActiveFrame(isAlreadyCollect == false && isSatisfiedCollection);

                // 레드닷
                mRedDotObj.SetActive(isAlreadyCollect == false && isSatisfiedCollection);
            }
        }

        public void PlayReceiveMotion()
        {
            var anim = gameObject.GetComponent<Animation>();

            anim.Play();
        }

        public void OnReceiveButton()
        {
            if (mData != null)
            {
                if (Lance.GameManager.CompleteCollection(mData.id))
                {
                    Refresh();

                    PlayReceiveMotion();

                    mParent.RefreshAllCompleteButton();
                    mParent.RefreshRedDots();

                    //mParent.PlayQuestItemReceiveMotion(mData.id);

                    SoundPlayer.PlayUIButtonTouchSound();

                    SoundPlayer.PlayShowReward();
                }
            }
        }
    }

    class CollectionSlotUI : MonoBehaviour
    {
        ItemSlotUI mItemSlotUI;
        Image mImageModal;
        public void Init()
        {
            var itemSlotObj = gameObject.FindGameObject("ItemSlotUI");
            mItemSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
        }

        public void Refresh(ItemType itemType, string requireId, int requireLevel)
        {
            if (itemType.IsEquipment())
            {
                var data = DataUtil.GetEquipmentData(requireId);
                var itemInfo = new ItemInfo(itemType)
                .SetId(requireId)
                .SetShowStr($"Lv.{requireLevel}").SetGrade(data.grade).SetSubGrade(data.subGrade);

                mItemSlotUI.Init(itemInfo);

                mImageModal.gameObject.SetActive(Lance.Account.IsSatisfiedCollectionEquipment(requireId, requireLevel) == false);
            }
            else if (itemType.IsSkill())
            {
                var data = DataUtil.GetSkillData(requireId);
                var itemInfo = new ItemInfo(itemType)
                .SetId(requireId)
                .SetShowStr($"Lv.{requireLevel}").SetGrade(data.grade);

                mItemSlotUI.Init(itemInfo);

                mImageModal.gameObject.SetActive(Lance.Account.IsSatisfiedCollectionSkill(requireId, requireLevel) == false);
            }
            else if (itemType.IsAccessory())
            {
                var data = DataUtil.GetAccessoryData(requireId);
                var itemInfo = new ItemInfo(itemType)
                .SetId(requireId)
                .SetShowStr($"Lv.{requireLevel}").SetGrade(data.grade).SetSubGrade(data.subGrade);

                mItemSlotUI.Init(itemInfo);

                mImageModal.gameObject.SetActive(Lance.Account.IsSatisfiedCollectionAccessory(requireId, requireLevel) == false);
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}