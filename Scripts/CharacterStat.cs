using System;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class CharacterStat
    {
        Character mParent;

        ObscuredInt mLevel;
        ObscuredInt mElementalType;
        ObscuredDouble mAtk;
        ObscuredDouble mAtkRatio;
        ObscuredFloat mAtkSpeed;
        ObscuredFloat mAtkRange;
        ObscuredDouble mAmplifyAtk;
        ObscuredDouble mBonusAtk;

        Shield mShield;
        ObscuredDouble mMaxHp;
        ObscuredDouble mCurHp;
        ObscuredDouble mHpRatio;
        ObscuredDouble mAmplifyHp;

        ObscuredDouble mCriDmg;
        ObscuredFloat mCriProb;
        ObscuredDouble mSuperCriDmg;
        ObscuredFloat mSuperCriProb;

        ObscuredFloat mMoveSpeed;
        ObscuredFloat mMoveSpeedRatio;

        ObscuredDouble mSkillDmg;
        ObscuredDouble mBossDmg;
        ObscuredDouble mMonsterDmg;
        ObscuredDouble mAddDmg;
        ObscuredDouble mExpIncreaseAmount;
        ObscuredDouble mGoldIncreaseAmount;

        ObscuredDouble mFireAddDmg;
        ObscuredDouble mWaterAddDmg;
        ObscuredDouble mGrassAddDmg;
        ObscuredDouble mManaSensitivity;

        ObscuredDouble mPowerLevel;
        public ObscuredBool IsDeath => mCurHp <= 0;
        public ObscuredDouble Atk => mAtk;
        public ObscuredDouble AtkRatio => mAtkRatio;
        public ObscuredDouble AmplifyAtk => mAmplifyAtk;
        public ObscuredDouble BonusAtk => mBonusAtk;
        public ObscuredDouble MaxHp => mMaxHp;
        public ObscuredDouble CurHp => mCurHp;
        public ObscuredDouble HpRate => mCurHp / mMaxHp;
        public ObscuredDouble AmplifyHp => mAmplifyHp;
        public ObscuredDouble CriDmg => mCriDmg;
        public ObscuredFloat CriProb => mCriProb;
        public ObscuredDouble SuperCriDmg => mSuperCriDmg;
        public ObscuredFloat SuperCriProb => mSuperCriProb;
        public ObscuredFloat MoveSpeed => mMoveSpeed;
        public ObscuredFloat MoveSpeedRatio => mMoveSpeedRatio;
        public ObscuredFloat AtkSpeed => mAtkSpeed;
        public ObscuredFloat AtkRange => mAtkRange;
        public ObscuredDouble BossDmg => mBossDmg;
        public ObscuredDouble MonsterDmg => mMonsterDmg;
        public ObscuredDouble AddDmg => mAddDmg;
        public ObscuredDouble FireAddDmg => mFireAddDmg;
        public ObscuredDouble WaterAddDmg => mWaterAddDmg;
        public ObscuredDouble GrassAddDmg => mGrassAddDmg;
        public ObscuredDouble IncreaseGoldAmount => mGoldIncreaseAmount;
        public ObscuredDouble IncreaseExpAmount => mExpIncreaseAmount;
        public ObscuredInt Level => mLevel;
        public ObscuredDouble SkillDmg => mSkillDmg;
        public ObscuredDouble ManaSensitivity => mManaSensitivity;
        public ObscuredDouble PowerLevel => mPowerLevel;
        public ElementalType ElementalType => (ElementalType)(int)mElementalType;

        public void RandomizeKey()
        {
            mElementalType.RandomizeCryptoKey();
            mLevel.RandomizeCryptoKey();

            mAtk.RandomizeCryptoKey();
            mAtkRatio.RandomizeCryptoKey();
            mAtkSpeed.RandomizeCryptoKey();
            mAtkRange.RandomizeCryptoKey();

            mShield?.RandomizeKey();
            mMaxHp.RandomizeCryptoKey();
            mCurHp.RandomizeCryptoKey();
            mHpRatio.RandomizeCryptoKey();

            mCriDmg.RandomizeCryptoKey();
            mCriProb.RandomizeCryptoKey();

            mMoveSpeedRatio.RandomizeCryptoKey();
            mMoveSpeed.RandomizeCryptoKey();

            mSkillDmg.RandomizeCryptoKey();
            mBossDmg.RandomizeCryptoKey();
            mMonsterDmg.RandomizeCryptoKey();
            mAddDmg.RandomizeCryptoKey();
            mExpIncreaseAmount.RandomizeCryptoKey();
            mGoldIncreaseAmount.RandomizeCryptoKey();

            mSuperCriDmg.RandomizeCryptoKey();
            mSuperCriProb.RandomizeCryptoKey();
            mFireAddDmg.RandomizeCryptoKey();
            mWaterAddDmg.RandomizeCryptoKey();
            mGrassAddDmg.RandomizeCryptoKey();

            mManaSensitivity.RandomizeCryptoKey();
            mPowerLevel.RandomizeCryptoKey();
            mBonusAtk.RandomizeCryptoKey();
        }

        public void InitPlayer(Player player)
        {
            mParent = player;

            UpdatePlayerStat();
        }

        public void UpdatePlayerStat()
        {
            Player player = mParent as Player;

            mElementalType = (int)Lance.Account.GetElementalType();
            mAtk = Lance.Account.GatherStat(StatType.Atk);
            mAtk += Lance.Account.GatherStat(StatType.PowerAtk);
            mAtkRatio = Lance.Account.GatherStat(StatType.AtkRatio) + 
                player.SkillManager?.GatherPassiveSkillValue(StatType.AtkRatio) ?? 0;
            mAmplifyAtk = Lance.Account.GatherStat(StatType.AmplifyAtk);
            mAtkSpeed = (float)Lance.Account.GatherStat(StatType.AtkSpeed)
                * (1 + (float)player.SkillManager?.GatherPassiveSkillValue(StatType.AtkSpeedRatio)
                + (float)Lance.Account.GatherStat(StatType.AtkSpeedRatio));
            mAtkRange = (float)Lance.Account.GatherStat(StatType.AtkRange);

            ObscuredDouble newHp = Lance.Account.GatherStat(StatType.Hp);
            ObscuredDouble newPowerHp = Lance.Account.GatherStat(StatType.PowerHp);
            ObscuredDouble newTotalHp = newHp + newPowerHp;
            mHpRatio = Lance.Account.GatherStat(StatType.HpRatio);
            double skillHpValue = player.SkillManager?.GatherPassiveSkillValue(StatType.HpRatio) ?? 0;
            mAmplifyHp = Lance.Account.GatherStat(StatType.AmplifyHp);

            newTotalHp *= (1 + mHpRatio + skillHpValue);
            newTotalHp *= (1 + mAmplifyHp);

            if (mMaxHp > 0)
            {
                if (newTotalHp > mMaxHp)
                {
                    mCurHp += (newTotalHp - mMaxHp);
                }

                mMaxHp = newTotalHp;
            }
            else
            {
                mMaxHp = mCurHp = newTotalHp;
            }

            // 최대 체력이 모두 확정된 뒤
            // 공격력이 달라질 수 있다.
            mBonusAtk += player.GatherPetPassiveSkillValue(StatType.Atk);

            mCriProb = (float)Lance.Account.GatherStat(StatType.CriProb) + (float)player.SkillManager.GatherPassiveSkillValue(StatType.CriProb);
            mCriDmg = Lance.Account.GatherStat(StatType.CriDmg);

            // 크리티컬 확률이 모두 확정된 뒤
            // 크리티컬 데미지가 변경될 수 있다.
            mCriDmg += player.GatherPetPassiveSkillValue(StatType.CriDmg);

            mSuperCriProb = (float)Lance.Account.GatherStat(StatType.SuperCriProb);
            mSuperCriDmg = Lance.Account.GatherStat(StatType.SuperCriDmg);
            mMoveSpeedRatio = (float)player.SkillManager?.GatherPassiveSkillValue(StatType.MoveSpeedRatio)
                + (float)Lance.Account.GatherStat(StatType.MoveSpeedRatio);
            mMoveSpeed = (float)Lance.Account.GatherStat(StatType.MoveSpeed) * (1 + mMoveSpeedRatio);

            mSkillDmg = Lance.Account.GatherStat(StatType.SkillDmg) + player.GatherPetPassiveSkillValue(StatType.SkillDmg);
            mBossDmg = (float)Lance.Account.GatherStat(StatType.BossDmg);
            mMonsterDmg = (float)Lance.Account.GatherStat(StatType.MonsterDmg);
            mAddDmg = Lance.Account.GatherStat(StatType.AddDmg);
            mFireAddDmg = Lance.Account.GatherStat(StatType.FireAddDmg);
            mWaterAddDmg = Lance.Account.GatherStat(StatType.WaterAddDmg);
            mGrassAddDmg = Lance.Account.GatherStat(StatType.GrassAddDmg);
            mExpIncreaseAmount = (float)Lance.Account.GatherStat(StatType.ExpAmount);
            mGoldIncreaseAmount = (float)Lance.Account.GatherStat(StatType.GoldAmount);

            mManaSensitivity = Lance.Account.GatherStat(StatType.ManaSensitivity);

            mParent.Anim.SetAtkSpeed(mAtkSpeed);

            // 태초의 정수가 활성화 되어 있으면 FX 바로 붙여주자
            if (Lance.Account.Essence.IsActiveCentralEssence())
            {
                player.PlayFX($"{EssenceType.Central}", Lance.GameData.EssenceCommonData.centralEssenceFx);
            }
            else
            {
                player.ReleaseFX($"{EssenceType.Central}");
            }

            double powerLevel = ((CalcAtk() + (mMaxHp * 0.5f)) * (mAtkSpeed)
                * (1 + Mathf.Min(1f, mCriProb)) * mCriDmg)
                * (1 + Mathf.Min(1f, mSuperCriProb) * mSuperCriDmg)
                * (1 + mSkillDmg + Lance.Account.SkillInventory.GetTotalEquippedSkillValues())
                * (1 + mBossDmg + mMonsterDmg)
                * (1 + mAddDmg) * (1 + mFireAddDmg + mWaterAddDmg + mGrassAddDmg);

            // 마나 감응도가 있다면 계산해주자
            if (mManaSensitivity > 0)
            {
                double manaSensitivityValue = (1.05 + mManaSensitivity) / 222;

                powerLevel = Math.Max(0, powerLevel * (1 + manaSensitivityValue));
            }

            mPowerLevel = powerLevel;

            double CalcAtk()
            {
                return (mAtk * (1 + mAtkRatio) * (1 + mAmplifyAtk) + mBonusAtk);
            }
        }

        public void InitMonster(Character parent, MonsterData data, int level)
        {
            mParent = parent;

            MonsterStatData statData = DataUtil.GetMonsterStatData(data, level);

            mLevel = level;
            mElementalType = data.type == MonsterType.raidBoss ? (int)data.elementalType : (int)ElementalType.Normal;
            mAtk = statData.atk;
            mMaxHp = mCurHp = statData.hp;
            mAtkRange = data.atkRange;
            mAtkSpeed = data.atkSpeed;
        }

        public void RefillHp()
        {
            mCurHp = mMaxHp;
        }

        public void OnRelease()
        {
            mShield?.OnRelease();
        }

        public void PowerUpRageModeRaidBoss(float value)
        {
            if (mParent.IsPlayer)
                return;

            if (mParent is RaidBoss)
            {
                mAtk += mAtk * value;
                mAtkSpeed = Mathf.Min(2f, mAtkSpeed + 1f);

                mParent.Anim.SetAtkSpeed(mAtkSpeed);
            }
        }

        public void SetShield(ShieldData data)
        {
            if (mShield == null)
                mShield = new Shield(mParent);
            else
                mShield.OnRelease();

            mShield.SetShiled(data);
        }

        public bool HaveShield()
        {
            return mShield?.GetShieldHp() > 0;
        }

        public void OnDamage(double damage)
        {
            if (mCurHp <= 0)
            {
                return;
            }
            else
            {
                if (HaveShield())
                {
                    mShield.OnDamge(damage);
                }
                else
                {
                    mCurHp -= damage;
                    mCurHp = Math.Max(0, mCurHp);
                }
            }
        }

        public void OnDamageShiled(double damage)
        {
            mShield.OnDamge(damage);
        }

        public void AddHp(double hp)
        {
            mCurHp = Math.Min(mCurHp + hp, mMaxHp);
        }

        public void SetMoveSpeed(float moveSpeed, float moveSpeedRatio)
        {
            mMoveSpeed = moveSpeed;
            mMoveSpeedRatio = moveSpeedRatio;
        }
    }

    class Shield
    {
        Character mParent;
        ObscuredDouble mShieldHp;
        GameObject mShieldFX;

        public Shield(Character parent)
        {
            mParent = parent;
        }

        public double GetShieldHp()
        {
            return mShieldHp;
        }

        public void SetShiled(ShieldData data)
        {
            if (data.value <= 0)
                return;

            mShieldHp = data.value;

            // 쉴드 fx 생성
            mShieldFX = Lance.ObjectPool.AcquireObject($"FX/{data.fx}", mParent.transform);

            foreach (var ps in mShieldFX.GetComponentsInChildren<ParticleSystem>())
            {
                ps.time = 0f;
                ps.Play();
            }
        }

        public void OnDamge(double damage)
        {
            if (damage <= 0)
                return;

            mShieldHp -= 1;
            if (mShieldHp <= 0)
            {
                BreakShield();
            }
        }

        public void OnRelease()
        {
            if (mShieldFX != null)
            {
                foreach (var ps in mShieldFX.GetComponentsInChildren<ParticleSystem>())
                {
                    ps.Stop();
                }

                Lance.ObjectPool.ReleaseObject(mShieldFX);

                mShieldFX = null;
            }
        }

        void BreakShield()
        {
            OnRelease();
        }

        public void RandomizeKey()
        {
            mShieldHp.RandomizeCryptoKey();
        }
    }
}