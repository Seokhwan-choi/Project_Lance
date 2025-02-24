using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class Skill_12 : Skill
    {
        ObscuredFloat mDuration;
        List<Skill_ChasingWeapon> mChasingWeaponList;

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

            mDuration = mSkillData.duration;

            CreateChasingWeapons();
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mDuration.RandomizeCryptoKey();
        }

        public override void OnRelease()
        {
            base.OnRelease();

            foreach(Skill_ChasingWeapon chasingWeapon in mChasingWeaponList)
            {
                chasingWeapon.OnRelease();
            }

            mChasingWeaponList = null;
        }

        protected override void OnUpdate(float dt)
        {
            mDuration -= dt;
            if (mDuration <= 0)
            {
                OnFinish();
            }
        }

        protected override void OnFixedUpdate()
        {
            foreach (var chasingWeapon in mChasingWeaponList)
            {
                chasingWeapon.OnFixedUpdate();
            }
        }

        void CreateChasingWeapons()
        {
            mChasingWeaponList = new List<Skill_ChasingWeapon>();

            Vector2[] startPos = new Vector2[] { 
                new Vector2(-1.25f, 1.4f), new Vector2(-1.1f, 1.3f),
                new Vector2(-1.25f, 1.2f), new Vector2(-1.1f, 1.1f), 
                new Vector2(-1.25f, 1f)
            };

            float[] startAngle = new float[] {
                135f,150f,
                180f,215f,
                240f
            };

            int index = 0;

            foreach(var weapon in gameObject.GetComponentsInChildren<Skill_ChasingWeapon>())
            {
                if (startPos.Length > index)
                {
                    weapon.Init(this, startPos[index], startAngle[index]);

                    mChasingWeaponList.Add(weapon);

                    index++;
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
    }
}