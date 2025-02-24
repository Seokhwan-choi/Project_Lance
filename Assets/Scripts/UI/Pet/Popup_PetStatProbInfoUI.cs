using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Popup_PetStatProbInfoUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_PetStatProbInfo"));

            for(int i = 0; i < (int)PetEvolutionStatGrade.Count; ++i)
            {
                PetEvolutionStatGrade grade = (PetEvolutionStatGrade)i;

                var textGradeProb = gameObject.FindComponent<TextMeshProUGUI>($"Text_GradeProb_{grade}");

                textGradeProb.text = $"{Lance.GameData.PetEvolutionStatGradeProbData.probs[(int)grade] * 100:F2}%";
            }

            for(int i = 0; i < DataUtil.PetEvolutionStatTypes.Length; ++i)
            {
                StatType statType = DataUtil.PetEvolutionStatTypes[i];

                var statObj = gameObject.FindGameObject($"StatUI_{statType}");

                var statUI = statObj.GetOrAddComponent<PetStatInfoUI>();

                statUI.Init(statType);
            }
        }
    }

    class PetStatInfoUI : MonoBehaviour
    {
        public void Init(StatType statType)
        {
            var textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.GetName($"{statType}");

            var minMaxRange = DataUtil.GetPetEvolutionStatMinMaxValue(statType);

            bool isPercentType = statType.IsPercentType();

            StringParam param = new StringParam("minValue", isPercentType ? $"{minMaxRange.min * 100f:F2}%" : minMaxRange.min);
            param.AddParam("maxValue", isPercentType ? $"{minMaxRange.max * 100f:F2}%" : minMaxRange.max);

            var textStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatRange");

            textStatValue.text = StringTableUtil.Get("UIString_Range", param);
        }
    }
}