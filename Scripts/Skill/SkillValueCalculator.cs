using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    static class SkillValueCalculatorUtil
    {
        public static SkillValueCalculatorInst Create(Character parent, SkillValueCalcType valueCalcType, float valueCalculatorValue)
        {
            SkillValueCalculatorInst inst = null;

            switch (valueCalcType)
            {
                case SkillValueCalcType.CriProbOverToCriDmg:
                    inst = new ValueCalculator_CriProbOverToCriDmg();
                    break;
                case SkillValueCalcType.HpToAtk:
                    inst = new ValueCalculator_HpToAtk();
                    break;
                case SkillValueCalcType.AtkSpeedOverToSkillDmg:
                    inst = new ValueCalculator_AtkSpeedOverToSkillDmg();
                    break;
                default:
                    break;
            }

            inst?.Init(parent, valueCalculatorValue);

            return inst;
        }

    }

    class SkillValueCalculatorInst
    {
        protected Character mParent;
        protected ObscuredFloat mValueCalculatorValue;

        public virtual void Init(Character parent, float valueCalculatorValue)
        {
            mParent = parent;

            mValueCalculatorValue = valueCalculatorValue;
        }

        public virtual double Calc(DamageInst inst = null)
        {
            return mValueCalculatorValue;
        }

        public void RandomizeKey()
        {
            mValueCalculatorValue.RandomizeCryptoKey();
        }
    }

    class ValueCalculator_CriProbOverToCriDmg : SkillValueCalculatorInst
    {
        public override double Calc(DamageInst inst = null)
        {
            float overCriProb = mParent.Stat.CriProb - 1f;

            if (overCriProb > 0)
            {
                // 0.1%당 치환
                double overCount = overCriProb / 0.01;

                return overCount * mValueCalculatorValue;
            }
            else
            {
                return 0;
            }
        }
    }

    class ValueCalculator_HpToAtk : SkillValueCalculatorInst
    {
        public override double Calc(DamageInst inst = null)
        {
            return mParent.Stat.MaxHp * mValueCalculatorValue;
        }
    }

    class ValueCalculator_AtkSpeedOverToSkillDmg : SkillValueCalculatorInst
    {
        public override double Calc(DamageInst inst = null)
        {
            float overAtkSpeed = mParent.Stat.AtkSpeed - Lance.GameData.PlayerStatMaxData.atkSpeedMax;

            if (overAtkSpeed > 0)
            {
                // 0.01당 치환
                double overCount = overAtkSpeed / 0.01;

                return overCount * mValueCalculatorValue;
            }
            else
            {
                return 0;
            }
        }
    }
}