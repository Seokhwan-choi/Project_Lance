using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CodeStage.AntiCheat.ObscuredTypes;


namespace Lance
{
    public partial class Account
    {
        public (string,int)[] DismantleAllSkills(SkillType skillType)
        {
            List<(string, int)> dismantleSkills = new List<(string, int)>();

            var skills = SkillInventory.GatherSkills(skillType);

            foreach (var skill in skills)
            {
                int skillCount = skill.GetCount();

                if (skillCount > 0 && skill.Dismantle(skillCount))
                {
                    dismantleSkills.Add((skill.GetSkillId(), skillCount));
                }
            }

            if (dismantleSkills.Count > 0)
            {
                SkillInventory.SetIsChangedData(true);
            }

            return dismantleSkills.ToArray();
        }

        public bool DismantleSkill(SkillType skillType, string id, int count)
        {
            return SkillInventory.DismantleSkill(skillType, id, count);
        }

        public string[] UpgradeAllSkills(SkillType skillType)
        {
            List<string> levelUpSkills = new List<string>();

            var skills = SkillInventory.GatherSkills(skillType);

            foreach(var skill in skills)
            {
                if (UpgradeSkill(skill, 1))
                {
                    levelUpSkills.Add(skill.GetSkillId());
                }
            }

            return levelUpSkills.ToArray();
        }

        public bool UpgradeSkill(SkillType skillType, string id, int count)
        {
            var skill = SkillInventory.GetSkill(skillType, id);
            if (skill == null)
                return false;

            return UpgradeSkill(skill, count);
        }

        bool UpgradeSkill(SkillInst skill, int count)
        {
            if (skill.IsMaxLevel())
                return false;

            var skillData = DataUtil.GetSkillData(skill.GetSkillId());
            if (skillData == null)
                return false;

            int requireCount = skill.GetUpgradeRequireCount(count);

            if (skillData.levelUpMaterial == SkillLevelUpMaterial.Skill)
            {
                if (skill.UseCount(requireCount))
                {
                    skill.LevelUp(count);

                    SkillInventory.SetIsChangedData(true);

                    return true;
                }
            }
            else
            {
                if (Currency.UseSkillPiece(requireCount))
                {
                    skill.LevelUp(count);

                    SkillInventory.SetIsChangedData(true);

                    return true;
                }
            }

            return false;
        }

        public bool IsMaxLevelSkill(SkillType skillType, string id)
        {
            var skill = SkillInventory.GetSkill(skillType, id);
            if (skill == null)
                return false;

            return skill.IsMaxLevel();
        }

        public bool IsEnoughSkillUpgradeRequireCount(SkillType skillType, string id, int count)
        {
            var skill = SkillInventory.GetSkill(skillType, id);
            if (skill == null)
                return false;

            var data = DataUtil.GetSkillData(skillType, id);
            if (data == null)
                return false;

            int requireCount = skill.GetUpgradeRequireCount(count);

            // 스킬
            if (data.levelUpMaterial == SkillLevelUpMaterial.Skill)
            {
                return skill.IsEnoughCount(requireCount);
            }
            // 스킬 조각
            else
            {
                return Lance.Account.Currency.IsEnoughSkillPiece(requireCount);
            }
        }

        public (bool haveEmpty, int slot) GetEmptySkillSlot(SkillType skillType)
        {
            var curSkillSets = SkillInventory.GetCurSkillsetIds(skillType);

            for(int i = 0; i < curSkillSets.Length; ++i)
            {
                int slot = i;

                if (IsUnlockSkillSlot(slot) && curSkillSets[slot].IsValid() == false)
                    return (true, slot);
            }

            return (false, -1);
        }

        public bool EquipSkill(SkillType skillType, int slot, string id)
        {
            if (IsUnlockSkillSlot(slot) == false)
            {
                return false;
            }

            return SkillInventory.EquipSkill(skillType, slot, id);
        }

        public bool UnEquipSkill(SkillType skillType, string id)
        {
            if (id.IsValid() == false)
                return false;

            return SkillInventory.UnEquipSkill(skillType, id);
        }

        public bool UnEquipSkill(SkillType skillType, int slot)
        {
            if (IsUnlockSkillSlot(slot) == false)
            {
                return false;
            }

            return SkillInventory.UnEquipSkill(skillType, slot);
        }

        public void AddSkill(string id, int count)
        {
            var data = DataUtil.GetSkillData(id);
            if (data != null)
            {
                SkillInventory.AddSkill(data.type, id, count);
            }
        }

        public int GetSkillCount(SkillType skillType, string id)
        {
            return SkillInventory.GetSkillCount(skillType, id);
        }

        public SkillInst GetSkill(SkillType skillType, string id)
        {
            return SkillInventory.GetSkill(skillType, id);
        }

        public int GetSkillLevel(SkillType skillType, string id)
        {
            return SkillInventory.GetSkillLevel(skillType, id);
        }

        public bool IsEquippedSkill(SkillType skillType, string id)
        {
            return SkillInventory.IsEquipped(skillType, id);
        }

        public bool AnyCurSkillSlotEmpty(SkillType skillType)
        {
            ObscuredString[] skillsetIds = SkillInventory.GetCurSkillsetIds(skillType);
            for (int i = 0; i < skillsetIds.Length; ++i)
            {
                // 아이디가 없는데 ( 비어 있는데 )
                int slot = i;
                if (skillsetIds[i].IsValid() == false)
                {
                    // 스킬 슬롯이 언락되어 있는거라면
                    // 그냥 비워둔 것 알려주자
                    if (IsUnlockSkillSlot(slot))
                        return true;
                }
            }

            return false;
        }

        public bool IsUnlockSkillSlot(int slot)
        {
            if (Lance.GameData.SkillSlotUnlockData.unlockLevel.Length <= slot)
                return false;

            int level = ExpLevel.GetLevel();
            int slotUnlockLevel = Lance.GameData.SkillSlotUnlockData.unlockLevel[slot];

            return level >= slotUnlockLevel;
        }

        public bool HaveSkill(SkillType skillType, string id)
        {
            return SkillInventory.HaveSkill(skillType, id);
        }

        public bool CanDismantleSkill(SkillType skillType, string id)
        {
            var skillData = DataUtil.GetSkillData(skillType, id);
            if (skillData == null)
                return false;

            var dismantleData = Lance.GameData.SkillDismantleData.TryGet(skillData.grade);
            if (dismantleData == null)
                return false;

            var skill = SkillInventory.GetSkill(skillType, id);
            if (skill == null)
                return false;

            return skill.GetCount() > 0;
        }

        public bool CanUpgradeSkill(SkillType skillType, string id, int count)
        {
            var skillData = DataUtil.GetSkillData(skillType, id);
            if (skillData == null)
                return false;

            if (HaveSkill(skillType, id) == false)
            {
                return false;
            }

            if (IsMaxLevelSkill(skillType, id))
            {
                return false;
            }

            if (Lance.Account.IsEnoughSkillUpgradeRequireCount(skillType, id, count) == false)
            {
                return false;
            }

            return true;
        }

        public bool CanResetSkills(SkillType skillType)
        {
            foreach(var skill in Lance.Account.SkillInventory.GatherSkills(skillType))
            {
                if (skill.GetLevel() > 1)
                    return true;
            }

            return false;
        }

        // 스킬 레벨 초기화
        public List<(string, int)> ResetSkills(SkillType skillType)
        {
            List<(string, int)> resetList = new List<(string, int)>();
            // 스킬 레벨 1로 초기화하면서 
            // 레벨만큼 스킬석 돌려주기

            foreach (var skill in Lance.Account.SkillInventory.GatherSkills(skillType))
            {
                string id = skill.GetSkillId();
                int level = skill.GetLevel();

                skill.SetLevel(1);

                resetList.Add((id, level));
            }

            if (resetList.Count > 0)
                Lance.Account.SkillInventory.SetIsChangedData(true);

            return resetList;
        }
    }
}
