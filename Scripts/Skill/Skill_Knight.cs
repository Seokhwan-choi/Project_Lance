using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Skill_Knight : MonoBehaviour
    {
        Skill_9 mParent;
        Animator mAnim;
        public void Init(Skill_9 parent)
        {
            mParent = parent;

            mAnim = GetComponent<Animator>();
        }

        public void OnRelease()
        {
            mAnim = null;
        }

        public void PlayReady()
        {
            mAnim.SetTrigger("Ready");
        }

        public void PlayDash()
        {
            mAnim.SetTrigger("Dash");

            Lance.CameraManager.Shake(duration: 0.25f);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (mParent?.Player.IsDeath ?? true || Lance.GameManager.StageManager.IsPlay == false)
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

                    mParent.PlayFX();
                }
            }
        }
    }
}
