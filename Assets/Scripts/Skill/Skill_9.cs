using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Skill_9 : Skill
    {
        ParticleSystem mAttackFX;
        Skill_Knight mKnight;

        public override void Init(Player player, SkillData skillData)
        {
            base.Init(player, skillData);

            ElementalType elementalType = player.Stat.ElementalType;

            for (int j = 0; j < (int)ElementalType.Count; ++j)
            {
                ElementalType type = (ElementalType)j;

                var fxObjChild = gameObject.FindGameObject($"FX_{type}");

                if (type == ElementalType.Fire)
                {
                    fxObjChild.SetActive(type == elementalType || elementalType == ElementalType.Normal);

                    if (type == elementalType || elementalType == ElementalType.Normal)
                    {
                        mAttackFX = fxObjChild.GetComponent<ParticleSystem>();
                    }
                }
                else
                {
                    fxObjChild.SetActive(type == elementalType);

                    if (type == elementalType)
                    {
                        mAttackFX = fxObjChild.GetComponent<ParticleSystem>();
                    }
                }
            }

            mKnight = GetComponentInChildren<Skill_Knight>();
            mKnight.Init(this);
        }

        public void PlayKnightDash()
        {
            mKnight.PlayDash();
        }

        public override void OnRelease()
        {
            base.OnRelease();

            mAttackFX.Stop();
            mAttackFX = null;

            mKnight.OnRelease();
            mKnight = null;
        }

        public void PlayFX()
        {
            mAttackFX?.Stop();
            mAttackFX?.Play();
        }
    }
}