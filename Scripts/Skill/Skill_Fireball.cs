using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_Fireball : MonoBehaviour
    {
        PetSkill mParent;
        float mDelay;
        public void Init(PetSkill parent)
        {
            mParent = parent;

            mDelay = 0f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (mDelay > 0)
            {
                mDelay -= dt;
            }
            else
            {
                mDelay = mParent.AtkDelay;
            }
        }

        private void OnTriggerStay2D(Collider2D col)
        {
            if (mDelay > 0)
                return;

            var monster = col.gameObject.GetComponent<Monster>();
            if (monster != null && monster.IsAlive)
            {
                string id = mParent.Id;

                var damageInst = mParent.CalcSkillDamage(monster);
                if (damageInst != null)
                {
                    monster.OnDamage(damageInst);

                    Lance.Lobby.StackDamage(id, damageInst.Damage);

                    if (monster.IsDeath)
                    {
                        if (monster.IsBoss)
                        {
                            Lance.Account.ExpLevel.StackBossKillCount(1);
                            Lance.GameManager.CheckQuest(QuestType.KillBoss, 1);
                        }
                        else
                        {
                            Lance.Account.ExpLevel.StackMonsterKillCount(1);
                            Lance.GameManager.CheckQuest(QuestType.KillMonster, 1);
                        }
                    }
                }
            }
        }
    }
}
