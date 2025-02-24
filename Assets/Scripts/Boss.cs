using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Lance
{
    class Boss : Monster
    {
        public virtual void OnSpawn()
        {
            if (mSpriteLibraryAssetData.sneer)
            {
                mAnim.PlaySneer();
            }
        }

        public virtual void OnSpawnFinish()
        {

        }

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

            // �������� �Ծ �׾��ٸ� ���� �Ŵ������� �˷�����
            if (mStat.IsDeath)
            {
                OnDeath();
            }

            inst.OnRelease();
            inst = null;
        }

        public override void OnDeath()
        {
            mAnim.PlayDeath();

            // ���ͷ� ���� ���� �� �ִ� ��ȭ�� ȹ��
            if (mRewardResult.IsEmpty() == false)
            {
                var player = Lance.GameManager.StageManager.Player;

                mRewardResult.exp = mRewardResult.exp * (1 + player.Stat.IncreaseExpAmount) * Lance.GameData.CommonData.bossRewardValue;
                mRewardResult.gold = mRewardResult.gold * (1 + player.Stat.IncreaseGoldAmount) * Lance.GameData.CommonData.bossRewardValue;

                Lance.GameManager.GiveReward(mRewardResult, ShowRewardType.Banner, ignoreUpdatePlayerStat: true);
            }

            Lance.GameManager.OnMonsterDeath(mRewardResult, mStat.Level);

            Lance.GameManager.CheckBountyQuest(mData.type, mData.id, 1);
        }

        public override bool AnyInAttackRangeOpponent()
        {
            // ���� �������� ��ų�� ���ؼ��� Ȯ���ؾ� ��

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