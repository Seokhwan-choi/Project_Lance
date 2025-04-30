using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Lance
{
    class AnimEventHandler : MonoBehaviour
    {
        Character mParent;
        Action mOnRage;
        Action mOnRageFinish;
        public void Init(Character parent)
        {
            mParent = parent;
        }

        public void SetRageAction(Action onRage)
        {
            mOnRage = null;
            mOnRage = onRage;
        }

        public void SetRageFinishAction(Action onRageFinish)
        {
            mOnRageFinish = null;
            mOnRageFinish = onRageFinish;
        }

        public void OnRage()
        {
            mOnRage?.Invoke();
        }

        public void OnRageFinish()
        {
            mOnRageFinish?.Invoke();
        }

        public void Damage()
        {
            // 범위 내의 적을 대상으로 데미지
            var targets = Lance.GameManager.StageManager.GatherInAttackRangeOpponents(mParent, mParent.Stat.AtkRange);
            if (targets == null || targets.Count() <= 0)
                return;

            bool anyCri = false;

            int bossDeath = 0;
            int monsterDeath = 0;
            foreach (var target in targets)
            {
                if (target != null && target.IsAlive)
                {
                    DamageInst inst = DamageCalculator.Create(mParent, target);

                    if (inst.IsCritical)
                        anyCri = true;

                    if (inst.Attacker is Player)
                        Lance.Lobby.StackDamage("NormalAtk", inst.Damage);

                    target.OnDamage(inst);

                    if (target.IsDeath && target.IsMonster)
                    {
                        if (target.IsBoss)
                        {
                            bossDeath++;
                        }
                        else
                        {
                            monsterDeath++;
                        }
                    }
                }
            }

            targets = null;

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
                

            if (mParent.IsPlayer)
            {
                if (anyCri)
                {
                    Lance.CameraManager.Shake(duration:0.125f, strength:0.0625f);
                }
            }
        }
    }
}