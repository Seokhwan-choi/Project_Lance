using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Skill_3 : Skill
    {
        bool mInAttack;
        Skill_ThrowingLance mLance;
        public bool InAttack => mInAttack;
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
                }
                else
                {
                    fxObjChild.SetActive(type == elementalType);
                }
            }

            mLance = GetComponentInChildren<Skill_ThrowingLance>();
            mLance.Init(this);

            var weaponCostume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Weapon);
            if (weaponCostume != null)
            {
                var costumeData = Lance.GameData.WeaponCostumeData.TryGet(weaponCostume);
                if (costumeData != null)
                {
                    // 플레이어가 착용중인 장비로 바꿔주자.
                    SpriteRenderer renderer = gameObject.GetComponentInChildren<SpriteRenderer>();

                    renderer.sprite = Lance.Atlas.GetPlayerSprite(costumeData.skillSprite);
                }
            }
        }

        public override void OnReady()
        {
            base.OnReady();

            mInAttack = true;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            mLance.OnRelease();
            mLance = null;
        }

        public override void OnAttack()
        {
            PlayAttackSound();

            Lance.CameraManager.Shake(duration: 0.25f);
        }

        public override void OnFinish()
        {
            base.OnFinish();

            mInAttack = false;
        }
    }
}