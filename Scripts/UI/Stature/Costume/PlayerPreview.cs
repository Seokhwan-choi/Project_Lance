using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.Experimental.Rendering;

namespace Lance
{
    class PlayerPreview : MonoBehaviour
    {
        SpriteLibrary mBodySpriteLibrary;
        SpriteLibrary mHandSpriteLibrary;
        SpriteLibrary mEtcSpriteLibrary;
        SpriteRenderer mSpriteEtc;
        SpriteRenderer mSpriteLance;
        Camera mPreviewCamera;
        RenderTexture mRenderTexture;
        public void Init()
        {
            var previewPlayer = gameObject.FindGameObject("PreviewPlayer");

            mBodySpriteLibrary = previewPlayer.FindComponent<SpriteLibrary>("Body");
            mHandSpriteLibrary = previewPlayer.FindComponent<SpriteLibrary>("Hand");
            mEtcSpriteLibrary = previewPlayer.FindComponent<SpriteLibrary>("Etc");
            mSpriteEtc = previewPlayer.FindComponent<SpriteRenderer>("Etc");
            mSpriteLance = previewPlayer.FindComponent<SpriteRenderer>("Weapon");
            mPreviewCamera = gameObject.FindComponent<Camera>("PreviewCamera");

            // 호환 가능한 GraphicsFormat 사용
            GraphicsFormat graphicsFormat = SystemInfo.GetCompatibleFormat(GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormatUsage.Render);
            GraphicsFormat depthStencilFormat = GraphicsFormat.D16_UNorm;

            mRenderTexture = new RenderTexture(512, 512, graphicsFormat, depthStencilFormat);
            mRenderTexture.filterMode = FilterMode.Point;

            mPreviewCamera.targetTexture = mRenderTexture;

            Refresh();

            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public RenderTexture GetRenderTexture()
        {
            return mRenderTexture;
        }

        public void Refresh(string[] costumes)
        {
            RefreshBody(costumes[(int)CostumeType.Body]);
            RefreshEtc(costumes[(int)CostumeType.Etc]);
            RefreshWeapon(costumes[(int)CostumeType.Weapon]);
        }

        public void RefreshBody(string bodyCostume)
        {
            if (bodyCostume.IsValid())
            {
                // 착용중인 코스튬이 있다면 해당 코스튬으로 변경해주자.
                var costumeData = DataUtil.GetCostumeData(bodyCostume);
                if (costumeData != null)
                {
                    var libraryAssetData = Lance.GameData.LibraryAssetData.TryGet(costumeData.libraryAsset);
                    if (libraryAssetData != null)
                    {
                        string bodyLibraryAsset = libraryAssetData.libraryAsset;

                        var bodyCostumeAsset = Lance.SpriteLibrary.GetAsset(bodyLibraryAsset);

                        if (bodyCostumeAsset != null)
                            mBodySpriteLibrary.spriteLibraryAsset = bodyCostumeAsset;

                        var handCostumeAsset = Lance.SpriteLibrary.GetAsset(costumeData.handLibraryAsset);

                        if (handCostumeAsset != null)
                            mHandSpriteLibrary.spriteLibraryAsset = handCostumeAsset;
                    }
                }
                else
                {
                    // 없다면 기본 코스튬 장착
                    RefreshDefault();
                }
            }
            else
            {
                // 없다면 기본 코스튬 장착
                RefreshDefault();
            }

            void RefreshDefault()
            {
                var libraryAssetData = Lance.GameData.LibraryAssetData.TryGet(Lance.GameData.CostumeCommonData.playerDefaultLibraryBodyAsset);
                if (libraryAssetData != null)
                {
                    string bodyLibraryAsset = libraryAssetData.libraryAsset;
                    
                    var bodyCostumeAsset = Lance.SpriteLibrary.GetAsset(bodyLibraryAsset);
                    
                    if (bodyCostumeAsset != null)
                        mBodySpriteLibrary.spriteLibraryAsset = bodyCostumeAsset;

                    var handCostumeAsset = Lance.SpriteLibrary.GetAsset(Lance.GameData.CostumeCommonData.playerDefaultLibraryHandAsset);

                    if (handCostumeAsset != null)
                        mHandSpriteLibrary.spriteLibraryAsset = handCostumeAsset;
                }
            }
        }

        public void RefreshEtc(string etcCostume)
        {
            if (etcCostume.IsValid())
            {
                // 착용중인 코스튬이 있다면 해당 코스튬으로 변경해주자.
                var costumeData = DataUtil.GetCostumeData(etcCostume);
                if (costumeData != null)
                {
                    var etcCostumeAsset = Lance.SpriteLibrary.GetAsset(costumeData.libraryAsset);
                    if (etcCostumeAsset != null)
                    {
                        mEtcSpriteLibrary.spriteLibraryAsset = etcCostumeAsset;
                        mSpriteEtc.sortingOrder = costumeData.orderInLayer;
                    }
                    else
                    {
                        // 없다면 기본 코스튬 장착
                        RefreshDefault();
                    }
                }
                else
                {
                    // 없다면 기본 코스튬 장착
                    RefreshDefault();
                }
            }
            else
            {
                // 없다면 기본 코스튬 장착
                RefreshDefault();
            }

            void RefreshDefault()
            {
                var libraryAssetData = Lance.GameData.LibraryAssetData.TryGet(Lance.GameData.CostumeCommonData.playerDefaultLibraryBodyAsset);
                if (libraryAssetData != null)
                {
                    var etcCostumeAsset = Lance.SpriteLibrary.GetAsset(Lance.GameData.CostumeCommonData.playerDefaultLibraryEtcAsset);

                    mEtcSpriteLibrary.spriteLibraryAsset = etcCostumeAsset;
                }
            }
        }

        public void RefreshWeapon(string weaponCostume)
        {
            if (weaponCostume.IsValid())
            {
                // 착용중인 코스튬이 있다면 해당 코스튬으로 변경해주자.
                var costumeData = DataUtil.GetCostumeData(weaponCostume);
                if (costumeData != null)
                {
                    mSpriteLance.sprite = Lance.Atlas.GetPlayerSprite(costumeData.sprite);
                }
                else
                {
                    // 없다면 기본 코스튬 장착
                    RefreshDefault();
                }
            }
            else
            {
                // 없다면 기본 코스튬 장착
                RefreshDefault();
            }

            void RefreshDefault()
            {
                mSpriteLance.sprite = Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultWeapon);
            }
        }

        public void OnSelectBodyCostume(string id)
        {
            RefreshBody(id);
        }

        public void OnSelectEtcCostume(string id)
        {
            RefreshEtc(id);
        }

        public void OnSelectWeaponCostume(string id)
        {
            RefreshWeapon(id);
        }

        void Refresh()
        {
            var bodyCostume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Body);
            var weaponCostume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Weapon);
            var etcCostume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Etc);

            RefreshBody(bodyCostume);
            RefreshWeapon(weaponCostume);
            RefreshEtc(etcCostume);
        }

        void OnDestroy()
        {
            // 메모리 누수를 방지하기 위해 RenderTexture 해제
            if (mRenderTexture != null)
            {
                mRenderTexture.Release();
                mRenderTexture = null;
            }
        }
    }
}