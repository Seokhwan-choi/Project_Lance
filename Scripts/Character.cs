using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.U2D.Animation;


namespace Lance
{
    class Character : MonoBehaviour
    {
        protected ActionManager mActionManager;
        protected HpGaugebarUI mHpGaugebarUI;
        protected CharacterAnim mAnim;
        protected CharacterPhysics mPhysics;
        protected CharacterStat mStat;
        protected LibraryAssetData mSpriteLibraryAssetData;
        public CharacterPhysics Physics => mPhysics;
        public CharacterAnim Anim => mAnim;
        public CharacterStat Stat => mStat;
        public ActionManager Action => mActionManager;
        public LibraryAssetData SpriteLibraryAssetData => mSpriteLibraryAssetData;
        public bool IsPlayer => this is Player;
        public bool IsMonster => !IsPlayer;
        public bool IsBoss => this is Boss;
        public bool IsRaidBoss => this is RaidBoss;
        public bool IsDeath => mStat.IsDeath;
        public bool IsAlive => !IsDeath;

        private void OnDrawGizmosSelected()
        {
            if (IsMonster)
            {
                var spriteRednerer = GetComponent<SpriteRenderer>();
                if (spriteRednerer != null)
                {
                    Bounds rect = spriteRednerer.bounds;

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(rect.center, rect.size);
                }
            }
            else
            {
                Bounds bounds = gameObject.FindComponent<SpriteRenderer>("Body").bounds;

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
        public virtual void Init() { }
        public virtual void OnUpdate(float dt)
        {
            if (IsDeath)
                return;

            mActionManager.OnUpdate(dt);
            mPhysics.OnUpdate(dt);
            mHpGaugebarUI?.UpdatePos();
        }

        public virtual void CreateHpGaugeUI()
        {
            mHpGaugebarUI = UIUtil.CreateHpGaugebarUI(this);
        }

        public virtual Bounds GetBounds()
        {
            var polygon = GetComponent<PolygonCollider2D>();

            return polygon.bounds;
        }

        public void SetPosition(Vector2 pos)
        {
            mPhysics.SetPosition(pos);
        }
        public Vector2 GetPosition()
        {
            return mPhysics.GetPosition();
        }
        public virtual void RandomizeKey() 
        {
            mStat.RandomizeKey();
        }
        public virtual float GetBodySize(){ return 0f; }
        public virtual bool AnyInAttackRangeOpponent() { return false; }
        public virtual float GetMoveSpeed() { return mStat.MoveSpeed; }
        public virtual float GetMoveSpeedRatio() { return mStat.MoveSpeedRatio; }
        public virtual void OnDamage(DamageInst inst) { }
        public virtual void OnRelease() 
        {
            mStat.OnRelease();
            mAnim.OnRelease();
            mPhysics.OnRelease();
            mActionManager.OnRelease();
        }
        public virtual void OnDeath() { }
    }
}