using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class JoustOpponent : Monster
    {
        SpriteRenderer mWeaponRenderer;
        public override void Init()
        {
            mAnim = new PlayerAnim();
            mAnim.Init(this);

            mPhysics = new CharacterPhysics();
            mPhysics.Init(this);

            mActionManager = new ActionManager();
            mActionManager.Init(this);

            mStat = new CharacterStat();

            mActionManager.PlayAction(ActionType.Move);

            mWeaponRenderer = gameObject.FindComponent<SpriteRenderer>("Weapon");

            SetPosition(new Vector2(51.15f, 1.2f));
        }

        public override void OnUpdate(float dt)
        {
            mActionManager.OnUpdate(dt);
            mPhysics.OnUpdate(dt);
        }

        public override bool AnyInAttackRangeOpponent()
        {
            return false;
        }

        public void UpdateCostumes(string[] costumes)
        {
            string bodyLibraryAsset = Lance.GameData.CostumeCommonData.playerDefaultJoustingBodyLibraryAsset;
            string handLibraryAsset = Lance.GameData.CostumeCommonData.playerDefaultLibraryHandAsset;

            string bodyCostume = costumes[(int)CostumeType.Body];
            if (bodyCostume.IsValid())
            {
                var bodyCostumeData = Lance.GameData.BodyCostumeData.TryGet(bodyCostume);
                if (bodyCostumeData != null)
                {
                    bodyLibraryAsset = bodyCostumeData.joustingLibraryAsset;
                    handLibraryAsset = bodyCostumeData.handLibraryAsset;
                }
            }

            mSpriteLibraryAssetData = Lance.GameData.LibraryAssetData.TryGet(bodyLibraryAsset);

            string etcLibrearyAsset = Lance.GameData.CostumeCommonData.playerDefaultLibraryEtcAsset;
            int etcOrderInLayer = 0;

            string etcCostume = costumes[(int)CostumeType.Etc];
            if (etcCostume.IsValid())
            {
                CostumeData etcCostumeData = Lance.GameData.EtcCostumeData.TryGet(etcCostume);
                if (etcCostumeData != null)
                {
                    etcLibrearyAsset = etcCostumeData.joustingLibraryAsset;
                    etcOrderInLayer = etcCostumeData.orderInLayer;
                }
            }

            mAnim.SetBodyLibraryAsset(mSpriteLibraryAssetData.libraryAsset);
            mAnim.SetHandLibraryAsset(handLibraryAsset);
            mAnim.SetEtcLibraryAsset(etcLibrearyAsset, etcOrderInLayer);

            string weaponSprite = Lance.GameData.CostumeCommonData.playerDefaultWeapon;

            string weaponCostume = costumes[(int)CostumeType.Weapon];
            if (weaponCostume.IsValid())
            {
                var weaponCostumeData = Lance.GameData.WeaponCostumeData.TryGet(weaponCostume);
                if (weaponCostumeData != null)
                {
                    weaponSprite = weaponCostumeData.sprite;
                }
            }

            mWeaponRenderer.sprite = Lance.Atlas.GetPlayerSprite(weaponSprite);
        }

        public override float GetBodySize()
        {
            return 0.5f;
        }
    }
}