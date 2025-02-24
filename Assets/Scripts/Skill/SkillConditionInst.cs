using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    static class SkillConditionUtil
    {
        public static SkillConditionInst Create(Character parent, SkillCondition condition, float conditionValue)
        {
            SkillConditionInst inst = null;

            switch(condition)
            {
                case SkillCondition.HpRateOver:
                    inst = new Condition_HpRateOver();
                    break;
                case SkillCondition.TargetHpRateUnder:
                    inst = new Condition_TargetHpRateUnder();
                    break;
                case SkillCondition.KillMonster:
                    inst = new Condition_KillMonster();
                    break;
                case SkillCondition.AttackBoss:
                    inst = new Condition_AttackBoss();
                    break;
                default:
                    break;
            }

            inst?.Init(parent, conditionValue);

            return inst;
        }

    }

    class SkillConditionInst
    {
        protected Character mParent;
        protected ObscuredFloat mConditionValue;

        public virtual void Init(Character parent, float conditionValue)
        {
            mParent = parent;
            mConditionValue = conditionValue;
        }

        public virtual bool Evaluate(DamageInst inst = null)
        {
            return true;
        }

        public void RandomizeKey()
        {
            mConditionValue.RandomizeCryptoKey();
        }
    }

    class Condition_KillMonster : SkillConditionInst { }

    class Condition_TargetHpRateUnder : SkillConditionInst
    {
        public override bool Evaluate(DamageInst inst = null)
        {
            if (inst != null)
            {
                if (inst.Attacker == mParent)
                {
                    Character target = inst.Defender;

                    return target?.Stat.HpRate <= mConditionValue;
                }
            }

            return false;
        }
    }

    class Condition_HpRateOver : SkillConditionInst
    {
        public override bool Evaluate(DamageInst inst = null)
        {
            return mParent.Stat.HpRate >= mConditionValue;
        }
    }

    class Condition_AttackBoss : SkillConditionInst
    {
        public override bool Evaluate(DamageInst inst = null)
        {
            if (inst == null)
                return false;

            var defender = inst.Defender;

            return defender is Boss;
        }
    }
}