using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Popup_PresetInfoUI : PopupBase
    {
        public void Init(int preset)
        {
            SetUpCloseAction();

            var presetInfo = Lance.Account.Preset.GetPresetInfo(preset);

            SetTitleText(presetInfo.GetPresetName());

            var preview = gameObject.FindComponent<RawImage>("Preview");

            Lance.GameManager.SetActiveRankProfilePreview(true);

            preview.texture = Lance.GameManager.GetRankProfilePreviewRenderTexture();

            // 코스튬
            var costumes = presetInfo.GetEquippedCostumes();
            if (costumes != null)
            {
                Lance.GameManager.RefreshRankProfilePreview(costumes);
            }

            var statInfoList = gameObject.FindGameObject("StatInfoList");

            var content = statInfoList.FindGameObject("Content");
            content.AllChildObjectOff();

            // 스탯
            foreach (var statInfo in presetInfo.GetStats())
            {
                if (statInfo.GetStatType().IsShowingUserInfo())
                {
                    var statInfoUIObj = Util.InstantiateUI("UserStatInfoUI", content.transform);

                    var statInfoUI = statInfoUIObj.GetOrAddComponent<PresetInfo_StatInfoUI>();

                    statInfoUI.Init(statInfo.GetStatType(), statInfo.GetStatValue());
                }
            }

            // 장비 & 장신구
            var equipmentsNAccessorys = gameObject.FindGameObject("Equipments&Accessorys");

            // 장비
            var equipments = equipmentsNAccessorys.FindGameObject("EquipEquipments");

            foreach (var equipment in presetInfo.GetEquipmentInfos())
            {
                if (equipment.GetIsEquipped())
                {
                    var data = DataUtil.GetEquipmentData(equipment.GetId());

                    var equippedItemObj = equipments.FindGameObject($"EquippedItem_{data.type}");

                    var equippedItemUI = equippedItemObj.GetOrAddComponent<PresetInfo_EquipmentEquippedItemUI>();
                    equippedItemUI.Init(equipment);
                }
            }

            // 장신구
            var accessorys = equipmentsNAccessorys.FindGameObject("EquipAccessorys");

            Dictionary<ItemType, int> slotIndexs = new Dictionary<ItemType, int>();

            foreach (var accessory in presetInfo.GetEquippedAccessorys())
            {
                var data = DataUtil.GetAccessoryData(accessory.GetId());

                if (slotIndexs.ContainsKey(data.type))
                {
                    slotIndexs[data.type]++;
                }
                else
                {
                    slotIndexs.Add(data.type, 0);
                }

                int slotIndex = slotIndexs.TryGet(data.type);

                var equippedItemObj = accessorys.FindGameObject($"EquippedItem_{data.type}_{slotIndex}");

                var equippedItemUI = equippedItemObj.GetOrAddComponent<PresetInfo_AccessoryEquippedItemUI>();
                equippedItemUI.Init(accessory);
            }

            // 스킬
            var equipSkills = gameObject.FindGameObject("EquipSkills");

            // 액티브 스킬
            var activeSkills = presetInfo.GetEquippedActiveSkills();
            for(int i = 0; i < activeSkills.Count; ++i)
            {
                int index = i + 1;

                PresetInfo_SkillInfo activeSkill = activeSkills[i];

                var skillItemObj = equipSkills.FindGameObject($"ActiveSkill_{index}");

                var skillItemUI = skillItemObj.GetOrAddComponent<PresetInfo_SkillInfoUI>();
                skillItemUI.Init(activeSkill, SkillType.Active);
            }
            // 패시브 스킬
            var passiveSkills = presetInfo.GetEquippedPassiveSkills();
            for (int i = 0; i < passiveSkills.Count; ++i)
            {
                int index = i + 1;

                PresetInfo_SkillInfo passiveSkill = passiveSkills[i];

                var skillItemObj = equipSkills.FindGameObject($"PassiveSkill_{index}");

                var skillItemUI = skillItemObj.GetOrAddComponent<PresetInfo_SkillInfoUI>();
                skillItemUI.Init(passiveSkill, SkillType.Passive);
            }

            // 신수 & 신물
            var petNPetEquipments = gameObject.FindGameObject("Pet&PetEquipments");

            // 신수
            var petInfo = presetInfo.GetEquippedPetInfo();
            if (petInfo != null)
            {
                var equippedPetItemObj = petNPetEquipments.FindGameObject("EquippedItem_Pet");

                var petItemUI = equippedPetItemObj.GetOrAddComponent<PresetInfo_EquippedPetItemUI>();
                petItemUI.Init(petInfo);
            }

            // 신물
            foreach (var petEquipment in presetInfo.GetEquippedPetEquipments())
            {
                var data = Lance.GameData.PetEquipmentData.TryGet(petEquipment.GetId());

                var equippedItemObj = gameObject.FindGameObject($"EquippedItem_{data.type}PetEquipment");

                var equippedItemUI = equippedItemObj.GetOrAddComponent<PresetInfo_EquippedPetEquipmentItemUI>();

                equippedItemUI.Init(petEquipment);
            }
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close(immediate, hideMotion);

            Lance.GameManager.SetActiveRankProfilePreview(false);
        }
    }

    class PresetInfo_SkillInfoUI : MonoBehaviour
    {
        public void Init(PresetInfo_SkillInfo skillInfo, SkillType type)
        {
            if (skillInfo != null)
            {
                var id = skillInfo.GetId();

                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.SetActive(!id.IsValid());

                var itemObj = gameObject.FindGameObject("SkillItemUI");
                itemObj.SetActive(id.IsValid());

                if (id.IsValid())
                {
                    int level = Lance.Account.GetSkillLevel(type, id);

                    var itemUI = itemObj.GetOrAddComponent<RankProfile_SkillItemUI>();
                    itemUI.Init(id, level);
                }
                
            }
        }
    }

    class PresetInfo_StatInfoUI : MonoBehaviour
    {
        public void Init(StatType statType, double statValue)
        {
            TextMeshProUGUI textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.Get($"Name_{statType}");

            TextMeshProUGUI textStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatValue");
            textStatValue.text = statType.IsPercentType() ? $"{statValue * 100f:F2}%" :
                statType.IsNoneAlphaType() ? $"{statValue:F2}" : $"{statValue.ToAlphaString(showDp: ((statType == StatType.Atk || statType == StatType.Hp) ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default))}";
        }
    }

    class PresetInfo_EquippedPetItemUI : MonoBehaviour
    {
        public void Init(PresetInfo_PetInfo petInfo)
        {
            if (petInfo != null)
            {
                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.SetActive(false);

                var itemObj = gameObject.FindGameObject("PetItemUI");
                itemObj.SetActive(true);

                var id = petInfo.GetId();

                var petInst = Lance.Account.Pet.GetPet(id);

                var itemUI = itemObj.GetOrAddComponent<RankProfile_PetItemUI>();
                itemUI.Init(id, petInst.GetLevel(), petInst.GetStep());
            }
        }
    }

    class PresetInfo_EquippedPetEquipmentItemUI : MonoBehaviour
    {
        public void Init(PresetInfo_PetEquipmentInfo petEquipmentInfo)
        {
            if (petEquipmentInfo != null)
            {
                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.SetActive(false);

                var itemObj = gameObject.FindGameObject("EquipmentItemUI");
                itemObj.SetActive(true);

                var id = petEquipmentInfo.GetId();

                var petInst = Lance.Account.GetPetEquipment(id);

                var itemUI = itemObj.GetOrAddComponent<RankProfile_PetEquipmentItemUI>();
                itemUI.Init(id, petInst.GetLevel(), petInst.GetReforgeStep());
            }
        }
    }

    class PresetInfo_AccessoryEquippedItemUI : MonoBehaviour
    {
        public void Init(PresetInfo_AccessoryInfo accessoryInfo)
        {
            if (accessoryInfo != null)
            {
                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.gameObject.SetActive(false);

                var itemObj = gameObject.FindGameObject("AccessoryItemUI");
                itemObj.SetActive(true);

                string id = accessoryInfo.GetId();

                var accessoryInst = Lance.Account.GetAccessory(id);

                var accessoryItemUI = itemObj.GetOrAddComponent<RankProfile_AccessoryItemUI>();
                accessoryItemUI.Init(id, accessoryInst.GetLevel(), accessoryInst.GetReforgeStep());
            }
        }
    }

    class PresetInfo_EquipmentEquippedItemUI : MonoBehaviour
    {
        public void Init(PresetInfo_EquipmentInfo equipmentInfo)
        {
            if (equipmentInfo != null)
            {
                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.gameObject.SetActive(false);

                var itemObj = gameObject.FindGameObject("EquipmentItemUI");
                itemObj.SetActive(true);

                var equipmentItemUI = itemObj.GetOrAddComponent<RankProfile_EquipmentItemUI>();

                string id = equipmentInfo.GetId();

                var equipmentInst = Lance.Account.GetEquipment(id);

                equipmentItemUI.Init(id, equipmentInst.GetLevel(), equipmentInst.GetReforgeStep());
            }
        }
    }
}