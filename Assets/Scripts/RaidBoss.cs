using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Lance
{
    class RaidBoss : Boss
    {
        public override void OnSpawn()
        {
            base.OnSpawn();

            SoundPlayer.PlayRage(mStat.ElementalType);

            Lance.CameraManager.Shake(1f, 0.5f);
        }

        public override void OnDamage(DamageInst inst)
        {
            if (IsDeath)
            {
                inst.OnRelease();
                inst = null;

                return;
            }

            if (mActionManager.CurrAction is Action_Rage rage)
            {
                if (rage.Invincibility)
                    return;
            }

            // 직접적으로 체력이 달지는 않는다.
            // 레이드 보스는 죽지 않는다.
            UIUtil.ShowDamageText(this, inst.Damage, inst.IsCritical, inst.IsSuperCritical);

            if (mStat.HaveShield())
            {
                mStat.OnDamageShiled(inst.Damage);

                SoundPlayer.PlayHitShield();
            }
            else
            {
                mAnim.PlayHit();

                SoundPlayer.PlayHit(inst.Attacker, inst.Defender);

                Lance.GameManager.OnBossDamage(inst.Damage);
            }

            inst.OnRelease();
            inst = null;
        }

        public override void OnDeath()
        {
            // 레이드 보스는 죽지 않는다.
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mActionManager.RandomizeKey();
        }

        public override bool AnyInAttackRangeOpponent()
        {
            // 일단은 기본 공격에 대해서만 체크
            var result = Lance.GameManager.StageManager.AnyInAttackRangeOpponent(this, mStat.AtkRange);
            return result.any;
        }

        public override float GetBodySize()
        {
            return mData.bodySize;
        }
    }
}