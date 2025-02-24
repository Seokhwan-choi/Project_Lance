using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Skill_8 : Skill
    {
        public override void Init(Player player, SkillData skillData)
        {
            base.Init(player, skillData);

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

        public override void OnAttack()
        {
            base.OnAttack();

            Lance.CameraManager.Shake(strength:0.25f);
        }
    }
}