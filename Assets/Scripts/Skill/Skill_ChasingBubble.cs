using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    // 타겟 추적 스킬
    class Skill_ChasingBubble : MonoBehaviour
    {
        const float Speed = 3f;
        const float RotateSpeed = 250f;

        ParticleSystem mParticleSystem;
        bool mIsHit;
        Skill_Pet_Water mParent;
        Monster mTarget;
        Rigidbody2D mRigid;
        BoxCollider2D mCollider;
        public void Init(Skill_Pet_Water parent)
        {
            mParent = parent;

            mParticleSystem = GetComponent<ParticleSystem>();
            mParticleSystem.Play();

            mCollider = GetComponent<BoxCollider2D>();
            mCollider.enabled = true;

            mRigid = GetComponent<Rigidbody2D>();
            mRigid.angularVelocity = 0;
            mRigid.linearVelocity = Vector2.zero;

            mTarget = null;
            mIsHit = false;
        }

        public void OnRelease()
        {
            mTarget = null;
            mRigid = null;
        }

        public void OnFixedUpdate()
        {
            if (mTarget == null || mTarget.IsDeath)
            {
                mTarget = Lance.GameManager.StageManager.GetFirstMonster();
            }

            Vector3 targetPos = GetTargetPos();

            Vector2 dir = (Vector2)targetPos - mRigid.position;

            dir.Normalize();

            if (mRigid.freezeRotation)
                mRigid.freezeRotation = false;

            float rotateAmount = Vector3.Cross(dir, transform.right).z;

            mRigid.angularVelocity = -rotateAmount * RotateSpeed;

            mRigid.linearVelocity = transform.right * Speed;

            //int monsterLayer = LayerMask.NameToLayer("Monster");
            //int bossLayer = LayerMask.NameToLayer("Boss");
            //int layerMask = (1 << monsterLayer) + (1 << bossLayer);
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 15f, layerMask);
            //Debug.DrawRay(transform.position, dir * 5f, Color.red, 0.2f);
            //if (hit.collider != null)
            //{
            //    var monster = hit.transform.GetComponent<Monster>();
            //    if (mTarget != null && monster == mTarget)
            //    {
            //        mRigid.velocity = transform.right * Speed;
            //    }
            //    else
            //    {
                    
            //    }
            //}
            //else
            //{
            //    mRigid.velocity = transform.right * Speed;
            //}

            Vector3 GetTargetPos()
            {
                if (mTarget != null && mTarget.IsAlive)
                {
                    return mTarget.GetPosition();
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (mParent == null || mIsHit)
                return;

            var monster = col.gameObject.GetComponent<Monster>();
            if (monster != null && monster.IsAlive)
            {
                string id = mParent.Id;

                var damageInst = mParent.CalcSkillDamage(monster);
                if (damageInst != null)
                {
                    mIsHit = true;

                    mParent.OnHit();

                    mParticleSystem.Stop();

                    mCollider.enabled = false;

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

                    gameObject.SetActive(false);
                }
            }
        }
    }
}