using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Action_SkillCast : ActionInst
    {
        SkillData mSkillData;
        public Action_SkillCast(SkillData skillData)
        {
            mSkillData = skillData;
        }

        public override void OnStart(Character parent)
        {
            base.OnStart(parent);

            var result = Lance.GameManager.StageManager.AnyInAttackRangeOpponent(parent, mSkillData.atkRange);
            if (result.any)
            {
                mDuration = mParent.Anim.PlaySkillCast();

                CastSkill();
            }
            else
            {
                mDuration = 0f;
            }
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            mDuration -= dt;
        }

        void CastSkill()
        {
            if (mSkillData == null)
                return;

            if (mSkillData.type != SkillType.Active)
                return;

            if (mParent is Player)
            {
                Player player = mParent as Player;

                if (player.SkillManager.CastSkill(mSkillData.id))
                {
                    // 실제로 스킬이 발동되자
                    GameObject skillObj = Lance.ObjectPool.AcquireObject($"Skill/{mSkillData.skillFX}", mParent.transform);

                    Skill skill = skillObj.GetComponent<Skill>();
                    skill.Init(player, mSkillData);

                    player.SkillManager.AddProcessingSkill(skill);

                    Lance.GameManager.CheckQuest(QuestType.CastSkill, 1);

                    Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.CastSkill);
                }
            }
        }
    }
}

