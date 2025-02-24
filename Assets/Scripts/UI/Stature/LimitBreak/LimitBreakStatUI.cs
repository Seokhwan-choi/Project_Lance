using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class LimitBreakStatUI : MonoBehaviour
    {
        StatType mStatType;
        GameObject mLevelObj;
        GameObject mMaxLevelObj;
        TextMeshProUGUI mTextStatName;
        TextMeshProUGUI mTextCurrentValue;
        TextMeshProUGUI mTextNextValue;
        TextMeshProUGUI mTextMaxValue;

        public void Init(StatType statType)
        {
            mStatType = statType;

            mLevelObj = gameObject.FindGameObject("Level");
            mMaxLevelObj = gameObject.FindGameObject("MaxLevel");

            mTextStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            mTextCurrentValue = mLevelObj.FindComponent<TextMeshProUGUI>("Text_CurrentValue");
            mTextNextValue = mLevelObj.FindComponent<TextMeshProUGUI>("Text_NextValue");
            mTextMaxValue = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_MaxValue");

            Refresh();
        }

        public void Refresh()
        {
            bool isMaxLimitBreak = Lance.Account.ExpLevel.IsMaxLimitBreak();
            bool isPercentType = mStatType.IsPercentType();

            double statValue = Lance.Account.ExpLevel.GetLimitBreakStatValue(mStatType);
            double nextStatValue = Lance.Account.ExpLevel.GetNextLimitBreakStatValue(mStatType);

            mLevelObj.SetActive(isMaxLimitBreak == false);
            mMaxLevelObj.SetActive(isMaxLimitBreak);

            mTextStatName.text = StringTableUtil.GetName($"{mStatType}");

            mTextMaxValue.text = isPercentType ? $"{statValue * 100f:F0}%" : $"{statValue.ToAlphaString()}";
            mTextCurrentValue.text = isPercentType ? $"{statValue * 100f:F0}%" : $"{statValue.ToAlphaString()}";
            mTextNextValue.text = isPercentType ? $"{nextStatValue * 100f:F0}%" : $"{nextStatValue.ToAlphaString()}";
        }
    }
}