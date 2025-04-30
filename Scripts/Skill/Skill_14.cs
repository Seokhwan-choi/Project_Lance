using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Skill_14 : Skill
    {
        ElementalType mElementalType;
        int mIndex;
        public override void Init(Player player, SkillData skillData)
        {
            mIndex = 0;

            base.Init(player, skillData);

            mElementalType = player.Stat.ElementalType;

            var weaponCostume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Weapon);
            if (weaponCostume != null)
            {
                var costumeData = Lance.GameData.WeaponCostumeData.TryGet(weaponCostume);
                if (costumeData != null)
                {
                    // 플레이어가 착용중인 장비로 바꿔주자.
                    foreach (var renderer in gameObject.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        renderer.sprite = Lance.Atlas.GetPlayerSprite(costumeData.skillSprite);
                    }
                }
            }
        }

        public override void OnAttack()
        {
            base.OnAttack();

            var lance = gameObject.FindGameObject($"Lance_{mIndex + 1}");

            if (SaveBitFlags.SkillEffect.IsOn())
            {
                var fxObj = Lance.ParticleManager.Aquire("Skill_14_FX", lance.transform);

                for (int j = 0; j < (int)ElementalType.Count; ++j)
                {
                    ElementalType type = (ElementalType)j;

                    var fxObjChild = fxObj.FindGameObject($"FX_{type}");

                    if (type == ElementalType.Fire)
                    {
                        fxObjChild.SetActive(type == mElementalType || mElementalType == ElementalType.Normal);
                    }
                    else
                    {
                        fxObjChild.SetActive(type == mElementalType);
                    }
                }
            }

            Lance.CameraManager.Shake();

            mIndex++;
        }
    }
}