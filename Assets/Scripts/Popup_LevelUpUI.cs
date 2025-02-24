using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_LevelUpUI : PopupBase
    {
        public void Init(int level, bool isUnlockSkillSlotLevel, bool canTryLimitBreakLevel)
        {
            var buttonModal = gameObject.FindComponent<Button>("Button_Modal");
            buttonModal.SetButtonAction(() => Close());

            var textLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            textLevel.text = $"{level}";

            var textLevelUpMessage = gameObject.FindComponent<TextMeshProUGUI>("Text_LevelUpMessage");

            string levelUpMessag = string.Empty;

            if (isUnlockSkillSlotLevel)
            {
                int unlockSkillSlot = DataUtil.GetUnlockSkillSlotByRequireLevel(level);

                StringParam param = new StringParam("skillSlot", unlockSkillSlot + 1);

                levelUpMessag += StringTableUtil.Get("UIString_UnlockSkillSlot", param);
                levelUpMessag += "\n";
            }

            if (canTryLimitBreakLevel)
            {
                int canTryLimitBreakStep = DataUtil.GetLimitBreakDataByRequireLevel(level).step;

                StringParam param = new StringParam("step", canTryLimitBreakStep);

                levelUpMessag += StringTableUtil.Get("UIString_CanTryLimitBreak", param);
                levelUpMessag += "\n";
            }

            textLevelUpMessage.text = levelUpMessag;

            levelUpMessag = null;
        }
    }
}


