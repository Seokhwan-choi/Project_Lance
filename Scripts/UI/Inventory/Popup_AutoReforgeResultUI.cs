using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_AutoReforgeResultUI : PopupBase
    {
        public void InitEquipment(string id, int startLevel)
        {
            SetUpCloseAction();

            var equipment = Lance.Account.GetEquipment(id);
            var data = DataUtil.GetEquipmentData(id);
            if (equipment != null && data != null)
            {
                var itemObj = gameObject.FindGameObject("Item");
                var itemUI = itemObj.GetOrAddComponent<AutoReforge_ItemUI>();
                itemUI.InitEquipment(id);

                int curLevel = equipment.GetLevel();
                int curReforgeStep = equipment.GetReforgeStep();

                var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
                textName.text = curReforgeStep > 0 ? $"{StringTableUtil.GetName(id)} +{curReforgeStep}" : StringTableUtil.GetName(id);

                // 레벨
                var levelValueObj = gameObject.FindGameObject("Info_Level");
                var levelValueInfoUI = levelValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                levelValueInfoUI.Init(StatType.Level, startLevel, curLevel);

                // 장착 효과
                double beforeEquipValue = data.baseValue + ((data.baseValue * data.levelUpValue) * startLevel);
                double afterEquipValue = data.baseValue + ((data.baseValue * data.levelUpValue) * curLevel);

                var equipValueObj = gameObject.FindGameObject("Info_EquipValue");
                var equipValueInfoUI = equipValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                equipValueInfoUI.Init(data.valueType, beforeEquipValue, afterEquipValue);

                // 보유 효과
                for (int i = 0; i < data.ownStats.Length; ++i)
                {
                    int index = i + 1;

                    var ownValueObj = gameObject.FindGameObject($"Info_OwnValue_{index}");

                    string ownStatId = data.ownStats[i];
                    if (ownStatId.IsValid() == false)
                    {
                        ownValueObj.SetActive(false);
                    }
                    else
                    {
                        var ownStatData = Lance.GameData.OwnStatData.TryGet(ownStatId);
                        if (ownStatData == null)
                            continue;

                        double beforeStatValue = ownStatData.baseValue + (ownStatData.levelUpValue * startLevel);
                        double afterStatValue = ownStatData.baseValue + (ownStatData.levelUpValue * curLevel);

                        ownValueObj.SetActive(true);

                        var ownValueInfoUI = ownValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                        ownValueInfoUI.Init(ownStatData.valueType, beforeStatValue, afterStatValue);
                    }
                }
            }
        }

        public void InitPetEquipment(string id, int startLevel)
        {
            SetUpCloseAction();

            var equipment = Lance.Account.GetPetEquipment(id);
            var data = Lance.GameData.PetEquipmentData.TryGet(id);
            if (equipment != null && data != null)
            {
                int curLevel = equipment.GetLevel();
                int curReforgeStep = equipment.GetReforgeStep();

                var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
                textName.text = curReforgeStep > 0 ? $"{StringTableUtil.GetName(id)} +{curReforgeStep}" : StringTableUtil.GetName(id);

                var itemObj = gameObject.FindGameObject("Item");
                var itemUI = itemObj.GetOrAddComponent<AutoReforge_ItemUI>();
                itemUI.InitPetEquipment(id);

                // 레벨
                var levelValueObj = gameObject.FindGameObject("Info_Level");
                var levelValueInfoUI = levelValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                levelValueInfoUI.Init(StatType.Level, startLevel, curLevel);

                // 장착 효과
                double beforeEquipValue = data.baseValue + ((data.baseValue * data.levelUpValue) * startLevel);
                double afterEquipValue = data.baseValue + ((data.baseValue * data.levelUpValue) * curLevel);

                var equipValueObj = gameObject.FindGameObject("Info_EquipValue");
                var equipValueInfoUI = equipValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                equipValueInfoUI.Init(data.valueType, beforeEquipValue, afterEquipValue);

                // 보유 효과
                for (int i = 0; i < data.ownStats.Length; ++i)
                {
                    int index = i + 1;

                    var ownValueObj = gameObject.FindGameObject($"Info_OwnValue_{index}");

                    string ownStatId = data.ownStats[i];
                    if (ownStatId.IsValid() == false)
                    {
                        ownValueObj.SetActive(false);
                    }
                    else
                    {
                        var ownStatData = Lance.GameData.PetEquipmentOwnStatData.TryGet(ownStatId);
                        if (ownStatData == null)
                            continue;

                        double beforeStatValue = ownStatData.baseValue + (ownStatData.levelUpValue * startLevel);
                        double afterStatValue = ownStatData.baseValue + (ownStatData.levelUpValue * curLevel);

                        ownValueObj.SetActive(true);
                        var ownValueInfoUI = ownValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                        ownValueInfoUI.Init(ownStatData.valueType, beforeStatValue, afterStatValue);
                    }
                }
            }
        }

        public void InitAccessory(string id, int startLevel)
        {
            SetUpCloseAction();

            var accessory = Lance.Account.GetAccessory(id);
            var data = DataUtil.GetAccessoryData(id);
            if (accessory != null && data != null)
            {
                int curLevel = accessory.GetLevel();
                int curReforgeStep = accessory.GetReforgeStep();

                var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
                textName.text = curReforgeStep > 0 ? $"{StringTableUtil.GetName(id)} +{curReforgeStep}" : StringTableUtil.GetName(id);

                var itemObj = gameObject.FindGameObject("Item");
                var itemUI = itemObj.GetOrAddComponent<AutoReforge_ItemUI>();
                itemUI.InitAccessory(id);

                // 레벨
                var levelValueObj = gameObject.FindGameObject("Info_Level");
                var levelValueInfoUI = levelValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                levelValueInfoUI.Init(StatType.Level, startLevel, curLevel);

                // 장착 효과
                double beforeEquipValue = data.baseValue + (data.levelUpValue * startLevel);
                double afterEquipValue = data.baseValue + (data.levelUpValue * curLevel);

                var equipValueObj = gameObject.FindGameObject("Info_EquipValue");
                var equipValueInfoUI = equipValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                equipValueInfoUI.Init(data.valueType, beforeEquipValue, afterEquipValue);

                // 보유 효과
                for (int i = 0; i < data.ownStats.Length; ++i)
                {
                    int index = i + 1;

                    var ownValueObj = gameObject.FindGameObject($"Info_OwnValue_{index}");

                    string ownStatId = data.ownStats[i];
                    if (ownStatId.IsValid() == false)
                    {
                        ownValueObj.SetActive(false);
                    }
                    else
                    {
                        var ownStatData = Lance.GameData.AccessoryOwnStatData.TryGet(ownStatId);
                        if (ownStatData == null)
                            continue;

                        double beforeStatValue = ownStatData.baseValue + (ownStatData.levelUpValue * startLevel);
                        double afterStatValue = ownStatData.baseValue + (ownStatData.levelUpValue * curLevel);

                        ownValueObj.SetActive(true);
                        var ownValueInfoUI = ownValueObj.GetOrAddComponent<AutoReforgeResultInfoUI>();
                        ownValueInfoUI.Init(ownStatData.valueType, beforeStatValue, afterStatValue);
                    }
                }
            }
        }
    }

    class AutoReforgeResultInfoUI : MonoBehaviour
    {
        public void Init(StatType statType, double beforeValue, double afterValue)
        {
            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.Get($"Name_{statType}");

            var textBeforeValue = gameObject.FindComponent<TextMeshProUGUI>("Text_BeforeValue");
            textBeforeValue.text = statType.IsPercentType() ? $"{beforeValue * 100f:F2}%" :
                statType.IsNoneAlphaType() ? $"{beforeValue}": beforeValue.ToAlphaString();

            var textAfterValue = gameObject.FindComponent<TextMeshProUGUI>("Text_AfterValue");
            textAfterValue.text = statType.IsPercentType() ? $"{afterValue * 100f:F2}%" :
                statType.IsNoneAlphaType() ? $"{afterValue}" : afterValue.ToAlphaString();
        }
    }
}