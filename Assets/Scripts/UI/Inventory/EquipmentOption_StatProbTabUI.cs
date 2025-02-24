using UnityEngine;
using TMPro;

namespace Lance
{
    class EquipmentOption_StatProbTabUI : EquipmentOptionInfoTabUI
    {
        public override void Init(EquipmentOptionInfoTab tab)
        {
            base.Init(tab);

            for (int i = 0; i < (int)EquipmentOptionStatGrade.Count; ++i)
            {
                EquipmentOptionStatGrade grade = (EquipmentOptionStatGrade)i;

                var textGradeProb = gameObject.FindComponent<TextMeshProUGUI>($"Text_GradeProb_{grade}");

                textGradeProb.text = $"{Lance.GameData.EquipmentOptionStatGradeProbData.probs[(int)grade] * 100:F2}%";
            }

            for (int i = 0; i < DataUtil.EquipmentOptionStatTypes.Length; ++i)
            {
                StatType statType = DataUtil.EquipmentOptionStatTypes[i];

                var statObj = gameObject.FindGameObject($"StatUI_{statType}");

                var statUI = statObj.GetOrAddComponent<EquipmentOptionStatProbUI>();

                statUI.Init(statType);
            }
        }
    }

    class EquipmentOptionStatProbUI : MonoBehaviour
    {
        public void Init(StatType statType)
        {
            var textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.GetName($"{statType}");

            var minMaxRange = DataUtil.GetEquipmentOptionStatMinMaxValue(statType);

            bool isPercentType = statType.IsPercentType();

            StringParam param = new StringParam("minValue", isPercentType ? $"{minMaxRange.min * 100f:F2}%" : minMaxRange.min);
            param.AddParam("maxValue", isPercentType ? $"{minMaxRange.max * 100f:F2}%" : minMaxRange.max);

            var textStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatRange");

            textStatValue.text = StringTableUtil.Get("UIString_Range", param);
        }
    }
}