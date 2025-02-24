using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class Skill : MonoBehaviour 
    {
        protected ObscuredBool mIsFinish;
        protected ObscuredFloat mDelay;
        protected Animation mAnim;
        protected Player mPlayer;
        protected SkillData mSkillData;
        protected List<AudioSource> mSounds;

        public string Id => mSkillData.id;
        public bool IsFinish => mIsFinish;
        public Player Player => mPlayer;
        public virtual void Init(Player player, SkillData skillData) 
        {
            mPlayer = player;
            mSkillData = skillData;
            mIsFinish = false;
            mDelay = 0;
            mSounds = new List<AudioSource>();
            mAnim = gameObject.GetComponent<Animation>();

            PlayAnim();

            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = SaveBitFlags.SkillEffect.IsOn();
            }
        }

        public virtual void RandomizeKey()
        {
            mIsFinish.RandomizeCryptoKey();
            mDelay.RandomizeCryptoKey();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            OnUpdate(dt);
        }

        private void FixedUpdate()
        {
            OnFixedUpdate();
        }

        protected void PlayAnim()
        {
            if (mAnim != null)
            {
                var state = mAnim[$"{mSkillData.skillFX}"];

                state.time = 0;
                mAnim.clip = state.clip;
                mAnim.Play();
            }
        }

        protected virtual void OnUpdate(float dt)
        {
            if (mDelay > 0)
            {
                mDelay -= dt;
            }
            else
            {
                mDelay = mSkillData.atkDelay;
            }

            var removeSounds = new List<AudioSource>();

            foreach (var sound in mSounds)
            {
                if (sound.isPlaying == false)
                {
                    removeSounds.Add(sound);
                }
            }

            foreach(var remove in removeSounds)
            {
                mSounds.Remove(remove);
            }

            removeSounds = null;
        }

        protected virtual void OnFixedUpdate() { }
        public virtual void OnReady() 
        {
            var sound = SoundPlayer.PlaySkillReady(mSkillData.id);
            if (sound != null)
                mSounds.Add(sound);
        }

        public void PlayAttackSound()
        {
            var sound = SoundPlayer.PlaySkillAttack(mSkillData.id);
            if (sound != null)
                mSounds.Add(sound);
        }
        public virtual void OnAttack() 
        {
            if (mPlayer.IsDeath || Lance.GameManager.StageManager.IsPlay == false)
                return;

            PlayAttackSound();

            string id = mSkillData.id;

            int targetCount = mSkillData.targetCount;
            if (targetCount > 0)
            {
                // PC를 기준으로 스킬 사정거리내 몬스터를 모두 찾는다.
                var opponents = Lance.GameManager.StageManager.GatherInAttackRangeOpponents(mPlayer, mSkillData.atkRange);
                if (opponents != null)
                {
                    int monsterDeath = 0;
                    int bossDeath = 0;
                    foreach (var opponent in opponents)
                    {
                        var damageInst = CalcSkillDamage(opponent);
                        if (damageInst != null && opponent != null)
                        {
                            opponent.OnDamage(damageInst);

                            Lance.Lobby.StackDamage(id, damageInst.Damage);

                            if (opponent.IsDeath)
                            {
                                if (opponent.IsBoss)
                                {
                                    bossDeath++;
                                }
                                else
                                {
                                    monsterDeath++;
                                }
                            }

                            targetCount--;

                            if (targetCount <= 0)
                                break;
                        }
                    }

                    if (monsterDeath > 0)
                    {
                        Lance.Account.ExpLevel.StackMonsterKillCount(monsterDeath);
                        Lance.GameManager.CheckQuest(QuestType.KillMonster, monsterDeath);
                    }
                        
                    if (bossDeath > 0)
                    {
                        Lance.Account.ExpLevel.StackBossKillCount(bossDeath);
                        Lance.GameManager.CheckQuest(QuestType.KillBoss, bossDeath);
                    }

                    opponents = null;
                }
            }
            
        }
        public virtual void OnFinish() 
        {
            mIsFinish = true;

            if (mSkillData.soundReleaseImmediately)
            {
                foreach (var sound in mSounds)
                {
                    sound.Stop();
                }

                mSounds.Clear();
            }
        }

        public virtual void OnRelease()
        {
            mAnim.Stop();
            mAnim = null;

            foreach (var sound in mSounds)
            {
                sound.Stop();
            }
            mSounds = null;

            mPlayer = null;
            mSkillData = null;

            Lance.ObjectPool.ReleaseObject(gameObject);
        }

        public DamageInst CalcSkillDamage(Character target)
        {
            if (mSkillData != null)
            {
                double skillValue = Lance.Account.SkillInventory.GetSkillValue(mSkillData.type, mSkillData.id);

                return DamageCalculator.Create(mPlayer, target, skillValue);
            }
            else
            {
                return null;
            }
        }
    }
}