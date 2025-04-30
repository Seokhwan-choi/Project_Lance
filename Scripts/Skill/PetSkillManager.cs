using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Linq;

namespace Lance
{
    class PetSkillManager
    {
        PlayerPet mPlayerPet;
        Player mParent;
        List<PetSkill> mProcessingSkills;
        PetSkillInfo mPetActiveSkillInfo;
        PetSkillInfo mPetPassiveSkillInfo;
        Dictionary<string, GameObject> mFXList;
        public void Init(PlayerPet playerPet, Player parent)
        {
            mPlayerPet = playerPet;
            mProcessingSkills = new List<PetSkill>();
            mFXList = new Dictionary<string, GameObject>();
            mParent = parent;

            RefreshSkillInfo();
        }

        public void RandomizeKey()
        {
            foreach (var skill in mProcessingSkills)
            {
                if (skill.IsFinish == false)
                    skill.RandomizeKey();
            }

            mPetActiveSkillInfo?.RandomizeKey();
            mPetPassiveSkillInfo?.RandomizeKey();
        }

        public void OnChangePet()
        {
            OnRelease();

            RefreshSkillInfo();
        }

        public void RefreshSkillInfo()
        {
            mPetActiveSkillInfo = null;

            PetInst petInst = Lance.Account.Pet.GetEquippedInst();

            var petActiveSkill = petInst?.GetActiveSkill() ?? string.Empty;
            if (petActiveSkill.IsValid())
            {
                PetSkillInfo skillInfo = new PetSkillInfo();

                skillInfo.Init(this, mParent, petActiveSkill);

                mPetActiveSkillInfo = skillInfo;
            }

            mPetPassiveSkillInfo = null;

            var petPassiveSkill = petInst?.GetPassiveSkill() ?? string.Empty;
            if (petPassiveSkill.IsValid())
            {
                PetSkillInfo skillInfo = new PetSkillInfo();

                skillInfo.Init(this, mParent, petPassiveSkill);

                mPetPassiveSkillInfo = skillInfo;
            }
        }

        public void ResetCoolTimes()
        {
            mPetActiveSkillInfo?.ResetCoolTime();
        }

        public void OnRelease()
        {
            foreach (var skill in mProcessingSkills)
            {
                skill.OnRelease();
            }

            mProcessingSkills.Clear();

            foreach (var skillFX in mFXList.Values)
            {
                foreach (var ps in skillFX.GetComponentsInChildren<ParticleSystem>())
                {
                    ps.Stop();
                }

                Lance.ObjectPool.ReleaseObject(skillFX);
            }

            mFXList.Clear();
        }

        public void OnUpdate(float dt)
        {
            var removeSkillList = new List<PetSkill>();

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

            if (mPetActiveSkillInfo != null)
            {
                mPetActiveSkillInfo.OnUpdate(dt);

                // 스킬 자동 사용이면 예약
                if (SaveBitFlags.PetSkillAutoCastOff.IsOff())
                {
                    if (mPetActiveSkillInfo.CanCast())
                    {
                        CastSkill(mPetActiveSkillInfo.Id);
                    }
                }
            }
        }

        public void CastSkill(string id)
        {
            PetInst petInst = Lance.Account.Pet.GetEquippedInst();
            if (petInst == null)
                return;

            var petActiveSkill = petInst?.GetActiveSkill() ?? string.Empty;
            if (petActiveSkill != id)
                return;

            SkillData skillData = DataUtil.GetPetSkillData(id);
            if (skillData == null)
                return;

            var result = Lance.GameManager.StageManager.AnyInAttackRangeOpponent(mPlayerPet.Parent, skillData.atkRange);
            if (result.any == false)
                return;

            if (mPetActiveSkillInfo == null)
                return ;

            if (mPetActiveSkillInfo.CanCast() == false)
                return ;

            mPetActiveSkillInfo.Cast();

            // 실제로 스킬이 발동되자
            GameObject skillObj = Lance.ObjectPool.AcquireObject($"Skill/{skillData.skillFX}", mPlayerPet.Tm);

            PetSkill skill = skillObj.GetComponent<PetSkill>();

            skill.Init(mPlayerPet, skillData);

            mProcessingSkills.Add(skill);
        }

        public void ReleaseFX(string id)
        {
            var fxObj = mFXList.TryGet(id);
            if (fxObj != null)
            {
                foreach (var ps in fxObj.GetComponentsInChildren<ParticleSystem>())
                {
                    ps.Stop();
                }

                Lance.ObjectPool.ReleaseObject(fxObj);

                mFXList.Remove(id);
            }
        }

        public double GatherPassiveSkillValue(StatType type, DamageInst inst = null)
        {
            if (mPetPassiveSkillInfo != null)
            {
                if (mPetPassiveSkillInfo.PassiveType == type)
                    return mPetPassiveSkillInfo.GetSkillValue(inst);
            }

            return 0;
        }
    }
}