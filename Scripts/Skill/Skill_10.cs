using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Skill_10 : Skill
    {
        public override void Init(Player player, SkillData skillData)
        {
            base.Init(player, skillData);

            ElementalType elementalType = player.Stat.ElementalType;

            for (int i = 0; i < 10; ++i)
            {
                var fxObj = gameObject.FindGameObject($"MagicPillarBlast_{i + 1}");

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
            }

            var fxObj2 = gameObject.FindGameObject($"FX");

            for (int j = 0; j < (int)ElementalType.Count; ++j)
            {
                ElementalType type = (ElementalType)j;

                var fxObjChild = fxObj2.FindGameObject($"FX_{type}");

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
                    foreach (var renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
                    {
                        renderer.sprite = Lance.Atlas.GetPlayerSprite(costumeData.skillSprite);
                    }
                }
            }
        }

        public override void OnAttack()
        {
            base.OnAttack();

            Lance.CameraManager.Shake();
        }
    }
}