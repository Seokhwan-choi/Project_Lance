using System.Linq;
using System.Collections.Generic;


namespace Lance
{
    public partial class Account
    {
        public void ApplyPreset(int preset)
        {
            // �������� �ٲ���ٸ� �������� ��� ���� �ٲ�����
            var presetInfo = Preset.GetPresetInfo(preset);
            if (presetInfo != null)
            {
                // �ڽ�Ƭ
                var costumes = presetInfo.GetEquippedCostumes();
                if (costumes != null)
                {
                    for (int i = 0; i < costumes.Length; ++i)
                    {
                        CostumeType costumeType = (CostumeType)i;

                        string costume = costumes[i];
                        if (costume.IsValid())
                        {
                            Costume.EquipCostume(costume);
                        }
                    }
                }

                // ���
                var equipmentInfos = presetInfo.GetEquipmentInfos();
                if (equipmentInfos != null)
                {
                    foreach (var info in equipmentInfos)
                    {
                        if (info.GetId().IsValid())
                        {
                            ChangeEquipmentOptionPreset(info.GetId(), info.GetPresetNum());

                            if (info.GetIsEquipped())
                                EquipEquipment(info.GetId());
                        }
                    }
                }

                // ��ű�
                var accessoryInfos = presetInfo.GetEquippedAccessorys();
                if (accessoryInfos != null)
                {
                    Lance.Account.AllUnEquipAccssory();

                    foreach (var info in accessoryInfos)
                    {
                        if (info.GetId().IsValid())
                        {
                            if (info.GetIsEquipped())
                                EquipAccessory(info.GetId());
                        }
                    }
                }

                // �ż�
                var petInfos = presetInfo.GetPetInfos();
                if (petInfos != null)
                {
                    foreach (var petInfo in petInfos)
                    {
                        if (petInfo != null && petInfo.GetId().IsValid())
                        {
                            if ( petInfo.GetIsEquipped() )
                            {
                                Pet.EquipPet(petInfo.GetId());
                            }

                            Pet.ChangeEvolutionStatPreset(petInfo.GetId(), petInfo.GetPresetNum());
                        }
                    }
                }

                // �Ź�
                var petEquipmentInfos = presetInfo.GetEquippedPetEquipments();
                if (petEquipmentInfos != null)
                {
                    foreach (var info in petEquipmentInfos)
                    {
                        if (info.GetId().IsValid())
                            EquipPetEquipment(info.GetId());
                    }
                }

                // ��Ƽ�� ��ų
                var activeSkillInfos = presetInfo.GetEquippedActiveSkills();
                if (activeSkillInfos != null)
                {
                    Lance.Account.SkillInventory.AllUnEquipSkill(SkillType.Active);

                    for (int i = 0; i < activeSkillInfos.Count; ++i)
                    {
                        string id = activeSkillInfos[i].GetId();
                        int slot = i;

                        if (id.IsValid())
                            EquipSkill(SkillType.Active, slot, id);
                    }
                }

                // �нú� ��ų
                var passiveSkillInfos = presetInfo.GetEquippedPassiveSkills();
                if (passiveSkillInfos != null)
                {
                    Lance.Account.SkillInventory.AllUnEquipSkill(SkillType.Passive);

                    for (int i = 0; i < passiveSkillInfos.Count; ++i)
                    {
                        string id = passiveSkillInfos[i].GetId();
                        int slot = i;

                        if (id.IsValid())
                            EquipSkill(SkillType.Passive, slot, id);
                    }
                }
            }
        }

        public bool UnlockPreset(int preset)
        {
            var data = Lance.GameData.PresetData.TryGet(preset);
            if (data == null)
                return false;

            var presetInfo = Preset.GetPresetInfo(preset);
            if (presetInfo == null)
                return false;

            if (presetInfo.IsUnlocked())
                return false;

            int requireUnlockPreset = data.requireUnlockPreset;
            var requirePresetInfo = Preset.GetPresetInfo(requireUnlockPreset);
            if (requirePresetInfo == null)
                return false;

            if (requirePresetInfo.IsUnlocked() == false)
                return false;

            if (IsEnoughGem(data.unlockPrice) == false)
                return false;

            presetInfo.Unlock();

            Preset.SetIsChangedData(true);

            return true;
        }

        public bool SavePreset(int preset)
        {
            var data = Lance.GameData.PresetData.TryGet(preset);
            if (data == null)
                return false;

            var presetInfo = Preset.GetPresetInfo(preset);
            if (presetInfo == null)
                return false;

            if (presetInfo.IsUnlocked() == false)
                return false;

            // �ڽ�Ƭ
            presetInfo.SetEquippedCostumes(Costume.GetEquippedCostumeIds());
            // ���
            presetInfo.SetEquipmentInfos(GatherEquipments());
            // ��ű�
            presetInfo.SetEquippedAccessorys(GetEquippedAccessorys());
            // �ż�
            presetInfo.SetPetInfos(Pet.GetPetInsts());
            // �Ź�
            presetInfo.SetEquippedPetEquipments(GatherPetEquipments());
            // ��Ƽ�� ��ų
            presetInfo.SetEquippedActiveSkills(SkillInventory.GetCurSkillsetIds(SkillType.Active).Select(x => x.ToString()).ToArray());
            // �нú� ��ų
            presetInfo.SetEquippedPassiveSkills(SkillInventory.GetCurSkillsetIds(SkillType.Passive).Select(x => x.ToString()).ToArray());
            // ����
            Dictionary<StatType, double> stats = new Dictionary<StatType, double>();
            for (int i = 0; i < (int)StatType.Count; ++i)
            {
                StatType statType = (StatType)i;
                if (statType.IsShowingUserInfo())
                {
                    stats.Add(statType, StatCalculator.GetPlayerStatValue(statType));
                }
            }
            presetInfo.SetStats(stats);

            Preset.SetIsChangedData(true);

            return true;
        }

        public bool ChangePresetName(int preset, string name)
        {
            var data = Lance.GameData.PresetData.TryGet(preset);
            if (data == null)
                return false;

            var presetInfo = Preset.GetPresetInfo(preset);
            if (presetInfo == null)
                return false;

            if (presetInfo.IsUnlocked() == false)
                return false;

            presetInfo.SetPresetName(name);

            Preset.SetIsChangedData(true);

            return true;
        }
    }
}