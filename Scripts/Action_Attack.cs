using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    class Action_Attack : ActionInst
    {
        public override void OnStart(Character parent)
        {
            base.OnStart(parent);

            int randAtkIndex = Util.RandomChoose(parent.SpriteLibraryAssetData.attackProb);

            float atkSpeed = Mathf.Min(Lance.GameData.PlayerStatMaxData.atkSpeedMax, parent.Stat.AtkSpeed);

            mDuration = parent.SpriteLibraryAssetData.attackTime[randAtkIndex] * (1f / atkSpeed);

            int atkIndex = randAtkIndex + 1;

            parent.Anim.PlayNormalAttack(atkIndex);

            if (parent.IsPlayer)
            {
                PlayAttackFX(parent, atkIndex, atkSpeed);
            }

            SoundPlayer.PlayAttack(parent, atkIndex);
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            mDuration -= dt;
        }

        public override void OnFinish()
        {
            base.OnFinish();

            mParent.Anim.SetInBattle(mParent.AnyInAttackRangeOpponent());
        }

        void PlayAttackFX(Character parent, int atkIndex, float atkSpeed)
        {
            Vector2 offsetPos = atkIndex == 1 ? new Vector2(0f, 0.3f) : new Vector2(0.3f, 0.3f);

            var attackFX = Lance.ParticleManager.Aquire($"Player_Attack_{atkIndex}_FX", parent.transform, offsetPos);

            // 공격속성 확인해서 적절하게 바꿔주자
            for(int i = 0; i < (int)ElementalType.Count; ++i)
            {
                ElementalType type = (ElementalType)i;

                var ps = attackFX.FindComponent<ParticleSystem>($"FX_{type}");

                if (type == ElementalType.Fire)
                {
                    if (type == parent.Stat.ElementalType || parent.Stat.ElementalType == ElementalType.Normal)
                    {
                        ps.gameObject.SetActive(true);

                        var main = ps.main;

                        main.simulationSpeed = atkSpeed;
                    }
                    else
                    {
                        ps.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (type == parent.Stat.ElementalType)
                    {
                        ps.gameObject.SetActive(true);

                        var main = ps.main;

                        main.simulationSpeed = atkSpeed;
                    }
                    else
                    {
                        ps.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}