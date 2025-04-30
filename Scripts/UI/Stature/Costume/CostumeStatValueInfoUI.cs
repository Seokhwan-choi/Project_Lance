using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class CostumeStatValueInfoUI : MonoBehaviour
    {
        TextMeshProUGUI mTextStatName;
        TextMeshProUGUI mTextStatValue;
        public void Init()
        {
            mTextStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            mTextStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatValue");
        }

        public void SetStatInfo(StatType statType, double stataValue)
        {
            string statName = StringTableUtil.GetName($"{statType}");
            bool isPercentType = statType.IsPercentType();

            mTextStatName.text = statName;
            mTextStatValue.text = isPercentType ? $"{stataValue * 100:F2}%" : $"{stataValue.ToAlphaString()}";
        }
    }
}


