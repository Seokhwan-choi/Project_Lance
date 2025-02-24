using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Mosframe;

namespace Lance
{
    class StageSelectItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        bool mInit;
        Popup_StageSelectUI mParent;
        TextMeshProUGUI mTextStageName;

        Image mImageModal;
        StageData mStageData;
        GameObject mStageRewardSlotList;
        GameObject mCurrentStageObj;

        Dictionary<ItemType, StageRewardSlotUI> mRewardSlotDics;

        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<Popup_StageSelectUI>();
            mTextStageName = gameObject.FindComponent<TextMeshProUGUI>("Text_StageName");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");

            mRewardSlotDics = new Dictionary<ItemType, StageRewardSlotUI>();
            mStageRewardSlotList = gameObject.FindGameObject("StageRewardSlotList");

            ItemType[] rewardItemTypes = new ItemType[]
            {
                ItemType.Exp, ItemType.Gold, ItemType.UpgradeStone,
                ItemType.ReforgeStone, ItemType.PetFood, ItemType.Random_Equipment, ItemType.Random_Accessory
            };

            for(int i = 0; i < rewardItemTypes.Length; ++i)
            {
                ItemType itemType = rewardItemTypes[i];

                var itemSlotObj = mStageRewardSlotList.FindGameObject($"StageRewardSlot_{itemType}");
                var rewardSlotUI = itemSlotObj.GetOrAddComponent<StageRewardSlotUI>();

                mRewardSlotDics.Add(itemType, rewardSlotUI);
            }
            
            mCurrentStageObj = gameObject.FindGameObject("CurrentStage");

            var buttonStageSelect = gameObject.FindComponent<Button>("Button_StageSelect");
            buttonStageSelect.SetButtonAction(OnStageSelect);
        }

        public void OnUpdateItem(int index)
        {
            mStageData = mParent?.GetSelectedStageData(index);

            Refresh();
        }

        void Refresh()
        {
            if (mStageData != null)
            {
                string chapterName = StringTableUtil.Get($"UIString_ChapterName_{mStageData.chapter}");

                mTextStageName.text = $"{chapterName} {mStageData.chapter}-{mStageData.stage}";

                mCurrentStageObj.SetActive(StageRecordsUtil.IsCurrentStageData(mStageData));

                mImageModal.gameObject.SetActive(!Lance.Account.StageRecords.CanChangeStage(mStageData.diff, mStageData.chapter, mStageData.stage));

                if (mStageData.monsterDropReward.IsValid())
                {
                    MonsterRewardData monsterDropReward = DataUtil.GetMonsterRewardData(mStageData.type, mStageData.monsterDropReward);
                    if (monsterDropReward != null)
                    {
                        mStageRewardSlotList.AllChildObjectOff();

                        if (monsterDropReward.exp > 0)
                        {
                            var rewardSlotUI = mRewardSlotDics.TryGet(ItemType.Exp);

                            rewardSlotUI.gameObject.SetActive(true);
                            rewardSlotUI.Init(new ItemInfo(ItemType.Exp, monsterDropReward.exp));
                            rewardSlotUI.SetActiveProb(false);
                        }

                        if (monsterDropReward.gold > 0)
                        {
                            var rewardSlotUI = mRewardSlotDics.TryGet(ItemType.Gold);

                            rewardSlotUI.gameObject.SetActive(true);
                            rewardSlotUI.Init(new ItemInfo(ItemType.Gold, monsterDropReward.gold));
                            rewardSlotUI.SetProb($"{monsterDropReward.goldProb * 100f:F2}%");
                        }

                        if (monsterDropReward.stones > 0)
                        {
                            var rewardSlotUI = mRewardSlotDics.TryGet(ItemType.UpgradeStone);

                            rewardSlotUI.gameObject.SetActive(true);
                            rewardSlotUI.Init(new ItemInfo(ItemType.UpgradeStone, monsterDropReward.stones));
                            rewardSlotUI.SetProb($"{monsterDropReward.stonesProb * 100f:F2}%");
                        }

                        if (monsterDropReward.reforgeStone > 0)
                        {
                            var rewardSlotUI = mRewardSlotDics.TryGet(ItemType.ReforgeStone);

                            rewardSlotUI.gameObject.SetActive(true);
                            rewardSlotUI.Init(new ItemInfo(ItemType.ReforgeStone, monsterDropReward.reforgeStone));
                            rewardSlotUI.SetProb($"{monsterDropReward.reforgeStoneProb * 100f:F2}%");
                        }

                        if (monsterDropReward.petFood > 0)
                        {
                            var rewardSlotUI = mRewardSlotDics.TryGet(ItemType.PetFood);

                            rewardSlotUI.gameObject.SetActive(true);
                            rewardSlotUI.Init(new ItemInfo(ItemType.PetFood, monsterDropReward.petFood).SetShowStr("X1"));
                            rewardSlotUI.SetProb($"{monsterDropReward.petFoodProb * 100f:F0}%");
                        }

                        if (monsterDropReward.randomEquipment.IsValid() && monsterDropReward.equipmentProb > 0)
                        {
                            float dropProb = monsterDropReward.equipmentProb;

                            var randomReward = Lance.GameData.RandomEquipmentRewardData.TryGet(monsterDropReward.randomEquipment);
                            if (randomReward != null)
                            {
                                ItemInfo itemInfo = new ItemInfo(ItemType.Random_Equipment)
                                        .SetGrade(randomReward.grade)
                                        .SetSubGrade(randomReward.subGrade)
                                        .SetShowStr("X1");

                                var rewardSlotUI = mRewardSlotDics.TryGet(ItemType.Random_Equipment);
                                rewardSlotUI.gameObject.SetActive(true);
                                rewardSlotUI.Init(itemInfo);
                                rewardSlotUI.SetProb($"{dropProb * 100f:F4}%");
                            }
                        }

                        if (monsterDropReward.randomAccessory.IsValid() && monsterDropReward.accessoryProb > 0)
                        {
                            float dropProb = monsterDropReward.accessoryProb;

                            var randomReward = Lance.GameData.RandomAccessoryRewardData.TryGet(monsterDropReward.randomAccessory);
                            if (randomReward != null)
                            {
                                ItemInfo itemInfo = new ItemInfo(ItemType.Random_Accessory)
                                        .SetGrade(randomReward.grade)
                                        .SetSubGrade(randomReward.subGrade)
                                        .SetShowStr("X1");

                                var rewardSlotUI = mRewardSlotDics.TryGet(ItemType.Random_Accessory);
                                rewardSlotUI.gameObject.SetActive(true);
                                rewardSlotUI.Init(itemInfo);
                                rewardSlotUI.SetProb($"{dropProb * 100f:F4}%");
                            }
                        }
                    }
                }
            }
        }

        void OnStageSelect()
        {
            if (mStageData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.StageRecords.CanChangeStage(mStageData.diff, mStageData.chapter, mStageData.stage) == false)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotReachedStage");

                return;
            }

            Lance.Account.StageRecords.ChangeStage(mStageData.diff, mStageData.chapter, mStageData.stage);

            mParent.OnStageSelect(mStageData);
        }
    }

    class StageRewardSlotUI : MonoBehaviour
    {
        TextMeshProUGUI mTextProb;
        ItemSlotUI mItemSlotUI;

        public void Init(ItemInfo itemInfo)
        {
            mTextProb = gameObject.FindComponent<TextMeshProUGUI>("Text_Prob");

            mItemSlotUI = gameObject.GetOrAddComponent<ItemSlotUI>();
            mItemSlotUI.Init(itemInfo);
        }

        public void SetActiveProb(bool isActive)
        {
            mTextProb.gameObject.SetActive(isActive);
        }

        public void SetProb(string prob)
        {
            mTextProb.text = prob;
        }
    }
}