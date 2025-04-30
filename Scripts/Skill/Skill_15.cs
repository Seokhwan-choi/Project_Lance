using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    class Skill_15 : Skill
    {
        public override void Init(Player player, SkillData skillData)
        {
            base.Init(player, skillData);

            ElementalType elementalType = player.Stat.ElementalType;

            for (int i = 0; i < 2; ++i)
            {
                var fxObj = gameObject.FindGameObject($"FX_{i + 1}");

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

        protected override void OnUpdate(float dt)
        {
            mDelay -= dt;
            if (mDelay <= 0)
            {
                OnAttack();

                mDelay = mSkillData?.atkDelay ?? 0;
            }
        }
    }
}