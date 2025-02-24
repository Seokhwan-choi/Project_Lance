using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;

namespace Lance
{
    class SkillProficiencyItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        const string LevelActiveColor = "FFFFFF";
        const string LevelInactiveColor = "000000";
        const string ValueActiveColor = "F3C244";
        const string ValueInactiveColor = "FFFFFF";

        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextValue;
        GameObject mCurrentObj;
        bool mInit;
        public void Init()
        {
            mInit = true;
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mTextValue = gameObject.FindComponent<TextMeshProUGUI>("Text_Value");
            mCurrentObj = gameObject.FindGameObject("Current");
        }

        public void OnUpdateItem(int index)
        {
            if (mInit == false)
                return;

            int level = index + 1;

            var data = Lance.GameData.SkillProficiencyData.TryGet(level);
            if (data != null)
            {
                int currentLevel = Lance.Account.SkillInventory.GetSkillProficiencyLevel();

                bool isActive = currentLevel == level;

                string levelStr = StringTableUtil.Get("Name_SkillProficiencyLevel", new StringParam("level", $"{level}"));

                Color newColor = Color.white;

                newColor.a = isActive ? 1f : 0.2f;

                mTextLevel.SetColor(newColor);
                mTextLevel.text = levelStr;

                string valueStr = StringTableUtil.GetName("SkillDmg") + $" {data.addSkillDmg * 100f:F2}%";
                string valueColor = isActive ? ValueActiveColor : ValueInactiveColor;

                Color newColor2;

                ColorUtility.TryParseHtmlString($"#{valueColor}", out newColor2);

                newColor2.a = isActive ? 1f : 0.2f;

                mTextValue.SetColor(newColor2);
                mTextValue.text = valueStr;

                mCurrentObj.SetActive(isActive);
            }
        }
    }
}