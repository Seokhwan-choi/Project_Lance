using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class EssenceStatValueInfoUI : MonoBehaviour
    {
        EssenceType mType;

        GameObject mLevelObj;
        GameObject mMaxLevelObj;
        TextMeshProUGUI mTextStatName;
        TextMeshProUGUI mTextCurrentValue;
        TextMeshProUGUI mTextNextValue;
        TextMeshProUGUI mTextMaxValue;

        public void Init()
        {
            InternalInit();
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

        public void ChangeType(EssenceType type)
        {
            mType = type;

            Refresh();
        }

        public void Refresh()
        {
            bool isMaxLevel = false;
            double value = 0;
            double nextValue = 0;
            StatType statType = StatType.Atk;
            bool isPercentType = false;
            string statName = string.Empty;

            EssenceData essenceData = Lance.GameData.EssenceData.TryGet(mType);
            if (essenceData != null)
            {
                isMaxLevel = Lance.Account.Essence.IsMaxStep(mType) && Lance.Account.Essence.IsMaxLevel(mType);

                statType = essenceData.valueType;
                statName = StringTableUtil.GetName($"{statType}");
                isPercentType = statType.IsPercentType();

                value = Lance.Account.Essence.GetStatValue(mType);
                nextValue = Lance.Account.Essence.GetNextStatValue(mType);
            }

            mLevelObj.SetActive(isMaxLevel == false);
            mMaxLevelObj.SetActive(isMaxLevel);
            mTextStatName.text = statName;
            mTextMaxValue.text = isPercentType ? $"{value * 100:F2}%" : $"{value.ToAlphaString()}";
            mTextCurrentValue.text = isPercentType ? $"{value * 100:F2}%" : $"{value.ToAlphaString()}";
            mTextNextValue.text = isPercentType ? $"{nextValue * 100:F2}%" : $"{nextValue.ToAlphaString()}";
        }
    }
}