using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.U2D.Animation;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using DigitalRuby.AdvancedPolygonCollider;

namespace Lance
{
    class Monster : Character
    {
        protected RewardResult mRewardResult;
        protected MonsterData mData;
        public MonsterData Data => mData;
        public RewardResult MonsterReward;

        public void SetMonsterInfo(MonsterData data, StageType stageType, StageDifficulty diff, int chapter, int stage, int level, string reward, float rewardBonusValue = 1f)
        {
            mData = data;

            mStat = new CharacterStat();
            mStat.InitMonster(this, data, level);

            if (reward.IsValid())
            {
                MonsterRewardData rewardData = DataUtil.GetMonsterRewardData(stageType, reward);
                if (rewardData != null)
                {
                    mRewardResult = DataUtil.GetMonsterReward(diff, stageType, chapter, stage, rewardData, rewardBonusValue);

                    if (diff >= Lance.GameData.StageCommonData.essenceDropDiff)
                    {
                        if (Util.Dice(Lance.GameData.StageCommonData.essenceDropProb))
                        {
                            int minAmount = Lance.GameData.StageCommonData.essenceMinDropAmount;
                            int maxAmount = Lance.GameData.StageCommonData.essenceMaxDropAmount;

                            int randAmount = minAmount == maxAmount ? minAmount : UnityEngine.Random.Range(minAmount, maxAmount + 1);

                            mRewardResult.essences = mRewardResult.essences.AddEssence(DataUtil.ChangeToEssenceType(chapter), randAmount);
                        }
                    }
                }
            }

            mAnim = new MonsterAnim();
            mAnim.Init(this);

            mPhysics = new CharacterPhysics();
            mPhysics.Init(this);

            mActionManager = new ActionManager();
            mActionManager.Init(this);

            mSpriteLibraryAssetData = Lance.GameData.LibraryAssetData.TryGet(mData.libraryAsset);

            mAnim.SetBodyLibraryAsset(mSpriteLibraryAssetData.libraryAsset);
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

            mHpGaugebarUI.UpdateGaugebar();

            SoundPlayer.PlayHit(inst.Attacker, inst.Defender);

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
            mHpGaugebarUI?.OnDeath();

            mAnim.PlayDeath();

            // �÷��̾� ��ų ����
            var player = Lance.GameManager.StageManager.Player;

            player.SkillManager.ApplyPassive(SkillCondition.KillMonster);

            // ���ͷ� ���� ���� �� �ִ� ��ȭ�� ȹ��
            if (mRewardResult.IsEmpty() == false)
            {
                mRewardResult.exp = mRewardResult.exp * (1 + player.Stat.IncreaseExpAmount);
                mRewardResult.gold = mRewardResult.gold * (1 + player.Stat.IncreaseGoldAmount);

                Lance.GameManager.GiveReward(mRewardResult, ShowRewardType.Banner, ignoreUpdatePlayerStat: true);
            }

            Lance.GameManager.OnMonsterDeath(mRewardResult, mStat.Level);

            Lance.GameManager.CheckBountyQuest(mData.type, mData.id, 1);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            var advancedPolygon = GetComponent<AdvancedPolygonCollider>();
            advancedPolygon?.ResetRecalculateCount();

            mHpGaugebarUI?.OnRelease();

            mRewardResult = new RewardResult();
        }

        public override float GetBodySize()
        {
            return mData.bodySize;
        }
    }
}