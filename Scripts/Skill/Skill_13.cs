using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Skill_13 : Skill
    {
        public override void Init(Player player, SkillData skillData)
        {
            base.Init(player, skillData);

            ElementalType elementalType = player.Stat.ElementalType;

            var fxObj = gameObject.FindGameObject($"FX");

            for (int j = 0; j < (int)ElementalType.Count; ++j)
            {
                ElementalType type = (ElementalType)j;

                var fxObjChild = fxObj.FindGameObject($"FX_{type}");

                if (type == ElementalType.Fire)
                {
                    fxObjChild.SetActive(type == elementalType || elementalType == ElementalType.Normal);
                }
                else
                {
                    fxObjChild.SetActive(type == elementalType);
                }
            }

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

            Lance.CameraManager.Shake(strength: 0.25f);
        }

        public override void OnAttack()
        {
            base.OnAttack();

            Lance.CameraManager.Shake(duration:1f, strength:0.5f);
        }
    }
}