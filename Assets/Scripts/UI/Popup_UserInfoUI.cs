using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Popup_UserInfoUI : PopupBase
    {
        Slider mSliderExp;
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextExp;
        TextMeshProUGUI mTextNickname;
        TextMeshProUGUI mTextPowerLevel;
        List<EquipmentEquippedItemUI> mEquippedItemList;
        List<AccessoryEquippedItemUI> mAccessoryEquippedItemUI;
        List<StatInfoUI> mStatInfos;
        EquippedPetItemUI mEquippedPetItem;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_UserInfo"));

            mSliderExp = gameObject.FindComponent<Slider>("Slider_Exp");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_LevelValue");
            mTextExp = gameObject.FindComponent<TextMeshProUGUI>("Text_Exp");
            mTextPowerLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_PowerLevel");
            mTextNickname = gameObject.FindComponent<TextMeshProUGUI>("Text_Nickname");

            var buttonChangeNickname = gameObject.FindComponent<Button>("Button_ChangeNickname");
            buttonChangeNickname.SetButtonAction(OnChangeNicknameButton);

            var imagePortraitFrame = gameObject.FindComponent<Image>("Image_Portrait_Frame");
            
            var buttonChangeFrame = gameObject.FindComponent<Button>("Button_ChangeFrame");
            var buttonRedDot = buttonChangeFrame.gameObject.FindGameObject("RedDot");
            buttonRedDot.SetActive(Lance.Account.Achievement.AnyCanReceive());
            buttonChangeFrame.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_AchievementUI>();
                popup.Init();
                popup.SetOnCloseAction(() =>
                {
                    buttonRedDot.SetActive(Lance.Account.Achievement.AnyCanReceive());

                    RefreshPortraitFrame();

                    foreach(var statInfo in mStatInfos)
                    {
                        statInfo.Refresh();
                    }
                });
            });

            RefreshPortraitFrame();

            var costume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Body);
            var costumeData = Lance.GameData.BodyCostumeData.TryGet(costume);
            if (costumeData != null)
            {
                var imagePortrait = gameObject.FindComponent<Image>("Image_Portrait");
                imagePortrait.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
            }

            var content = gameObject.FindGameObject("Content");

            content.AllChildObjectOff();

            mStatInfos = new List<StatInfoUI>();

            // 스탯 생성
            for (int i = 0; i < (int)StatType.Count; ++i)
            {
                StatType type = (StatType)i;
                if (type.IsShowingUserInfo())
                {
                    var statInfoUIObj = Util.InstantiateUI("UserStatInfoUI", content.transform);

                    var statInfoUI = statInfoUIObj.GetOrAddComponent<StatInfoUI>();

                    statInfoUI.Init(type);

                    mStatInfos.Add(statInfoUI);
                }
            }

            mEquippedItemList = new List<EquipmentEquippedItemUI>();
            // 착용중인 장비
            ItemType[] equipmentTypes = new ItemType[] { ItemType.Weapon, ItemType.Armor,
                                                         ItemType.Gloves, ItemType.Shoes };
            for (int i = 0; i < equipmentTypes.Length; ++i)
            {
                ItemType equipmentType = equipmentTypes[i];

                var equippedItemObj = gameObject.FindGameObject($"EquippedItem_{equipmentType}");

                var equippedItemUI = equippedItemObj.GetOrAddComponent<EquipmentEquippedItemUI>();
                equippedItemUI.Init(equipmentType);

                mEquippedItemList.Add(equippedItemUI);
            }

            var equippedPetItemObj = gameObject.FindGameObject("EquippedItem_Pet");

            mEquippedPetItem = equippedPetItemObj.GetOrAddComponent<EquippedPetItemUI>();
            mEquippedPetItem.Init();

            mAccessoryEquippedItemUI = new List<AccessoryEquippedItemUI>();

            // 착용중인 장비
            ItemType[] accessoryTypes = new ItemType[] { ItemType.Necklace, ItemType.Earring,
                                                         ItemType.Ring };

            for (int i = 0; i < accessoryTypes.Length; ++i)
            {
                ItemType accessoryType = accessoryTypes[i];

                for(int j = 0; j < DataUtil.GetAccessoryMaxEquipCount(accessoryType); ++j)
                {
                    int slot = j;

                    var equippedItemObj = gameObject.FindGameObject($"EquippedItem_{accessoryType}_{slot}");

                    var equippedItemUI = equippedItemObj.GetOrAddComponent<AccessoryEquippedItemUI>();
                    equippedItemUI.Init(accessoryType, slot);

                    mAccessoryEquippedItemUI.Add(equippedItemUI);
                }
            }

            Refresh();

            void RefreshPortraitFrame()
            {
                var achievement = Lance.Account.Achievement.GetEquippedAchievement();
                var achievementData = Lance.GameData.AchievementData.TryGet(achievement);
                if (achievementData != null)
                {
                    imagePortraitFrame.sprite = Lance.Atlas.GetUISprite(achievementData.uiSprite);
                }
                else
                {
                    imagePortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                }
            }
        }

        void Refresh()
        {
            RefreshExp();
            RefreshNickname();
            RefreshEquippedEquipments();
            RefrshEquippedAccessorys();
            RefreshEquippedPet();
            RefreshPowerLevel();
        }

        public void RefreshEquippedEquipments()
        {
            foreach (var equippedItem in mEquippedItemList)
            {
                equippedItem.Refresh();
            }
        }

        public void RefrshEquippedAccessorys()
        {
            foreach(var equippedItem in mAccessoryEquippedItemUI)
            {
                equippedItem.Refresh();
            }
        }

        public void RefreshEquippedPet()
        {
            mEquippedPetItem.Refresh();
        }

        public void RefreshExp()
        {
            int level = Lance.Account.ExpLevel.GetLevel();

            mTextLevel.text = $"{level}";

            double currentExp = Lance.Account.ExpLevel.GetExp();
            double currentRequireExp = Lance.Account.ExpLevel.GetCurrentRequireExp();

            var percent = (currentExp / currentRequireExp);
            mTextExp.text = $"{currentExp.ToAlphaString()}/{currentRequireExp.ToAlphaString()} ( {percent*100f:F2}% )";
            mSliderExp.value = (float)percent;
        }

        public void RefreshPowerLevel()
        {
            mTextPowerLevel.text = Lance.Account.UserInfo.GetPowerLevel().ToAlphaString();
        }

        void RefreshNickname()
        {
            mTextNickname.text = Backend.UserNickName;
        }

        void OnChangeNicknameButton()
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_NicknameUI>();

            popup.Init(() =>
            {
                RefreshNickname();
                
                Lance.BackEnd.ChattingManager.ReEntryChattinChannel();
            });
        }
    }

    class StatInfoUI : MonoBehaviour
    {
        StatType mStatType;
        public void Init(StatType statType)
        {
            mStatType = statType;

            TextMeshProUGUI textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.Get($"Name_{statType}");

            Refresh();
        }

        public void Refresh()
        {
            double statValue = StatCalculator.GetPlayerStatValue(mStatType);

            if (mStatType == StatType.AtkSpeed)
                statValue = Math.Min(Lance.GameData.PlayerStatMaxData.atkSpeedMax, statValue);
            else if (mStatType == StatType.MoveSpeed)
                statValue = Math.Min(Lance.GameData.PlayerStatMaxData.moveSpeedMax, statValue);

            TextMeshProUGUI textStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatValue");
            textStatValue.text = mStatType.IsPercentType() ? $"{statValue * 100f:F2}%" :
                mStatType.IsNoneAlphaType() ? $"{statValue:F2}" : $"{statValue.ToAlphaString(showDp: ((mStatType == StatType.Atk || mStatType == StatType.Hp) ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default))}";
        }
    }

    class EquippedPetItemUI : MonoBehaviour
    {
        GameObject mEmptyObj;
        PetItemUI mEquippedPetItemUI;

        public void Init()
        {
            mEmptyObj = gameObject.FindGameObject("Empty");

            var itemObj = gameObject.FindGameObject("PetItemUI");
            mEquippedPetItemUI = itemObj.GetOrAddComponent<PetItemUI>();
        }

        public void Refresh()
        {
            var petInst = Lance.Account.Pet.GetEquippedInst();
            if (petInst == null)
            {
                mEmptyObj.SetActive(true);
                mEquippedPetItemUI.gameObject.SetActive(false);
            }
            else
            {
                mEmptyObj.SetActive(false);
                mEquippedPetItemUI.gameObject.SetActive(true);
                mEquippedPetItemUI.Init(petInst.GetId(), ignoreButton:true);
                mEquippedPetItemUI.SetIsEquippedActive(false);
                mEquippedPetItemUI.SetRedDotActive(false);
            }
        }
    }

    class AccessoryEquippedItemUI : MonoBehaviour
    {
        int mSlot;
        ItemType mItemType;
        GameObject mEmptyObj;
        Inventory_AccessoryItemUI mAccessoryItemUI;
        public void Init(ItemType itemType, int slot)
        {
            mSlot = slot;
            mItemType = itemType;
            mEmptyObj = gameObject.FindGameObject("Empty");

            var imageType = gameObject.FindComponent<Image>("Image_Type");
            imageType.sprite = Lance.Atlas.GetUISprite($"Image_Silhouette_{itemType}");

            var itemObj = gameObject.FindGameObject("AccessoryItemUI");
            mAccessoryItemUI = itemObj.GetOrAddComponent<Inventory_AccessoryItemUI>();
            mAccessoryItemUI.Init();
        }

        public void Refresh()
        {
            var id = Lance.Account.GetEquippedAccessoryId(mItemType, mSlot);
            if (id.IsValid() == false)
            {
                mEmptyObj.SetActive(true);
                mAccessoryItemUI.gameObject.SetActive(false);
            }
            else
            {
                mEmptyObj.SetActive(false);
                mAccessoryItemUI.gameObject.SetActive(true);
                mAccessoryItemUI.SetIgnoreRedDot(true);
                mAccessoryItemUI.SetId(id);
                mAccessoryItemUI.SetActiveModal(false);
                mAccessoryItemUI.SetActiveIsEquipped(false);
            }
        }
    }

    class EquipmentEquippedItemUI : MonoBehaviour
    {
        ItemType mItemType;
        GameObject mEmptyObj;
        Inventory_EquipmentItemUI mEquipmentItemUI;
        public void Init(ItemType itemType)
        {
            mItemType = itemType;
            mEmptyObj = gameObject.FindGameObject("Empty");

            var imageType = gameObject.FindComponent<Image>("Image_Type");
            imageType.sprite = Lance.Atlas.GetUISprite($"Image_Silhouette_{itemType}");

            var itemObj = gameObject.FindGameObject("EquipmentItemUI");
            mEquipmentItemUI = itemObj.GetOrAddComponent<Inventory_EquipmentItemUI>();
            mEquipmentItemUI.Init();
        }

        public void Refresh()
        {
            var equipmentInst = Lance.Account.GetEquippedEquipment(mItemType);
            if (equipmentInst == null)
            {
                mEmptyObj.SetActive(true);
                mEquipmentItemUI.gameObject.SetActive(false);
            }
            else
            {
                mEmptyObj.SetActive(false);
                mEquipmentItemUI.gameObject.SetActive(true);
                mEquipmentItemUI.SetIgnoreRedDot(true);
                mEquipmentItemUI.SetId(equipmentInst.GetId());
                //mEquipmentItemUI.SetButtonAction(OnSelectedEquipment);
                mEquipmentItemUI.SetActiveModal(false);
                mEquipmentItemUI.SetActiveIsEquipped(false);
            }
        }

        //void OnSelectedEquipment(string id)
        //{
        //    var tabUI = Lance.Lobby.GetInventoryTabUI(mItemType);

        //    var popup = Lance.PopupManager.CreatePopup<Popup_EquipmentInfoUI>();
        //    popup.Init(tabUI, id);
        //}
    }
}