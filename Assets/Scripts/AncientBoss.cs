using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class AncientBoss : Boss
    {
        public override void OnDamage(DamageInst inst)
        {
            if (IsDeath)
            {
                inst.OnRelease();
                inst = null;

                return;
            }

            // 데미지 입음
            mStat.OnDamage(inst.Damage);

            UIUtil.ShowDamageText(this, inst.Damage, inst.IsCritical, inst.IsSuperCritical);

            mAnim.PlayHit();

            SoundPlayer.PlayHit(inst.Attacker, inst.Defender);

            Lance.GameManager.OnBossDamage(inst.Damage);

            if (IsDeath)
            {
                // 최고 단계인지 확인
                // 최고 단계라면 몬스터 완전 사망처리
                if (Lance.GameManager.IsDungeonMaxStep())
                {
                    OnDeath();
                }
                else // 다음 단계로 바뀌자
                {
                    // 몬스터로 부터 얻을 수 있는 재화를 획득
                    if (mRewardResult.IsEmpty() == false)
                    {
                        var player = Lance.GameManager.StageManager.Player;

                        mRewardResult.exp = mRewardResult.exp * (1 + player.Stat.IncreaseExpAmount) * Lance.GameData.CommonData.bossRewardValue;
                        mRewardResult.gold = mRewardResult.gold * (1 + player.Stat.IncreaseGoldAmount) * Lance.GameData.CommonData.bossRewardValue;

                        Lance.GameManager.GiveReward(mRewardResult, ShowRewardType.Banner, ignoreUpdatePlayerStat: true);
                    }

                    Lance.GameManager.OnAncientBossDeath(mRewardResult);
                }
            }

            inst.OnRelease();
            inst = null;
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