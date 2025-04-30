using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Lance
{
    class StatValueInfoUI : MonoBehaviour
    {
        ItemType mType;
        string mId;
        string mOwnStatId;

        GameObject mLevelObj;
        GameObject mMaxLevelObj;
        TextMeshProUGUI mTextStatName;
        TextMeshProUGUI mTextCurrentValue;
        TextMeshProUGUI mTextNextValue;
        TextMeshProUGUI mTextMaxValue;

        public void InitEquipment(string id)
        {
            InternalInit();

            var data = DataUtil.GetEquipmentData(id);
            if (data != null)
            {
                mType = data.type;
                mId = id;
            }
        }

        public void InitAccessory(string id)
        {
            InternalInit();

            var data = DataUtil.GetAccessoryData(id);
            if (data != null)
            {
                mType = data.type;
                mId = id;
            }
        }

        public void InitPetEquipment(string id)
        {
            InternalInit();

            var data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (data != null)
            {
                mType = ItemType.PetEquipment;
                mId = id;
            }
        }

        public void InitPet(string id)
        {
            InternalInit();

            var data = Lance.GameData.PetData.TryGet(id);
            if (data != null)
            {
                mType = ItemType.Pet;
                mId = id;
            }
        }

        public void InitCostume(string id)
        {
            InternalInit();

            var data = DataUtil.GetCostumeData(id);
            if (data != null)
            {
                mType = ItemType.Costume;
                mId = id;
            }
        }

        public void InitSkill(string id)
        {
            InternalInit();

            var data = DataUtil.GetSkillData(id);
            if (data != null)
            {
                mType = ItemType.Skill;
                mId = id;
            }
        }

        public void ChangeId(string id)
        {
            mId = id;
        }

        public void SetOwnStatId(string id)
        {
            mOwnStatId = id;
        }

        void InternalInit()
        {
            mLevelObj = gameObject.FindGameObject("Level");
            mMaxLevelObj = gameObject.FindGameObject("MaxLevel");

            mTextStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            mTextCurrentValue = mLevelObj.FindComponent<TextMeshProUGUI>("Text_CurrentValue");
            mTextNextValue = mLevelObj.FindComponent<TextMeshProUGUI>("Text_NextValue");
            mTextMaxValue = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_MaxValue");
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void Refresh()
        {
            bool isMaxLevel = false;
            double value = 0;
            double nextValue = 0;
            StatType statType = StatType.Atk;
            bool isPercentType = false;
            bool isAlpha = true;
            string statName = string.Empty;
            if (mType.IsEquipment())
            {
                EquipmentData equipmentData = DataUtil.GetEquipmentData(mId);
                if (equipmentData != null)
                {
                    int level = Lance.Account.GetEquipmentLevel(mId);
                    int nextLevel = level + 1;
                    int maxLevel = equipmentData.maxLevel;

                    isMaxLevel = level == maxLevel;

                    string ownStatId = mOwnStatId;
                    if (ownStatId.IsValid())
                    {
                        OwnStatData ownStatData = Lance.GameData.OwnStatData.TryGet(ownStatId);
                        if (ownStatData != null)
                        {
                            statType = ownStatData.valueType;
                            statName = StringTableUtil.GetName($"{statType}");
                            isPercentType = statType.IsPercentType();
                            value = ownStatData.baseValue + (level * ownStatData.levelUpValue);
                            nextValue = ownStatData.baseValue + (nextLevel * ownStatData.levelUpValue);
                        }
                    }
                    else
                    {
                        statType = equipmentData.valueType;
                        statName = StringTableUtil.GetName($"{statType}");
                        isPercentType = statType.IsPercentType();

                        double baseValue = equipmentData.baseValue;

                        value = baseValue + ((baseValue * equipmentData.levelUpValue) * level);
                        nextValue = baseValue + ((baseValue * equipmentData.levelUpValue) * nextLevel);
                    }
                }
            }
            else if (mType.IsAccessory())
            {
                AccessoryData accessoryData = DataUtil.GetAccessoryData(mId);
                if (accessoryData != null)
                {
                    int level = Lance.Account.GetAccessoryLevel(mId);
                    int nextLevel = level + 1;
                    int maxLevel = accessoryData.maxLevel;

                    isMaxLevel = level == maxLevel;

                    string ownStatId = mOwnStatId;
                    if (ownStatId.IsValid())
                    {
                        OwnStatData ownStatData = Lance.GameData.AccessoryOwnStatData.TryGet(ownStatId);
                        if (ownStatData != null)
                        {
                            statType = ownStatData.valueType;
                            statName = StringTableUtil.GetName($"{statType}");
                            isPercentType = statType.IsPercentType();
                            value = ownStatData.baseValue + (level * ownStatData.levelUpValue);
                            nextValue = ownStatData.baseValue + (nextLevel * ownStatData.levelUpValue);
                        }
                    }
                    else
                    {
                        statType = accessoryData.valueType;
                        statName = StringTableUtil.GetName($"{statType}");
                        isPercentType = statType.IsPercentType();

                        double baseValue = accessoryData.baseValue;

                        value = baseValue + (accessoryData.levelUpValue * level);
                        nextValue = baseValue + (accessoryData.levelUpValue * nextLevel);
                    }
                }
            }
            else if (mType == ItemType.PetEquipment)
            {
                PetEquipmentData equipmentData = Lance.GameData.PetEquipmentData.TryGet(mId);
                if (equipmentData != null)
                {
                    int level = Lance.Account.GetPetEquipmentLevel(mId);
                    int nextLevel = level + 1;
                    int maxLevel = equipmentData.maxLevel;

                    isMaxLevel = level == maxLevel;

                    string ownStatId = mOwnStatId;
                    if (ownStatId.IsValid())
                    {
                        OwnStatData ownStatData = Lance.GameData.PetEquipmentOwnStatData.TryGet(ownStatId);
                        if (ownStatData != null)
                        {
                            statType = ownStatData.valueType;
                            statName = StringTableUtil.GetName($"{statType}");
                            isPercentType = statType.IsPercentType();
                            value = ownStatData.baseValue + (level * ownStatData.levelUpValue);
                            nextValue = ownStatData.baseValue + (nextLevel * ownStatData.levelUpValue);
                        }
                    }
                    else
                    {
                        statType = equipmentData.valueType;
                        statName = StringTableUtil.GetName($"{statType}");
                        isPercentType = statType.IsPercentType();

                        double baseValue = equipmentData.baseValue;

                        value = baseValue + ((baseValue * equipmentData.levelUpValue) * level);
                        nextValue = baseValue + ((baseValue * equipmentData.levelUpValue) * nextLevel);
                    }
                }
            }
            else if (mType == ItemType.Pet)
            {
                PetData petData = Lance.GameData.PetData.TryGet(mId);
                if (petData != null)
                {
                    string ownStatId = mOwnStatId;
                    if (ownStatId.IsValid())
                    {
                        int level = Lance.Account.Pet.GetLevel(mId);
                        int nextLevel = level + 1;

                        isMaxLevel = Lance.Account.Pet.IsMaxLevel(mId);

                        OwnStatData ownStatData = Lance.GameData.PetOwnStatData.TryGet(ownStatId);
                        if (ownStatData != null)
                        {
                            statType = ownStatData.valueType;
                            statName = StringTableUtil.GetName($"{statType}");
                            isPercentType = statType.IsPercentType();
                            value = ownStatData.baseValue + (level * ownStatData.levelUpValue);
                            nextValue = ownStatData.baseValue + (nextLevel * ownStatData.levelUpValue);
                        }
                    }
                    else
                    {
                        int step = Lance.Account.Pet.GetStep(mId);

                        isMaxLevel = !Lance.Account.CanEvolutionPet(mId) || Lance.Account.Pet.IsMaxStep(mId);

                        var petEquipStatData = Lance.GameData.PetEquipStatData.TryGet(step);
                        if (petEquipStatData != null)
                        {
                            statType = petEquipStatData.valueType;
                            statName = StringTableUtil.GetName($"{statType}");
                            isPercentType = statType.IsPercentType();

                            value = petEquipStatData.value;

                            var nextPetEquipStatData = Lance.GameData.PetEquipStatData.TryGet(step + 1);
                            if (nextPetEquipStatData != null)
                            {
                                nextValue = nextPetEquipStatData.value;
                            }
                            else
                            {
                                nextValue = petEquipStatData.value;
                            }
                        }
                    }
                }
            }
            else if (mType == ItemType.Costume)
            {
                CostumeData costumeData = DataUtil.GetCostumeData(mId);
                if (costumeData != null)
                {
                    int level = Lance.Account.GetCostumeLevel(mId);
                    int nextLevel = level + 1;
                    int maxLevel = costumeData.maxLevel;

                    isMaxLevel = level == maxLevel;

                    string ownStatId = mOwnStatId;
                    if (ownStatId.IsValid())
                    {
                        OwnStatData ownStatData = Lance.GameData.CostumeOwnStatData.TryGet(ownStatId);
                        if (ownStatData != null)
                        {
                            statType = ownStatData.valueType;
                            statName = StringTableUtil.GetName($"{statType}");
                            isPercentType = statType.IsPercentType();

                            int ownLevel = Math.Max(0, level - 1);
                            int nextOwnLevel = Math.Max(0, nextLevel - 1);

                            value = ownStatData.baseValue + (ownLevel * ownStatData.levelUpValue);
                            nextValue = ownStatData.baseValue + (nextOwnLevel * ownStatData.levelUpValue);
                        }
                    }
                    else
                    {
                        isAlpha = false;
                        statType = StatType.Level;
                        statName = StringTableUtil.GetName($"{statType}");
                        isPercentType = false;
                        value = level;
                        nextValue = nextLevel;
                    }
                }
            }
            else
            {
                var skillData = DataUtil.GetSkillData(mId);
                if (skillData != null)
                {
                    int level = Lance.Account.GetSkillLevel(skillData.type, mId);
                    int nextLevel = level + 1;
                    int maxLevel = DataUtil.GetSkillMaxLevel(mId);

                    isMaxLevel = level == maxLevel;

                    statName = skillData.type == SkillType.Active ?
                        StringTableUtil.Get("Name_SkillDmg") :
                        StringTableUtil.Get($"Name_{skillData.passiveType}");

                    statType = StatType.SkillDmg;
                    isPercentType = true;
                    value = skillData.skillValue + (skillData.levelUpValue * level);
                    nextValue = skillData.skillValue + (skillData.levelUpValue * nextLevel);
                }
            }

            mLevelObj.SetActive(isMaxLevel == false);
            mMaxLevelObj.SetActive(isMaxLevel);
            mTextStatName.text = statName;
            mTextMaxValue.text = isPercentType ? $"{value * 100:F2}%" : isAlpha ? $"{value.ToAlphaString()}" : $"{value}";
            mTextCurrentValue.text = isPercentType ? $"{value * 100:F2}%" : isAlpha ? $"{value.ToAlphaString()}" : $"{value}";
            mTextNextValue.text = isPercentType ? $"{nextValue * 100:F2}%" : isAlpha ? $"{nextValue.ToAlphaString()}" : $"{value}";
        }
    }
}