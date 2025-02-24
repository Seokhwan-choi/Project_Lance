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

            // ������ ����
            mStat.OnDamage(inst.Damage);

            UIUtil.ShowDamageText(this, inst.Damage, inst.IsCritical, inst.IsSuperCritical);

            mAnim.PlayHit();

            SoundPlayer.PlayHit(inst.Attacker, inst.Defender);

            Lance.GameManager.OnBossDamage(inst.Damage);

            if (IsDeath)
            {
                // �ְ� �ܰ����� Ȯ��
                // �ְ� �ܰ��� ���� ���� ���ó��
                if (Lance.GameManager.IsDungeonMaxStep())
                {
                    OnDeath();
                }
                else // ���� �ܰ�� �ٲ���
                {
                    // ���ͷ� ���� ���� �� �ִ� ��ȭ�� ȹ��
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
            // �ϴ��� �⺻ ���ݿ� ���ؼ��� üũ
            var result = Lance.GameManager.StageManager.AnyInAttackRangeOpponent(this, mStat.AtkRange);

            return result.any;
        }

        public override float GetBodySize()
        {
            return mData.bodySize;
        }
    }
}