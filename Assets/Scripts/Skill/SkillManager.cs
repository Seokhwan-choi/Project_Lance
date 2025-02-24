using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Linq;

namespace Lance
{
    class SkillManager
    {
        Player mPlayer;
        List<Skill> mProcessingSkills;
        Dictionary<string, SkillInfo> mPassiveSkillInfos;
        Dictionary<string, SkillInfo> mActvieSkillInfos;
        
        public void Init(Player player)
        {
            mPlayer = player;
            mProcessingSkills = new List<Skill>();
            mPassiveSkillInfos = new Dictionary<string, SkillInfo>();
            mActvieSkillInfos = new Dictionary<string, SkillInfo>();

            RefreshAllSlot();
        }

        public void RandomizeKey()
        {
            foreach (var skill in mProcessingSkills)
            {
                if (skill.IsFinish == false)
                    skill.RandomizeKey();
            }

            foreach (var info in mPassiveSkillInfos.Values)
            {
                info.RandomizeKey();
            }

            foreach (var info in mActvieSkillInfos.Values)
            {
                info.RandomizeKey();
            }
        }

        public void OnChangePreset()
        {
            OnRelease();

            RefreshAllSlot();

            Lance.GameManager.UpdatePlayerStat();
        }

        public void RefreshAllSlot()
        {
            mActvieSkillInfos.Clear();
            mPassiveSkillInfos.Clear();

            ObscuredString[] actives = Lance.Account.SkillInventory.GetCurSkillsetIds(SkillType.Active);
            ObscuredString[] passives = Lance.Account.SkillInventory.GetCurSkillsetIds(SkillType.Passive);

            for (int i = 0; i < Lance.GameData.SkillCommonData.skillMaxSlot; ++i)
            {
                int slot = i;
                string activeId = actives[i];
                string passiveId = passives[i];

                if (activeId.IsValid())
                {
                    SkillInfo skillInfo = new SkillInfo();

                    skillInfo.Init(this, mPlayer, slot, activeId);

                    mActvieSkillInfos.Add(activeId, skillInfo);
                }

                if (passiveId.IsValid())
                {
                    SkillInfo skillInfo = new SkillInfo();

                    skillInfo.Init(this, mPlayer, slot, passiveId);

                    mPassiveSkillInfos.Add(passiveId, skillInfo);
                }
            }

            foreach(var activeSkill in mActvieSkillInfos)
            {
                activeSkill.Value.FillCoolTime();
            }

            foreach(var passiveSkill in mPassiveSkillInfos)
            {
                passiveSkill.Value.FillCoolTime();
            }

            actives = null;
            passives = null;
        }

        public void ResetCoolTimes()
        {
            foreach(var skillInfo in mActvieSkillInfos.Values)
            {
                skillInfo.ResetCoolTime();
            }

            foreach (var skillInfo in mPassiveSkillInfos.Values)
            {
                skillInfo.ResetCoolTime();
            }
        }

        public void OnRelease()
        {
            foreach(var skill in mProcessingSkills)
            {
                skill.OnRelease();
            }

            mProcessingSkills.Clear();
        }
        
        SkillInfo GetActiveSkillInfo(string id)
        {
            return mActvieSkillInfos.TryGet(id);
        }

        SkillInfo FindSkillInfoById(string id)
        {
            var skillInfo = GetActiveSkillInfo(id);
            if (skillInfo == null)
                skillInfo = mPassiveSkillInfos.TryGet(id);

            return skillInfo;
        }

        public bool CastSkill(string id)
        {
            var skillInfo = GetActiveSkillInfo(id);
            if (skillInfo == null)
                return false;

            if (skillInfo.CanCast() == false)
                return false;

            return skillInfo.Cast();
        }

        public void AddProcessingSkill(Skill skill)
        {
            mProcessingSkills.Add(skill);
        }

        public float GetSkillRemainCoolTime(string id)
        {
            SkillInfo skillInfo = FindSkillInfoById(id);

            return skillInfo?.GetCoolTime() ?? 0;
        }

        public void OnUpdate(float dt)
        {
            var removeSkillList = new List<Skill>();

            foreach (var skill in mProcessingSkills)
            {
                if (skill.IsFinish)
                {
                    removeSkillList.Add(skill);
                }
            }

            if (removeSkillList.Count > 0)
            {
                for (int i = 0; i < removeSkillList.Count; ++i)
                {
                    var remove = removeSkillList[i];

                    mProcessingSkills.Remove(remove);

                    remove.OnRelease();
                }

                removeSkillList = null;
            }

            foreach (var info in mActvieSkillInfos.Values)
            {
                info.OnUpdate(dt);

                // 스킬 자동 사용이면 예약
                if (SaveBitFlags.SkillAutoCast.IsOn())
                {
                    if (info.CanCast())
                    {
                        ReserveActiveSkill(info.Id, info.Slot);
                    }
                }
            }

            foreach (var info in mPassiveSkillInfos.Values)
            {
                info.OnUpdate(dt);
            }
        }

        public void ReserveActiveSkill(string id)
        {
            var info = FindSkillInfoById(id);
            if (info == null)
                return;

            if (info.CanCast() == false)
            {
                UIUtil.ShowSystemErrorMessage("CanNotCastSkillInCoolTime");

                return;
            }

            ReserveActiveSkill(info.Id, info.Slot);
        }

        public void ReserveActiveSkill(string id, int slot)
        {
            var skillInst = Lance.Account.SkillInventory.GetSkill(SkillType.Active, id);
            if (skillInst == null)
                return;

            int equippedSlot = Lance.Account.SkillInventory.GetEquippedSkillSlot(SkillType.Active, id);
            if (equippedSlot == -1 || equippedSlot != slot)
                return;

            if (Lance.Account.IsUnlockSkillSlot(slot) == false)
                return;

            SkillData skillData = DataUtil.GetSkillData(id);
            if (skillData == null)
                return;

            if (skillData.type != SkillType.Active)
                return;

            var result = Lance.GameManager.StageManager.AnyInAttackRangeOpponent(mPlayer, skillData.atkRange);
            if (result.any == false)
                return;

            // 스킬을 사용할 수 있으면 바로 스킬 예약
            mPlayer.Action.ReserveSkillCast(skillData);
        }

        public void ApplyPassive(SkillCondition condition, DamageInst inst = null)
        {
            foreach (SkillInfo info in mPassiveSkillInfos.Values)
            {
                if (info.Condition != condition)
                    continue;

                if (info.Apply(inst))
                {
                    // 보여줄 FX가 있다면 보여주자
                    if (info.Data.skillFX.IsValid())
                    {
                        mPlayer.PlayFX(info.Data.id, info.Data.skillFX);
                    }
                }
            }
        }

        public double GatherPassiveSkillValue(StatType type, DamageInst inst = null)
        {
            double totalValue = 0;

            foreach (SkillInfo info in mPassiveSkillInfos.Values)
            {
                if (info.Data.passiveType != type)
                    continue;

                if (info.IsApply() || info.Envaluate(inst))
                    totalValue += info.GetSkillValue(inst);
            }

            return totalValue;
        }

        //public bool CastSkill(string id)
        //{
        //    return mActiveManager.CastSkill(id);
        //}

        //public float GetSkillRemainCoolTime(string id)
        //{
        //    return mActiveManager.GetSkillRemainCoolTime(id);
        //}

        //public void AddProcessingSkill(Skill skill)
        //{
        //    mActiveManager.AddProcessingSkill(skill);
        //}

        //public double GatherPassievSkillValue(SkillType type, DamageInst inst = null)
        //{
        //    return mPassiveManager.GatherSkillValue(type, inst);
        //}

        //public void OnUpdate(float dt)
        //{
        //    mActiveManager.OnUpdate(dt);
        //    mPassiveManager.OnUpdate(dt);
        //}

        //public void RandomizeKey()
        //{
        //    mActiveManager.RandomizeKey();
        //    mPassiveManager.RandomizeKey();
        //}

        //public void OnChangePreset()
        //{
        //    mActiveManager.OnChangePreset();
        //    mPassiveManager.OnChangePreset();
        //}

        //public void RefreshSlot(int slot, string id)
        //{
        //    mActiveManager.RefreshSlot(slot, id);
        //    mPassiveManager.RefreshSlot(slot, id);
        //}
    }
}