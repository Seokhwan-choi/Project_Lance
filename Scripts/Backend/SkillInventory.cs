using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using BackEnd;
using System;
using System.Linq;

namespace Lance
{
    public class SkillInventory : AccountBase
    {
        int mCurrentPreset;
        SkillInventoryInfo mActiveSkill = new();
        SkillInventoryInfo mPassiveSkill = new();

        public override string GetTableName()
        {
            return "SkillInventory";
        }

        public IEnumerable<SkillInst> GatherSkills(SkillType type)
        {
            if (type == SkillType.Active)
            {
                return mActiveSkill.GetSkillInsts();
            }
            else
            {
                return mPassiveSkill.GetSkillInsts();
            }
        }

        public int GetStackedSkillsLevelUpCount(SkillType skillType)
        {
            int totalLevelUpCount = 0;

            if (skillType == SkillType.Active)
            {
                foreach (var activeSkill in mActiveSkill.GetSkillInsts())
                {
                    totalLevelUpCount += activeSkill.GetLevel() - 1;
                }
            }
            else
            {
                foreach (var passiveSkill in mPassiveSkill.GetSkillInsts())
                {
                    totalLevelUpCount += passiveSkill.GetLevel() - 1;
                }
            }

            return totalLevelUpCount;
        }

        public int GetStackedSkillsLevelUpCount()
        {
            int totalLevelUpCount = 0;

            foreach(var activeSkill in mActiveSkill.GetSkillInsts())
            {
                totalLevelUpCount += activeSkill.GetLevel() - 1;
            }

            foreach (var passiveSkill in mPassiveSkill.GetSkillInsts())
            {
                totalLevelUpCount += passiveSkill.GetLevel() - 1;
            }

            return totalLevelUpCount;
        }

        public int GetSkillProficiencyLevel()
        {
            var data = DataUtil.GetSatisfiedSkillProficiencyData(GetStackedSkillsLevelUpCount());

            return data?.step ?? 0;
        }

        public double GetSkillProficiencyValue()
        {
            var data = DataUtil.GetSatisfiedSkillProficiencyData(GetStackedSkillsLevelUpCount());

            return data?.addSkillDmg ?? 0;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            mActiveSkill.SetServerDataToLocal(gameDataJson["ActiveSkills"], gameDataJson["ActiveEquippedSkills"]);
            mPassiveSkill.SetServerDataToLocal(gameDataJson["PassiveSkills"], gameDataJson["PassiveEquippedSkills"]);
            mCurrentPreset = 0;
        }

        public override Param GetParam()
        {
            Param param = new Param();

            var activeSaveResult = mActiveSkill.ReadyToSave();
            var passiveSaveResult = mPassiveSkill.ReadyToSave();

            param.Add("ActiveSkills", activeSaveResult.saveSkills);
            param.Add("ActiveEquippedSkills", activeSaveResult.saveEquippedSkills);
            param.Add("PassiveSkills", passiveSaveResult.saveSkills);
            param.Add("PassiveEquippedSkills", passiveSaveResult.saveEquippedSkills);
            param.Add("CurrentPreset", mCurrentPreset);

            return param;
        }

        public bool AnyCanEquipSkill(SkillType type)
        {
            if (type == SkillType.Active)
            {
                return AnyCanEquipSkill(mActiveSkill.GetSkillInsts());
            }
            else
            {
                return AnyCanEquipSkill(mPassiveSkill.GetSkillInsts());
            }

            bool AnyCanEquipSkill(IEnumerable<SkillInst> skills)
            {
                foreach (var skill in skills)
                {
                    if (IsEquipped(type, skill.GetSkillId()) == false)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mActiveSkill.RandomizeKey();
            mPassiveSkill.RandomizeKey();
        }

        public void ChangePreset(int preset)
        {
            var maxPreset = Lance.GameData.SkillCommonData.presetMaxCount;
            if (maxPreset < preset || preset < 0)
                return;

            mCurrentPreset = preset;

            SetIsChangedData(true);
        }

        public ObscuredString[] GetCurSkillsetIds(SkillType skillType)
        {
            if (skillType == SkillType.Active)
            {
                return mActiveSkill.GetSkillsetIds(mCurrentPreset);
            }
            else
            {
                return mPassiveSkill.GetSkillsetIds(mCurrentPreset);
            }
        }

        public int GetCurrentPreset()
        {
            return mCurrentPreset;
        }

        //public IEnumerable<SkillInst> GatherCurSkillset()
        //{
        //    var skillSet = GetCurSkillsetIds();

        //    foreach (var skill in skillSet)
        //    {
        //        string id = skill;
        //        if (id.IsValid())
        //            yield return mSkills.TryGet(id);
        //    }
        //}

        public bool IsEmptySkillSlot(SkillType skillType, int slot)
        {
            var ids = GetCurSkillsetIds(skillType);
            if (ids == null)
                return false;

            if (Lance.GameData.SkillCommonData.skillMaxSlot <= slot)
                return false;

            if (ids.Length <= slot)
                return false;

            return ids[slot].IsValid() == false;
        }

        public int GetEquippedSkillSlot(SkillType skillType, string id)
        {
            var ids = GetCurSkillsetIds(skillType);
            if (ids != null)
            {
                for (int i = 0; i < ids.Length; ++i)
                {
                    int slot = i;

                    if (id == ids[i])
                    {
                        return slot;
                    }
                }
            }

            return -1;
        }

        public SkillInst GetSkill(SkillType skillType, string id)
        {
            if (skillType == SkillType.Active)
            {
                return mActiveSkill.GetSkill(id);
            }
            else
            {
                return mPassiveSkill.GetSkill(id);
            }
        }

        public void AddSkill(SkillType skillType, string id, int count)
        {
            if (skillType == SkillType.Active)
            {
                mActiveSkill.AddSkill(id, count);
            }
            else
            {
                mPassiveSkill.AddSkill(id, count);
            }

            SetIsChangedData(true);
        }

        //public bool CanDismantle(SkillType skillType, string id)
        //{
        //    SkillInst skill = GetSkill(skillType, id);
        //    if (skill == null)
        //        return false;

        //    var skillData = DataUtil.GetSkillData(skillType, id);
        //    if (skillData == null)
        //        return false;

        //    if (Lance.GameData.SkillDismantleData.ContainsKey(skillData.grade) == false)
        //        return false;

        //    if (skill.GetCount() <= 0)
        //        return false;

        //    return true;
        //}

        public bool DismantleSkill(SkillType skillType, string id, int count)
        {
            SkillInst skill = GetSkill(skillType, id);
            if (skill == null)
                return false;

            if (skill.Dismantle(count))
            {
                SetIsChangedData(true);

                return true;
            }
            else
            {
                return false;
            }
        }

        public double GetTotalEquippedSkillValues()
        {
            double totalValue = 0;

            var activeSkills = GetCurSkillsetIds(SkillType.Active);
            foreach(var skill in activeSkills)
            {
                if (skill.IsValid() == false)
                    continue;

                var skillData = DataUtil.GetSkillData(skill);
                if (skillData == null)
                    continue;

                totalValue += ((double)skillData.grade * 0.125f);
            }

            var passiveSkills = GetCurSkillsetIds(SkillType.Passive);
            foreach (var skill in passiveSkills)
            {
                if (skill.IsValid() == false)
                    continue;

                var skillData = DataUtil.GetSkillData(skill);
                if (skillData == null)
                    continue;

                totalValue += ((double)skillData.grade * 0.125f);
            }

            return totalValue;
        }

        public void AllUnEquipSkill(SkillType skillType)
        {
            ObscuredString[] skillSet = GetCurSkillsetIds(skillType);
            if (skillSet != null)
            {
                for (int i = 0; i < skillSet.Length; ++i)
                {
                    skillSet[i] = string.Empty;
                }
            }

            SetIsChangedData(true);
        }

        public bool UnEquipSkill(SkillType skillType, string id)
        {
            if (id.IsValid() == false)
                return false;

            ObscuredString[] skillSet = GetCurSkillsetIds(skillType);
            if (skillSet != null)
            {
                for (int i = 0; i < skillSet.Length; ++i)
                {
                    if (skillSet[i] == id)
                    {
                        skillSet[i] = string.Empty;

                        SetIsChangedData(true);

                        return true;
                    }
                }
            }

            return false;
        }

        public bool UnEquipSkill(SkillType skillType, int slot)
        {
            if (Lance.GameData.SkillCommonData.skillMaxSlot <= slot)
                return false;

            ObscuredString[] skillSet = GetCurSkillsetIds(skillType);
            if (skillSet == null)
                return false;

            if (skillSet.Length <= slot)
                return false;

            skillSet[slot] = string.Empty;

            SetIsChangedData(true);

            return true;
        }
        

        public bool EquipSkill(SkillType skillType, int slot, string id)
        {
            if (Lance.GameData.SkillCommonData.skillMaxSlot <= slot)
                return false;

            var skill = GetSkill(skillType, id);
            if (skill == null)
                return false;

            ObscuredString[] skillSet = GetCurSkillsetIds(skillType);
            if (skillSet == null)
                return false;

            if (skillSet.Length <= slot)
                return false;

            // 다른 곳에 착용중이었다면 비워주자
            for(int i = 0; i < skillSet.Length; ++i)
            {
                if (skillSet[i] == id)
                {
                    skillSet[i] = string.Empty;
                }
            }

            skillSet[slot] = id;

            SetIsChangedData(true);

            return true;
        }

        public bool HaveSkill(SkillType skillType, string id)
        {
            if (skillType == SkillType.Active)
            {
                return mActiveSkill.HaveSkill(id);
            }
            else
            {
                return mPassiveSkill.HaveSkill(id);
            }
        }


        public bool HaveLimitBreakSkill()
        {
            foreach(var activeSkill in mActiveSkill.GetSkillInsts())
            {
                var skillData = DataUtil.GetSkillData(activeSkill.GetSkillId());
                if (skillData.requireLimitBreak > 0)
                    return true;
            }

            return false;
        }

        public int GetSkillLevel(SkillType skillType, string id)
        {
            var skill = GetSkill(skillType, id);

            return skill?.GetLevel() ?? 0;
        }

        public bool IsEquipped(SkillType skillType, string id)
        {
            foreach(var skillId in GetCurSkillsetIds(skillType))
            {
                if ( skillId == id )
                {
                    return true;
                }
            }

            return false;
        }

        public int GetSkillCount(SkillType skillType, string id)
        {
            var skill = GetSkill(skillType, id);

            return skill?.GetCount() ?? 0;
        }

        public double GetSkillValue(SkillType skillType, string id)
        {
            var skill = GetSkill(skillType, id);

            return skill?.GetValue() ?? 0;
        }

#if UNITY_EDITOR
        public void Reset()
        {
            mActiveSkill.Reset();
            mPassiveSkill.Reset();

            SetIsChangedData(true);
        }
#endif

    }

    public class SkillInventoryInfo
    {
        Dictionary<int, ObscuredString[]> mEquippedSkils = new();
        Dictionary<ObscuredString, SkillInst> mSkills = new();

        public SkillInventoryInfo()
        {
            int maxSlot = Lance.GameData.SkillCommonData.skillMaxSlot;

            for (int i = 0; i < Lance.GameData.SkillCommonData.presetMaxCount; ++i)
            {
                int preset = i;

                mEquippedSkils.Add(preset, Enumerable.Repeat<ObscuredString>(string.Empty, maxSlot).ToArray());
            }
        }

        public void RandomizeKey()
        {
            if (mSkills != null && mSkills.Count > 0)
            {
                foreach (var item in mSkills)
                {
                    item.Key.RandomizeCryptoKey();
                    item.Value.RandomizeKey();
                }
            }
        }

        public void SetServerDataToLocal(JsonData skillDatas, JsonData equippedSkillDatas)
        {
            for (int i = 0; i < skillDatas.Count; i++)
            {
                JsonData skillData = skillDatas[i];

                if (skillData.ContainsKey("Id"))
                {
                    string id = skillData["Id"].ToString();

                    int levelTemp = 0;
                    int countTemp = 0;

                    int.TryParse(skillData["Level"].ToString(), out levelTemp);
                    int.TryParse(skillData["Count"].ToString(), out countTemp);

                    if (levelTemp > 0)
                    {
                        var skillInst = new SkillInst(id, levelTemp, countTemp);

                        if (mSkills.ContainsKey(id))
                        {
                            mSkills[id] = skillInst;
                        }
                        else
                        {
                            mSkills.Add(id, skillInst);
                        }
                    }
                }
            }

            var infoList = new List<ObscuredString>();

            for (int i = 0; i < equippedSkillDatas.Count; ++i)
            {
                int preset = i;

                JsonData equippedSkillData = equippedSkillDatas[preset];

                infoList.Clear();

                for (int slot = 0; slot < Lance.GameData.SkillCommonData.skillMaxSlot; ++slot)
                {
                    int skillSlot = slot;

                    string id = equippedSkillData[skillSlot].ToString();

                    infoList.Add(id);
                }

                if (mEquippedSkils.ContainsKey(preset))
                {
                    mEquippedSkils[preset] = infoList.ToArray();
                }
                else
                {
                    mEquippedSkils.Add(preset, infoList.ToArray());
                }
            }

            infoList = null;
        }

        public (Dictionary<string, SkillInst> saveSkills, Dictionary<int, string[]> saveEquippedSkills) ReadyToSave()
        {
            Dictionary<string, SkillInst> saveSkills = new Dictionary<string, SkillInst>();

            foreach (var item in mSkills)
            {
                item.Value.ReadyToSave();

                saveSkills.Add(item.Key, item.Value);
            }

            Dictionary<int, string[]> saveEquippedSkills = new Dictionary<int, string[]>();

            foreach (var item in mEquippedSkils)
            {
                int preset = item.Key;
                ObscuredString[] equippedSkills = item.Value;

                saveEquippedSkills.Add(preset, equippedSkills.Select(x => x.ToString()).ToArray());
            }

            return (saveSkills, saveEquippedSkills);
        }

        public IEnumerable<SkillInst> GetSkillInsts()
        {
            return mSkills.Values;
        }

        public ObscuredString[] GetSkillsetIds(int preset)
        {
            return mEquippedSkils.TryGet(preset);
        }

        public SkillInst GetSkill(string id)
        {
            return mSkills.TryGet(id);
        }

        public void AddSkill(string id, int count)
        {
            if (mSkills.ContainsKey(id))
            {
                var skillInst = mSkills.TryGet(id);

                skillInst.AddCount(count);
            }
            else
            {
                var skillInst = new SkillInst(id, 1, count);

                mSkills.Add(id, skillInst);
            }
        }

        public bool HaveSkill(string id)
        {
            return mSkills.ContainsKey(id);
        }

#if UNITY_EDITOR
        public void Reset()
        {
            mEquippedSkils = new();

            int maxSlot = Lance.GameData.SkillCommonData.skillMaxSlot;

            for (int i = 0; i < Lance.GameData.SkillCommonData.presetMaxCount; ++i)
            {
                int preset = i;

                mEquippedSkils.Add(preset, Enumerable.Repeat<ObscuredString>(string.Empty, maxSlot).ToArray());
            }

            mSkills = new();
        }
#endif
    }
}


