using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_ThrowingLance : MonoBehaviour
    {
        Skill_3 mParent;
        public void Init(Skill_3 parent)
        {
            mParent = parent;
        }

        public void OnRelease()
        {
            mParent = null;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (mParent == null)
                return;

            if (mParent?.Player.IsDeath ?? true || Lance.GameManager.StageManager.IsPlay == false)
                return;

            string id = mParent.Id;

            var monster = col.gameObject.GetComponent<Monster>();
            if (monster != null && monster.IsAlive)
            {
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
