using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_Pet_Grass : PetSkill
    {
        protected override void OnUpdate(float dt)
        {
            mDelay -= dt;

            if (mDelay <= 0)
            {
                OnAttack();

                mDelay = mSkillData?.atkDelay ?? 0;
            }
        }

        public override void OnAttack()
        {
            if (mPlayerPet.Parent.IsDeath || Lance.GameManager.StageManager.IsPlay == false)
                return;

            string id = mSkillData.id;

            int targetCount = mSkillData.targetCount;
            if (targetCount > 0)
            {
                // PC를 기준으로 스킬 사정거리내 몬스터를 모두 찾는다.
                var opponents = Lance.GameManager.StageManager.GatherInAttackRangeOpponents(mPlayerPet.Parent, mSkillData.atkRange);
                if (opponents != null)
                {
                    int monsterDeath = 0;
                    int bossDeath = 0;
                    foreach (var opponent in opponents)
                    {
                        var damage = CalcSkillDamage(opponent);
                        if (damage != null)
                        {
                            opponent.OnDamage(damage);

                            Lance.Lobby.StackDamage(id, damage.Damage);

                            PlayAttackSound();

                            Lance.ParticleManager.Aquire("Skill_Pet_Grass_Hit", opponent.transform, Vector2.one * opponent.GetBounds().center * 0.25f);

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
    }
}