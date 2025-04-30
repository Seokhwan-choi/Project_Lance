using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class Action_Rage : ActionInst
    {
        bool mInvincibility;
        RageActionData mData;
        public bool Invincibility => mInvincibility;
        public Action_Rage(RageActionData data)
        {
            mData = data;
        }

        public override void OnStart(Character parent)
        {
            base.OnStart(parent);

            mDuration = parent.SpriteLibraryAssetData.rageTime;

            parent.Anim.PlayRage(OnRage, OnRageFinish);

            void OnRage()
            {
                SoundPlayer.PlayRage(parent.Stat.ElementalType);

                if (mData.type == RageActionType.Knockback)
                {
                    Lance.CameraManager.Shake(1f, 0.5f);

                    Lance.GameManager.PlayKnockback(mData.value);
                }
                else if (mData.type == RageActionType.Shield) // 쉴드 생성
                {
                    Lance.CameraManager.Shake(0.5f, 0.25f);

                    var shieldData = Lance.GameData.ShieldData.TryGet(mData.etc);

                    parent.Stat.SetShield(shieldData);
                }
                else // 공격속도 업
                {
                    mInvincibility = true;

                    parent.Stat.PowerUpRageModeRaidBoss(mData.value);
                }
            }

            void OnRageFinish()
            {
                SoundPlayer.PlayRageFinish(parent.Stat.ElementalType);

                Lance.CameraManager.Shake(0.5f, 0.5f);

                Lance.GameManager.PlayKnockback(3.5f);

                mInvincibility = false;
            }
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            mDuration -= dt;
        }
    }
}