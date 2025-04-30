using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    // 타겟 추적 스킬
    class Skill_ChasingWeapon : MonoBehaviour
    {
        const float Speed = 5f;
        const float WaitSpeed = 10f;
        const float BoostSpeed = 15f;
        const float RotateSpeed = 900f;
        const float HitTime = 0.15f;

        float mHitTime;
        Skill mParent;
        Monster mTarget;
        ParticleSystem mHitFX;
        Rigidbody2D mRigid;
        public void Init(Skill parent, Vector2 startPos, float startAngle)
        {
            mParent = parent;
            mRigid = GetComponent<Rigidbody2D>();
            mRigid.angularVelocity = 0;
            mRigid.linearVelocity = Vector2.zero;

            transform.localPosition = startPos;
            transform.localRotation = Quaternion.Euler(0, 0, startAngle);

            ElementalType elementalType = parent.Player.Stat.ElementalType;

            var fxObj = gameObject.FindGameObject($"MagicPillarBlast");

            for (int j = 0; j < (int)ElementalType.Count; ++j)
            {
                ElementalType type = (ElementalType)j;

                var fxObjChild = fxObj.FindGameObject($"FX_{type}");

                if (type == ElementalType.Fire)
                {
                    fxObjChild.SetActive(type == elementalType || elementalType == ElementalType.Normal);

                    if (type == elementalType || elementalType == ElementalType.Normal)
                    {
                        mHitFX = fxObjChild.GetComponent<ParticleSystem>();
                    }
                }
                else
                {
                    fxObjChild.SetActive(type == elementalType);

                    if (type == elementalType)
                    {
                        mHitFX = fxObjChild.GetComponent<ParticleSystem>();
                    }
                }
            }

            mTarget = null;
        }

        public void OnRelease()
        {
            mTarget = null;
            mRigid = null;

            mHitFX.Stop();
            mHitFX = null;
        }

        public void OnFixedUpdate()
        {
            if (mTarget == null || mTarget.IsDeath)
            {
                mTarget = Lance.GameManager.StageManager.GetFirstMonster();
            }

            if (mHitTime > 0f)
            {
                mHitTime -= Time.fixedDeltaTime;
                mRigid.freezeRotation = true;
                mRigid.linearVelocity = transform.right * BoostSpeed;
            }
            else
            {
                Vector3 targetPos = GetTargetPos();

                Vector2 dir = (Vector2)targetPos - mRigid.position;

                dir.Normalize();

                if (mRigid.freezeRotation)
                    mRigid.freezeRotation = false;

                float rotateAmount = Vector3.Cross(dir, transform.right).z;

                mRigid.angularVelocity = -rotateAmount * RotateSpeed;

                int monsterLayer = LayerMask.NameToLayer("Monster");
                int bossLayer = LayerMask.NameToLayer("Boss");
                int layerMask = (1 << monsterLayer) + (1 << bossLayer);

                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 15f, layerMask);
                Debug.DrawRay(transform.position, dir * 5f, Color.red, 0.2f);
                if (hit.collider != null)
                {
                    var monster = hit.transform.GetComponent<Monster>();
                    if (mTarget != null && monster == mTarget && mHitTime <= 0f)
                    {
                        mRigid.linearVelocity = transform.right * BoostSpeed;
                    }
                    else
                    {
                        mRigid.linearVelocity = transform.right * Speed;
                    }
                }
                else
                {
                    mRigid.linearVelocity = transform.right * Speed;
                }
            }

            Vector3 GetTargetPos()
            {
                if (mTarget != null && mTarget.IsAlive)
                {
                    return mTarget.GetPosition();
                }
                else
                {
                    return new Vector3(0,3);
                }
            }
        }

        void OnTriggerEnter2D(Collider2D col)
        {
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

                    mHitFX?.Stop();
                    mHitFX?.Play();

                    mHitTime = HitTime;
                }
            }
        }
    }
}
