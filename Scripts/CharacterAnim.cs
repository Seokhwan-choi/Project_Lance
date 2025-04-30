using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.U2D.Animation;
using System;


namespace Lance
{
    class CharacterAnim
    {
        protected Animator mAnim;
        protected AnimEventHandler mHandler;
        public virtual void Init(Character parent)
        {
            mAnim = parent.GetComponentInChildren<Animator>();
            mHandler = parent.GetComponentInChildren<AnimEventHandler>();
            mHandler.Init(parent);
        }
        public virtual void SetBodyLibraryAsset(string bodyAsset) { }
        public virtual void SetHandLibraryAsset(string handAsset) { }
        public virtual void SetEtcLibraryAsset(string etcAsset, int orderInLayer) { }
        public virtual void OnRelease() { }
        public virtual void PlayDeath()
        {
            mAnim.SetTrigger("Death");
        }
        public virtual void PlayHit() { }

        public void PlayKnockback()
        {
            mAnim.SetTrigger("Knockback");
        }

        public float PlaySneer()
        {
            mAnim.SetTrigger("Sneer");

            return 0.5f;
        }

        public void PlayRage(Action onRage, Action onRageFinish)
        {
            mAnim.SetTrigger("Rage");

            mHandler.SetRageAction(onRage);
            mHandler.SetRageFinishAction(onRageFinish);
        }

        public void PlayIdle()
        {
            mAnim.SetTrigger("Idle");
        }

        public void PlayMove()
        {
            mAnim.SetTrigger("Move");
        }

        // To Do : 트리거를 통해서 애니메이터의 스테이트를 변경할 때
        // 스테이트가 즉시 반영되는것이 아니기 때문에..
        // 액션이 실행되는 순간에 스테이트의 정보를 받아와서
        // 애니메이션의 시간을 정확하게 계산할 수가 없다..
        // 일단 애니메이션 시간을 전부 0.5f로 맞춰났으니.. 나중에 바꿔라
        public void PlayNormalAttack(int index)
        {
            SetInBattle(true);

            string trigger = $"Attack_{index}";

            mAnim.SetTrigger(trigger);
        }

        public float PlaySkillCast()
        {
            SetInBattle(true);

            mAnim.SetTrigger("SkillCast");

            return 0.5f;
        }

        public void SetInBattle(bool inBattle)
        {
            mAnim.SetBool("InBattle", inBattle);
        }

        public void SetAtkSpeed(float atkSpeed)
        {
            float applyAtkSpeed = Mathf.Min(Lance.GameData.PlayerStatMaxData.atkSpeedMax, atkSpeed);

            mAnim.SetFloat("AtkSpeed", applyAtkSpeed);
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            float applyMoveSpeed = Mathf.Min(Lance.GameData.PlayerStatMaxData.moveSpeedMax, moveSpeed);

            mAnim.SetFloat("MoveSpeed", applyMoveSpeed);
        }
    }
}